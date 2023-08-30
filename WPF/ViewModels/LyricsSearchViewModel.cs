using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MultiLyricsProviderInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WPF.Utils;

namespace WPF.ViewModels
{
    public class LyricsSearchViewModel : ObservableObject
    {
        public event ExceptionRetryEventHandler OnError;
        public event EventHandler OnWindowCloseRequest;

        public IEnumerable<IMultiLyricsProvider> AvailableProviders { get; private set; }
        public IMultiLyricsProvider CurrentProvider { get; set; }
        public ObservableCollection<FoundTrack> SearchResults { get; private set; }
        public ObservableCollection<Dictionary<string, string>> SearchResultsProperties { get; private set; }

        public Dictionary<string, string> SelectedTrackProperties
        {
            get => selectedTrackProperties;
            set => SetProperty(ref selectedTrackProperties, value);
        }
        public string SelectedTrackLyrics
        {
            get => selectedTrackLyrics;
            set
            {
                SetProperty(ref selectedTrackLyrics, value);
                ApplyCommand.NotifyCanExecuteChanged();
            }
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        public string Album
        {
            get => album;
            set => SetProperty(ref album, value);
        }
        public string Artist
        {
            get => artist;
            set => SetProperty(ref artist, value);
        }

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        public RelayCommand ApplyCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public AsyncRelayCommand SearchCommand { get; private set; }
        public RelayCommand SearchCancelCommand { get; private set; }

        private readonly EventWaitHandle @event;
        private CancellationTokenSource tokenSource;

        private Dictionary<string, string> selectedTrackProperties;
        private string selectedTrackLyrics;
        private string title;
        private string album;
        private string artist;
        private bool isBusy;

        private bool isCanceled = false;

        public LyricsSearchViewModel()
        {
            @event = new EventWaitHandle(false, EventResetMode.ManualReset);

            AvailableProviders = App.PluginManager.LyricsProviders;
            CurrentProvider = AvailableProviders.FirstOrDefault();

            SearchResults = new ObservableCollection<FoundTrack>();
            SearchResultsProperties = new ObservableCollection<Dictionary<string, string>>();

            PropertyChanged += LyricsSearchViewModel_PropertyChanged;

            CancelCommand = new RelayCommand(Cancel);
            ApplyCommand = new RelayCommand(Apply, ApplyCanExecute);
            SearchCommand = new AsyncRelayCommand(Search, SearchCanExecute);
            SearchCancelCommand = new RelayCommand(SearchCancel, SearchCancelCanExecute);
        }

        private async Task SearchAsync(CancellationToken token)
        {
            IsBusy = true;

            SearchResults.Clear();
            SearchResultsProperties.Clear();

            if (CurrentProvider is null)
            {
                @event.Set();
                isBusy = false;
                return;
            }

            bool retry = false;

            do
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    var searchResult = await App.PluginManager.SearchLyricsAsync(CurrentProvider, Album, Title, Artist, token);

                    if (searchResult is null) break;

                    foreach (var item in searchResult)
                    {
                        token.ThrowIfCancellationRequested();

                        SearchResults.Add(item);
                        SearchResultsProperties.Add(item.TrackProperties);
                    }

                    retry = false;
                }
                catch (OperationCanceledException)
                {
                    // that's ok
                }
                catch (HttpRequestException e) when (e.InnerException is OperationCanceledException)
                {
                    //same
                }
                catch (Exception e)
                {
                    var eventArgs = new ExceptionRetryEventArgs(e);
                    OnError?.Invoke(this, eventArgs);
                    retry = eventArgs.IsRetry;
                }
            } while (retry);

            IsBusy = false;
            OnPropertyChanged(nameof(SearchResults));
            OnPropertyChanged(nameof(SearchResultsProperties));
        }

        private void LyricsSearchViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedTrackProperties))
                SelectedTrackLyrics = SelectedTrackProperties is null
                    ? null
                    : SearchResults.Single(t => SelectedTrackProperties == t.TrackProperties).Lyrics;
        }

        public string WaitForResult()
        {
            @event.WaitOne();
            return isCanceled ? "" : SelectedTrackLyrics;
        }

        private void Cancel()
        {
            isCanceled = true;
            OnWindowCloseRequest?.Invoke(this, EventArgs.Empty);
        }
        private void Apply()
        {
            @event.Set();
            OnWindowCloseRequest?.Invoke(this, EventArgs.Empty);
        }
        private bool ApplyCanExecute()
        {
            return !string.IsNullOrWhiteSpace(SelectedTrackLyrics);
        }
        private async Task Search()
        {
            tokenSource = new CancellationTokenSource();
            await SearchAsync(tokenSource.Token);
        }
        private bool SearchCanExecute()
        {
            return !IsBusy;
        }
        private void SearchCancel()
        {
            tokenSource.Cancel();
        }
        private bool SearchCancelCanExecute()
        {
            return IsBusy;
        }
    }
}