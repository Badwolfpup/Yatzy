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
using System.Net.Sockets;

namespace Yatzy
{
    /// <summary>
    /// Interaction logic for YatzyLobby.xaml
    /// </summary>
    public partial class YatzyLobby : Window, INotifyPropertyChanged
    {
        private  HubConnection _connection;
        private string _username = "";
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
        public bool InQueue = true;

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
           .WithUrl("http://localhost:5000/lobbyHub")
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
                    ChatMessages.Add(new ChatMessage { Sender = sender, Message = message });
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

            _connection.On<int[], bool[]>("UpdateDice", (number, saved) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _mainWindow.UnPackDices(number, saved);
                });
            });

            Task.Run(() => TryConnectAsync());

        }

        public async Task UpdateDice(int[] nums, bool[] saves)
        {
            _connection.InvokeAsync("UpdateDice", nums, saves);
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
        //protected override async void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);
        //    cancellationTokenSource.Cancel(); // Cancel any running tasks
        //    try
        //    {
        //        await _connection.StopAsync(); // Gracefully stop the connection
        //        await _connection.DisposeAsync(); // Dispose connection asynchronously
        //    }
        //    catch { }
        //    cancellationTokenSource.Dispose(); // Dispose cancellation token
        //}

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
        }

        private void Random_opponent_Click(object sender, RoutedEventArgs e)
        {
            _connection.InvokeAsync("QueueForGame");
            //var queueing = new Queueing(this);
            //queueing.Owner = this;
            //queueing.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //queueing.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);
            }
        }

        private void ShowToolTip_Click(object sender, RoutedEventArgs e)
        {
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    double targetWidth = MultiPlayerButton.ActualWidth;
            //    double popupWidth = MultiPlayerPopUp.ActualWidth;
            //    MultiPlayerPopUp.HorizontalOffset = (targetWidth - popupWidth) / 2;
            //}), DispatcherPriority.Loaded);
        }
    }

    
}
