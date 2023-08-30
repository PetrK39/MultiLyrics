using System;
using System.Collections.Generic;
using System.Threading;
using WPF;
using System.Linq;
using PreferenceManagerLibrary.Manager;
using PreferenceManagerLibrary.PreferenceStorage;
using System.IO;
using System.Windows;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private readonly PluginInfo about = new PluginInfo();

        private App host;
        private Thread hostThread;
        private EventWaitHandle hostReadyWaitHandle;

        private PreferenceManager preferenceManager;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "MultiLyrics";
            about.Description = "Alternative lyrics provider for cases when there's multiple languages / multiple songs with the same title";
            about.Author = "PetrK39";
            about.TargetApplication = "";   //  the name of a Plugin Storage device or panel header for a dockable panel
            about.Type = PluginType.LyricsRetrieval;
            about.VersionMajor = 1;  // your plugin version
            about.VersionMinor = 0;
            about.Revision = 1;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            about.ConfigurationPanelHeight = 0;   // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function
            
            preferenceManager = new PreferenceManager(new XMLPreferenceStorage(Path.Combine(mbApiInterface.Setting_GetPersistentStoragePath(), "MultiLyrics", "preferences.xml")));
            
            return about;
        }
        public bool Configure(IntPtr panelHandle)
        {
            host.UpdateSkin(GetSkin());
            host.ShowConfig();
            return true;
        }
        public void SaveSettings()
        {
            // Unused API method intentionally left empty.
        }
        public void Close(PluginCloseReason reason)
        {
            host?.Dispatcher.Invoke(() => host.Shutdown());
            hostThread?.Abort();

            host = null;
            hostThread = null;
            preferenceManager = null;
        }
        public void Uninstall()
        {
            // TODO: clear config, clear providers
        }
        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                    hostReadyWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

                    hostThread = new Thread(() =>
                    {
                        // help needed: can't avoid AppDomain exception on plugin restart
                        host = new App() { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
                        host.Startup += (s, e) => hostReadyWaitHandle.Set();

                        host.Initialize(mbApiInterface.MB_GetWindowHandle(), preferenceManager, GetSkin());
                        host.Run();
                    });

                    hostThread.SetApartmentState(ApartmentState.STA);
                    hostThread.Start();
                    break;
            }
        }
        public string[] GetProviders()
        {
            return new[] { "MultiLyrics" };
        }

        public string RetrieveLyrics(string sourceFileUrl, string artist, string trackTitle, string album, bool synchronisedPreferred, string provider)
        {
            hostReadyWaitHandle.WaitOne();
            host.UpdateSkin(GetSkin());
            return host.SearchLyrics(artist, trackTitle, album);
        }
        private IEnumerable<KeyValuePair<string, int>> GetSkin()
        {
            return from SkinElement element        in Enum.GetValues(typeof(SkinElement))
                   from ElementState state         in Enum.GetValues(typeof(ElementState))
                   from ElementComponent component in Enum.GetValues(typeof(ElementComponent))

                   let colorName = $"{element}.{state}.{component}"
                   let colorInt = mbApiInterface.Setting_GetSkinElementColour(element, state, component)

                   select new KeyValuePair<string, int>(colorName, colorInt);
        }
    }
}