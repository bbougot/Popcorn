using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Popcorn.Converters
{
    /// <summary>
    /// Convert an object to a Visibility depending on its nullity
    /// </summary>
    public class NullAsVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert an object to a Visibility depending on its nullity
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Null if true, false otherwise</returns>
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

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