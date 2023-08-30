using System.Windows;
using System.Windows.Controls;

namespace WPF.Utils
{
    // Using TemplateSelector to template an interface
    internal class LyricsProviderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LyricsProviderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return LyricsProviderTemplate;
        }
    }
}