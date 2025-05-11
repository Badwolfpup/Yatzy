using ControlzEx.Standard;
using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
            Title = lobby._username;
            Players = players;
            if (Players.Count > 0)
            {
                _myplayer = Players.IndexOf(Players.FirstOrDefault(x => x.UserName == lobby._username));
                _opponent = _myplayer == 0 ? 1 : 0;
            }
            _activeplayer = Players.Count > 0 ? Players[0] : new Player();
            _activeplayer.InitializePoints();
            _activeplayer.MyTurn = true;
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
        private int _myplayer;
        private int _opponent;

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
                _activeplayer._numberofrolls = 4;
                _newgame = false;
            }
            if (_activeplayer.Points.All(x => x.HasPoints) && !_newgame)
            {
                //CheckHighscore();
                _activeplayer.StartButton = $"pack://application:,,,/Images/playagain.png";
                _newgame = true;
                return;
            }
            Random random = new Random();

            //if (!_activeplayer.Started)
            //{   
            //    _activeplayer.ResetBackground();

            //    _activeplayer.Dices = new ObservableCollection<Dice>();
            //    LoadImageNumberOfRolls(_activeplayer._numberofrolls);
            //    _activeplayer.Started = true;
            //    for (int i = 0; i < 5; i++)
            //    {
            //        //_activeplayer.RolledDices[i] = random.Next(1, 7);
            //        _activeplayer.Dices.Add(new Dice(random.Next(1,7)));
            //    }
                
            //    CheckCombo();
            //}
            //else
            //{
            
            
            if (!_activeplayer.Started)
            {
                _activeplayer.Started = true;
                if (_activeplayer.Dices != null)
                {
                    foreach (var item in _activeplayer.Dices)
                    {
                        item.Issaved = false;
                        if (!SinglePlayerGame) _lobby.UpdateDiceBorder((_activeplayer.Dices.IndexOf(item), false));
                    }
                }
            }
            
            if (_activeplayer.Dices == null) _activeplayer.InititalizeDices();

            for (int i = 0; i < 5; i++)
            {
                //if (!_activeplayer.Dices[i].Issaved)
                if (!_activeplayer.Dices[i].Issaved)
                {
                    //_activeplayer.RolledDices[i] = random.Next(1, 7);
                    _activeplayer.Dices[i].UpdateDice(random.Next(1, 7));
                }
            }
            if (SinglePlayerGame) UpdateGameInfo();
            else _lobby.UpdateDicevalue(_activeplayer.Dices);

            if (_activeplayer._numberofrolls <= 0) { _activeplayer.OutofRolls = false; return; }
            
        }

        private void UpdateGameInfo()
        {
            _activeplayer._numberofrolls--;
            LoadImageNumberOfRolls(_activeplayer._numberofrolls);
            _activeplayer.ResetBackground();
            CheckCombo();
        }

        public void UpdateDiceValue(string dice)
        {
            var updateddice = JsonConvert.DeserializeObject<List<int>>(dice);
            if (_activeplayer.Dices == null) _activeplayer.InititalizeDices();
            for (int i = 0; i < updateddice.Count; i++)
            {
                _activeplayer.Dices[i].UpdateDice(updateddice[i]);
            }
            UpdateGameInfo();
        }

        public void UpdateDiceBorder(string dice)
        {
            var updateddice = JsonConvert.DeserializeObject<(int index, bool border)>(dice);
            _activeplayer.Dices[updateddice.index].Issaved = updateddice.border;

        }

        public void UpdateTurn()
        {
            if (SinglePlayerGame) return;
            _activeplayer.StartButton = $"pack://application:,,,/Images/notyourturn.png";
            NewRound();
        }



        public void UpdatePoints(string points)
        {
            var updatedpoints = JsonConvert.DeserializeObject<ObservableCollection<PointsClass>>(points);

            for (int i = 0; i < Math.Min(updatedpoints.Count, _activeplayer.Points.Count); i++)
            {
                _activeplayer.Points[i].Point = updatedpoints[i].Point;
                _activeplayer.Points[i].PlusMinus = updatedpoints[i].PlusMinus;
                _activeplayer.Points[i].Font = updatedpoints[i].Font;
                _activeplayer.Points[i].FontColor = updatedpoints[i].FontColor;
                _activeplayer.Points[i].HasPoints = updatedpoints[i].HasPoints;
                _activeplayer.Points[i].LeftButtonEnabled = updatedpoints[i].LeftButtonEnabled;
                _activeplayer.Points[i].RightButtonEnabled = updatedpoints[i].RightButtonEnabled;
                _activeplayer.Points[i].CanSelect = updatedpoints[i].CanSelect;
                _activeplayer.Points[i].ShowButton = updatedpoints[i].ShowButton;
                _activeplayer.Points[i].IsBonus = updatedpoints[i].IsBonus;
                _activeplayer.Points[i].BakGrund = updatedpoints[i].BakGrund;
            }
        }

        private void LoadImageNumberOfRolls(int rolls)
        {
            _activeplayer.StartButton = _activeplayer._numberofrolls != 0 ? $"pack://application:,,,/Images/dice{rolls}.png" : $"pack://application:,,,/Images/redx.png";
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (_activeplayer._numberofrolls <= 0) return;
            if (Players.IndexOf(_activeplayer) != _myplayer) return;
            if (sender is Button button && button.DataContext is Player player && !player.Equals(_activeplayer)) return;
            RollDices();
        }

        private void NewRound()
        {
            foreach (var item in _activeplayer.Dices)
            {
                item.Issaved = false;
            }

            _activeplayer.MyTurn = false;
            if (!SinglePlayerGame) _activeplayer = Players.IndexOf(_activeplayer) == 0 ? Players[1] : Players[0];
            _activeplayer.MyTurn = true;
            _activeplayer.Started = false;
            _activeplayer.OutofRolls = true;
            _activeplayer._numberofrolls = 4;
            if (Players.IndexOf(_activeplayer) == _myplayer) RollDices();
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Players.IndexOf(_activeplayer) != _myplayer) return;
            if (sender is Border b && b.DataContext is Dice dice && _activeplayer.Equals(Players.FirstOrDefault(x => x.Dices.Contains(dice))))
            {
                if (b.Tag is not Dice d) return;
                d.Issaved = !d.Issaved;
                if (!SinglePlayerGame) _lobby.UpdateDiceBorder((_activeplayer.Dices.IndexOf(d), d.Issaved));
                //var index = _activeplayer.Dices.IndexOf(d);
                //if (index < 0) return;
                ////_activeplayer.IsDiceSaved[index] = !_activeplayer.IsDiceSaved[index];
            }

            //Border? b = sender as Border;
            //if (b == null) return;
            //var dice = b.DataContext as Dice;
            //if (dice == null) return;
            
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
            _activeplayer._bonus.CalculatePlusMinus((int)_activeplayer.Points.Take(6).ToList().Sum(x =>
            {
                if (string.IsNullOrEmpty(x.PlusMinus)) return 0;
                else return int.Parse(x.PlusMinus);
            }));
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
            if  (!SinglePlayerGame && Players.Count > 1 && !Players.All(x => x.Points.All(y => y.HasPoints)))
            {
                MessageBoxResult result = MessageBox.Show("Vill du verkligen avbryta spelat och gå tillbaka till lobbyn?", "Avsluta", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }
            if (!SinglePlayerGame && Players.Count > 1) Players.RemoveAt(_myplayer);
            _closedapp = false;
            Close();
            _closedapp = true;
            _lobby.Show();
        }

        private void AddScore_Click(object sender, RoutedEventArgs e)
        {
            if (!SinglePlayerGame && Players.IndexOf(_activeplayer) != _myplayer) return;
            //if (sender is Button b && b.DataContext is Player player && !player.Equals(_activeplayer)) return;
            if (sender is Button b && b.DataContext is PointsClass points)
            {
                points.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9bf29f"));
                points.FontColor = Brushes.Gray;
                points.LeftButtonEnabled = false;
                points.RightButtonEnabled = false;
                points.HasPoints = true;
                AddPoints(points);
                CheckBonus();
                if (!SinglePlayerGame)
                {
                    _lobby.UpdatePoints(_activeplayer.Points);
                    _lobby.UpdateTurn();
                }
                else NewRound();
            }
        }

        private void StrikeScore_Click(object sender, RoutedEventArgs e)
        {
            if (!SinglePlayerGame && Players.IndexOf(_activeplayer) != _myplayer) return;
            if (sender is Button b && b.DataContext is PointsClass points)
            {
                if (_activeplayer.Dices.Count == 0) return;
                points.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7927c")); 
                points.FontColor = Brushes.Gray;
                points.LeftButtonEnabled = false;
                points.RightButtonEnabled = false;
                points.HasPoints = true;
                points.Point = 0;
                if (!SinglePlayerGame)
                {
                    _lobby.UpdatePoints(_activeplayer.Points);
                    _lobby.UpdateTurn();
                }
                else NewRound();
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

    public class HideBonusButton : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)values[0];
            var point = values[1] as PointsClass;
            if (point.Name == "Totalpoäng:")
            {
                return Visibility.Collapsed;
            }

            if (point.IsBonus) return boolValue ? Visibility.Collapsed : Visibility.Visible;
            else return boolValue ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HideButton : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is bool isbonus)
            {
                if (isbonus) return boolValue ? Visibility.Visible : Visibility.Collapsed;
                else return boolValue ? Visibility.Visible : Visibility.Hidden;

            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CalculatePlusMinus: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var point = values[0] as int?;
            var pointclass = values[1] as PointsClass;
            if (point == null || pointclass == null || point == 0) return "";
            switch (pointclass.Name)
            {
                case "Ettor": return $"{(point - 3 != 0 ? point - 3 > 0 ? "+" : "-" : "")}{(point - 3 != 0 ? point - 3 > 0 ? point - 3 : 3 - point : 0)}";
                case "Tvåor": return $"{(point - 6 != 0 ? point - 6 > 0 ? "+" : "-" : "")}{(point - 6 != 0 ? point - 6 > 0 ? point - 6 : 6 - point : 0)}";
                case "Treor": return $"{(point - 9 != 0 ? point - 9 > 0 ? "+" : "-" : "")}{(point - 9 != 0 ? point - 9 > 0 ? point - 9 : 9 - point : 0)}";
                case "Fyror": return $"{(point - 12 != 0 ? point - 12 > 0 ? "+" : "-" : "")}{(point - 12 != 0 ? point - 12 > 0 ? point - 12 : 12 - point : 0)}";
                case "Femmor": return $"{(point - 15 != 0 ? point - 15 > 0 ? "+" : "-" : "")}{(point - 15 != 0 ? point - 15 > 0 ? point - 15 : 15 - point : 0)}";
                case "Sexor": return $"{(point - 18 != 0 ? point - 18 > 0 ? "+" : "-" : "")}{(point - 18 != 0 ? point - 18 > 0 ? point - 18 : 18 - point : 0)}";
                case "Bonus": return $"{(point - 63 != 0 ? point - 63 > 0 ? "+" : "-" : "")}{(point - 63 != 0 ? point - 63 > 0 ? point - 63 : 63 - point : 0)}";
            }
            return "";

        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowBonusText: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)values[0];
            var point = values[1] as PointsClass;
            if (point.Name == "Totalpoäng:")
            {
                return Visibility.Collapsed;
            }
            else if (point.Name == "Summa:") return boolValue ? Visibility.Collapsed : Visibility.Visible;
            else
            if (point.IsBonus) return boolValue ? Visibility.Visible : Visibility.Collapsed;
            else return boolValue ? Visibility.Hidden : Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}