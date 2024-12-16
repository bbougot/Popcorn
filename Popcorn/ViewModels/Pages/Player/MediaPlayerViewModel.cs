using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Extensions;
using Popcorn.Messaging;
using Popcorn.Models.Bandwidth;
using Popcorn.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Popcorn.Helpers;
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using OSDB.Models;
using Popcorn.Services.Subtitles;
using Popcorn.Events;
using Popcorn.Services.Cache;
using Popcorn.ViewModels.Pages.Home.Movie.Details;
using System.Threading;
using Popcorn.Enums;

namespace Popcorn.ViewModels.Pages.Player
{
    /// <summary>
    /// Manage media player
    /// </summary>
    public class MediaPlayerViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// <see cref="ShowSubtitleButton"/>
        /// </summary>
        private bool _showSubtitleButton;

        /// <summary>
        /// <see cref="Volume"/>
        /// </summary>
        private double _volume;

        /// <summary>
        /// <see cref="PlayerTime"/>
        /// </summary>
        private double _playerTime;

        /// <summary>
        /// <see cref="CurrentSubtitle"/>
        /// </summary>
        private Subtitle _currentSubtitle;

        /// <summary>
        /// <see cref="IsSeeking"/>
        /// </summary>
        private bool _isSeeking;

        /// <summary>
        /// <see cref="MediaLength"/>
        /// </summary>
        private double _mediaLength;

        /// <summary>
        /// <see cref="IsDragging"/>
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// Media action to execute when media has been stopped
        /// </summary>
        private readonly Action _mediaStoppedAction;

        /// <summary>
        /// <see cref="MediaType"/>
        /// </summary>
        private MediaType _mediaType;

        /// <summary>
        /// <see cref="IsSubtitleChosen"/>
        /// </summary>
        private bool _isSubtitleChosen;

        /// <summary>
        /// Subtitles
        /// </summary>
        private ObservableCollection<Subtitle> _subtitles;

        /// <summary>
        /// The cache service
        /// </summary>
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Subtitle service
        /// </summary>
        private readonly ISubtitlesService _subtitlesService;

        /// <summary>
        /// The playing progress
        /// </summary>
        private readonly IProgress<double> _playingProgress;

        private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the MediaPlayerViewModel class.
        /// </summary>
        /// <param name="subtitlesService"></param>
        /// <param name="cacheService">Caching service</param>
        /// <param name="mediaPath">Media path</param>
        /// <param name="mediaName">Media name</param>
        /// <param name="type">Media type</param>
        /// <param name="mediaStoppedAction">Media action to execute when media has been stopped</param>
        /// <param name="playingProgress">Media playing progress</param>
        /// <param name="bufferProgress">The buffer progress</param>
        /// <param name="bandwidthRate">THe bandwidth rate</param>
        /// <param name="currentSubtitle">Subtitle</param>
        /// <param name="subtitles">Subtitles</param>
        public MediaPlayerViewModel(ISubtitlesService subtitlesService,
            ICacheService cacheService,
            string mediaPath,
            string mediaName, MediaType type, Action mediaStoppedAction, IProgress<double> playingProgress = null,
            Progress<double> bufferProgress = null,
            Progress<BandwidthRate> bandwidthRate = null, Subtitle currentSubtitle = null,
            IEnumerable<Subtitle> subtitles = null)
        {
            Logger.Info(
                $"Loading media : {mediaPath}.");
            RegisterCommands();
            _subtitlesService = subtitlesService;
            _cacheService = cacheService;
            MediaPath = mediaPath;
            MediaName = mediaName;
            MediaType = type;
            _mediaStoppedAction = mediaStoppedAction;
            BufferProgress = bufferProgress;
            BandwidthRate = bandwidthRate;
            ShowSubtitleButton = MediaType != MediaType.Trailer;
            Volume = 1d;
            _playingProgress = playingProgress;
            _subtitles = new ObservableCollection<Subtitle>();
            if (subtitles != null)
            {
                _subtitles = new ObservableCollection<Subtitle>(subtitles);
            }

            if (currentSubtitle != null && currentSubtitle.LanguageName !=
                LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel") &&
                !string.IsNullOrEmpty(currentSubtitle.FilePath))
            {
                CurrentSubtitle = currentSubtitle;
            }
        }

        /// <summary>
        /// The media path
        /// </summary>
        public readonly string MediaPath;

        /// <summary>
        /// The media name
        /// </summary>
        public readonly string MediaName;

        /// <summary>
        /// The buffer progress
        /// </summary>
        public readonly Progress<double> BufferProgress;

        /// <summary>
        /// The download rate
        /// </summary>
        public readonly Progress<BandwidthRate> BandwidthRate;

        /// <summary>
        /// Event fired on stopped playing the media
        /// </summary>
        public event EventHandler<EventArgs> StoppedMedia;

        /// <summary>
        /// Event fired when subtitle changed
        /// </summary>
        public event EventHandler<SubtitleChangedEventArgs> SubtitleChanged;

        /// <summary>
        /// Command used to stop playing the media
        /// </summary>
        public ICommand StopPlayingMediaCommand { get; set; }

        /// <summary>
        /// The media duration in seconds
        /// </summary>
        public double MediaDuration { get; set; }

