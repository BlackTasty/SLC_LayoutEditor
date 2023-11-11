using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace SLC_LayoutEditor.Converter
{
    class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool visible)
            {
                return visible ? GetHidingMode(parameter) : Visibility.Visible;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
                return false;
            else
                return true;
        }

        private Visibility GetHidingMode(object parameter)
        {
            if (parameter is string paramString)
            {
                return paramString == "hide" ? Visibility.Hidden : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
    }
}
