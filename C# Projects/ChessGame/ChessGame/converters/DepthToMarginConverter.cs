using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChessGame.Converters
{
    /// <summary>
    /// A value converter that converts an integer depth value to a Thickness margin for UI representation.
    /// </summary>
    public class DepthToMarginConverter : IValueConverter
    {
        /// <summary>
        /// Converts an integer depth value to a Thickness margin. The left margin is calculated as depth multiplied by 20, while the top, right, and bottom margins are set to 0.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A Thickness value representing the margin based on the depth.</returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is int depth)
            {
                return new Thickness(depth * 20, 0, 0, 0);
            }

            return new Thickness(0);
        }

        /// <summary>
        /// Converts a Thickness margin back to an integer depth value. This method is not supported and will throw a NotSupportedException if called.
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
