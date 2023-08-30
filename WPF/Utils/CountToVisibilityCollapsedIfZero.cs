using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Utils
{
    public class CountToVisibilityCollapsedIfZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int num && num > 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}