using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    internal class HistoryStepDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] data = value?.ToString().Split('|');
            if (int.TryParse(parameter?.ToString() ?? "0", out int index) && data.Length > index)
            {
                return data[index];
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
