using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using OSDB.Models;
using Popcorn.Enums;
using Popcorn.Extensions;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Cast;
using Popcorn.Models.Torrent.Movie;
using Popcorn.Models.Media;
using Popcorn.Models.Torrent;

namespace Popcorn.Models.Movie
{
    [DataContract]
    public class MovieJson : ObservableObject, IMediaFile, IMovie, IMedia
    {
        private List<CastJson> _cast;

        private ObservableCollection<Subtitle> _availableSubtitles =
            new ObservableCollection<Subtitle>();

        private string _dateUploaded;
        private string _posterImage;
        private int _dateUploadedUnix;
        private string _descriptionFull;
        private string _descriptionIntro;
        private int _downloadCount;
        private string _filePath;
        private bool _fullHdAvailable;
        private List<string> _genres;
        private bool _hasBeenSeen;
        private string _backgroundImage;
        private string _imdbId;
        private bool _isFavorite;
        private string _language;
        private int _likeCount;
        private string _mpaRating;
        private double _rating;
        private int _runtime;
        private Subtitle _selectedSubtitle;
        private string _title;
        private string _titleLong;
        private string _slug;
        private List<TorrentJson> _torrents;
        private string _url;
        private bool _watchInFullHdQuality;
        private int _year;
        private string _ytTrailerCode;
        private List<string> _similars;
        private int _tmdbId;

        [DataMember(Name = "url")]
        public string Url
        {
            get => _url;
            set { Set(() => Url, ref _url, value); }
        }

        [DataMember(Name = "imdbCode")]
        public string ImdbId
        {
            get => _imdbId;
            set { Set(() => ImdbId, ref _imdbId, value); }
        }

        [DataMember(Name = "title")]
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
        }

        [DataMember(Name = "titleLong")]
        public string TitleLong
        {
            get => _titleLong;
            set { Set(() => TitleLong, ref _titleLong, value); }
        }

        [DataMember(Name = "slug")]
        public string Slug
        {
            get => _slug;
            set { Set(() => Slug, ref _slug, value); }
        }

        [DataMember(Name = "year")]
        public int Year
        {
            get => _year;
            set { Set(() => Year, ref _year, value); }
        }

        [DataMember(Name = "rating")]
        public double Rating
        {
            get => _rating;
            set { Set(() => Rating, ref _rating, value); }
        }

        [DataMember(Name = "runtime")]
        public int Runtime
        {
            get => _runtime;
            set { Set(() => Runtime, ref _runtime, value); }
        }

        [DataMember(Name = "genres")]
        public List<string> Genres
        {
            get => _genres;
            set { Set(() => Genres, ref _genres, value); }
        }

        [DataMember(Name = "language")]
        public string Language
        {
            get => _language;
            set { Set(() => Language, ref _language, value); }
        }

        [DataMember(Name = "mpaRating")]
        public string MpaRating
        {
            get => _mpaRating;
            set { Set(() => MpaRating, ref _mpaRating, value); }
        }

        [DataMember(Name = "downloadCount")]
        public int DownloadCount
        {
            get => _downloadCount;
            set { Set(() => DownloadCount, ref _downloadCount, value); }
        }

        [DataMember(Name = "likeCount")]
        public int LikeCount
        {
            get => _likeCount;
            set { Set(() => LikeCount, ref _likeCount, value); }
        }

        [DataMember(Name = "descriptionIntro")]
        public string DescriptionIntro
        {
            get => _descriptionIntro;
            set { Set(() => DescriptionIntro, ref _descriptionIntro, value); }
        }

        [DataMember(Name = "descriptionFull")]
        public string DescriptionFull
        {
            get => _descriptionFull;
            set { Set(() => DescriptionFull, ref _descriptionFull, value); }
        }

        [DataMember(Name = "ytTrailerCode")]
        public string YtTrailerCode
        {
            get => _ytTrailerCode;
            set { Set(() => YtTrailerCode, ref _ytTrailerCode, value); }
        }

