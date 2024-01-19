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
using System.Windows.Media.Imaging;

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

        public static Dictionary<CabinSlot, bool> GetDiff(this IEnumerable<CabinSlot> newItems, IEnumerable<CabinSlot> oldItems)
        {
            Dictionary<CabinSlot, bool> diff = new Dictionary<CabinSlot, bool>();
            if (oldItems != null)
            {
                foreach (CabinSlot added in newItems.Where(x => !oldItems.Any(y => y.Guid == x.Guid)))
                {
                    diff.Add(added, true);
                }

                foreach (CabinSlot removed in oldItems.Where(x => !newItems.Any(y => y.Guid == x.Guid)))
                {
                    diff.Add(removed, false);
                }
            }
            else
            {
                foreach (CabinSlot added in newItems)
                {
                    diff.Add(added, true);
                }
            }

            return diff;
        }

        public static Size Modify(this Size size, double width, double height)
        {
            return new Size(size.Width + width, size.Height + height);
        }

        public static Size GetSize(this FormattedText text)
        {
            return new Size(text.Width, text.Height);
        }

        public static Point VectorToPoint(this Vector vector)
        {
            return new Point(vector.X, vector.Y);
        }

        public static RenderTargetBitmap RenderVisual(this DrawingVisual visual, Size size)
        {
            return visual.RenderVisual(size.Width, size.Height);
        }

        public static RenderTargetBitmap RenderVisual(this DrawingVisual visual, double width, double height)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), 96, 96, PixelFormats.Pbgra32);

            renderBitmap.Render(visual);
            renderBitmap.Freeze();

            return renderBitmap;
        }

        public static void RedrawArea(this WriteableBitmap writeableBitmap, BitmapSource source, Rect redrawRect)
        {
            try
            {
                writeableBitmap.Lock();

                unsafe
                {
                    int sourceX = 0;
                    int sourceY = 0;

                    for (int x = (int)redrawRect.X; x < redrawRect.Right; x++)
                    {
                        for (int y = (int)redrawRect.Y; y < redrawRect.Bottom; y++)
                        {
                            IntPtr backBuffer = writeableBitmap.BackBuffer;
                            backBuffer += y * writeableBitmap.BackBufferStride;
                            backBuffer += x * 4;

                            int colorData = GetColorData(source, sourceX, sourceY);

                            *((int*)backBuffer) = colorData;

                            sourceY++;
                        }

                        sourceX++;
                        sourceY = 0;
                    }

                    writeableBitmap.AddDirtyRect(GetInt32Rect(redrawRect));
                }
            }
            finally
            {
                writeableBitmap.Unlock();
            }
        }

        private static Int32Rect GetInt32Rect(Rect rect)
        {
            return new Int32Rect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        private static int GetColorData(BitmapSource bitmap, int x, int y)
        {
            Color color;
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var rect = new Int32Rect(x, y, 1, 1);

            bitmap.CopyPixels(rect, bytes, bytesPerPixel, 0);

            if (bitmap.Format == PixelFormats.Pbgra32)
            {
                color = Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
            }
            else
            {
                color = Colors.Black;
            }

            int colorData = color.A << 24 | color.R << 16 | color.G << 8 | color.B;

            return colorData;
        }
    }
}
