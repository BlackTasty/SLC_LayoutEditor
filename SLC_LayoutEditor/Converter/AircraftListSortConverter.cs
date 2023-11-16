using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    class AircraftListSortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<CabinLayoutSet> layoutSets)
            {
                return layoutSets.OrderByDescending(x => x.LayoutCount).ThenBy(x => x.AirplaneName);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
