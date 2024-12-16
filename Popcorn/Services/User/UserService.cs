using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Localization;
using Popcorn.Models.Movie;
using Popcorn.Models.Shows;
using Popcorn.Models.User;
using Popcorn.Services.Movies.Movie;
using Popcorn.Utils;
using WPFLocalizeExtension.Engine;
using Popcorn.Services.Shows.Show;
using Language = Popcorn.Models.User.Language;

namespace Popcorn.Services.User
{
    /// <summary>
    /// Services used to interact with user history
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Services used to interact with movies
        /// </summary>
        private IMovieService MovieService { get; }

        /// <summary>
        /// Services used to interact with shows
        /// </summary>
        private IShowService ShowService { get; }

        /// <summary>
        /// User
        /// </summary>
        private Models.User.User User { get; set; }

        /// <summary>
        /// True if user is synced
        /// </summary>
        private bool _isSynced;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="movieService"><see cref="IMovieService"/></param>
        /// <param name="showService"><see cref="IShowService"/></param>
        public UserService(IMovieService movieService, IShowService showService)
        {
            ShowService = showService;
            MovieService = movieService;
        }

        public async Task<Models.User.User> GetUser()
        {
            if (!_isSynced)
            {
                await SyncUser();
                _isSynced = true;
            }

            return User;
        }

