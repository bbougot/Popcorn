using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Unosquare.FFME;

namespace Popcorn.Converters
{
    /// <summary>
    /// Formsts timespan time measures as string with 3-decimal milliseconds
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class TimeSpanFormatter : IValueConverter
    {
        /// <summary>
        /// Converts the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public object Convert(object position, Type targetType, object duration, CultureInfo culture)
        {
            if (duration != null)
            {
                var media = (duration as MediaElement);
                duration = media?.NaturalDuration;
                if (duration == null) duration = TimeSpan.Zero;
            }

            var p = TimeSpan.Zero;
            var d = TimeSpan.Zero;

            if (position is TimeSpan) p = (TimeSpan)position;
            if (position is Duration) p = ((Duration)position).HasTimeSpan ? ((Duration)position).TimeSpan : TimeSpan.Zero;

            if (duration != null)
            {
                if (duration is TimeSpan) d = (TimeSpan)duration;
                if (duration is Duration) d = ((Duration)duration).HasTimeSpan ? ((Duration)duration).TimeSpan : TimeSpan.Zero;

                if (d == TimeSpan.Zero) return string.Empty;
                p = TimeSpan.FromTicks(d.Ticks - p.Ticks);

            }

            return $"{(int)(p.TotalHours):00}:{p.Minutes:00}:{p.Seconds:00}";
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }
    }
}
