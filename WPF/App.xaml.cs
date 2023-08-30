using Infralution.Localization.Wpf;
using PreferenceManagerLibrary.Manager;
using PreferenceManagerLibrary.Preferences;
using PreferenceManagerLibrary.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Xml.Serialization;
using WPF.Utils;
using WPF.ViewModels;
using WPF.Views;

namespace WPF
{
    public partial class App
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

        public new static App Current => Application.Current as App;
        public static ThemeManager ThemeManager => Current.themeManager;
        public static PluginManager PluginManager => Current.pluginManager;

        private PluginManager pluginManager;
        private ThemeManager themeManager;
        private PreferenceManager preferenceManager;

        private IntPtr mainHWnd;

        public void Initialize(IntPtr mainHWnd, PreferenceManager preferenceManager, IEnumerable<KeyValuePair<string, int>> skin)
        {
            this.mainHWnd = mainHWnd;
            this.preferenceManager = preferenceManager;

            themeManager = new ThemeManager(preferenceManager, skin);

            preferenceManager.Preferences.Add(BuildGlobalPreferences());
            preferenceManager.Preferences.Add(themeManager.BuildThemePreferences());

            pluginManager = new PluginManager();
            pluginManager.OnError += Application_OnError;
            pluginManager.LoadPlugins();

            foreach (var prefTab in pluginManager.BuildPluginPreferences())
            {
                preferenceManager.Preferences.Add(prefTab);
            }

            preferenceManager.LoadPreferences();
        }
        public void UpdateSkin(IEnumerable<KeyValuePair<string, int>> skin)
        {
            Dispatcher.Invoke(() => {
                    ThemeManager.UpdateSkin(skin);
            });
        }

