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

        private ObservableCollection<HighScore> _topscorer;
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

        private ObservableCollection<Dice> _dices;
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

        private string _startbutton = "Start game";
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

        private string? _rollstext;
        public string? Rollstext
        {
            get => _rollstext;
            set
            {
                if (_rollstext != value)
                {
                    _rollstext = value;
                    OnPropertyChanged(nameof(Rollstext));
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
        "Ettor", "Tvåor", "Treor", "Fyror", "Femmor", "Sexor", "Summa:", "Bonus",
        "Ett par", "Två par", "Triss", "Fyrtal", "Liten Stege", "Stor Stege",
        "Kåk", "Chans", "Yatzy", "Totalpoäng:"
    }.Select(name => new PointsClass(name, 0))
);
            //Points =  new ObservableCollection<PointsClass>
            //{
            //    new PointsClass("Ettor", 0),
            //    new PointsClass ("Tvåor", 0),
            //    new PointsClass ("Treor", 0),
            //    new PointsClass ("Fyror", 0),
            //    new PointsClass ("Femmor", 0),
            //    new PointsClass ("Sexor", 0),
            //    new PointsClass ("Summa:", 0),
            //    new PointsClass ("Bonus", 0),
            //    new PointsClass ("Ett par", 0),
            //    new PointsClass ("Två par", 0),
            //    new PointsClass ("Triss", 0),
            //    new PointsClass ("Fyrtal", 0),
            //    new PointsClass ("Liten Stege", 0),
            //    new PointsClass ("Stor Stege", 0),
            //    new PointsClass ("Kåk", 0),
            //    new PointsClass ("Chans", 0),
            //    new PointsClass ("Yatzy", 0),
            //    new PointsClass ("Totalpoäng:", 0),
            //};
        }

        private void Reset()
        {
            foreach(var item in ValOriginal )
            {
                Val.Add(item);
            }
            StartButton = "Start  game";
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
                StartButton = "Start new game";
                _newgame = true;
                return;
            }
            Random random = new Random();

            if (!Started)
            {
                SelectedIndex = 0;
                Dices = new ObservableCollection<Dice>();
                Rollstext = _numberofrolls == 1 ? $"{_numberofrolls} roll remaining." : $"{_numberofrolls} rolls remaining.";
                StartButton = "Roll";
                Started = true;
                for (int i = 0; i < 5; i++)
                {
                    Dices.Add(new Dice(random.Next(1, 7)));
                }
            }
            else
            {
                _numberofrolls--;
                Rollstext = _numberofrolls == 1 ? $"{_numberofrolls} roll remaining." : $"{_numberofrolls} rolls remaining.";

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
                    Points[17].Point += Points[7].Point;
                    _hasaddednonus = true;

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
            Points[17].Point += Points[pos].Point;
            Val.Remove(combo);
        }

        private bool Ettpar()
        {
            if (Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).Count() > 0)
            {
                var templist = Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() >= 2).OrderByDescending(group => group.Key).FirstOrDefault();
                Points[8].Point = templist.Key * 2;
                Points[8].Font = FontWeights.Bold;
                Val.Remove("Ett par");
                Points[17].Point += Points[8].Point;
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
                    Points[9].Point += item.Key * 2;

                }
                Val.Remove("Två par");
                Points[9].Font = FontWeights.Bold;
                Points[17].Point += Points[9].Point;
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
                Points[10].Point = templist.Key * 3;
                Val.Remove("Triss");
                Points[10].Font = FontWeights.Bold;
                Points[17].Point += Points[10].Point;
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
                Points[11].Point = templist.Key * 4;
                Val.Remove("Fyrtal");
                Points[11].Font = FontWeights.Bold;
                Points[17].Point += Points[11].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Lstege()
        {
            
            if (Dices.Select(x => x.DiceValue).OrderBy(value => value).SequenceEqual(new[] { 1, 2, 3, 4, 5 }))
            {
                Points[12].Point = 15;
                Val.Remove("Liten Stege");
                Points[12].Font = FontWeights.Bold;
                Points[17].Point += Points[12].Point;
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
                Points[13].Point = 20;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                Val.Remove("Stor Stege");
                Points[13].Font = FontWeights.Bold;
                Points[17].Point += Points[13].Point;
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
                    Points[14].Point += item.Count() == 2 ? item.Key * 2 : item.Key * 3;

                }
                Val.Remove("Kåk");
                Points[14].Font = FontWeights.Bold;
                Points[17].Point += Points[14].Point;
                return true;
            }
            MessageBox.Show("Du har inte något ettpar");
            return false;
        }

        private bool Chans()
        {
            Points[15].Point = Dices.Sum(x => x.DiceValue);
            Points[15].Font = FontWeights.Bold;
            Val.Remove("Chans");
            Points[17].Point += Points[15].Point;
            return true;
        }

        private bool Yatzy()
        {
            if (Dices.GroupBy(x => x.DiceValue).Where(group => group.Count() == 5).Count() > 0)
            {
                Points[16].Point = 50;
                Val.Remove("Yatzy");
                Points[16].Font = FontWeights.Bold;
                Points[17].Point += Points[16].Point;
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
            Topscorer.Add(new HighScore(Points[17].Point));
            Topscorer = new ObservableCollection<HighScore>(Topscorer.OrderByDescending(x => x.Score));
            if (Topscorer?.Count > 10) Topscorer = new ObservableCollection<HighScore>(Topscorer.Take(10));
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

   
}