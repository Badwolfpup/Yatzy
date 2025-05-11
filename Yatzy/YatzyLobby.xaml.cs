using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace Yatzy
{
    /// <summary>
    /// Interaction logic for YatzyLobby.xaml
    /// </summary>
    public partial class YatzyLobby : Window, INotifyPropertyChanged
    {
        private  HubConnection _connection;
        public string _username = "";
        private MainWindow _mainWindow;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken; 
        public ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();

        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new ObservableCollection<ChatMessage>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _connectionattempts = 0;

        private string _randomopponentbutton = "Random opponent";
        public string RandomOpponentButton
        {
            get { return _randomopponentbutton; }
            set
            {
                _randomopponentbutton = value;
                OnPropertyChanged(nameof(RandomOpponentButton));
            }
        }

        private bool _isconnected;
        public bool IsConnected
        {
            get { return _isconnected; }
            set
            {
                _isconnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        private bool _enableSinglePlayerButton;

        public bool EnableSinglePlayerButton
        {
            get { return _enableSinglePlayerButton; }
            set
            {
                _enableSinglePlayerButton = value;
                OnPropertyChanged(nameof(EnableSinglePlayerButton));
            }
        }


        private string _newmessage;
        public bool InQueue;

        public string NewMessage
        {
            get { return _newmessage; }
            set
            {
                _newmessage = value;
                OnPropertyChanged(nameof(NewMessage));
            }
        }

        public YatzyLobby()
        {
            InitializeComponent();
            DataContext = this;
            //OpenNickName();
            Loaded += async (s, e) => await OpenNickName();
            Closing += YatzyLobby_Closing;
            ChatMessages.CollectionChanged += (s, e) =>
            {
                if (ChatMessages.Any())
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ChatMessageBox.ScrollIntoView(ChatMessages.Last());
                    });
                }
            };
            //OpenConnection();
        }


        public async void YatzyLobby_Closing(object? sender, CancelEventArgs e)
        {
            cancellationTokenSource?.Cancel();

            try
            {
                if (_connection != null)
                {
                    await _connection.StopAsync();       // Await to ensure graceful shutdown
                    await _connection.DisposeAsync();    // Dispose asynchronously
                }
            }
            catch (Exception ex)
            {
                // Optional: log or handle error
            }
            finally
            {
                cancellationTokenSource?.Dispose();
            }

        }

        private async Task OpenConnection()
        {
            //  _connection = new HubConnectionBuilder()
            //.WithUrl("http://193.181.23.229:50001/lobbyHub")
            //.Build();
            _connection = new HubConnectionBuilder()
           .WithUrl("http://192.168.0.3:5000/lobbyHub")
           .Build();
            // Handle connection closed event for reconnection
            _connection.Closed += async (error) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ChatMessages.Add(new ChatMessage
                    {
                        Sender = "System",
                        Message = error != null ? $"Connection lost: {error.Message}" : "Connection closed"
                    });
                });
                await Task.Delay(5000); // Retry after 5 seconds
                await TryConnectAsync();
            };

            _connection.On<string>("PlayerJoined", (username) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"{username} joined the lobby!" });
                });
            });

            _connection.On<string>("PlayerDisconnected", (username) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"{username} left the lobby!" });
                });
            });

            _connection.On<object[]>("UpdatePlayerList", (playerlist) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Players.Clear();
                    foreach (var item in playerlist)
                    {
                        var player = JsonConvert.DeserializeObject<Player>(item.ToString());
                        Players.Add(player);
                    }
                });
            });

            _connection.On<string, string>("ReceiveMessage", (sender, message) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ChatMessages.Add(new ChatMessage { Sender = "System", Message = message });
                });
            });

            _connection.On<string, string>("GameStarted", (player1, player2) => {
                Dispatcher.Invoke(() =>
                 {
                     _mainWindow = new MainWindow(this, new ObservableCollection<Player>
                     {
                        Players.FirstOrDefault(p => p.UserName == player1),
                        Players.FirstOrDefault(p => p.UserName == player2)
                     });
                     InQueue = false;
                     _mainWindow.Show();
                     cancellationTokenSource.Cancel();
                     Hide();
                 });
            });

            _connection.On<string>("UpdateDiceValue", (json) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _mainWindow.UpdateDiceValue(json);
                });
            });

            _connection.On<string>("UpdateDiceBorder", (json) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _mainWindow.UpdateDiceBorder(json);
                });
            });

            _connection.On("UpdateTurn", () =>
            {
                Dispatcher.Invoke(() =>
                {
                    _mainWindow.UpdateTurn();
                });
            });

            _connection.On<string>("UpdatePoints", (json) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _mainWindow.UpdatePoints(json);
                });
            });

            _connection.On("PlayerLeft", () => {
                Dispatcher.Invoke(() =>
                 {
                     _mainWindow.PlayerLeft();
                 });
            });

            _connection.On<string, string>("InvitePlayer", (id, username) =>
            {
                Dispatcher.Invoke(() =>
                 {
                     InvitePlayer(id, username);
                 });
            });

            _connection.On<string>("RejectedInvite", (username) =>
            {
                Dispatcher.Invoke(() =>
                 {
                     RejectedInvite(username);
                 });
            });

            _connection.On("GameFinished", () =>
            {
                Dispatcher.Invoke(() =>
                 {
                     _mainWindow.GameFinished();
                 });
            });

            Task.Run(() => TryConnectAsync());

        }

        public async Task UpdateDicevalue(ObservableCollection<Dice> dices)
        {
            var dicevalues = dices.Select(d => d.DiceValue).ToList();
            var json = JsonConvert.SerializeObject(dicevalues, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            _connection.InvokeAsync("UpdateDicevalue", json);
        }

        public async Task UpdateDiceBorder((int index, bool border) changeborder)
        {
            var json = JsonConvert.SerializeObject(changeborder, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            _connection.InvokeAsync("UpdateDiceBorder", json);
        }

        public async Task UpdateTurn()
        {
            _connection.InvokeAsync("UpdateTurn");
        }

        public async Task PlayerLeft()
        {
            _connection.InvokeAsync("PlayerLeft");
        }

        public async Task UpdatePoints(ObservableCollection<PointsClass> points)
        {
            var json = JsonConvert.SerializeObject(points, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            _connection.InvokeAsync("UpdatePoints", json);
        }

        public async Task InvitePlayer(string username)
        {
            _connection.InvokeAsync("InvitePlayer", username);
        }

        public async Task AnswerToInvite(bool answer, string id)
        {
            _connection.InvokeAsync("AnswerToInvite", answer, id);
        }

        public async Task GameFinished()
        {
            _connection.InvokeAsync("GameFinished");
        }

        private async Task OpenNickName()
        {
            NickName popup = new NickName
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            bool? result = popup.ShowDialog();
            _username = popup.NickNameText != "" ? popup.NickNameText : "Player" + new Random().Next(1000, 9999);

            OpenConnection();
        }


        private async Task TryConnectAsync()
        {
            cancellationToken = cancellationTokenSource.Token;
            var retryUntil = DateTime.UtcNow.AddMinutes(1);
            
            IsConnected = false;
            while (_connection.State != HubConnectionState.Connected)
            {


                if (DateTime.UtcNow > retryUntil)
                {
                    Dispatcher.Invoke(() => ChatMessages.Add(new ChatMessage { Sender = "System", Message = "Connection retry limit reached (1 minutes)." }));
                    MessageBox.Show("Connection retry limit reached (1 minutes). You will only be able to play single player", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    EnableSinglePlayerButton = true;
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Dispatcher.Invoke(() => ChatMessages.Add(new ChatMessage { Sender = "System", Message = "Connection attempt canceled." }));
                    break;
                }
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        ChatMessages.Add(new ChatMessage { Sender = "System", Message = "Attempting to connect..." });
                    });

                    try
                    {
                        var connectTask = _connection.StartAsync();
                        if (await Task.WhenAny(connectTask, Task.Delay(19000, cancellationToken)) == connectTask)
                        {
                            await connectTask;
                            Dispatcher.Invoke(() =>
                            {
                                ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Connection established: {_connection.State}" });
                            });

                             await _connection.InvokeAsync("JoinLobby", _username, cancellationToken);

                            IsConnected = true;
                            EnableSinglePlayerButton = true;
                            _ = MonitorConnection(); // Fire-and-forget
                            break; // Exit loop on success
                        }
                        else
                        {
                            throw new TimeoutException("Connection attempt timed out.");
                        }
                    }
                    catch (ObjectDisposedException) { } // Handle disposed token
                    catch (OperationCanceledException ex) { }
                    catch (Exception ex) 
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _connectionattempts++;
                            ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Connection failed ({_connectionattempts}): {ex.Message}" });
                        });

                        await Task.Delay(5000); // Retry after 5 seconds
                    }

                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _connectionattempts++;
                        ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Connection failed ({_connectionattempts}): {ex.Message}" });
                    });

                    await Task.Delay(5000); // Retry after 5 seconds
                }
            }
        }

        

        private async Task MonitorConnection()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _connection.InvokeAsync("Ping", cancellationToken);
                    Dispatcher.Invoke(() =>
                    {
                        IsConnected = _connection.State == HubConnectionState.Connected;
                    });

                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        IsConnected = false;
                        ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Connection lost: {ex.Message}" });
                        ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Attempting to reconnect..." });
                    });

                    // Attempt to reconnect
                    try
                    {
                        await TryConnectAsync(); // Directly await
                        if (_connection.State == HubConnectionState.Connected)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ChatMessages.Add(new ChatMessage { Sender = "System", Message = "Reconnected successfully." });
                            });
                        }
                    }
                    catch (Exception reconEx)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Reconnection failed: {reconEx.Message}" });
                        });
                    }

                    // Wait before next ping/reconnect attempt
                    try
                    {
                        await Task.Delay(10000, cancellationToken);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (ObjectDisposedException) { break; }
                    continue; // Continue monitoring loop
                }
            }

        }

        public async Task LeaveQueue()
        {
            try
            {
                await _connection.InvokeAsync("LeaveQueue");
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Error leaving queue: {ex.Message}" });
                });
            }
        }

        public void InvitePlayer(string id, string user)
        {
            bool result = MessageBox.Show($"Do you want to play with {user}?", "Invite", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            _connection.InvokeAsync("AnswerToInvite", result, id);
        }

        public void RejectedInvite(string user)
        {
            MessageBox.Show($"{user} rejected your invite", "Invite", MessageBoxButton.OK);
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(NewMessage))
            {
                await _connection.InvokeAsync("SendMessage", NewMessage);
                NewMessage = string.Empty;
            }
        }

        private void Start_SinglePlayer_Click(object sender, RoutedEventArgs e)
        {
            var player = Players.FirstOrDefault(p => p.UserName == _username);
            if (player == default || player == null)
            {
                player = new Player()
                {
                    UserName = _username,
                };
  
            }
            _mainWindow = new MainWindow(this, new ObservableCollection<Player> { player });
            _mainWindow.SinglePlayerGame = true;
            _mainWindow.Show();
            cancellationTokenSource.Cancel();
            Hide();
        }



        private void Invite_Player_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerListbox is ListBox listBox && listBox.SelectedItem is Player selectedPlayer)
            {
                if (selectedPlayer.UserName == _username) return;

                if (selectedPlayer.Status == Status.Playing)
                {
                    MessageBox.Show("Player is already in a game");
                    return;
                }
                _connection.InvokeAsync("InvitePlayer", selectedPlayer.UserName);
            }
        }

        private void Random_opponent_Click(object sender, RoutedEventArgs e)
        {
            
            if (!InQueue)
            {
                RandomOpponentButton = "Cancel queue";
                InQueue = true;
                _connection.InvokeAsync("QueueForGame");
            }
            else
            {
                RandomOpponentButton = "Random opponent";
                InQueue = false;
                _connection.InvokeAsync("LeaveQueue");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);
            }
        }

    }

    
}