        public string SearchLyrics(string artist, string title, string album)
        {
            LyricsSearchViewModel lsvm = null;
            LyricsSearchView lsv = null;

            Dispatcher.Invoke(() =>
            {
                preferenceManager.LoadPreferences();

                lsvm = new LyricsSearchViewModel();
                lsv = new LyricsSearchView(lsvm);

                WindowInteropHelper wih = new WindowInteropHelper(lsv)
                {
                    Owner = mainHWnd
                };

                lsv.Loaded += LyricsSearchView_RestoreSizePos;
                lsv.Closing += LyricsSearchView_SaveSizePos;
                lsv.Show();

                lsvm.Artist = artist;
                lsvm.Title = title;
                lsvm.Album = album;

                lsvm.SearchCommand.Execute(null);
            });

            lsvm.OnError += (s, e) =>
            {
                var resx = new LocalizedResxConverter();
                var resxTitle = resx.Convert("l10n.retryTitle", typeof(string), null, CultureInfo.CurrentUICulture) as string;

                e.IsRetry = MessageBox.Show(e.Exception.Message, resxTitle, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes;
            };

            var result = lsvm.WaitForResult();

            return result;
        }
        public void ShowConfig()
        {
            ConfigViewModel cvm = null;
            ConfigView cv = null;

            Dispatcher.Invoke(() =>
            {
                preferenceManager.LoadPreferences();
                cvm = new ConfigViewModel(preferenceManager);
                cv = new ConfigView(cvm);
                cv.Show();
            });

            cvm.WaitForResult();
        }

        private void LyricsSearchView_SaveSizePos(object sender, CancelEventArgs e)
        {
            var win = (LyricsSearchView)sender;

            var sizeMode = (WindowSizeMode)Enum.Parse(typeof(WindowSizeMode),
                ((SingleSelectPreference<string>)preferenceManager.FindPreferenceByKey("general.window.sizeMode")).Value);

            var sizeWidth = (InputPreference)preferenceManager.FindPreferenceByKey("general.window.size.width");
            var sizeHeight = (InputPreference)preferenceManager.FindPreferenceByKey("general.window.size.height");

            if (sizeMode == WindowSizeMode.SizeModeRemember)
            {
                preferenceManager.BeginEdit();
                sizeWidth.EditableValue = win.Width.ToString();
                sizeHeight.EditableValue = win.Height.ToString();
                preferenceManager.EndEdit();
            }

            var positionMode = (WindowPositionMode)Enum.Parse(typeof(WindowPositionMode),
                ((SingleSelectPreference<string>)preferenceManager.FindPreferenceByKey("general.window.positionMode")).Value);

            var posLeft = (InputPreference)preferenceManager.FindPreferenceByKey("general.window.position.width");
            var posTop = (InputPreference)preferenceManager.FindPreferenceByKey("general.window.position.height");

            if (positionMode == WindowPositionMode.PosModeRemember)
            {
                preferenceManager.BeginEdit();
                posLeft.EditableValue = win.Left.ToString();
                posTop.EditableValue = win.Top.ToString();
                preferenceManager.EndEdit();
                preferenceManager.SavePreferences();
            }
        }
        private void LyricsSearchView_RestoreSizePos(object sender, RoutedEventArgs e)
        {
            var win = (LyricsSearchView)sender;

            var sizeMode = (WindowSizeMode)Enum.Parse(typeof(WindowSizeMode),
                ((SingleSelectPreference<string>)preferenceManager.FindPreferenceByKey("general.window.sizeMode")).Value);

            double.TryParse(((InputPreference)preferenceManager.FindPreferenceByKey("general.window.size.width")).Value, out var sizeWidth);
            double.TryParse(((InputPreference)preferenceManager.FindPreferenceByKey("general.window.size.height")).Value, out var sizeHeight);

            switch (sizeMode)
            {
                case WindowSizeMode.SizeModeAuto:
                    win.SizeToContent = SizeToContent.Width;
                    break;

                case WindowSizeMode.SizeModeFixed:
                case WindowSizeMode.SizeModeRemember:
                    win.Width = sizeWidth;
                    win.Height = sizeHeight;
                    break;

                case WindowSizeMode.SizeModeMaximized:
                    win.WindowState = WindowState.Maximized;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var positionMode = (WindowPositionMode)Enum.Parse(typeof(WindowPositionMode),
                ((SingleSelectPreference<string>)preferenceManager.FindPreferenceByKey("general.window.positionMode")).Value);

            double.TryParse(((InputPreference)preferenceManager.FindPreferenceByKey("general.window.position.width")).Value, out var posLeft);
            double.TryParse(((InputPreference)preferenceManager.FindPreferenceByKey("general.window.position.height")).Value, out var posTop);

            switch (positionMode)
            {
                case WindowPositionMode.PosModeAuto:
                    break;

                case WindowPositionMode.PosModeFixed:
                case WindowPositionMode.PosModeRemember:
                    win.Left = posLeft;
                    win.Top = posTop;
                    break;

                case WindowPositionMode.PosModeCenter:
                    win.Left = (SystemParameters.PrimaryScreenWidth / 2) - (win.Width / 2);
                    win.Top = (SystemParameters.PrimaryScreenHeight / 2) - (win.Height / 2);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Application_OnError(object sender, ErrorEventArgs e)
        {
            var resx = new LocalizedResxConverter();
            var resxTitle = resx.Convert("l10n.errorTitle", typeof(string), null, CultureInfo.CurrentUICulture) as string;

            MessageBox.Show(e.GetException().Message, resxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private PreferenceCollection BuildGlobalPreferences()
        {
            var gTab = new PreferenceCollection("general", "l10n.general", "l10n.general.description");

            //general
            var generalCat = new PreferenceCollection("general", "l10n.general");

            var lang = new SingleSelectPreference<CultureInfo>("general.language", GetAvailableLanguages(), "l10n.general.language", defaultValue: GetDefaultCulture());
            lang.PropertyChanged += (s, e) => ApplyCulture();

            generalCat.ChildrenPreferences.Add(lang);
            gTab.ChildrenPreferences.Add(generalCat);

            //window
            var winCat = new PreferenceCollection("general.window", "l10n.general.window");

            //  pos
            var posGroup = new PreferenceCollection("general.window.position.prefGroup", "l10n.general.window.position.prefGroup");

            var posModesAsStrings = Enum.GetValues(typeof(WindowPositionMode)).Cast<WindowPositionMode>().Select(i => i.ToString()).ToList();
            var lvPosM = new SingleSelectPreference<string>("general.window.positionMode", posModesAsStrings, "l10n.general.window.positionMode", defaultValue: posModesAsStrings.First());

            var lvPosW = new InputPreference("general.window.position.width", "l10n.general.window.position.width", valueValidator: new NumberValidator<double>().AddGreaterThan(0));
            var lvPosH = new InputPreference("general.window.position.height", "l10n.general.window.position.height", valueValidator: new NumberValidator<double>().AddGreaterThan(0));

            lvPosM.PropertyChanged += (s, e) =>
            {
                switch (lvPosM.EditableValue)
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
            var sizeGroup = new PreferenceCollection("general.window.size.prefGroup", "l10n.general.window.size.prefGroup");

            var sizeModesAsStrings = Enum.GetValues(typeof(WindowSizeMode)).Cast<WindowSizeMode>().Select(i => i.ToString()).ToList();
            var lvSizeM = new SingleSelectPreference<string>("general.window.sizeMode", sizeModesAsStrings, "l10n.general.window.sizeMode", defaultValue: sizeModesAsStrings.First());

            var lvSizeW = new InputPreference("general.window.size.width", "l10n.general.window.size.width", valueValidator: new NumberValidator<double>().AddGreaterThan(0));
            var lvSizeH = new InputPreference("general.window.size.height", "l10n.general.window.size.height", valueValidator: new NumberValidator<double>().AddGreaterThan(0));

            lvSizeM.PropertyChanged += (s, e) =>
            {
                switch (lvSizeM.EditableValue)
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

            return gTab;
        }

        private void ApplyCulture()
        {
            CultureManager.UICulture = ((SingleSelectPreference<CultureInfo>)preferenceManager.FindPreferenceByKey("general.language")).Value;
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
                catch (CultureNotFoundException)
                {
                }
            }
            return result;
        }
        private static CultureInfo GetDefaultCulture()
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture;

            return GetAvailableLanguages().Contains(currentCulture) ? 
                currentCulture : CultureInfo.CreateSpecificCulture("en");
        }
    }
}