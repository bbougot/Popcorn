using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Popcorn.Converters
{
    /// <summary>
    /// Used to convert a boolean to a Visibility class property
    /// </summary>
    public class BoolToInverseVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert bool to Visibility
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Visible if true, Collapsed otherwise</returns>
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof (Visibility))
                throw new InvalidOperationException("The target must be a VisibilityProperty");

            if ((bool) value)
                return Visibility.Visible;

            return Visibility.Collapsed;
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