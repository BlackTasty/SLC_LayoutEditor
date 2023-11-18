using SLC_LayoutEditor.Core.Patcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SLC_LayoutEditor.Core
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Replaces multiple strings inside the target string
        /// </summary>
        /// <param name="source">Any string where strings shall be replaced</param>
        /// <param name="oldValues">A number of strings which shall be replaced</param>
        /// <param name="newValue">The new value which replaces all occurences</param>
        /// <returns></returns>
        public static string Replace(this string source, string[] oldValues, string newValue = "")
        {
            StringBuilder b = new StringBuilder(source);
            foreach (var str in oldValues)
                if (str != "")
                    b.Replace(str, newValue);
            return b.ToString();
        }

        public static bool ExceedsSelectionThreshold(this Point start, Point current, int minSizeThreshold,
            int minTotalPxThreshold)
        {
            Rect selectionRect = new Rect(start, current);
            return Math.Abs(start.X - current.X) >= minSizeThreshold &&
                Math.Abs(start.Y - current.Y) >= minSizeThreshold &&
                selectionRect.Width * selectionRect.Height > minTotalPxThreshold;
        }

        public static double ExtractDouble(this object value)
        {
            var d = value as double? ?? double.NaN;
            return double.IsInfinity(d) ? double.NaN : d;
        }

        public static bool AnyNan(this IEnumerable<double> values)
        {
            return values.Any(double.IsNaN);
        }
    }
}
