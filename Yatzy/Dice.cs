using System.ComponentModel;
using System.Windows.Media;

namespace Yatzy
{
    public class Dice : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public Dice(int dice)
        {
            DiceValue = dice;
            SetSource();
        }

        public Dice()
        {
        }

        private void SetSource()
        {
            ImageSource = $"pack://application:,,,/Images/dice{DiceValue}.png";
        }

        public void UpdateDice(int dice, bool saved)
        {
            DiceValue = dice;
            //Issaved = saved;
            SetSource();
        }

        public void UpdateBorderColor(bool clicked)
        {
            if (clicked)
            {
                Color = new SolidColorBrush(Colors.Crimson);
            }
            else
            {
                Color = new SolidColorBrush(Colors.Blue);
            }
        }

        private SolidColorBrush _color = new SolidColorBrush(Colors.Blue);
        public SolidColorBrush Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        private int _dicevalue;
        public int DiceValue
        {
            get => _dicevalue;
            set
            {
                if (_dicevalue != value)
                {
                    _dicevalue = value;
                    OnPropertyChanged(nameof(DiceValue));
                }
            }
        }
        private string? _imagesource;
        public string? ImageSource
        {
            get => _imagesource;
            set
            {
                if (_imagesource != value)
                {
                    _imagesource = value;
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }
        private bool _issaved;
        public bool Issaved
        {
            get => _issaved;
            set
            {
                if (_issaved != value)
                {
                    _issaved = value;
                    OnPropertyChanged(nameof(Issaved));
                }
            }
        }
    }
}
