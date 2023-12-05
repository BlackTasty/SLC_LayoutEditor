using SLC_LayoutEditor.Core.Patcher;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    class UpdateProgressBarIndeterminateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UpdateStatus status)
            {
                switch (status)
                {
                    case UpdateStatus.EXTRACTING:
                    case UpdateStatus.INSTALLING:
                        return false;
                }
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
