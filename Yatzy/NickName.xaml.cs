using System;
using System.Collections.Generic;
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

namespace Yatzy
{
    /// <summary>
    /// Interaction logic for NickName.xaml
    /// </summary>
    public partial class NickName : Window, INotifyPropertyChanged
    {
        private string _nickName = "";
        public string NickNameText
        {
            get { return _nickName; }
            set
            {
                _nickName = value;
                OnPropertyChanged(nameof(NickNameText));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    
        public NickName()
        {
            InitializeComponent();
            DataContext = this;
            enternickname.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NickNameText))
            {
                MessageBox.Show("Please enter a nickname.");
            }
            else
            {
                // Save the nickname and close the window
                this.DialogResult = true;
                this.Close();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(sender, e);
            }
        }
    }
}
