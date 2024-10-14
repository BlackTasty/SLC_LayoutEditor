using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    class AircraftListSortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<CabinLayoutSet> layoutSets)
            {
                return Sort(layoutSets);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        public static IEnumerable<CabinLayoutSet> Sort(IEnumerable<CabinLayoutSet> layoutSets)
        {
            bool isTemplatingMode = layoutSets.FirstOrDefault()?.IsTemplatingMode ?? false;

            return !isTemplatingMode ? layoutSets.OrderByDescending(x => x.LayoutCount > 0).ThenBy(x => x.AircraftName) :
                layoutSets.OrderByDescending(x => x.TemplateCount > 0).ThenBy(x => x.AircraftName);
        }
    }
}
