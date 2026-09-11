using System.Globalization;
using System.Windows.Data;

namespace ChessGame.Converters
{
    /// <summary>
    /// A value converter that converts a string representation of a boolean value to a boolean type and vice versa.
    /// </summary>
    internal class ConvertToBool : IValueConverter
    {
        /// <summary>
        /// Converts a string representation of a boolean value to a boolean type.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A boolean value corresponding to the string representation of a boolean.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          if(value?.ToString() is string str)
          {
             return !string.IsNullOrWhiteSpace(str) && str.Equals("true", StringComparison.OrdinalIgnoreCase);
          }

           return false;
        }

        /// <summary>
        /// Converts a boolean value back to its string representation. This method is not fully 
        /// implemented and will return the original value if it is a boolean and the parameter is a string; otherwise, it returns Binding.DoNothing.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type of the binding source property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The original value if it is a boolean and the parameter is a string; otherwise, Binding.DoNothing.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && parameter is string)
            {
                return value;
            }

            return Binding.DoNothing;
        }
    }
}
