using System;
using System.Globalization;
using System.Windows.Data;

namespace Popcorn.Converters
{
    /// <summary>
    /// Convert a boolean to null if true
    /// </summary>
    [ValueConversion(typeof (object), typeof (bool))]
    public class NullAsBoolConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean to null if true
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Null if true, false otherwise</returns>
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            var boolean = parameter as bool?;
            if (boolean.HasValue && boolean.Value)
                return value == null;

            return value != null;
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
            var boolean = parameter as bool?;
            if (boolean.HasValue && boolean.Value)
                return value == null;

            return value != null;
        }
    }
}