        private async Task SyncUser()
        {
            User = new Models.User.User();
            try
            {
                var user = await BlobCache.UserAccount.GetObject<Models.User.User>("user");
                if (user != null)
                    User = user;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            if (User.Language == null)
            {
                User.Language = new Language();
            }

            if (User.MovieHistory == null)
            {
                User.MovieHistory = new List<MovieHistory>();
            }

            if (User.ShowHistory == null)
            {
                User.ShowHistory = new List<ShowHistory>();
            }

            if (User.CacheLocation == null)
            {
                User.CacheLocation = Path.GetTempPath() + @"Popcorn";
            }
        }

        public async Task UpdateUser()
        {
            await BlobCache.UserAccount.InsertObject("user", User);
        }

        /// <summary>
        /// Set if movies have been seen or set as favorite
        /// </summary>
        /// <param name="movies">All movies to compute</param>
        public void SyncMovieHistory(IEnumerable<IMovie> movies)
        {
            try
            {
                foreach (var movie in movies)
                {
                    var updatedMovie = User.MovieHistory.FirstOrDefault(p => p.ImdbId == movie.ImdbId);
                    if (updatedMovie == null) continue;
                    movie.IsFavorite = updatedMovie.Favorite;
                    movie.HasBeenSeen = updatedMovie.Seen;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"SyncMovieHistory: {exception.Message}");
            }
        }

        /// <summary>
        /// Set if shows have been seen or set as favorite
        /// </summary>
        /// <param name="shows">All shows to compute</param>
        public void SyncShowHistory(IEnumerable<IShow> shows)
        {
            try
            {
                foreach (var show in shows)
                {
                    var updatedShow = User.ShowHistory.FirstOrDefault(p => p.ImdbId == show.ImdbId);
                    if (updatedShow == null) continue;
                    show.IsFavorite = updatedShow.Favorite;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"SyncShowHistory: {exception.Message}");
            }
        }

        /// <summary>
        /// Set the movie
        /// </summary>
        /// <param name="movie">Movie</param>
        public void SetMovie(IMovie movie)
        {
            try
            {
                var movieToUpdate = User.MovieHistory.FirstOrDefault(a => a.ImdbId == movie.ImdbId);
                if (movieToUpdate == null)
                {
                    User.MovieHistory.Add(new MovieHistory
                    {
                        ImdbId = movie.ImdbId,
                        Favorite = movie.IsFavorite,
                        Seen = movie.HasBeenSeen
                    });
                }
                else
                {
                    movieToUpdate.Seen = movie.HasBeenSeen;
                    movieToUpdate.Favorite = movie.IsFavorite;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"SetMovie: {exception.Message}");
            }
        }

        /// <summary>
        /// Set the show
        /// </summary>
        /// <param name="show">Show</param>
        public void SetShow(IShow show)
        {
            var showToUpdate = User.ShowHistory.FirstOrDefault(a => a.ImdbId == show.ImdbId);
            if (showToUpdate == null)
            {
                User.ShowHistory.Add(new ShowHistory
                {
                    ImdbId = show.ImdbId,
                    Favorite = show.IsFavorite,
                });
            }
            else
            {
                showToUpdate.Favorite = show.IsFavorite;
            }
        }

        /// <summary>
        /// Get seen movies
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        public (IEnumerable<string> movies, IEnumerable<string> allMovies, int nbMovies)
            GetSeenMovies(int page)
        {
            try
            {
                var movies = User.MovieHistory.Where(a => a.Seen).Select(a => a.ImdbId).ToList();
                var skip = (page - 1) * Constants.MaxMoviesPerPage;
                if (movies.Count <= Constants.MaxMoviesPerPage)
                {
                    skip = 0;
                }

                return (movies.Skip(skip).Take(Constants.MaxMoviesPerPage), movies, movies.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (new List<string>(), new List<string>(), 0);
            }
        }

        /// <summary>
        /// Get seen shows
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        public (IEnumerable<string> shows, IEnumerable<string> allShows, int nbShows) GetSeenShows(int page)
        {
            try
            {
                var shows = User.ShowHistory.Where(a => a.Seen).Select(a => a.ImdbId).ToList();
                var skip = (page - 1) * Constants.MaxShowsPerPage;
                if (shows.Count <= Constants.MaxShowsPerPage)
                {
                    skip = 0;
                }

                return (shows.Skip(skip).Take(Constants.MaxShowsPerPage), shows, shows.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (new List<string>(), new List<string>(), 0);
            }
        }

        /// <summary>
        /// Get favorites movies
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        public (IEnumerable<string> movies, IEnumerable<string> allMovies, int nbMovies) GetFavoritesMovies(int page)
        {
            try
            {
                var movies = User.MovieHistory.Where(a => a.Favorite).Select(a => a.ImdbId).ToList();
                var skip = (page - 1) * Constants.MaxMoviesPerPage;
                if (movies.Count <= Constants.MaxMoviesPerPage)
                {
                    skip = 0;
                }

                return (movies.Skip(skip).Take(Constants.MaxMoviesPerPage), movies, movies.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (new List<string>(), new List<string>(), 0);
            }
        }

        /// <summary>
        /// Get favorites shows
        /// </summary>
        /// <param name="page">Pagination</param>
        /// <returns>List of ImdbId</returns>
        public (IEnumerable<string> shows, IEnumerable<string> allShows, int nbShows) GetFavoritesShows(int page)
        {
            try
            {
                var shows = User.ShowHistory.Where(a => a.Favorite).Select(a => a.ImdbId).ToList();
                var skip = (page - 1) * Constants.MaxShowsPerPage;
                if (shows.Count <= Constants.MaxShowsPerPage)
                {
                    skip = 0;
                }

                return (shows.Skip(skip).Take(Constants.MaxShowsPerPage), shows, shows.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (new List<string>(), new List<string>(), 0);
            }
        }

        /// <summary>
        /// Set the download rate
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public void SetDownloadLimit(int limit)
        {
            User.DownloadLimit = limit;
        }

        /// <summary>
        /// Set the upload rate
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public void SetUploadLimit(int limit)
        {
            User.UploadLimit = limit;
        }

        /// <summary>
        /// Get the download limit
        /// </summary>
        /// <returns></returns>
        public int GetDownloadLimit()
        {
            return User.DownloadLimit;
        }

        /// <summary>
        /// Get the upload limit
        /// </summary>
        /// <returns></returns>
        public int GetUploadLimit()
        {
            return User.UploadLimit;
        }

        /// <summary>
        /// Set default HD quality
        /// </summary>
        /// <param name="hd"></param>
        /// <returns></returns>
        public void SetDefaultHdQuality(bool hd)
        {
            User.DefaultHdQuality = hd;
        }

        /// <summary>
        /// Set default subtitle language
        /// </summary>
        /// <param name="englishName"></param>
        /// <returns></returns>
        public void SetDefaultSubtitleLanguage(string englishName)
        {
            User.DefaultSubtitleLanguage = englishName;
        }

        /// <summary>
        /// Get default subtitle language
        /// </summary>
        /// <returns></returns>
        public string GetDefaultSubtitleLanguage()
        {
            return User.DefaultSubtitleLanguage;
        }

        /// <summary>
        /// Get HD quality
        /// </summary>
        /// <returns></returns>
        public bool GetDefaultHdQuality()
        {
            return User.DefaultHdQuality;
        }

        /// <summary>
        /// Get the cache location path
        /// </summary>
        /// <returns>Cache location</returns>
        public string GetCacheLocationPath()
        {
            return User.CacheLocation;
        }

        /// <summary>
        /// Set the cache location path
        /// </summary>
        /// <param name="path">Cache location path</param>
        public void SetCacheLocationPath(string path)
        {
            User.CacheLocation = path;
        }

        /// <summary>
        /// Get all available languages from the database
        /// </summary>
        /// <returns>All available languages</returns>
        public ICollection<Language> GetAvailableLanguages()
        {
            var watch = Stopwatch.StartNew();
            ICollection<Language> availableLanguages = new List<Language>
            {
                new EnglishLanguage(),
                new FrenchLanguage(),
                new GermanLanguage(),
                new SpanishLanguage()
            };
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Logger.Trace(
                $"GetAvailableLanguages in {elapsedMs} milliseconds.");

            return availableLanguages;
        }

        /// <summary>
        /// Get the current language of the application
        /// </summary>
        /// <returns>Current language</returns>
        public Language GetCurrentLanguage()
        {
            try
            {
                Language currentLanguage;
                var language = User.Language;
                if (language != null)
                {
                    switch (language.Culture)
                    {
                        case "fr":
                            currentLanguage = new FrenchLanguage();
                            break;
                        case "es":
                            currentLanguage = new SpanishLanguage();
                            break;
                        case "de":
                            currentLanguage = new GermanLanguage();
                            break;
                        default:
                            currentLanguage = new EnglishLanguage();
                            break;
                    }
                }
                else
                {
                    currentLanguage = new EnglishLanguage();
                }

                return currentLanguage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new EnglishLanguage();
            }
        }

        /// <summary>
        /// Set the current language of the application
        /// </summary>
        /// <param name="language">Language</param>
        public async Task SetCurrentLanguage(Language language)
        {
            try
            {
                User.Language.Culture = language.Culture;
                await MovieService.ChangeTmdbLanguage(language);
                await ShowService.ChangeTmdbLanguage(language);
                LocalizeDictionary.Instance.Culture = new CultureInfo(language.Culture);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Messenger.Default.Send(new ChangeLanguageMessage());
        }
    }
}