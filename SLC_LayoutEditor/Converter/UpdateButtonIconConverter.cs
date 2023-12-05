using SLC_LayoutEditor.Core.Patcher;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    class UpdateButtonIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string iconName = "Update";

            if (value is UpdateStatus status)
            {
                switch (status)
                {
                    case UpdateStatus.READY:
                        iconName = "Restart";
                        break;
                    case UpdateStatus.UPDATES_FOUND:
                        iconName = "Download";
                        break;
                    case UpdateStatus.UPTODATE:
                        iconName = "ProgressCheck";
                        break;
                    default:
                        iconName = "Restart";
                        break;
                }
            }

            return (string)App.Current.FindResource(iconName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}

