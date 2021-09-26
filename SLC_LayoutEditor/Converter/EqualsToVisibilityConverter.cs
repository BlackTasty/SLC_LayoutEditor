using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    class EqualsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && int.TryParse(value.ToString(), out int intValue))
            {
                if (parameter != null && int.TryParse(parameter.ToString(), out int intParameter))
                {
                    return intValue == intParameter ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
