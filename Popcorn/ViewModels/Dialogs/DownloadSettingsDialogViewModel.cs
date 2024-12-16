using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using ltnet;
using NLog;
using OSDB.Models;
using Popcorn.Enums;
using Popcorn.Helpers;
using Popcorn.Models.Episode;
using Popcorn.Models.Media;
using Popcorn.Models.Torrent;
using Popcorn.Services.Subtitles;
using Popcorn.Utils;
using Popcorn.ViewModels.Pages.Home.Settings;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;

namespace Popcorn.ViewModels.Dialogs
{
    public class DownloadSettingsDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The service used to interact with subtitles
        /// </summary>
        private readonly ISubtitlesService _subtitlesService;

        /// <summary>
        /// The media to manage
        /// </summary>
        private IMedia _media;

        /// <summary>
        /// Torrent health, from 0 to 10
        /// </summary>
        private double _torrentHealth;

        /// <summary>
        /// True if subtitles are loading
        /// </summary>
        private bool _loadingSubtitles;

        /// <summary>
        /// The selected torrent
        /// </summary>
        private ITorrent _selectedTorrent;

        private ICommand _downloadCommand;

        private ICommand _cancelCommand;

        private ICommand _loadSubtitlesCommand;

        /// <summary>
        /// The selected media to manage
        /// </summary>
        public IMedia Media
        {
            get => _media;
            set { Set(() => Media, ref _media, value); }
        }

        /// <summary>
        /// The SD label
        /// </summary>
        public string SdLabel { get; set; }

        /// <summary>
        /// The HD label
        /// </summary>
        public string HdLabel { get; set; }

        /// <summary>
        /// Torrent health, from 0 to 10
        /// </summary>
        public double TorrentHealth
        {
            get => _torrentHealth;
            set { Set(() => TorrentHealth, ref _torrentHealth, value); }
        }

        /// <summary>
        /// The selected torrent
        /// </summary>
        public ITorrent SelectedTorrent
        {
            get => _selectedTorrent;
            set { Set(() => SelectedTorrent, ref _selectedTorrent, value); }
        }

        public ICommand CancelCommand
        {
            get => _cancelCommand;
            set => Set(ref _cancelCommand, value);
        }

        public ICommand DownloadCommand
        {
            get => _downloadCommand;
            set => Set(ref _downloadCommand, value);
        }

        public ICommand LoadSubtitlesCommand
        {
            get => _loadSubtitlesCommand;
            set => Set(ref _loadSubtitlesCommand, value);
        }

        private CancellationTokenSource _computeHealthTokenSource;

        /// <summary>
        /// True if subtitles are loading
        /// </summary>
        public bool LoadingSubtitles
        {
            get => _loadingSubtitles;
            set { Set(() => LoadingSubtitles, ref _loadingSubtitles, value); }
        }

        public Action<bool> OnCloseAction { get; set; }

