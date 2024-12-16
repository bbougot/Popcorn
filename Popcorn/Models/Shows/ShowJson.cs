using System.Collections.Generic;
using System.Runtime.Serialization;
using GalaSoft.MvvmLight;
using Popcorn.Models.Episode;
using Popcorn.Models.Image;
using Popcorn.Models.Rating;

namespace Popcorn.Models.Shows
{
    [DataContract]
    public class ShowJson : ObservableObject, IShow
    {
        private bool _isFavorite;

        private string _imdbId;

        private int _tmdbId;

        private string _tvdbId;

        private string _title;

        private int _year;

        private string _slug;

        private string _synopsis;

        private string _runtime;

        private string _country;

        private string _network;

        private string _airDay;

        private string _airTime;

        private string _status;

        private int _numSeasons;

        private long _lastUpdated;

        private List<EpisodeShowJson> _episodes;

        private List<string> _genres;

        private ImageShowJson _images;

        private RatingJson _rating;

        private List<string> _similars;

        public int TmdbId
        {
            get => _tmdbId;
            set => Set(ref _tmdbId, value);
        }

        [DataMember(Name = "imdbId")]
        public string ImdbId
        {
            get => _imdbId;
            set => Set(ref _imdbId, value);
        }

        [DataMember(Name = "tvdbId")]
        public string TvdbId
        {
            get => _tvdbId;
            set => Set(ref _tvdbId, value);
        }

        [DataMember(Name = "title")]
        public string Title
        {
            get => _title;
            set
            {
                var newTitle = value.Replace("&amp;", "&");
                Set(ref _title, newTitle);
            }
        }

        [DataMember(Name = "year")]
        public int Year
        {
            get => _year;
            set => Set(ref _year, value);
        }

        [DataMember(Name = "slug")]
        public string Slug
        {
            get => _slug;
            set => Set(ref _slug, value);
        }

        [DataMember(Name = "synopsis")]
        public string Synopsis
        {
            get => _synopsis;
            set => Set(ref _synopsis, value);
        }

        [DataMember(Name = "runtime")]
        public string Runtime
        {
            get => _runtime;
            set => Set(ref _runtime, value);
        }

        [DataMember(Name = "country")]
        public string Country
        {
            get => _country;
            set => Set(ref _country, value);
        }

        [DataMember(Name = "network")]
        public string Network
        {
            get => _network;
            set => Set(ref _network, value);
        }

        [DataMember(Name = "airDay")]
        public string AirDay
        {
            get => _airDay;
            set => Set(ref _airDay, value);
        }

        [DataMember(Name = "airTime")]
        public string AirTime
        {
            get => _airTime;
            set => Set(ref _airTime, value);
        }

        [DataMember(Name = "status")]
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        [DataMember(Name = "numSeasons")]
        public int NumSeasons
        {
            get => _numSeasons;
            set => Set(ref _numSeasons, value);
        }

        [DataMember(Name = "lastUpdated")]
        public long LastUpdated
        {
            get => _lastUpdated;
            set => Set(ref _lastUpdated, value);
        }

        [DataMember(Name = "episodes")]
        public List<EpisodeShowJson> Episodes
        {
            get => _episodes;
            set => Set(ref _episodes, value);
        }

        [DataMember(Name = "genres")]
        public List<string> Genres
        {
            get => _genres;
            set => Set(ref _genres, value);
        }

        [DataMember(Name = "images")]
        public ImageShowJson Images
        {
            get => _images;
            set => Set(ref _images, value);
        }

        [DataMember(Name = "rating")]
        public RatingJson Rating
        {
            get => _rating;
            set => Set(ref _rating, value);
        }

        [DataMember(Name = "similar")]
        public List<string> Similars
        {
            get => _similars;
            set => Set(ref _similars, value);
        }

        /// <summary>
        /// Indicate if movie is favorite
        /// </summary>
        public bool IsFavorite
        {
            get => _isFavorite;
            set { Set(() => IsFavorite, ref _isFavorite, value); }
        }
    }
}
