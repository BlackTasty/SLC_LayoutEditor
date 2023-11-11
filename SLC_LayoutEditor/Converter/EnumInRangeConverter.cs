using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    class EnumInRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out int intValue))
            {
                string[] paramList = parameter?.ToString().Split(';');

                for (int i = 0; i < paramList.Length; i++)
                {
                    if (int.TryParse(paramList[i], out int result) && intValue == result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
