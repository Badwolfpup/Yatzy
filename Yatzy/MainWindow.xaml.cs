using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Printing;
using System.Diagnostics.Metrics;
using System.Windows.Shell;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.IO;
using System.Globalization;
using System.Collections;
using MahApps.Metro.Controls;

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
        public ObservableCollection<HighScore> Topscorer
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
        private FontIcon? _icon;
        public FontIcon Icon
        {
            get => _icon;
            set
            {
                if (value != _icon)
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
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

        private int _numberofrolls = 3;
        private bool _hasaddednonus = false;
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
        public  ObservableCollection<string> Val { get; } = new ObservableCollection<string>(
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
        private bool _newgame = false;

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

        public MainWindow()
        {
            InitializeComponent();
            Dices = new ObservableCollection<Dice>();
            Icon = new FontIcon();
            DataContext = this;
            Topscorer = Load();
            AddIndex();
            SetIcon("\xE777");
            InitializePoints();
        }

        private void SetIcon(string glyph)
        {

            Icon.FontFamily = new FontFamily("Segoe MDL2 Assets");
            Icon.Glyph = glyph;

        }

        private void InitializePoints()
        {
            Points = new ObservableCollection<PointsClass>(
            new[]
                {
                    "Ettor", "Tvåor", "Treor", "Fyror", "Femmor", "Sexor", "Summa:", "Bonus", "", "",
                    "Ett par", "Två par", "Triss", "Fyrtal", "Liten Stege", "Stor Stege",
                    "Kåk", "Chans", "Yatzy", "Totalpoäng:"
                }.Select(name =>  new PointsClass(name, 0))
            );
        }

        private void Reset()
        {
            foreach(var item in ValOriginal )
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
                if (Points?[6].Point >= 63)
                {
                    Points[7].Point = 50;
                    Points[19].Point += Points[7].Point;
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
                        Points[7].Point = difference;
                        Points[7].FontColor = Points[7].Point < 0 ? Brushes.Red : Brushes.Green;
                    }
                    else
                    {
                        Points[7].Point = 0;
                        Points[7].FontColor = Brushes.Red;
                    }
                }
            }
            
        }

        private bool CheckCombination(string combo)
        {
            switch (combo)
            {
                case "Ettor": { SelectNumber(1, 0, combo); return true; }
                case "Tvåor": { SelectNumber(2, 1, combo); return true; }
                case "Treor": { SelectNumber(3, 2, combo); return true; }
                case "Fyror": { SelectNumber(4, 3, combo); return true; }
                case "Femmor": {SelectNumber(5, 4, combo); return true; }
                case "Sexor": {SelectNumber(6, 5, combo); return true; }
                case "Ett par":  return Ettpar();
                case "Två par": return Tvåpar(); 
                case "Triss": return Triss();
                case "Fyrtal": return Fyrtal(); 
                case "Liten Stege": return Lstege(); 
                case "Stor Stege": return Sstege(); ;
                case "Kåk": return Kåk();
                case "Chans": return Chans();
                case "Yatzy": return Yatzy();
                default: MessageBox.Show("Ogiltigt val"); return false;
            }; 

            
        }

        

        private void SelectNumber(int värde, int pos, string combo)
        {
            Points[pos].Font = FontWeights.Bold; 
            Points[pos].Point = Dices.Sum(x => x.DiceValue == värde ? värde : 0); 
           
            Points[6].Point += Points[pos].Point; 
            Points[19].Point += Points[pos].Point;
            Points[pos].HasPoints = true;
            Val.Remove(combo);
        }

        private bool Ettpar()
        {
            if (Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).Count() > 0)
            {
                var templist = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).OrderByDescending(group => group.Key).FirstOrDefault();
                Points[10].Point = templist.Key * 2;
                Points[10].Font = FontWeights.Bold;
                Val.Remove("Ett par");
                Points[19].Point += Points[10].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Tvåpar()
        {
            if (Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).Count() > 1)
            {
                var templist = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).OrderByDescending(group => group.Key);
                foreach (var item in templist)
                {
                    Points[11].Point += item.Key * 2;

                }
                Val.Remove("Två par");
                Points[11].Font = FontWeights.Bold;
                Points[19].Point += Points[11].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Triss()
        {
            if (Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 3).Count() > 0)
            {
                var templist = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 3).FirstOrDefault();
                Points[12].Point = templist.Key * 3;
                Val.Remove("Triss");
                Points[12].Font = FontWeights.Bold;
                Points[19].Point += Points[12].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Fyrtal()
        {
            if (Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 4).Count() > 0)
            {
                var templist = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 4).FirstOrDefault();
                Points[13].Point = templist.Key * 4;
                Val.Remove("Fyrtal");
                Points[13].Font = FontWeights.Bold;
                Points[19].Point += Points[13].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Lstege()
        {
            
            if (Dices.Select(x => x.DiceValue).OrderBy(value => value).SequenceEqual(new[] { 1, 2, 3, 4, 5 }))
            {
                Points[14].Point = 15;
                Val.Remove("Liten Stege");
                Points[14].Font = FontWeights.Bold;
                Points[19].Point += Points[14].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Sstege()
        {
            if (Dices.Select(x => x.DiceValue).OrderBy(value => value).SequenceEqual(new[] { 2, 3, 4, 5, 6 }))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Points[15].Point = 20;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                Val.Remove("Stor Stege");
                Points[15].Font = FontWeights.Bold;
                Points[19].Point += Points[15].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;

        }

        private bool Kåk()
        {
            var groups = Dices.GroupBy(x => x.DiceValue);
            if (groups.Any(group => group.Count() == 2) && groups.Any(group => group.Count() == 3))
            {
                foreach (var item in groups.OrderByDescending(x => x.Key))
                {
                    Points[16].Point += item.Count() == 2 ? item.Key * 2 : item.Key * 3;

                }
                Val.Remove("Kåk");
                Points[16].Font = FontWeights.Bold;
                Points[19].Point += Points[16].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Chans()
        {
            Points[17].Point = Dices.Sum(x => x.DiceValue);
            Points[17].Font = FontWeights.Bold;
            Val.Remove("Chans");
            Points[19].Point += Points[17].Point;
            return true;
        }

        private bool Yatzy()
        {
            if (Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() == 5).Count() > 0)
            {
                Points[18].Point = 50;
                Val.Remove("Yatzy");
                Points[18].Font = FontWeights.Bold;
                Points[19].Point += Points[18].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock? t = sender as TextBlock;
            if (t == null) return;
            PointsClass points = t?.DataContext as PointsClass;
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

        private ObservableCollection<HighScore> Load()
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + "Topplista.json";
            if (!File.Exists(filename)) return new ObservableCollection<HighScore>(); 
            
            string file = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(file)) return new ObservableCollection<HighScore>();
            return JsonConvert.DeserializeObject<ObservableCollection<HighScore>>(file);
        }

        private void CheckHighscore()
        {
            if (Points[17].Point > Topscorer.Min(x => x.Score))
            {
                HighScore s = Topscorer.SingleOrDefault(x => x.IsLast);
                s.IsLast = false;
            }
            Topscorer = new ObservableCollection<HighScore>(
                Topscorer.Append(new HighScore(Points[19].Point) { IsLast = true })
                .OrderByDescending(x => x.Score).Take(10)
            );
            //Topscorer.Add(new HighScore(Points[19].Point) { IsLast = true });
            //Topscorer = new ObservableCollection<HighScore>(Topscorer.OrderByDescending(x => x.Score));
            //Topscorer = new ObservableCollection<HighScore>(Topscorer.Take(10));
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