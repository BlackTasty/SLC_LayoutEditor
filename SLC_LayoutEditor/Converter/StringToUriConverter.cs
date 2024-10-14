using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    internal class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string relativePath = value.ToString();

                if (File.Exists(relativePath))
                {
                    return new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath), UriKind.Absolute);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
