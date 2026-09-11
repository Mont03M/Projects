using ChessGame.Enums;
using System.Globalization;
using System.Windows.Data;

namespace ChessGame.Converters
{
    /// <summary>
    /// A value converter that converts a ChessPieceColors enum value to a boolean value based on a specified parameter, and vice versa. 
    /// This is useful for binding UI elements to specific chess piece colors.
    /// </summary>
    public class ConvertToChessPieceColor : IValueConverter
    {
        /// <summary>
        /// Converts a ChessPieceColors enum value to a boolean value based on the specified parameter.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A boolean value indicating whether the ChessPieceColors value matches the specified parameter.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChessPieceColors color && parameter is string colorParam)
            {
                return color.ToString().Equals(colorParam, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Converts a boolean value back to a ChessPieceColors enum value based on the specified parameter.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type of the binding source property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The corresponding ChessPieceColors value if the boolean is true and the parameter is valid; otherwise, Binding.DoNothing.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string colorParam)
            {
                if (Enum.TryParse(typeof(ChessPieceColors), colorParam, true, out var result))
                {
                    return (ChessPieceColors)result;
                }
            }

            return Binding.DoNothing;
        }
    }
}
