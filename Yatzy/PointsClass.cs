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
        private FontWeight _font = FontWeights.Normal;
        private Brush _fontcolor = Brushes.Black;
        private TextDecorationCollection? _overstruken;
        private bool _hasPoints = false;
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
        public TextDecorationCollection? Overstruken
        {
            get => _overstruken;
            set
            {
                if (_overstruken != value)
                {
                    _overstruken = value;
                    OnPropertyChanged(nameof(Overstruken));
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
