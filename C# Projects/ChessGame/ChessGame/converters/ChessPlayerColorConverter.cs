using ChessGame.Enums;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChessGame.Converters
{
    /// <summary>
    /// A value converter that converts a ChessPieceColors enum value to a corresponding Brush color for UI representation.
    /// </summary>
    public class ChessPlayerColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts a ChessPieceColors enum value to a Brush color.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A Brush color corresponding to the ChessPieceColors value.</returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is ChessPieceColors playerColor)
            {
                return playerColor == ChessPieceColors.WHITE
                    ? Brushes.NavajoWhite
                    : Brushes.CornflowerBlue;
            }

            return Brushes.White;
        }
        /// <summary>
        /// Converts a value back to a ChessPieceColors enum value. This method is not supported and will throw a NotSupportedException if called.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type of the binding source property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Nothing. This method always throws a NotSupportedException.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}