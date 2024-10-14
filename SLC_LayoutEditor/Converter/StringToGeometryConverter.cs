using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SLC_LayoutEditor.Converter
{
    class StringToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string rawGeometry && !string.IsNullOrWhiteSpace(rawGeometry))
                return Geometry.Parse(rawGeometry);
            else if (value is Geometry)
                return value;
            else
                return Geometry.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
