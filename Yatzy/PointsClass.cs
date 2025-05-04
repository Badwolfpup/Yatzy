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
        private FontWeight _font = FontWeights.Bold;
        private Brush _fontcolor = Brushes.Black;
        private bool _hasPoints = false;
        private bool _leftbuttonenabled = false;
        private bool _rightbuttonenabled = true;
        private bool _canselect;
        private SolidColorBrush _bakgrund = new SolidColorBrush(Colors.AntiqueWhite);
        #endregion

        #region Public INotifyPropertyChanged   
        public int? Point
        {
            get => _point;
            set
            {
                if (_point != value)
                {
                    _point = value;
                    OnPropertyChanged(nameof(Point));
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
