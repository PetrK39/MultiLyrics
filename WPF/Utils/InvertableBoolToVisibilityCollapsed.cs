using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Utils
{
    internal class InvertableBoolToVisibilityCollapsed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool state)
            {
                if (parameter is bool inverted && inverted)
                    return state ? Visibility.Collapsed : Visibility.Visible;
                return state ? Visibility.Visible : Visibility.Collapsed;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}