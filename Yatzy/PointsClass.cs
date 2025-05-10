using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Yatzy
{
    public class PointsClass : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Private Fields
        private int? _point;
        private string _plusminus;
        private FontWeight _font = FontWeights.Bold;
        private Brush _fontcolor = Brushes.Black;
        private bool _hasPoints = false;
        private bool _leftbuttonenabled = false;
        private bool _rightbuttonenabled = true;
        private bool _canselect;

        private SolidColorBrush _bakgrund = new SolidColorBrush(Colors.AntiqueWhite);
        #endregion

        #region Public Properties   
        public int? Point
        {
            get => _point;
            set
            {
                if (_point != value)
                {
                    _point = value;
                    CalculatePlusMinus(0);
                    OnPropertyChanged(nameof(Point));
                }
            }
        }
        public string PlusMinus
        {
            get => _plusminus;
            set
            {
                if (_plusminus != value)
                {
                    _plusminus = value;
                    OnPropertyChanged(nameof(PlusMinus));
                }
            }
        }
        public FontWeight Font
        {
            get => _font;
            set
            {
                if (_font != value)
                {
                    _font = value;
                    OnPropertyChanged(nameof(Font));
                }
            }
        }
        public Brush FontColor
        {
            get => _fontcolor;
            set
            {
                if (_fontcolor != value)
                {
                    _fontcolor = value;
                    OnPropertyChanged(nameof(FontColor));
                }
            }
        }
        public bool HasPoints
        {
            get => _hasPoints;
            set
            {
                if (_hasPoints != value)
                {
                    _hasPoints = value;
                    OnPropertyChanged(nameof(HasPoints));
                }
            }
        }
        public bool LeftButtonEnabled
        {
            get => _leftbuttonenabled;
            set
            {
                if (_leftbuttonenabled != value)
                {
                    _leftbuttonenabled = value;
                    OnPropertyChanged(nameof(LeftButtonEnabled));
                }
            }
        }
        public bool RightButtonEnabled
        {
            get => _rightbuttonenabled;
            set
            {
                if (_rightbuttonenabled != value)
                {
                    _rightbuttonenabled = value;
                    OnPropertyChanged(nameof(RightButtonEnabled));
                }
            }
        }
        public bool CanSelect
        {
            get => _canselect;
            set
            {
                if (_canselect != value)
                {
                    _canselect = value;
                    OnPropertyChanged(nameof(CanSelect));
                }
            }
        }
        public bool ShowButton { get; set; } = true;
        public bool IsBonus { get; set; }
        public SolidColorBrush BakGrund
        {
            get => _bakgrund;
            set
            {
                if (_bakgrund != value)
                {
                    _bakgrund = value;
                    OnPropertyChanged(nameof(BakGrund));
                }
            }
        }
        #endregion

        public void CalculatePlusMinus(int summa)
        {
            if (IsBonus)
            {
                switch (Name)
                {
                    case "Ettor": PlusMinus = $"{(Point - 3 != 0 ? Point - 3 > 0 ? "+" : "-" : "")}{(Point - 3 != 0 ? Point - 3 > 0 ? Point - 3 : 3 - Point : "")}"; break;
                    case "Tvåor": PlusMinus = $"{(Point - 6 != 0 ? Point - 6 > 0 ? "+" : "-" : "")}{(Point - 6 != 0 ? Point - 6 > 0 ? Point - 6 : 6 - Point : "")}"; break;
                    case "Treor": PlusMinus = $"{(Point - 9 != 0 ? Point - 9 > 0 ? "+" : "-" : "")}{(Point - 9 != 0 ? Point - 9 > 0 ? Point - 9 : 9 - Point : "")}"; break;
                    case "Fyror": PlusMinus = $"{(Point - 12 != 0 ? Point - 12 > 0 ? "+" : "-" : "")}{(Point - 12 != 0 ? Point - 12 > 0 ? Point - 12 : 12 - Point : "")}"; break;
                    case "Femmor": PlusMinus = $"{(Point - 15 != 0 ? Point - 15 > 0 ? "+" : "-" : "")}{(Point - 15 != 0 ? Point - 15 > 0 ? Point - 15 : 15 - Point : "")}"; break;
                    case "Sexor": PlusMinus = $"{(Point - 18 != 0 ? Point - 18 > 0 ? "+" : "-" : "")}{(Point - 18 != 0 ? Point - 18 > 0 ? Point - 18 : 18 - Point : "")}"; break;
                    case "Bonus": PlusMinus = $"{( summa > 0 ? "+" : "")}{(summa != 0 ? summa : "")}"; break;
                }
            }
        }

        public string Name { get; set; }

        public PointsClass(string name, int point)
        {
            Name = name;
            if (name == "")
            {
                Point = null;

            }
            else
            {
                Point = point;
            }
        }

    }
}
