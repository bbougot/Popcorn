using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Popcorn.Converters
{
    [ValueConversion(typeof(double), typeof(Brush))]
    public class ValueToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number = (double)value;
            double min = 0;
            double max = 100;

            // Get the value limits from parameter
            try
            {
                string[] limits = (parameter as string).Split(new char[] { '|' });
                min = double.Parse(limits[0], CultureInfo.InvariantCulture);
                max = double.Parse(limits[1], CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new ArgumentException("Parameter not valid. Enter in format: 'MinDouble|MaxDouble'");
            }

            if (max <= min)
            {
                throw new ArgumentException("Parameter not valid. MaxDouble has to be greater then MinDouble.");
            }

            if (number >= min && number <= max)
            {
                // Calculate color channels
                double range = (max - min) / 2;
                number -= max - range;
                double factor = 255 / range;
                double green = number < 0 ? number * factor : 255;
                double red = number > 0 ? (range - number) * factor : 255;

                // Create and return brush
                Color color = Color.FromRgb((byte)red, (byte)green, 0);
                SolidColorBrush brush = new SolidColorBrush(color);
                return brush;
            }

            // Fallback brush
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
