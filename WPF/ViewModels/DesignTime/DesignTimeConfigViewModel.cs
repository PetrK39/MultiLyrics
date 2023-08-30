using PreferenceManagerLibrary.Manager;
using PreferenceManagerLibrary.Preferences;
using PreferenceManagerLibrary.PreferenceStorage;
using PreferenceManagerLibrary.Validation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;

namespace WPF.ViewModels.DesignTime
{
#if DEBUG
    public class DesignTimeConfigViewModel : ConfigViewModel
    {
        private enum WindowSizeMode
        {
            SizeModeAuto,
            SizeModeFixed,
            SizeModeMaximized,
            SizeModeRemember
        }

        private enum WindowPositionMode
        {
            PosModeAuto,
            PosModeFixed,
            PosModeCenter,
            PosModeRemember
        }

        private static readonly PreferenceManager prefMan = new PreferenceManager(new XMLPreferenceStorage(new FileInfo("designtime")));

        public DesignTimeConfigViewModel() : base(prefMan)
        {
            prefMan.Preferences.Clear();

            #region General tab

            var gTab = new PreferenceCollection("general", "m_General", "m_General.description");

            //general
            var generalCat = new PreferenceCollection("general", "m_General");

            var lang = new SingleSelectPreference<CultureInfo>("general.language", GetAvailableLanguages(), "general.language", defaultValue: GetDefaultCulture());

            generalCat.ChildrenPreferences.Add(lang);
            gTab.ChildrenPreferences.Add(generalCat);

            //window
            var winCat = new PreferenceCollection("general.window", "m_General.Window");

            //  pos
            var posGroup = new PreferenceCollection("general.window.position.prefGroup", "general.window.position.prefGroup");

            var posModesAsStrings = Enum.GetValues(typeof(WindowPositionMode)).Cast<WindowPositionMode>().Select(i => i.ToString()).ToList();
            var lvPosM = new SingleSelectPreference<string>("general.window.positionMode", posModesAsStrings, "general.window.positionMode", defaultValue: posModesAsStrings.First());

            var lvPosW = new InputPreference("general.window.position.width", "general.window.position.width", valueValidator: new NumberValidator<double>().AddGreaterThan(0));
            var lvPosH = new InputPreference("general.window.position.height", "general.window.position.height", valueValidator: new NumberValidator<double>().AddGreaterThan(0));

            lvPosM.PropertyChanged += (s, e) =>
            {
                switch (lvPosM.Value)
                {
                    case nameof(WindowPositionMode.PosModeCenter):
                    case nameof(WindowPositionMode.PosModeAuto):
                        lvPosW.IsEnabled = false;
                        lvPosH.IsEnabled = false;
                        break;

                    default:
                        lvPosW.IsEnabled = true;
                        lvPosH.IsEnabled = true;
                        break;
                }
            };

            posGroup.ChildrenPreferences.Add(lvPosW);
            posGroup.ChildrenPreferences.Add(lvPosH);

            //  size
            var sizeGroup = new PreferenceCollection("general.window.size.prefGroup", "", "general.window.size.prefGroup");

            var sizeModesAsStrings = Enum.GetValues(typeof(WindowSizeMode)).Cast<WindowSizeMode>().Select(i => i.ToString()).ToList();
            var lvSizeM = new SingleSelectPreference<string>("general.window.sizeMode", sizeModesAsStrings, "general.window.sizeMode", defaultValue: sizeModesAsStrings.First());

            var lvSizeW = new InputPreference("general.window.size.width", "general.window.size.width", valueValidator: new NumberValidator<double>().AddGreaterThan(0));
            var lvSizeH = new InputPreference("general.window.size.height", "general.window.size.height", valueValidator: new NumberValidator<double>().AddGreaterThan(0));

            lvSizeM.PropertyChanged += (s, e) =>
            {
                switch (lvSizeM.Value)
                {
                    case nameof(WindowSizeMode.SizeModeMaximized):
                    case nameof(WindowSizeMode.SizeModeAuto):
                        lvSizeW.IsEnabled = false;
                        lvSizeH.IsEnabled = false;
                        break;

                    default:
                        lvSizeW.IsEnabled = true;
                        lvSizeH.IsEnabled = true;
                        break;
                }
            };

            sizeGroup.ChildrenPreferences.Add(lvSizeW);
            sizeGroup.ChildrenPreferences.Add(lvSizeH);

            winCat.ChildrenPreferences.Add(lvPosM);
            winCat.ChildrenPreferences.Add(posGroup);

            winCat.ChildrenPreferences.Add(lvSizeM);
            winCat.ChildrenPreferences.Add(sizeGroup);

            gTab.ChildrenPreferences.Add(winCat);

            prefMan.Preferences.Add(gTab);

            #endregion General tab

            #region Theme tab

            var tabTheme = new PreferenceCollection("theme", "m_Theme", "m_Theme.description");

            var themeGeneralGroup = new PreferenceCollection("theme.general", "m_General");

            var themeEn = new BoolPreference("theme.general.enabled", "theme.general.enabled.description", defaultValue: true);
            themeGeneralGroup.ChildrenPreferences.Add(themeEn);

            var themeColorGroup = new PreferenceCollection("theme.colors", "m_Theme.colors");

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
                var subGroup = new PreferenceCollection(prefKey + ".description", "", prefKey + ".prefGroup");

                subGroup.ChildrenPreferences.Add(new SingleSelectPreference<string>(prefKey + "Background", Enumerable.Empty<string>(), "m_Background"));
                subGroup.ChildrenPreferences.Add(new SingleSelectPreference<string>(prefKey + "Foreground", Enumerable.Empty<string>(), "m_Foreground"));

                themeColorGroup.ChildrenPreferences.Add(subGroup);
            }

            var themePreview = new PreferenceCollection("theme.previewGroup", "m_Theme.preview");
            themePreview.ChildrenPreferences.Add(new LabelPreference("theme.preview"));

            tabTheme.ChildrenPreferences.Add(themeGeneralGroup);
            tabTheme.ChildrenPreferences.Add(themeColorGroup);
            tabTheme.ChildrenPreferences.Add(themePreview);
            prefMan.Preferences.Add(tabTheme);

            #endregion Theme tab
        }

        private static IEnumerable<CultureInfo> GetAvailableLanguages()
        {
            var result = new List<CultureInfo>();

            var rm = new ResourceManager(typeof(Properties.Resources));

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (var culture in cultures)
            {
                try
                {
                    if (culture.Equals(CultureInfo.InvariantCulture)) continue;

                    var rs = rm.GetResourceSet(culture, true, false);

                    if (rs != null) result.Add(culture);
                }
                catch (CultureNotFoundException) { }
            }
            return result;
        }

        private static CultureInfo GetDefaultCulture()
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture;

            return GetAvailableLanguages().Contains(currentCulture) ? currentCulture : CultureInfo.CreateSpecificCulture("en");
        }
    }
#endif
}