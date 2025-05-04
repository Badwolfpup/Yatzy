using ControlzEx.Standard;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Yatzy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {

        private bool _closedapp = true;

        public MainWindow(YatzyLobby lobby, ObservableCollection<Player> players)
        {
            InitializeComponent();
            _lobby = lobby;

            Players = players;
            _activeplayer = Players.Count > 0 ? Players[0] : new Player();
            _activeplayer.InitializePoints();
            DataContext = this;
            Topscorer = Load();
            AddIndex();
            Closing += CloseGame;
        }

        private void CloseGame(object? sender, CancelEventArgs e)
        {
            if (!_closedapp) return;
            foreach (Window window in Application.Current.Windows)
            {
                if (window != this) window.Close();
            }
        }      




        #region Properties
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private readonly YatzyLobby _lobby;

        private Player _activeplayer;

        private ObservableCollection<HighScore>? _topscorer;
        public ObservableCollection<HighScore>? Topscorer
        {
            get => _topscorer;
            set
            {
                if (value != _topscorer)
                {
                    _topscorer = value;
                    OnPropertyChanged(nameof(Topscorer));
                }
            }
        }

        private ObservableCollection<Player> _players = new ObservableCollection<Player>();
        public ObservableCollection<Player> Players
        {
            get => _players;
            set
            {
                if (_players != value)
                {
                    _players = value;
                    OnPropertyChanged(nameof(Players));
                }
            }
        }

        private bool _newgame = false;
        public bool SinglePlayerGame { get; set; } = false;
        #endregion


        private void RollDices()
        {
            if (_newgame)
            {
                _activeplayer.Reset();
                _activeplayer._numberofrolls = 3;
                _newgame = false;
            }
            if (_activeplayer.Points.All(x => x.HasPoints) && !_newgame)
            {
                CheckHighscore();
                _activeplayer.StartButton = $"pack://application:,,,/Images/playagain.png";
                _newgame = true;
                return;
            }
            Random random = new Random();

            if (!_activeplayer.Started)
            {
                _activeplayer.ResetBackground();

                _activeplayer.Dices = new ObservableCollection<Dice>();
                LoadImageNumberOfRolls(_activeplayer._numberofrolls);
                _activeplayer.Started = true;
                for (int i = 0; i < 5; i++)
                {
                    //_activeplayer.RolledDices[i] = random.Next(1, 7);
                    _activeplayer.Dices.Add(new Dice(random.Next(1,7)));
                }
                _lobby.UpdateDice(_activeplayer.RolledDices, _activeplayer.IsDiceSaved);
                CheckCombo();
            }
            else
            {
                _activeplayer.ResetBackground();
                _activeplayer._numberofrolls--;
                LoadImageNumberOfRolls(_activeplayer._numberofrolls);

                for (int i = 0; i < 5; i++)
                {
                    //if(_activeplayer.IsDiceSaved[i])
                    if (!_activeplayer.Dices[i].Issaved)
                    {
                        //_activeplayer.RolledDices[i] = random.Next(1, 7);
                        _activeplayer.Dices[i].UpdateDice(random.Next(1, 7), true);
                    }
                }
                //_lobby.UpdateDice(_activeplayer.RolledDices, _activeplayer.IsDiceSaved);
                CheckCombo();
                if (_activeplayer._numberofrolls <= 0) { _activeplayer.OutofRolls = false; return; }
            }
        }

        public void UnPackDices(int[] nums, bool[] saves)
        {
            //for (int i = 0; i < nums.Length; i++)
            //{
            //    _activeplayer.Dices.Add(new Dice());
            //    _activeplayer.Dices[i].UpdateDice(nums[i], saves[i]);
            //}
        }

        private void LoadImageNumberOfRolls(int rolls)
        {
            _activeplayer.StartButton = _activeplayer._numberofrolls != 0 ? $"pack://application:,,,/Images/dice{rolls}.png" : $"pack://application:,,,/Images/redx.png";
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _activeplayer.ResetBackground();
            RollDices();
        }

        private void NewRound()
        {
            _activeplayer.MyTurn = false;
            if (!SinglePlayerGame) _activeplayer = Players.IndexOf(_activeplayer) == 0 ? Players[1] : Players[0];
            _activeplayer.MyTurn = true;
            _activeplayer.Started = false;
            _activeplayer.OutofRolls = true;
            _activeplayer._numberofrolls = 3;
            RollDices();
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border? b = sender as Border;
            if (b == null) return;
            if (b.Tag is not Dice d) return;
            int index = _activeplayer.Dices.IndexOf(d);
            if (!d.Issaved)
            {
                b.BorderBrush = Brushes.Crimson;
                d.Issaved = true;
                _activeplayer.IsDiceSaved[index] = true;
            }
            else
            {
                b.BorderBrush = Brushes.Blue;
                d.Issaved = false;
                _activeplayer.IsDiceSaved[index] = false;
            }
        }



        private void CheckCombo()
        {
            foreach (var item in _activeplayer.Points)
            {
                if (item == _activeplayer._bonus || item == _activeplayer._summa || item == _activeplayer._bonus) continue;
                if (IsValidCombo(item.Name) && !item.HasPoints)
                {
                    item.LeftButtonEnabled = true;
                    item.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bbbf2"));
                }
            }
            CheckBonus();
        }


        private void CheckBonus()
        {
            if (!_activeplayer._hasaddedbonus)
            {
                if (_activeplayer._summa.Point >= 63)
                {
                    _activeplayer._bonus.Point = 50;
                    _activeplayer._total.Point += _activeplayer._bonus.Point;
                    _activeplayer._hasaddedbonus = true;
                    _activeplayer._summa.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bf29f"));
                    _activeplayer._bonus.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bf29f"));
                }
            }
        }

        private bool AddPoints(PointsClass point)
        {
            switch (point.Name)
            {
                case "Ettor": { SelectNumber(1, point); return true; }
                case "Tvåor": { SelectNumber(2, point); return true; }
                case "Treor": { SelectNumber(3, point); return true; }
                case "Fyror": { SelectNumber(4, point); return true; }
                case "Femmor": { SelectNumber(5, point); return true; }
                case "Sexor": { SelectNumber(6, point); return true; }
                case "Ett par":
                    {
                        point.Point = _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).OrderByDescending(group => group.Key).FirstOrDefault().Key * 2;
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Två par":
                    {
                        var templist = _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).OrderByDescending(group => group.Key);
                        foreach (var item in templist)
                        {
                            point.Point += item.Key * 2;

                        }
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Triss":
                    {
                        point.Point = _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 3).FirstOrDefault().Key * 3;
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Fyrtal":
                    {
                        point.Point = _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 4).FirstOrDefault().Key * 4;
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Liten Stege":
                    {
                        point.Point = 15;
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Stor Stege":
                    {
                        point.Point = 20;
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Kåk":
                    {
                        foreach (var item in _activeplayer.Dices.GroupBy(x => x.DiceValue).OrderByDescending(x => x.Key))
                        {
                            point.Point += item.Count() == 2 ? item.Key * 2 : item.Key * 3;

                        }
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Chans":
                    {
                        point.Point = _activeplayer.Dices.Sum(x => x.DiceValue);
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                case "Yatzy":
                    {
                        point.Point = 50;
                        _activeplayer._total.Point += point.Point;
                        return true;
                    }
                default: MessageBox.Show("Ogiltigt val"); return false;
            };
        }

        private void SelectNumber(int värde, PointsClass point)
        {
            point.Point = _activeplayer.Dices.Sum(x => x.DiceValue == värde ? värde : 0);
            _activeplayer._summa.Point += point.Point;
            _activeplayer._total.Point += point.Point;
            point.HasPoints = true;
        }

        private bool IsValidCombo(string combo)
        {
            bool result = combo switch
            {
                "Ettor" => true,
                "Tvåor" => true,
                "Treor" => true,
                "Fyror" => true,
                "Femmor" => true,
                "Sexor" => true,
                "Ett par" => _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).Count() > 0,
                "Två par" => _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).Count() > 1,
                "Triss" => _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 3).Count() > 0,
                "Fyrtal" => _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 4).Count() > 0,
                "Liten Stege" => _activeplayer.Dices.Select(x => x.DiceValue).OrderBy(value => value).SequenceEqual(new[] { 1, 2, 3, 4, 5 }),
                "Stor Stege" => _activeplayer.Dices.Select(x => x.DiceValue).OrderBy(value => value).SequenceEqual(new[] { 2, 3, 4, 5, 6 }),
                "Kåk" => _activeplayer.Dices.GroupBy(x => x.DiceValue).Any(group => group.Count() == 2) && _activeplayer.Dices.GroupBy(x => x.DiceValue).Any(group => group.Count() == 3),
                "Chans" => true,
                "Yatzy" => _activeplayer.Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() == 5).Count() > 0,
                _ => false
            };
            return result;

        }

        private void CheckHighscore()
        {
            if (_activeplayer._total.Point > Topscorer?.Min(x => x.Score) || Topscorer.Count < 10)
            {
                HighScore? s = Topscorer.FirstOrDefault(x => x.IsLast);
                if (s != null)
                {
                    s.IsLast = false;
                    s.FontColor = Brushes.Black;
                }

            }
            Topscorer = new ObservableCollection<HighScore>(
                Topscorer.Append(new HighScore(_activeplayer._total.Point) { FontColor = Brushes.Green, IsLast = true })
                .OrderByDescending(x => x.Score).Take(10)
            );
            AddIndex();
            Save();

        }

        private void AddIndex()
        {
            for (int i = 0; i < Topscorer?.Count; i++)
            {
                Topscorer[i].Index = i + 1;
            }
        }


        private void Save()
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + "Topplista.json";
            string file = JsonConvert.SerializeObject(Topscorer);
            File.WriteAllText(filename, file);
        }

        private ObservableCollection<HighScore>? Load()
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + "Topplista.json";
            if (!File.Exists(filename)) return new ObservableCollection<HighScore>();

            string file = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(file)) return new ObservableCollection<HighScore>();
            return JsonConvert.DeserializeObject<ObservableCollection<HighScore>>(file);
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        { 
            MessageBoxResult result = MessageBox.Show("Vill du verkligen radera highscore?", "Radera highscore", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Topscorer = new ObservableCollection<HighScore>();
                Save();
            }
        }

        private void TextBlock_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock text)
            {
                if (text.DataContext is HighScore highScore)
                {
                    Topscorer.Remove(highScore);
                    AddIndex();
                    Save();
                }
            }
        }

        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                _newgame = true;
                RollDices();
            }
        }

        private void Return_Lobby_Click(object sender, RoutedEventArgs e)
        {
            _closedapp = false;
            Close();
            _closedapp = true;
            _lobby.Show();
        }

        private void AddScore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is PointsClass points)
            {
                points.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bf29f"));
                points.FontColor = Brushes.Gray;
                points.LeftButtonEnabled = false;
                points.RightButtonEnabled = false;
                points.HasPoints = true;
                AddPoints(points);
                NewRound();
            }
        }

        private void StrikeScore_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is PointsClass points)
            {
                if (_activeplayer.Dices.Count == 0) return;
                points.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7927c")); 
                points.FontColor = Brushes.Gray;
                points.LeftButtonEnabled = false;
                points.RightButtonEnabled = false;
                points.HasPoints = true;
                points.Point = 0;
                NewRound();
            }
        }
    }

    //public class SignConverter : IMultiValueConverter
    //{


    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var main = values[2] as MainWindow;
    //        //var points = main._Points;


    //        var name = values[0].ToString();
    //        var point = values[1] as int?;
    //        //if (name == "Bonus" && point > 0 && points[6].Point < 63) return $"+{point}";
    //        return point.ToString();

    //    }


    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class AddSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var num = value as int?;
            if (num == null) return "";
            if (num < 10) return $"{num}.   ";
            return $"{num}. ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HideButton : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                
                return boolValue ? Visibility.Visible : Visibility.Hidden;
                    
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}