using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WPF.Utils
{
    internal class LocalizedResxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string str)) return null;

            if (str.StartsWith("%"))
            {
                var pluginName = str.Substring(1, str.IndexOf('%', 1) - 1);
                var resKey = str.Substring(pluginName.Length + 2 + 1);

                var plugin = App.PluginManager.LyricsProviders.Single(p => p.Name == pluginName);
                return plugin.GetResource(resKey, culture) ?? "#" + str;
            }
            else
            {
                return Properties.Resources.ResourceManager.GetString(str, culture) ?? "#" + str;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}