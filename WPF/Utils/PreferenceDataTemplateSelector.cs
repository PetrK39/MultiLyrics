using PreferenceManagerLibrary.Preferences;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WPF.Utils
{
    public class PreferenceDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PreferenceCollectionTemplate { private get; set; }
        public DataTemplate InputPreferenceTemplate { private get; set; }
        public DataTemplate BoolPreferenceTemplate { private get; set; }
        public DataTemplate ListStringPreferenceThemeTemplate { private get; set; }
        public DataTemplate ListStringPreferenceTemplate { private get; set; }
        public DataTemplate ThemePreviewTemplate { private get; set; }
        public DataTemplate PreferenceGroupCollectionTemplate { private get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case PreferenceCollection _ when item is PreferenceCollection coll && coll.Key.EndsWith(".prefGroup"):
                    return PreferenceGroupCollectionTemplate;

                case PreferenceCollection _:
                    return PreferenceCollectionTemplate;

                case InputPreference _:
                    return InputPreferenceTemplate;

                case BoolPreference _:
                    return BoolPreferenceTemplate;

                case SingleSelectPreference<string> listPref when listPref.Key.StartsWith("theme."):
                    return ListStringPreferenceThemeTemplate;

                case SingleSelectPreference<string> _:
                case SingleSelectPreference<CultureInfo> _:
                    return ListStringPreferenceTemplate;

                case LabelPreference _ when item is LabelPreference labelPref && labelPref.Key == "theme.preview":
                    return ThemePreviewTemplate;

                case LabelPreference _:
                    return null;

                default:
                    throw new NotImplementedException($"There is no specified template for {item.GetType()}");
            }
        }
    }
}