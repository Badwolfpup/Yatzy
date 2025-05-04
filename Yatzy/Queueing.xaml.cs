using System;
using System.Collections.Generic;
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

namespace Yatzy
{
    /// <summary>
    /// Interaction logic for Queueing.xaml
    /// </summary>
    public partial class Queueing : Window
    {
        private readonly YatzyLobby _lobby;

        public Queueing(YatzyLobby lobby)
        {
            InitializeComponent();
            _lobby = lobby;
            while (lobby.InQueue)
            {
                // Wait for the lobby to finish queueing
            }
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _lobby.LeaveQueue();
            Close();
        }
    }
}
