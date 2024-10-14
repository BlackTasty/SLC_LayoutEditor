using System;
using System.Globalization;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    internal class ModifyNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                if (parameter != null && double.TryParse(parameter.ToString(), out double doubleParam))
                {
                    return doubleValue + doubleParam;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