        [DataMember(Name = "cast")]
        public List<CastJson> Cast
        {
            get => _cast;
            set { Set(() => Cast, ref _cast, value); }
        }

        [DataMember(Name = "torrents")]
        public List<TorrentJson> Torrents
        {
            get => _torrents;
            set { Set(() => Torrents, ref _torrents, value); }
        }

        [DataMember(Name = "dateUploaded")]
        public string DateUploaded
        {
            get => _dateUploaded;
            set { Set(() => DateUploaded, ref _dateUploaded, value); }
        }

        [DataMember(Name = "posterImage")]
        public string PosterImage
        {
            get => _posterImage;
            set { Set(() => PosterImage, ref _posterImage, value); }
        }

        [DataMember(Name = "dateUploadedUnix")]
        public int DateUploadedUnix
        {
            get => _dateUploadedUnix;
            set { Set(() => DateUploadedUnix, ref _dateUploadedUnix, value); }
        }

        [DataMember(Name = "backgroundImage")]
        public string BackgroundImage
        {
            get => _backgroundImage;
            set { Set(() => BackgroundImage, ref _backgroundImage, value); }
        }

        [DataMember(Name = "similar")]
        public List<string> Similars
        {
            get => _similars;
            set { Set(() => Similars, ref _similars, value); }
        }

        /// <summary>
        /// Local path of the downloaded movie file
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set { Set(() => FilePath, ref _filePath, value); }
        }

        /// <summary>
        /// Decide if movie has to be watched in full HQ or not
        /// </summary>
        public bool WatchInFullHdQuality
        {
            get => _watchInFullHdQuality;
            set
            {
                var odlValue = _watchInFullHdQuality;
                Set(() => WatchInFullHdQuality, ref _watchInFullHdQuality, value);
                Messenger.Default.Send(new PropertyChangedMessage<bool>(this,
                    odlValue, value, nameof(WatchInFullHdQuality)));
            }
        }

        /// <summary>
        /// Indicate if full HQ quality is available
        /// </summary>
        public bool FullHdAvailable
        {
            get => _fullHdAvailable;
            set { Set(() => FullHdAvailable, ref _fullHdAvailable, value); }
        }

        public ITorrent SelectedTorrent { get; set; }

        public ObservableCollection<ITorrent> AvailableTorrents { get; set; }

        /// <summary>
        /// Available subtitles
        /// </summary>
        public ObservableCollection<Subtitle> AvailableSubtitles
        {
            get => _availableSubtitles;
            set { Set(() => AvailableSubtitles, ref _availableSubtitles, value); }
        }

        /// <summary>
        /// Selected subtitle
        /// </summary>
        public Subtitle SelectedSubtitle
        {
            get => _selectedSubtitle;
            set
            {
                Set(() => SelectedSubtitle, ref _selectedSubtitle, value);
                if (SelectedSubtitle != null && SelectedSubtitle.LanguageName == LocalizationProviderHelper.GetLocalizedValue<string>("CustomLabel"))
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(async () =>
                    {
                        var message = new ShowCustomSubtitleMessage();
                        await Messenger.Default.SendAsync(message);
                        if (!message.Error && !string.IsNullOrEmpty(message.FileName))
                        {
                            SelectedSubtitle.FilePath = message.FileName;
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Indicate if movie is favorite
        /// </summary>
        public bool IsFavorite
        {
            get => _isFavorite;
            set { Set(() => IsFavorite, ref _isFavorite, value); }
        }

        /// <summary>
        /// Indicate if movie has been seen by the user
        /// </summary>
        public bool HasBeenSeen
        {
            get => _hasBeenSeen;
            set { Set(() => HasBeenSeen, ref _hasBeenSeen, value); }
        }

        /// <summary>
        /// Tmdb id
        /// </summary>
        public int TmdbId
        {
            get => _tmdbId;
            set { Set(() => TmdbId, ref _tmdbId, value); }
        }

        public string TranslationLanguage { get; set; }

        public int? Season { get; set; }

        public int? EpisodeNumber { get; set; }

        public MediaType Type { get; } = MediaType.Movie;
    }
}