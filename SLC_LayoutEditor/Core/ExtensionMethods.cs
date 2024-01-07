using SLC_LayoutEditor.Core.Cabin;
using SLC_LayoutEditor.Core.Enum;
using SLC_LayoutEditor.Core.Patcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

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

        public static Point MakeOffset(this Point current, double offsetX, double offsetY)
        {
            return new Point(current.X + offsetX, current.Y + offsetY);
        }

        public static Adorner AttachAdorner(this UIElement uiElement, Type adornerType)
        {
            return AttachAdorner(uiElement, adornerType, null);
        }

        public static Adorner AttachAdorner(this UIElement uiElement, Type adornerType, params object[] args)
        {
            IEnumerable<Type> constructorTypes = adornerType.GetConstructors().First().GetParameters().Select(x => x.ParameterType);
            List<object> constructorParams = new List<object> { uiElement };
            if (args?.Length > 0)
            {
                //constructorTypes.AddRange(args.Select(t => t.GetType()));
                constructorParams.AddRange(args);
            }

            ConstructorInfo ctor = adornerType.GetConstructor(constructorTypes.ToArray());
            Adorner instance = (Adorner)ctor.Invoke(constructorParams.ToArray());

            uiElement.AttachAdorner(instance);
            return instance;
        }

        public static void RemoveAdorner(this UIElement uiElement, Adorner adorner)
        {
            AdornerLayer.GetAdornerLayer(uiElement).Remove(adorner);
        }

        public static void AttachAdorner(this UIElement uiElement, Adorner adorner)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(uiElement);
            adornerLayer?.Add(adorner);
        }

        public static int CountSlots(this IEnumerable<CabinSlot> cabinSlots, CabinSlotType targetType)
        {
            return cabinSlots.Count(x => x.Type == targetType);
        }

        public static int CountSlots(this IEnumerable<CabinSlot> cabinSlots, params CabinSlotType[] targetTypes)
        {
            return cabinSlots.Count(x => targetTypes.Contains(x.Type));
        }

        public static void AddRangeDistinct(this List<CabinSlot> source, IEnumerable<CabinSlot> items)
        {
            foreach (var item in items)
            {
                if (!source.Contains(item))
                {
                    source.Add(item);
                }
            }
        }
    }
}
