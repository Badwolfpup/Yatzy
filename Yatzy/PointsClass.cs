using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Yatzy
{
    public class PointsClass: INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string Name { get; set; }
        private int _point;
        public int Point
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
        private FontWeight _font = FontWeights.Normal;
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

        private Brush _fontcolor = Brushes.Black;
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
        private TextDecorationCollection? _overstruken;
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

        public PointsClass(string name, int point)
        {
            Name = name;
            Point = point;
        }

    }
}
