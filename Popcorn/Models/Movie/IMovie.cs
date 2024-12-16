namespace Popcorn.Models.Movie
{
    public interface IMovie
    {
        string ImdbId { get; set; }

        bool IsFavorite { get; set; }

        bool HasBeenSeen { get; set; }

        string TranslationLanguage { get; set; }

        string Title { get; set; }
    }
}
