﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace SLC_LayoutEditor.Converter
{
    public class NotZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse((value ?? "").ToString(), out double val))
            {
                return Math.Abs(val) > 0.0;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
