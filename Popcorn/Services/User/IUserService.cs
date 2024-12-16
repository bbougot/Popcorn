using System.Collections.Generic;
using System.Threading.Tasks;
using Popcorn.Models.Movie;
using Popcorn.Models.Shows;
using Popcorn.Models.User;

namespace Popcorn.Services.User
{
    public interface IUserService
    {
        /// <summary>
        /// Get all available languages
        /// </summary>
        /// <returns>All available languages</returns>
        ICollection<Language> GetAvailableLanguages();

        /// <summary>
        /// Get the current language of the application
        /// </summary>
        Language GetCurrentLanguage();

        /// <summary>
        /// Set the current language of the application
        /// </summary>
        /// <param name="language">Language</param>
        Task SetCurrentLanguage(Language language);

        /// <summary>
        /// Set if movies have been seen or set as favorite
        /// </summary>
        /// <param name="movies">All movies to compute</param>
        void SyncMovieHistory(IEnumerable<IMovie> movies);

        /// <summary>
        /// Set if shows have been seen or set as favorite
        /// </summary>
        /// <param name="shows">All movies to compute</param>
        void SyncShowHistory(IEnumerable<IShow> shows);

        /// <summary>
        /// Update user
        /// </summary>
        /// <returns></returns>
        Task UpdateUser();

        /// <summary>
        /// Set the movie
        /// </summary>
        /// <param name="movie">Favorite movie</param>
        void SetMovie(IMovie movie);

        /// <summary>
        /// Set the show
        /// </summary>
        /// <param name="show">Show</param>
        void SetShow(IShow show);

        /// <summary>
        /// Get seen movies
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        (IEnumerable<string> movies, IEnumerable<string> allMovies, int nbMovies) GetSeenMovies(int page);

        /// <summary>
        /// Get seen shows
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        (IEnumerable<string> shows, IEnumerable<string> allShows, int nbShows) GetSeenShows(int page);

        /// <summary>
        /// Get favorites movies
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        (IEnumerable<string> movies, IEnumerable<string> allMovies, int nbMovies) GetFavoritesMovies(int page);

        /// <summary>
        /// Get favorites shows
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        (IEnumerable<string> shows, IEnumerable<string> allShows, int nbShows) GetFavoritesShows(int page);

        /// <summary>
        /// Get current user
        /// </summary>
        /// <returns></returns>
        Task<Models.User.User> GetUser();

        /// <summary>
        /// Set the download limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        void SetDownloadLimit(int limit);

        /// <summary>
        /// Set the upload limit
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        void SetUploadLimit(int limit);

        /// <summary>
        /// Get the download limit
        /// </summary>
        /// <returns></returns>
        int GetDownloadLimit();

        /// <summary>
        /// Get the upload limit
        /// </summary>
        /// <returns></returns>
        int GetUploadLimit();

        /// <summary>
        /// Set default HD quality
        /// </summary>
        /// <param name="hd"></param>
        /// <returns></returns>
        void SetDefaultHdQuality(bool hd);

        /// <summary>
        /// Set default subtitle language
        /// </summary>
        /// <param name="englishName"></param>
        /// <returns></returns>
        void SetDefaultSubtitleLanguage(string englishName);

        /// <summary>
        /// Get default subtitle language
        /// </summary>
        /// <returns></returns>
        string GetDefaultSubtitleLanguage();

        /// <summary>
        /// Get HD quality
        /// </summary>
        /// <returns></returns>
        bool GetDefaultHdQuality();

        /// <summary>
        /// Get cache location
        /// </summary>
        /// <returns></returns>
        string GetCacheLocationPath();

        /// <summary>
        /// Set cache location
        /// </summary>
        /// <param name="path"></param>
        void SetCacheLocationPath(string path);
    }
}