        /// <summary>
        /// True if user is dragging the media player slider
        /// </summary>
        public bool IsDragging
        {
            get => _isDragging;
            set => Set(ref _isDragging, value);
        }

        /// <summary>
        /// True if media is seeking
        /// </summary>
        public bool IsSeeking
        {
            get => _isSeeking;
            set => Set(ref _isSeeking, value);
        }

        /// <summary>
        /// The media type
        /// </summary>
        public MediaType MediaType
        {
            get => _mediaType;
            set => Set(ref _mediaType, value);
        }

        /// <summary>
        /// Show subtitle button
        /// </summary>
        public bool ShowSubtitleButton
        {
            get { return _showSubtitleButton; }
            set { Set(ref _showSubtitleButton, value); }
        }

        /// <summary>
        /// True if subtitle has been chosen
        /// </summary>
        public bool IsSubtitleChosen
        {
            get => _isSubtitleChosen;
            set => Set(ref _isSubtitleChosen, value);
        }

        /// <summary>
        /// Current subtitle for the media
        /// </summary>
        public ObservableCollection<Subtitle> Subtitles
        {
            get => _subtitles;
            set => Set(ref _subtitles, value);
        }

        /// <summary>
        /// Current subtitle for the media
        /// </summary>
        public Subtitle CurrentSubtitle
        {
            get => _currentSubtitle;
            set
            {
                DispatcherHelper.CheckBeginInvokeOnUI(async () =>
                {
                    await ChangeSubtitle(value);
                    Set(ref _currentSubtitle, value);
                });
            }
        }

        /// <summary>
        /// The media length in seconds
        /// </summary>
        public double MediaLength
        {
            get => _mediaLength;
            set => Set(ref _mediaLength, value);
        }

        /// <summary>
        /// The media player progress in seconds
        /// </summary>
        public double PlayerTime
        {
            get => _playerTime;
            set
            {
                Set(ref _playerTime, value);
                _playingProgress?.Report(value / MediaLength);
            }
        }

        /// <summary>
        /// Get or set the media volume between 0 and 1
        /// </summary>
        public double Volume
        {
            get => _volume;
            set { Set(ref _volume, value); }
        }

        /// <summary>
        /// Register commands
        /// </summary>
        private void RegisterCommands()
        {
            StopPlayingMediaCommand =
                new RelayCommand(() =>
                {
                    try
                    {
                        _mediaStoppedAction?.Invoke();
                        OnStoppedMedia(new EventArgs());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                });
        }

        public async Task ChangeSubtitle(Subtitle subtitle)
        {
            if (Semaphore.CurrentCount == 0) return;
            try
            {
                await Semaphore.WaitAsync();
                if (subtitle.LanguageName !=
                    LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel") &&
                    subtitle.LanguageName != LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel"))
                {
                    if (CurrentSubtitle != null &&
                        CurrentSubtitle.ISO639 == subtitle.ISO639)
                    {
                        return;
                    }

                    var path = Path.Combine(_cacheService.Subtitles + subtitle.IDMovieImdb);
                    Directory.CreateDirectory(path);
                    var subtitlePath = await
                        _subtitlesService.DownloadSubtitleToPath(path, subtitle);
                    subtitle.FilePath = _subtitlesService.LoadCaptions(subtitlePath);
                    OnSubtitleChosen(new SubtitleChangedEventArgs(subtitle));
                }
                else if (subtitle.LanguageName == LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel"))
                {
                    var subMessage = new ShowCustomSubtitleMessage();
                    await Messenger.Default.SendAsync(subMessage);
                    if (!subMessage.Error && !string.IsNullOrEmpty(subMessage.FileName))
                    {
                        subtitle.FilePath = subMessage.FileName;
                        OnSubtitleChosen(
                            new SubtitleChangedEventArgs(subtitle));
                    }
                }
                else if (subtitle.LanguageName ==
                         LocalizationProviderHelper.GetLocalizedValue<string>("NoneLabel"))
                {
                    OnSubtitleChosen(new SubtitleChangedEventArgs(subtitle));
                }
            }
            catch (Exception ex)
            {
                Logger.Trace(ex);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        /// <summary>
        /// When a media has been ended, invoke the <see cref="StopPlayingMediaCommand"/>
        /// </summary>
        public void MediaEnded()
        {
            if (MediaType == MediaType.Movie)
            {
                try
                {
                    var movieDetails = SimpleIoc.Default.GetInstance<MovieDetailsViewModel>();
                    movieDetails.SetWatchedMovieCommand.Execute(true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            StopPlayingMediaCommand.Execute(null);
        }

        /// <summary>
        /// Fire OnSubtitleChosen event
        /// </summary>
        /// <param name="e">Event args</param>
        private void OnSubtitleChosen(SubtitleChangedEventArgs e)
        {
            Logger.Debug(
                "Subtitle chosen");

            IsSubtitleChosen = !string.IsNullOrEmpty(e.Subtitle.FilePath);
            var handler = SubtitleChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Fire StoppedMedia event
        /// </summary>
        /// <param name="e">Event args</param>
        private void OnStoppedMedia(EventArgs e)
        {
            Logger.Debug(
                "Stop playing a media");

            var handler = StoppedMedia;
            handler?.Invoke(this, e);
        }
    }
}