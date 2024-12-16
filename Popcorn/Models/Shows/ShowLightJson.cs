using System.Runtime.Serialization;
using GalaSoft.MvvmLight;
using Popcorn.Models.Image;
using Popcorn.Models.Rating;

namespace Popcorn.Models.Shows
{
    [DataContract]
    public class ShowLightJson : ObservableObject, IShow
    {
        private bool _isFavorite;

        private string _imdbId;

        private string _tvdbId;

        private string _title;

        private int _year;

        private string _genres;

        private ImageShowJson _images;

        private RatingJson _rating;

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

        [DataMember(Name = "genres")]
        public string Genres
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
