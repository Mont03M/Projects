using System.Globalization;
using System.Windows.Data;

namespace ChessGame.Converters
{
    /// <summary>
    /// A value converter that converts a value to its string representation and checks for equality with a specified parameter.
    /// </summary>
    public class ConvertToString : IValueConverter
    {
        public ConvertToString() { }

        /// <summary>
        /// Converts a value to its string representation and checks for equality with a specified parameter.
        /// </summary>
        /// <param name="values">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>True if the string representation of the value matches the parameter; otherwise, false.</returns>
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            return values?.ToString() == parameter?.ToString();
        }

        /// <summary>
        /// Converts a boolean value back to its string representation based on the specified parameter.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type of the binding source property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The string representation of the parameter if the boolean is true; otherwise, Binding.DoNothing.</returns>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is true && parameter != null)
            {
                return parameter?.ToString();
            }

            return Binding.DoNothing;
        }
    }
}
