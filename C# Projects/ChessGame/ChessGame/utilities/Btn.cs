using System.Windows;
using System.Windows.Controls;

namespace ChessGame.Utilities
{
    /// <summary>
    /// A custom button control that inherits from RadioButton and provides a default style.
    /// </summary>
    public class Btn : RadioButton
    {
        /// <summary>
        /// Initializes the static members of the <see cref="Btn"/> class and overrides the default style key.
        /// </summary>
        static Btn()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Btn), new FrameworkPropertyMetadata(typeof(Btn)));
        }
    }
}
