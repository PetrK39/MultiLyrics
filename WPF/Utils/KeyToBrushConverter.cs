using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Utils
{
    internal class KeyToBrushConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key)
                return App.ThemeManager.GetBrushByKey(key);
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}