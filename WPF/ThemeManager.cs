using PreferenceManagerLibrary.Manager;
using PreferenceManagerLibrary.Preferences;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WPF
{
    public class ThemeManager
    {
        private readonly Dictionary<string, Brush> skinDictionary;
        private readonly PreferenceManager preferenceManager;
        private bool isThemingEnabled;

        private readonly Dictionary<string, string> preferenceKeyResourceKeyDictionary = new Dictionary<string, string> {
            {"theme.colorsBackground","DynamicBackground"},
            {"theme.colorsForeground","DynamicForeground"},

            {"theme.colors.buttonBackground","DynamicButtonBackground"},
            {"theme.colors.buttonForeground","DynamicButtonForeground"},

            {"theme.colors.buttonHoverBackground","DynamicButtonHoverBackground" },
            {"theme.colors.buttonHoverForeground","DynamicButtonHoverForeground" },

            {"theme.colors.disabledButtonBackground","DynamicDisabledButtonBackground" },
            {"theme.colors.disabledButtonForeground","DynamicDisabledButtonForeground" },

            {"theme.colors.textBoxBackground","DynamicTextBoxBackground" },
            {"theme.colors.textBoxForeground","DynamicTextBoxForeground" },

            {"theme.colors.progressBarBackground","DynamicProgressBarBackground" },
            {"theme.colors.progressBarForeground","DynamicProgressBarForeground" },
        };

        public IEnumerable<string> Keys => skinDictionary.Keys;

        public ThemeManager(PreferenceManager preferenceManager, IEnumerable<KeyValuePair<string, int>> skin)
        {
            this.preferenceManager = preferenceManager;

            skinDictionary = new Dictionary<string, Brush>();
            UpdateSkin(skin);
        }

        private void ThemeManager_EnabledChanged(object sender, PropertyChangedEventArgs e)
        {
            var pref = (BoolPreference)sender;
            isThemingEnabled = pref.IsEditing ? pref.EditableValue : pref.Value;

            ThemeManager_PropertyChanged(this, null);
        }
        private void ThemeManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            var res = new ResourceDictionary();

            foreach (var item in preferenceKeyResourceKeyDictionary)
            {
                var pref = (SingleSelectPreference<string>)preferenceManager[item.Key];
                var prefValue = pref.IsEditing ? pref.EditableValue : pref.Value;
                res.Add(item.Value, GetBrush(item.Value, prefValue));
            }
            Application.Current.Dispatcher.Invoke(() => Application.Current.Resources.MergedDictionaries.Add(res));
        }


        public void UpdateSkin(IEnumerable<KeyValuePair<string, int>> skin)
        {
            foreach (var kc in skin)
            {
                var key = kc.Key;
                var color = kc.Value;

                switch (color)
                {
                    case -1:
                        skinDictionary[key] = Brushes.White;
                        break;
                    default:
                        byte[] bytes = BitConverter.GetBytes(color);
                        SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]));
                        skinDictionary[key] = brush;
                        break;
                }
            }
        }

        private Brush GetBrush(string brushName, string prefValue)
        {
            return (prefValue is null || !isThemingEnabled) ? GetDefaultBrush(brushName) : GetBrushByKey(prefValue);
        }
        public Brush GetBrushByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            return skinDictionary.SingleOrDefault(kv => kv.Key == key).Value;
        }
        private Brush GetDefaultBrush(string key)
        {
            switch (key)
            {
                case "DynamicBackground":
                    return SystemColors.WindowBrush;

                case "DynamicForeground":
                    return SystemColors.WindowTextBrush;

                case "DynamicButtonBackground":
                    return SystemColors.ControlLightBrush;

                case "DynamicButtonForeground":
                    return SystemColors.ControlTextBrush;

                case "DynamicButtonHoverBackground":
                    return new SolidColorBrush(Color.FromRgb(0xBE, 0xE6, 0xFD));

                case "DynamicButtonHoverForeground":
                    return SystemColors.ControlTextBrush;

                case "DynamicDisabledButtonBackground":
                    return SystemColors.ControlLightBrush;

                case "DynamicDisabledButtonForeground":
                    return SystemColors.GrayTextBrush;

                case "DynamicTextBoxBackground":
                    return SystemColors.ControlLightLightBrush;

                case "DynamicTextBoxForeground":
                    return SystemColors.ControlTextBrush;

                case "DynamicComboBoxSelection":
                    return SystemColors.MenuHighlightBrush;

                case "DynamicProgressBarBackground":
                    return SystemColors.ControlLightBrush;

                case "DynamicProgressBarForeground":
                    return new SolidColorBrush(Color.FromRgb(0x01, 0xD3, 0x28));

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), $"Trying to get default brush for unknown key \"{key}\"");
            }
        }

        public PreferenceCollection BuildThemePreferences()
        {
            var tabTheme = new PreferenceCollection("theme", "l10n.theme", "l10n.theme.description");

            var themeGeneralGroup = new PreferenceCollection("theme.general", "l10n.general");

            var themeEn = new BoolPreference("theme.general.enabled", "l10n.theme.general.enabled.description", defaultValue: true);
            themeEn.PropertyChanged += ThemeManager_EnabledChanged;
            themeGeneralGroup.ChildrenPreferences.Add(themeEn);

            var themeColorGroup = new PreferenceCollection("theme.colors", "l10n.theme.colors");
            themeColorGroup.PropertyChanged += ThemeManager_PropertyChanged;

            var preferenceKeys = new[]
            {
                "theme.colors",
                "theme.colors.button",
                "theme.colors.buttonHover",
                "theme.colors.disabledButton",
                "theme.colors.textBox",
                "theme.colors.progressBar"
            };

            foreach (var prefKey in preferenceKeys)
            {
                var subGroup = new PreferenceCollection(prefKey + ".prefGroup", "l10n." + prefKey + ".description");

                subGroup.ChildrenPreferences.Add(new SingleSelectPreference<string>(prefKey + "Background", Keys, "l10n.theme.background"));
                subGroup.ChildrenPreferences.Add(new SingleSelectPreference<string>(prefKey + "Foreground", Keys, "l10n.theme.foreground"));

                themeColorGroup.ChildrenPreferences.Add(subGroup);
            }

            var themePreview = new PreferenceCollection("theme.previewGroup", "l10n.theme.preview");
            themePreview.ChildrenPreferences.Add(new LabelPreference("theme.preview"));

            tabTheme.ChildrenPreferences.Add(themeGeneralGroup);
            tabTheme.ChildrenPreferences.Add(themeColorGroup);
            tabTheme.ChildrenPreferences.Add(themePreview);

            return tabTheme;
        }
    }
}