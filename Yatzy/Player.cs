using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Yatzy
{
    public enum Status
    {
        Waiting,
        Playing,
        AFK
    }

    public class Player: INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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

        //public int[] RolledDices { get; set; } = new int[5];
        //public bool[] IsDiceSaved { get; set; } = new bool[5];

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

        private bool _myturn;
        public bool MyTurn
        {
            get => _myturn;
            set
            {
                if (_myturn != value)
                {
                    _myturn = value;
                    BorderBrush = _myturn ? Brushes.Red : Brushes.Black;
                    OnPropertyChanged(nameof(MyTurn));
                }
            }
        }

        private Brush _borderbrush = Brushes.Black;
        public Brush BorderBrush
        {
            get => _borderbrush;
            set
            {
                if (_borderbrush != value)
                {
                    _borderbrush = value;
                    OnPropertyChanged(nameof(BorderBrush));
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

        public int _numberofrolls { get; set; } = 4;
        public bool _hasaddedbonus { get; set; } = false;
        private string _username;
        public PointsClass? _total {get; set;}
        public PointsClass? _summa { get; set; }
        public PointsClass? _bonus { get; set; }

        public Player()
        {
            InitializePoints();
        }

        public void InititalizeDices()
        {
            Dices = new ObservableCollection<Dice>()
            {
                new Dice(1),
                new Dice(2),
                new Dice(3),
                new Dice(4),
                new Dice(5)
            };
        }

        public void InitializePoints()
        {
            Points = new ObservableCollection<PointsClass>(
            new[]
                {
                    "Ettor", "Tvåor", "Treor", "Fyror", "Femmor", "Sexor", "Summa:", "Bonus",
                    "Ett par", "Två par", "Triss", "Fyrtal", "Liten Stege", "Stor Stege",
                    "Kåk", "Chans", "Yatzy", "Totalpoäng:"
                }.Select(name => new PointsClass(name, 0))
            );
            for (int i = 0; i < 6; i++)
            {
                Points[i].IsBonus = true;
            }
            var chans = Points.Where(x => x.Name == "Chans").FirstOrDefault();
            if (chans != null || chans != default)
            {
                chans.ShowButton = true;
                chans.IsBonus = true;
            }

            _summa = Points.Where(x => x.Name == "Summa:").FirstOrDefault();
            _summa = Points.Where(x => x.Name == "Summa:").FirstOrDefault();
            _summa.ShowButton = false;
            _summa.HasPoints = true;
            _summa.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0aaed"));

            _bonus = Points.Where(x => x.Name == "Bonus").FirstOrDefault();
            _bonus = Points.Where(x => x.Name == "Bonus").FirstOrDefault();
            _bonus.ShowButton = true;
            _bonus.HasPoints = true;
            _bonus.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f7927c"));
            _bonus.IsBonus = true;

            _total = Points.Where(x => x.Name == "Totalpoäng:").FirstOrDefault();
            _total = Points.Where(x => x.Name == "Totalpoäng:").FirstOrDefault();
            _total.ShowButton = true;
            _total.IsBonus = true;
            _total.HasPoints = true;
            _total.BakGrund = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0aaed"));
        }

        public void ResetBackground()
        {
            foreach (var item in Points)
            {
                if (item.HasPoints) continue;
                item.BakGrund = new SolidColorBrush(Colors.AntiqueWhite);
                item.FontColor = Brushes.Black;
                item.Font = FontWeights.Bold;
                item.LeftButtonEnabled = false;
                item.RightButtonEnabled = true;
            }
        }

        public void Reset()
        {
            StartButton = $"pack://application:,,,/Images/playagain.png";
            Started = false;
            _hasaddedbonus = false;
            InitializePoints();
            //ResetHasPoints();
        }


        //[JsonProperty("username")]
        public string UserName
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public Status Status { get; set; } = Status.Waiting;
    }
}
