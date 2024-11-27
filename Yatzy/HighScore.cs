using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Yatzy
{
    public class HighScore : INotifyPropertyChanged
    {
        public HighScore(int? score)
        {
            Score = score;
            Datum = DateTime.Now.ToString("yyyy-MM-dd");
            
        }
        public bool IsLast { get; set; }
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

        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (_index != value) {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                } 
            } 
        }

        public int? Score { get; set; }
        public string Datum { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