        public DownloadSettingsDialogViewModel(IMedia media, ISubtitlesService subtitlesService)
        {
            _subtitlesService = subtitlesService;
            _computeHealthTokenSource = new CancellationTokenSource();
            Media = media;
            LoadSubtitlesCommand = new RelayCommand(async () => { await LoadSubtitles(Media); });

            SdLabel = media.Type == MediaType.Movie ? "720p" : "480p";
            HdLabel = media.Type == MediaType.Movie ? "1080p" : "720p";

            Messenger.Default.Register<PropertyChangedMessage<bool>>(this, async e =>
            {
                if (e.PropertyName != GetPropertyName(() => Media.WatchInFullHdQuality)) return;

                try
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            _computeHealthTokenSource.Cancel();
                            _computeHealthTokenSource = new CancellationTokenSource();
                            ComputeTorrentHealth();
                            if (SelectedTorrent == null || SelectedTorrent.Peers > 0 && SelectedTorrent.Seeds > 0)
                                return;
                            if (Media is EpisodeShowJson)
                            {
                                string filePath = Path.GetTempFileName();
                                using (var session = new Session())
                                {
                                    var magnet = new MagnetUri();
                                    using (var error = new ErrorCode())
                                    {
                                        var addParams = new AddTorrentParams()
                                        {
                                            save_path = filePath
                                        };
                                        magnet.parse_magnet_uri(SelectedTorrent.Url, addParams, error);
                                        using (var handle = session.add_torrent(addParams))
                                        {
                                            handle.pause();
                                            while (!_computeHealthTokenSource.IsCancellationRequested)
                                            {
                                                try
                                                {
                                                    ComputeTorrentHealth();
                                                    var status = handle.status();
                                                    SelectedTorrent.Peers = status.num_peers;
                                                    SelectedTorrent.Seeds = status.num_seeds;

                                                    await Task.Delay(1000, _computeHealthTokenSource.Token);
                                                }
                                                catch (Exception)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                try
                                {
                                    File.Delete(filePath);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Trace(ex);
                        }
                    }, _computeHealthTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Logger.Trace(ex);
                }
            });

            DownloadCommand = new RelayCommand(() =>
            {
                _computeHealthTokenSource.Cancel();
                OnCloseAction.Invoke(true);
            });

            CancelCommand = new RelayCommand(() =>
            {
                _computeHealthTokenSource.Cancel();
                OnCloseAction.Invoke(false);
            });

            var applicationSettings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
            Media.WatchInFullHdQuality =
                Media.AvailableTorrents.Any(torrent =>
                    (Media.Type == MediaType.Movie && torrent.Quality == "1080p") ||
                    (Media.Type == MediaType.Show && torrent.Quality == "720p")) &&
                Media.AvailableTorrents.Count == 1 ||
                Media.AvailableTorrents.Any(torrent =>
                    (Media.Type == MediaType.Movie && torrent.Quality == "1080p") ||
                    (Media.Type == MediaType.Show && torrent.Quality == "720p")) &&
                applicationSettings.DefaultHdQuality;
        }

        /// <summary>
        /// Compute torrent health
        /// </summary>
        private void ComputeTorrentHealth()
        {
            if (Media.AvailableTorrents == null) return;

            Media.FullHdAvailable = Media.AvailableTorrents.Count > 1 &&
                                    Media.AvailableTorrents.Any(torrent =>
                                        (Media.Type == MediaType.Movie && torrent.Quality == "1080p") ||
                                        (Media.Type == MediaType.Show && torrent.Quality == "720p"));
            SelectedTorrent = Media.WatchInFullHdQuality
                ? Media.AvailableTorrents.FirstOrDefault(torrent =>
                    (Media.Type == MediaType.Movie && torrent.Quality == "1080p") ||
                    (Media.Type == MediaType.Show && torrent.Quality == "720p"))
                : Media.AvailableTorrents.FirstOrDefault(torrent =>
                    (Media.Type == MediaType.Movie && torrent.Quality == "720p") ||
                    (Media.Type == MediaType.Show && torrent.Quality == "480p"));
            if (SelectedTorrent == null)
                SelectedTorrent = Media.AvailableTorrents.Where(torrent => !string.IsNullOrWhiteSpace(torrent.Url))
                    .Aggregate((torrent1, torrent2) => torrent1.Seeds > torrent2.Seeds ? torrent1 : torrent2);
            if (SelectedTorrent != null && SelectedTorrent.Seeds < 4)
            {
                TorrentHealth = 0;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 6)
            {
                TorrentHealth = 1;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 8)
            {
                TorrentHealth = 2;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 10)
            {
                TorrentHealth = 3;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 12)
            {
                TorrentHealth = 4;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 14)
            {
                TorrentHealth = 5;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 16)
            {
                TorrentHealth = 6;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 18)
            {
                TorrentHealth = 7;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 20)
            {
                TorrentHealth = 8;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds < 22)
            {
                TorrentHealth = 9;
            }
            else if (SelectedTorrent != null && SelectedTorrent.Seeds >= 22)
            {
                TorrentHealth = 10;
            }
        }

        /// <summary>
        /// Load the media's subtitles asynchronously
        /// </summary>
        /// <param name="media">The media</param>
        private async Task LoadSubtitles(IMedia media)
        {
            Logger.Info(
                $"Load subtitles for media: {media.Title}");
            Media = media;
            media.AvailableSubtitles = new ObservableCollection<Subtitle>();
            LoadingSubtitles = true;
            try
            {
                var languages = await _subtitlesService.GetSubLanguages();
                if (int.TryParse(new string(media.ImdbId
                    .SkipWhile(x => !char.IsDigit(x))
                    .TakeWhile(char.IsDigit)
                    .ToArray()), out int imdbId))
                {
                    var subtitles = await _subtitlesService.SearchSubtitlesFromImdb(
                        languages.Select(lang => lang.SubLanguageID).Aggregate((a, b) => a + "," + b),
                        imdbId.ToString(), media.Season, media.EpisodeNumber);

                    media.AvailableSubtitles =
                        new ObservableCollection<Subtitle>(subtitles.OrderBy(a => a.LanguageName)
                            .GroupBy(x => x.LanguageName,
                                (k, g) =>
                                    g.Aggregate(
                                        (a, x) =>
                                            (Convert.ToDouble(x.Score, CultureInfo.InvariantCulture) >=
                                             Convert.ToDouble(a.Score, CultureInfo.InvariantCulture))
                                                ? x
                                                : a)));

                    media.AvailableSubtitles.Insert(0, new Subtitle
                    {
                        LanguageName = LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel"),
                    });

                    media.AvailableSubtitles.Insert(1, new Subtitle
                    {
                        LanguageName = LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel"),
                    });

                    var applicationSettings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                    if (!string.IsNullOrEmpty(applicationSettings.DefaultSubtitleLanguage) &&
                        media.AvailableSubtitles.Any(
                            a => a.LanguageName == applicationSettings.DefaultSubtitleLanguage))
                    {
                        media.SelectedSubtitle =
                            media.AvailableSubtitles.FirstOrDefault(
                                a => a.LanguageName == applicationSettings.DefaultSubtitleLanguage);
                    }
                    else
                    {
                        media.SelectedSubtitle = media.AvailableSubtitles.FirstOrDefault();
                    }

                    LoadingSubtitles = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed loading subtitles for : {media.Title}. {ex.Message}");
                LoadingSubtitles = false;
                media.AvailableSubtitles.Insert(0, new Subtitle
                {
                    LanguageName = LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel"),
                });

                media.SelectedSubtitle = media.AvailableSubtitles.FirstOrDefault();
            }
        }
    }
}