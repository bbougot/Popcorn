using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace Popcorn.Models.Movie
{
    [DataContract]
    public class MovieLightJson : ObservableObject, IMovie
    {
        private string _posterImage;
        private string _genres;
        private bool _hasBeenSeen;
        private string _imdbCode;
        private bool _isFavorite;
        private double _rating;
        private string _title;
        private int _year;

        [DataMember(Name = "imdbCode")]
        public string ImdbId
        {
            get => _imdbCode;
            set { Set(() => ImdbId, ref _imdbCode, value); }
        }

        [DataMember(Name = "title")]
        public string Title
        {
            get => _title;
            set { Set(() => Title, ref _title, value); }
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

        [DataMember(Name = "genres")]
        public string Genres
        {
            get => _genres;
            set { Set(() => Genres, ref _genres, value); }
        }

        [DataMember(Name = "posterImage")]
        public string PosterImage
        {
            get => _posterImage;
            set { Set(() => PosterImage, ref _posterImage, value); }
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

        public string TranslationLanguage { get; set; }
    }
}
