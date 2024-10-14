using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    internal class ContentSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScrollViewer scroll && scroll.Content is FrameworkElement content && bool.TryParse(parameter?.ToString() ?? "", out bool pullHeight))
            {
                return pullHeight ? content.ActualHeight : content.ActualWidth;
            }

            return 10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
