using ChessGame.Utilities;
using System.Windows.Media;

namespace ChessGame.Colors_
{
    /// <summary>
    /// Represents a color item with a name, color value, and index. 
    /// This class implements the INotifyPropertyChanged interface to support data binding in WPF applications.
    /// </summary>
    public class ColorItem : ObservableObject
    {
        private string name = default!;
        private Color color = default!;
        private int index = default!;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
        public int Index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        public ColorItem(string name, Color color, int index = -1) 
        {
            Name = name;
            Color = color;
            Index = index;
        }
    }
}
