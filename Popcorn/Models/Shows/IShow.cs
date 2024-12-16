namespace Popcorn.Models.Shows
{
    public interface IShow
    {
        string ImdbId { get; set; }

        bool IsFavorite { get; set; }
    }
}
