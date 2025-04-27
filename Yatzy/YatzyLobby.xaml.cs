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

namespace Yatzy
{
    /// <summary>
    /// Interaction logic for YatzyLobby.xaml
    /// </summary>
    public partial class YatzyLobby : Window, INotifyPropertyChanged
    {
        private readonly HubConnection _connection;
        private readonly string _username;
        public ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();

        public ObservableCollection<ChatMessage> ChatMessages { get; } = new ObservableCollection<ChatMessage>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _newmessage;
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
            _connection = new HubConnectionBuilder()
                .WithUrl("http://193.181.23.229:50001/lobbyHub")
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
                    foreach(var item in playerlist)
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

            _username = "Player" + new Random().Next(1000, 9999);

            Task.Run(() => TryConnectAsync());
        }

        private async Task TryConnectAsync()
        {
            while (_connection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        ChatMessages.Add(new ChatMessage { Sender = "System", Message = "Attempting to connect..." });
                    });

                    await _connection.StartAsync();

                    Dispatcher.Invoke(() =>
                    {
                        ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Connection established: {_connection.State}" });
                    });

                    await _connection.InvokeAsync("JoinLobby", _username);
                    break; // Exit loop on success
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ChatMessages.Add(new ChatMessage { Sender = "System", Message = $"Connection failed: {ex.Message}" });
                    });
                    await Task.Delay(5000); // Retry after 5 seconds
                }
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NewMessage))
            {
                await _connection.InvokeAsync("SendMessage", NewMessage);
                NewMessage = string.Empty;
            }
        }

        private void StartGame_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Start Game not implemented yet.");
        }
    }

    
}
