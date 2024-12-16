using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Popcorn.Models.Genres;

namespace Popcorn.Services.Genres
{
    public interface IGenreService
    {
        /// <summary>
        /// Get all genres
        /// </summary>
        /// <param name="language">Genre language</param>
        /// <param name="ct">Used to cancel loading genres</param>
        /// <returns>Genres</returns>
        Task<List<GenreJson>> GetGenresAsync(string language, CancellationToken ct);
    }
}
