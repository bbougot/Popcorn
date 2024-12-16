using System;
using System.Globalization;
using System.Windows.Data;

namespace Popcorn.Converters
{
    /// <summary>
    /// Convert a boolean to its inverse
    /// </summary>
    [ValueConversion(typeof (bool), typeof (bool))]
    public class BoolToInverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean to its inverse
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Inversed boolean</returns>
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) => !(bool) value;

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}