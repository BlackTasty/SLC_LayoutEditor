using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using SLC_LayoutEditor.Core.Patcher;

namespace SLC_LayoutEditor.Converter
{
    class UpdateIconVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInvert = parameter != null && parameter.ToString().ToLower() == "true";

            if (value is UpdateStatus status)
            {
                switch (status)
                {
                    case UpdateStatus.INIT:
                    case UpdateStatus.IDLE:
                    case UpdateStatus.READY:
                    case UpdateStatus.ERROR:
                    case UpdateStatus.UPDATES_FOUND:
                    case UpdateStatus.UPTODATE:
                        return !isInvert ? Visibility.Visible : Visibility.Collapsed;
                    default:
                        return !isInvert ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
