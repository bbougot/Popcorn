using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.IconPacks;

namespace Popcorn.Converters
{
    /// <summary>
    /// Converts a <see cref="PackIcon{TKind}" /> to an ImageSource.
    /// Use the ConverterParameter to pass a Brush.
    /// </summary>
    public abstract class PackIconImageSourceConverterBase<TKind> : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Gets or sets the thickness to draw the icon with.
        /// </summary>
        public double Thickness { get; set; } = 0.25;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = value as string;
            if (!string.IsNullOrEmpty(source))
                return new BitmapImage(new Uri(source));

            var fallback = (TKind)parameter;
            return CreateImageSource(value, fallback, Thickness);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        protected abstract ImageSource CreateImageSource(object value, TKind parameter, double penThickness);
    }
    
    public class PackIconMaterialImageSourceConverter : PackIconImageSourceConverterBase<PackIconMaterialKind>
    {
        protected override ImageSource CreateImageSource(object value, PackIconMaterialKind fallback,
            double penThickness)
        {
            var packIcon = new PackIconMaterial {Kind = fallback};

            var geometryDrawing = new GeometryDrawing
            {
                Geometry = Geometry.Parse(packIcon.Data),
                Brush = Brushes.White,
                Pen = new Pen(Brushes.White, penThickness)
            };

            var drawingGroup = new DrawingGroup {Children = {geometryDrawing}};

            return new DrawingImage {Drawing = drawingGroup};
        }
    }
}
