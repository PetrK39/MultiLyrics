using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.Utils
{
    internal class InvertableBoolToVisibilityHidden : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool state)
            {
                if (parameter is bool inverted && inverted)
                    return state ? Visibility.Hidden : Visibility.Visible;
                return state ? Visibility.Visible : Visibility.Hidden;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}