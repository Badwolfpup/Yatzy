using ControlzEx.Standard;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
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
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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


        private ObservableCollection<Dice>? _dices;
        public ObservableCollection<Dice> Dices
        {
            get => _dices;
            set
            {
                if (_dices != value)
                {
                    _dices = value;
                    OnPropertyChanged(nameof(Dices));
                }
            }
        }

        private string _startbutton = $"pack://application:,,,/Images/play.png";
        public string StartButton
        {
            get => _startbutton;
            set
            {
                if (_startbutton != value)
                {
                    _startbutton = value;
                    OnPropertyChanged(nameof(StartButton));
                }
            }
        }

        private int _selectedindex;
        public int SelectedIndex
        {
            get => _selectedindex;
            set
            {
                if (_selectedindex != value)
                {
                    _selectedindex = value;
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }
        }



        private bool _started;
        public bool Started
        {
            get => _started;
            set
            {
                if (_started != value)
                {
                    _started = value;
                    OnPropertyChanged(nameof(Started));
                }
            }
        }

        public ObservableCollection<string> Val { get; } = new ObservableCollection<string>(
            new[]
            {
            "Ettor", "Tvåor", "Treor", "Fyror", "Femmor", "Sexor", "Ett par", "Två par",
            "Triss", "Fyrtal", "Liten Stege", "Stor Stege", "Kåk", "Chans", "Yatzy"

            });

        private ObservableCollection<string> ValOriginal { get; } = new ObservableCollection<string>(
            new[]
            {
            "Ettor", "Tvåor", "Treor", "Fyror", "Femmor", "Sexor", "Ett par", "Två par",
            "Triss", "Fyrtal", "Liten Stege", "Stor Stege", "Kåk", "Chans", "Yatzy"

            });
        private ObservableCollection<PointsClass>? _points;
        public ObservableCollection<PointsClass>? Points
        {
            get => _points;
            set
            {
                if (_points != value)
                {
                    _points = value;
                    OnPropertyChanged(nameof(Points));
                }
            }
        }

        private bool _outofrolls = true;
        public bool OutofRolls
        {
            get => _outofrolls;
            set
            {
                if (_outofrolls != value)
                {
                    _outofrolls = value;
                    OnPropertyChanged(nameof(OutofRolls));
                }
            }
        }

        private bool _newgame = false;
        private int _numberofrolls = 3;
        private bool _hasaddednonus = false;
        private PointsClass? _total;
        private PointsClass? _summa;
        private PointsClass? _bonus;

        public MainWindow()
        {
            InitializeComponent();
            Dices = new ObservableCollection<Dice>();
            DataContext = this;
            Topscorer = Load();
            AddIndex();
            InitializePoints();
        }


        private void InitializePoints()
        {
            Points = new ObservableCollection<PointsClass>(
            new[]
                {
                    "Ettor", "Tvåor", "Treor", "Fyror", "Femmor", "Sexor", "Summa:", "Bonus", "", "",
                    "Ett par", "Två par", "Triss", "Fyrtal", "Liten Stege", "Stor Stege",
                    "Kåk", "Chans", "Yatzy", "Totalpoäng:"
                }.Select(name => new PointsClass(name, 0))
            );
            _summa = Points.Where(x => x.Name == "Summa:").FirstOrDefault();
            _bonus = Points.Where(x => x.Name == "Bonus").FirstOrDefault();
            _total = Points.Where(x => x.Name == "Totalpoäng:").FirstOrDefault();
        }

        private void Reset()
        {
            Val.Clear();
            foreach (var item in ValOriginal)
            {
                Val.Add(item);
            }
            StartButton = $"pack://application:,,,/Images/playagain.png";
            Started = false;
            _hasaddednonus = false;
            InitializePoints();

        }

        private void RollDices()
        {
            if (_newgame)
            {
                Reset();
                _numberofrolls = 3;
                _newgame = false;
            }
            if (Val.Count == 0 && !_newgame)
            {
                CheckHighscore();
                StartButton = $"pack://application:,,,/Images/playagain.png";
                _newgame = true;
                return;
            }
            Random random = new Random();

            if (!Started)
            {
                SelectedIndex = 0;
                Dices = new ObservableCollection<Dice>();
                LoadImageNumberOfRolls(_numberofrolls);
                Started = true;
                for (int i = 0; i < 5; i++)
                {
                    Dices.Add(new Dice(random.Next(1, 7)));
                }
            }
            else
            {
                _numberofrolls--;
                LoadImageNumberOfRolls(_numberofrolls);

                for (int i = 0; i < 5; i++)
                {
                    if (!Dices[i].Issaved)
                    {
                        Dices[i].UpdateDice(random.Next(1, 7));
                    }
                }
                if (_numberofrolls <= 0) { OutofRolls = false; return; }
            }
        }

        private void LoadImageNumberOfRolls(int rolls)
        {
            StartButton = _numberofrolls != 0 ? $"pack://application:,,,/Images/dice{rolls}.png" : $"pack://application:,,,/Images/redx.png";
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            RollDices();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border? b = sender as Border;
            if (b == null) return;
            if (b.Tag is not Dice d) return;
            if (!d.Issaved)
            {
                b.BorderBrush = Brushes.Crimson;
                d.Issaved = true;
            }
            else
            {
                b.BorderBrush = Brushes.Blue;
                d.Issaved = false;
            }
        }



        private void CheckCombo(string combo)
        {
            if (CheckCombination(combo))
            {
                Started = false;
                CheckBonus();
                OutofRolls = true;
                _numberofrolls = 3;
                RollDices();
            }
        }


        private void Crossout(string combo)
        {
            if (combo == "Ettor" || combo == "Tvåor" || combo == "Treor" || combo == "Fyror" || combo == "Femmor" || combo == "Sexor") return;
            if (!Val.Contains(combo)) return;
            PointsClass? p = Points?.Where(x => x.Name == combo).FirstOrDefault();
            if (p != null)
            {
                p.Overstruken = TextDecorations.Strikethrough;
                p.FontColor = Brushes.Red;
                Val.Remove(combo);
                Started = false;
                OutofRolls = true;
                _numberofrolls = 3;
                RollDices();
            }
        }

        private void CheckBonus()
        {
            if (!_hasaddednonus)
            {
                if (_summa.Point >= 63)
                {
                    _bonus.Point = 50;
                    _total.Point += _bonus.Point;
                    _hasaddednonus = true;

                }
                else
                {
                    int? difference = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        if (Points[i].HasPoints)
                        {
                            difference += Points[i].Point - ((i + 1) * 3);
                            Points[i].HasPoints = true;
                        }
                    }
                    if (Points.Take(6).Any(x => !x.HasPoints))
                    {
                        _bonus.Point = difference;
                        _bonus.FontColor = _bonus.Point < 0 ? Brushes.Red : Brushes.Green;
                    }
                    else
                    {
                        _bonus.Point = 0;
                        _bonus.FontColor = Brushes.Red;
                    }
                }
            }

        }

        private bool CheckCombination(string combo)
        {
            if (IsValidCombo(combo))
            {
                PointsClass? point = Points.Where(x => x.Name == combo).FirstOrDefault();
                PointsClass? total = Points.Where(x => x.Name == "Totalpoäng:").ToList().FirstOrDefault();
                if (point == null || total == null) return false;
                switch (combo)
                {
                    case "Ettor": { SelectNumber(1, 0, combo); return true; }
                    case "Tvåor": { SelectNumber(2, 1, combo); return true; }
                    case "Treor": { SelectNumber(3, 2, combo); return true; }
                    case "Fyror": { SelectNumber(4, 3, combo); return true; }
                    case "Femmor": { SelectNumber(5, 4, combo); return true; }
                    case "Sexor": { SelectNumber(6, 5, combo); return true; }
                    case "Ett par":
                        {
                            point.Point = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).OrderByDescending(group => group.Key).FirstOrDefault().Key * 2;
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Två par":
                        {
                            var templist = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).OrderByDescending(group => group.Key);
                            foreach (var item in templist)
                            {
                                point.Point += item.Key * 2;

                            }
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Triss":
                        {
                            point.Point = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 3).FirstOrDefault().Key * 3;
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Fyrtal":
                        {
                            point.Point = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 4).FirstOrDefault().Key * 4;
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Liten Stege":
                        {
                            point.Point = 15;
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Stor Stege":
                        {
                            point.Point = 20;
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Kåk":
                        {
                            foreach (var item in Dices.GroupBy(x => x.DiceValue).OrderByDescending(x => x.Key))
                            {
                                point.Point += item.Count() == 2 ? item.Key * 2 : item.Key * 3;

                            }
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Chans":
                        {
                            point.Point = Dices.Sum(x => x.DiceValue);
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    case "Yatzy":
                        {
                            point.Point = 50;
                            ChangeScore(point, combo, total);
                            return true;
                        }
                    default: MessageBox.Show("Ogiltigt val"); return false;
                };
            }
            else
            {
                MessageBox.Show("Du kan inte välja den kombinationen. \nDu har antingen inte kombinationen eller så har du redan valt den.");
                return false;
            }

        }

        private void ChangeScore(PointsClass? point, string combo, PointsClass? total)
        {
            if (point == null || total == null) return;
            point.Font = FontWeights.Bold;
            Val.Remove(combo);
            total.Point += point.Point;
        }

        private void SelectNumber(int värde, int pos, string combo)
        {
            Points[pos].Font = FontWeights.Bold;
            Points[pos].Point = Dices.Sum(x => x.DiceValue == värde ? värde : 0);

            _summa.Point += Points[pos].Point;
            _total.Point += Points[pos].Point;
            Points[pos].HasPoints = true;
            Val.Remove(combo);
        }

        private bool IsValidCombo(string combo)
        {
            bool result = combo switch
            {
                "Ettor" => Val.Contains(combo),
                "Tvåor" => Val.Contains(combo),
                "Treor" => Val.Contains(combo),
                "Fyror" => Val.Contains(combo),
                "Femmor" => Val.Contains(combo),
                "Sexor" => Val.Contains(combo),
                "Ett par" => Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).Count() > 0 && Val.Contains(combo),
                "Två par" => Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).Count() > 1 && Val.Contains(combo),
                "Triss" => Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 3).Count() > 0 && Val.Contains(combo),
                "Fyrtal" => Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 4).Count() > 0 && Val.Contains(combo),
                "Liten Stege" => Dices.Select(x => x.DiceValue).OrderBy(value => value).SequenceEqual(new[] { 1, 2, 3, 4, 5 }) && Val.Contains(combo),
                "Stor Stege" => Dices.Select(x => x.DiceValue).OrderBy(value => value).SequenceEqual(new[] { 2, 3, 4, 5, 6 }) && Val.Contains(combo),
                "Kåk" => Dices.GroupBy(x => x.DiceValue).Any(group => group.Count() == 2) && Dices.GroupBy(x => x.DiceValue).Any(group => group.Count() == 3) && Val.Contains(combo),
                "Chans" => Val.Contains(combo),
                "Yatzy" => Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() == 5).Count() > 0 && Val.Contains(combo),
                _ => false
            };
            return result;

        }

        private void CheckHighscore()
        {
            if (_total.Point > Topscorer?.Min(x => x.Score) || Topscorer.Count < 10)
            {
                HighScore? s = Topscorer.FirstOrDefault(x => x.IsLast);
                if (s != null)
                {
                    s.IsLast = false;
                    s.FontColor = Brushes.Black;
                }

            }
            Topscorer = new ObservableCollection<HighScore>(
                Topscorer.Append(new HighScore(_total.Point) { FontColor = Brushes.Green, IsLast = true })
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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock? t = sender as TextBlock;
            if (t == null) return;
            PointsClass? points = t?.DataContext as PointsClass;
            if (points == null) return;
            if (points?.Name != "Summa:" && points?.Name != "Bonus" && points?.Name != "Totalpoäng:")
            {
                CheckCombo(points.Name);
            }
        }

        private void TextBlock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            TextBlock? t = sender as TextBlock;
            PointsClass? points = t?.DataContext as PointsClass;
            if (points?.Name != "Summa:" && points?.Name != "Bonus" && points?.Name != "Totalpoäng:")
            {
                Crossout(points.Name);
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
    }

    public class SignConverter : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var main = values[2] as MainWindow;
            var points = main.Points;


            var name = values[0].ToString();
            var point = values[1] as int?;
            if (name == "Bonus" && point > 0 && points[6].Point < 63) return $"+{point}";
            return point.ToString();

        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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
}