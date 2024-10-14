using SLC_LayoutEditor.Core;
using SLC_LayoutEditor.Core.Cabin;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    internal class LayoutThumbnailGrabber : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CabinLayout cabinLayout)
            {
                string thumbnailDirectory = cabinLayout.ThumbnailDirectory;
                if (thumbnailDirectory != null && Directory.Exists(thumbnailDirectory))
                {
                    FileInfo firstThumbnail = new DirectoryInfo(thumbnailDirectory).EnumerateFiles().FirstOrDefault();
                    if (firstThumbnail != null)
                    {
                        return Util.LoadImage(firstThumbnail.FullName);
                    }
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
