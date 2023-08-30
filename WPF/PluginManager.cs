using MultiLyricsProviderInterface;
using PreferenceManagerLibrary.Preferences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WPF
{
    public class PluginManager
    {
        public event ErrorEventHandler OnError;

        public List<IMultiLyricsProvider> LyricsProviders { get; }
        private readonly DirectoryInfo providerDirectory;

        public PluginManager()
        {
            LyricsProviders = new List<IMultiLyricsProvider>();
            providerDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).Directory
                .CreateSubdirectory("Plugins")
                .CreateSubdirectory("MultiLyrics_LyricsProviderPlugins");
        }

        public void LoadPlugins()
        {
            try
            {
                var assemblies = providerDirectory.GetFiles("*.dll");

                foreach (var addInAssembly in assemblies.Select(a => Assembly.LoadFile(a.FullName)))
                {
                    var initType = addInAssembly.ExportedTypes.First(t => t.IsClass && t.Name == "CosturaInitialization");
                    Activator.CreateInstance(initType);

                    foreach (var type in addInAssembly.ExportedTypes.Where(t => t.IsClass && typeof(IMultiLyricsProvider).IsAssignableFrom(t)))
                    {
                        var providerInstance = (IMultiLyricsProvider)Activator.CreateInstance(type);
                        LyricsProviders.Add(providerInstance);
                    }

                }

            }
            catch (Exception e)
            {
                OnError?.Invoke(this, new ErrorEventArgs(e));
            }
        }

        public async Task<IEnumerable<FoundTrack>> SearchLyricsAsync(IMultiLyricsProvider provider, string artist,
            string title, string album, CancellationToken token)
        {
            return await provider.FindLyricsAsync(album, title, artist, token);
        }

        public IEnumerable<PreferenceCollection> BuildPluginPreferences()
        {
            foreach (var provider in LyricsProviders)
            {
                yield return provider.Preferences;
            }
        }
    }
}