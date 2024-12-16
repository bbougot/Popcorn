using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Popcorn.Models.Movie;
using TMDbLib.Objects.Movies;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Popcorn.Models.Genres;
using Popcorn.Models.User;
using Popcorn.Exceptions;
using Polly;
using Polly.Timeout;
using Popcorn.Extensions;
using Popcorn.Helpers;
using Popcorn.Services.Tmdb;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;
using RestSharp;
using TMDbLib.Objects.Find;
using TMDbLib.Objects.People;
using VideoLibrary;

namespace Popcorn.Services.Movies.Movie
{
    /// <summary>
    /// Services used to interact with movies
    /// </summary>
    public class MovieService : IMovieService
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Movies to translate
        /// </summary>
        private readonly Subject<IMovie> _moviesToTranslateObservable;

        /// <summary>
        /// <see cref="ITmdbService"/>
        /// </summary>
        private readonly ITmdbService _tmdbService;

        /// <summary>
        /// <see cref="RestClient"/>
        /// </summary>
        private readonly RestClient _restClient;

        /// <summary>
        /// Initialize a new instance of MovieService class
        /// </summary>
        public MovieService(ITmdbService tmdbService)
        {
            _moviesToTranslateObservable = new Subject<IMovie>();
            _tmdbService = tmdbService;

            _restClient = new RestClient(Constants.PopcornApi)
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };

            try
            {
                _moviesToTranslateObservable.Drain(s => Observable.Return(s).Delay(TimeSpan.FromMilliseconds(250)))
                    .Subscribe(async movieToTranslate =>
                    {
                        if (movieToTranslate == null)
                            return;

                        var timeoutPolicy =
                            Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond,
                                TimeoutStrategy.Optimistic);
                        try
                        {
                            await timeoutPolicy.ExecuteAsync(async () =>
                            {
                                try
                                {
                                    var movie = await (await _tmdbService.GetClient).GetMovieAsync(
                                        movieToTranslate.ImdbId,
                                        MovieMethods.Credits);
                                    if (movieToTranslate is MovieJson refMovie)
                                    {
                                        refMovie.TranslationLanguage = (await _tmdbService.GetClient).DefaultLanguage;
                                        refMovie.Title = movie?.Title;
                                        refMovie.Genres = movie?.Genres?.Select(a => a.Name).ToList();
                                        refMovie.DescriptionFull = movie?.Overview;
                                    }
                                    else if (movieToTranslate is MovieLightJson refMovieLight)
                                    {
                                        refMovieLight.TranslationLanguage =
                                            (await _tmdbService.GetClient).DefaultLanguage;
                                        refMovieLight.Title = movie?.Title;
                                        refMovieLight.Genres = movie?.Genres != null
                                            ? string.Join(", ", movie.Genres?.Select(a => a.Name))
                                            : string.Empty;
                                    }
                                }
                                catch (Exception exception) when (exception is TaskCanceledException)
                                {
                                    Logger.Debug(
                                        "TranslateMovieAsync cancelled.");
                                }
                                catch (Exception exception)
                                {
                                    Logger.Error(
                                        $"TranslateMovieAsync: {exception.Message}");
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(
                                $"Movie {movieToTranslate.ImdbId} has not been translated in {Constants.DefaultRequestTimeoutInSecond} seconds. Error {ex.Message}");
                        }
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Change the culture of TMDb
        /// </summary>
        /// <param name="language">Language to set</param>
        public async Task ChangeTmdbLanguage(Language language)
        {
            (await _tmdbService.GetClient).DefaultLanguage = language.Culture;
        }

        /// <summary>
        /// Get movie by its Imdb code
        /// </summary>
        /// <param name="imdbCode">Movie's Imdb code</param>
        /// <param name="ct">Cancellation</param>
        /// <returns>The movie</returns>
        public async Task<MovieJson> GetMovieAsync(string imdbCode, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var request = new RestRequest("/{segment}/{movie}", Method.GET);
                    request.AddUrlSegment("segment", "movies");
                    request.AddUrlSegment("movie", imdbCode);
                    var movie = new MovieJson();

                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        movie = JsonConvert.DeserializeObject<MovieJson>(response.Content);
                        movie.TranslationLanguage = (await _tmdbService.GetClient).DefaultLanguage;
                        movie.TmdbId =
                            (await (await _tmdbService.GetClient).GetMovieAsync(movie.ImdbId,
                                cancellationToken: cancellation)).Id;
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetMovieAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMovieAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetMovieAsync ({imdbCode}) in {elapsedMs} milliseconds.");
                    }

                    return movie;
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Get light movie by its Imdb code
        /// </summary>
        /// <param name="imdbCode">Movie's Imdb code</param>
        /// <param name="ct">Cancellation</param>
        /// <returns>The movie</returns>
        public async Task<MovieLightJson> GetMovieLightAsync(string imdbCode, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var request = new RestRequest("/{segment}/light/{movie}", Method.GET);
                    request.AddUrlSegment("segment", "movies");
                    request.AddUrlSegment("movie", imdbCode);
                    var movie = new MovieLightJson();

                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        movie = JsonConvert.DeserializeObject<MovieLightJson>(response.Content);
                        movie.TranslationLanguage = (await _tmdbService.GetClient).DefaultLanguage;
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetMovieLightAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMovieLightAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetMovieLightAsync ({imdbCode}) in {elapsedMs} milliseconds.");
                    }

                    return movie;
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Get movies similar async
        /// </summary>
        /// <param name="movie">Movie</param>
        /// <param name="ct">Cancellation</param>
        /// <returns>Movies</returns>
        public async Task<IEnumerable<MovieLightJson>> GetMoviesSimilarAsync(MovieJson movie, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    (IEnumerable<MovieLightJson> movies, int nbMovies) similarMovies = (new List<MovieLightJson>(), 0);
                    try
                    {
                        if (movie.Similars != null && movie.Similars.Any())
                        {
                            similarMovies = await GetMoviesByIds(movie.Similars,
                                CancellationToken.None);
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMoviesSimilarAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetMoviesSimilarAsync in {elapsedMs} milliseconds.");
                    }

                    return similarMovies.movies.Where(
                        a => a.ImdbId != movie.ImdbId);
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new List<MovieLightJson>();
            }
        }

        /// <summary>
        /// Get movies by page
        /// </summary>
        /// <param name="page">Page to return</param>
        /// <param name="limit">The maximum number of movies to return</param>
        /// <param name="ct">Cancellation token</param>
        /// <param name="genre">The genre to filter</param>
        /// <param name="sortBy">The sort</param>
        /// <param name="ratingFilter">Used to filter by rating</param>
        /// <returns>Popular movies and the number of movies found</returns>
        public async Task<(IEnumerable<MovieLightJson> movies, int nbMovies)> GetMoviesAsync(int page,
            int limit,
            double ratingFilter,
            string sortBy,
            CancellationToken ct,
            GenreJson genre = null)
        {
            return await SearchMoviesAsync(string.Empty,
                page,
                limit,
                genre,
                ratingFilter,
                sortBy,
                ct);
        }

        /// <summary>
        /// Process translations for a list of movies
        /// </summary>
        /// <param name="movies"></param>
        /// <returns></returns>
        private async Task ProcessTranslations(IEnumerable<IMovie> movies)
        {
            foreach (var movie in movies)
            {
                await TranslateMovie(movie);
            }
        }

        /// <summary>
        /// Get movies by ids
        /// </summary>
        /// <param name="imdbIds">The imdbIds of the movies, split by comma</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Similar movies</returns>
        public async Task<(IEnumerable<MovieLightJson> movies, int nbMovies)> GetMoviesByIds(
            IList<string> imdbIds,
            CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var wrapper = new MovieLightResponse();
                    var request = new RestRequest("/{segment}/{subsegment}", Method.POST);
                    request.AddUrlSegment("segment", "movies");
                    request.AddUrlSegment("subsegment", "ids");
                    request.AddJsonBody(imdbIds);

                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        wrapper = JsonConvert.DeserializeObject<MovieLightResponse>(response.Content);
                        foreach (var movie in wrapper.Movies)
                        {
                            movie.TranslationLanguage = (await _tmdbService.GetClient).DefaultLanguage;
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetMoviesByIds cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMoviesByIds: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetMoviesByIds ({string.Join(",", imdbIds)}) in {elapsedMs} milliseconds.");
                    }

                    var result = wrapper?.Movies ?? new List<MovieLightJson>();
                    await ProcessTranslations(result);
                    var nbResult = wrapper?.TotalMovies ?? 0;
                    return (result, nbResult);
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (new List<MovieLightJson>(), 0);
            }
        }

        /// <summary>
        /// Get similar movies
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="limit">Limit</param>
        /// <param name="ratingFilter">Rating</param>
        /// <param name="sortBy">SortBy</param>
        /// <param name="imdbIds">The imdbIds of the movies, split by comma</param>
        /// <param name="ct">Cancellation token</param>
        /// <param name="genre">Genre</param>
        /// <returns>Similar movies</returns>
        public async Task<(IEnumerable<MovieLightJson> movies, int nbMovies)> GetSimilar(int page,
            int limit,
            double ratingFilter,
            string sortBy,
            IList<string> imdbIds,
            CancellationToken ct,
            GenreJson genre = null)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var wrapper = new MovieLightResponse();
                    if (limit < 1 || limit > 50)
                        limit = Constants.MaxMoviesPerPage;

                    if (page < 1)
                        page = 1;

                    var request = new RestRequest("/{segment}/{subsegment}", Method.POST);
                    request.AddUrlSegment("segment", "movies");
                    request.AddUrlSegment("subsegment", "similar");
                    request.AddQueryParameter("limit", limit.ToString());
                    request.AddQueryParameter("page", page.ToString());
                    request.AddQueryParameter("genre", genre != null ? genre.EnglishName : string.Empty);
                    request.AddQueryParameter("minimum_rating", Convert.ToInt32(ratingFilter).ToString());
                    request.AddQueryParameter("sort_by", sortBy);
                    request.AddQueryParameter("query_term", string.Empty);
                    request.AddJsonBody(imdbIds);

                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        wrapper = JsonConvert.DeserializeObject<MovieLightResponse>(response.Content);
                        foreach (var movie in wrapper.Movies)
                        {
                            movie.TranslationLanguage = (await _tmdbService.GetClient).DefaultLanguage;
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetMoviesByIds cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMoviesByIds: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetMoviesByIds ({string.Join(",", imdbIds)}) in {elapsedMs} milliseconds.");
                    }

                    var result = wrapper?.Movies ?? new List<MovieLightJson>();
                    await ProcessTranslations(result);
                    var nbResult = wrapper?.TotalMovies ?? 0;
                    return (result, nbResult);
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (new List<MovieLightJson>(), 0);
            }
        }

        /// <summary>
        /// Search movies by criteria
        /// </summary>
        /// <param name="criteria">Criteria used for search</param>
        /// <param name="page">Page to return</param>
        /// <param name="limit">The maximum number of movies to return</param>
        /// <param name="genre">The genre to filter</param>
        /// <param name="ratingFilter">Used to filter by rating</param>
        /// <param name="sortBy">Sort by</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Searched movies and the number of movies found</returns>
        public async Task<(IEnumerable<MovieLightJson> movies, int nbMovies)> SearchMoviesAsync(string criteria,
            int page,
            int limit,
            GenreJson genre,
            double ratingFilter,
            string sortBy,
            CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var wrapper = new MovieLightResponse();
                    if (limit < 1 || limit > 50)
                        limit = Constants.MaxMoviesPerPage;

                    if (page < 1)
                        page = 1;

                    var request = new RestRequest("/{segment}", Method.GET);
                    request.AddUrlSegment("segment", "movies");
                    request.AddParameter("limit", limit);
                    request.AddParameter("page", page);
                    request.AddParameter("genre", genre != null ? genre.EnglishName : string.Empty);
                    request.AddParameter("minimum_rating", Convert.ToInt32(ratingFilter));
                    request.AddParameter("query_term", criteria);
                    request.AddParameter("sort_by", sortBy);

                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        wrapper = JsonConvert.DeserializeObject<MovieLightResponse>(response.Content);
                        foreach (var movie in wrapper.Movies)
                        {
                            movie.TranslationLanguage = (await _tmdbService.GetClient).DefaultLanguage;
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "SearchMoviesAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"SearchMoviesAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"SearchMoviesAsync ({criteria}, {page}, {limit}) in {elapsedMs} milliseconds.");
                    }

                    var result = wrapper?.Movies ?? new List<MovieLightJson>();
                    await ProcessTranslations(result);
                    var nbResult = wrapper?.TotalMovies ?? 0;
                    return (result, nbResult);
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Translate movie informations (title, description, ...)
        /// </summary>
        /// <param name="movieToTranslate">Movie to translate</param>
        /// <returns>Task</returns>
        public async Task TranslateMovie(IMovie movieToTranslate)
        {
            if (movieToTranslate.TranslationLanguage == null ||
                (await _tmdbService.GetClient).DefaultLanguage == "en" &&
                movieToTranslate.TranslationLanguage == (await _tmdbService.GetClient).DefaultLanguage) return;
            _moviesToTranslateObservable.OnNext(movieToTranslate);
        }

        /// <summary>
        /// Get the link to the youtube trailer of a movie
        /// </summary>
        /// <param name="movie">The movie</param>
        /// <param name="ct">Used to cancel loading trailer</param>
        /// <returns>Video trailer</returns>
        public async Task<string> GetMovieTrailerAsync(MovieJson movie, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var uri = string.Empty;
                    try
                    {
                        var tmdbMovie =
                            await (await _tmdbService.GetClient).GetMovieAsync(movie.ImdbId, MovieMethods.Videos, cancellation);
                        var trailers = tmdbMovie?.Videos;
                        if (trailers != null && trailers.Results.Any())
                        {
                            var trailer = trailers.Results
                                .First()
                                .Key;
                            var video = await GetVideoFromYtVideoId(trailer);
                            uri = await video.GetUriAsync();
                        }
                        else
                        {
                            throw new PopcornException("No trailer found.");
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetMovieTrailerAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMovieTrailerAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetMovieTrailerAsync ({movie.ImdbId}) in {elapsedMs} milliseconds.");
                    }

                    return uri;
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        public async Task<YouTubeVideo> GetVideoFromYtVideoId(string ytVideoId)
        {
            using var service = Client.For(YouTube.Default);
            var videos =
                (await service.GetAllVideosAsync($"https://youtube.com/watch?v={ytVideoId}"))
                .ToList();
            if (videos.Any())
            {
                var settings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                var maxRes = settings.DefaultHdQuality ? 1080 : 720;
                return
                    videos.Where(a => !a.Is3D && a.Resolution <= maxRes &&
                                      a.Format == VideoFormat.Mp4 && a.AudioBitrate > 0)
                        .Aggregate((i1, i2) => i1.Resolution > i2.Resolution ? i1 : i2);
            }

            return null;
        }

        /// <summary>
        /// Get cast
        /// </summary>
        /// <param name="imdbCode">Tmdb cast Id</param>
        /// <returns><see cref="Person"/></returns>
        public async Task<Person> GetCast(string imdbCode)
        {
            var search = await (await _tmdbService.GetClient).FindAsync(FindExternalSource.Imdb, $"nm{imdbCode}");
            return await (await _tmdbService.GetClient).GetPersonAsync(search.PersonResults.First().Id,
                PersonMethods.Images | PersonMethods.TaggedImages);
        }

        /// <summary>
        /// Get movies for a cast by its ImdbId
        /// </summary>
        /// <param name="imdbCode"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MovieLightJson>> GetMovieFromCast(string imdbCode, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var wrapper = new MovieLightResponse();
                    var request = new RestRequest("/movies/cast/{segment}", Method.GET);
                    request.AddUrlSegment("segment", imdbCode);
                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        wrapper = JsonConvert.DeserializeObject<MovieLightResponse>(response.Content);
                        foreach (var movie in wrapper.Movies)
                        {
                            movie.TranslationLanguage = (await _tmdbService.GetClient).DefaultLanguage;
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetMovieFromCast cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMovieFromCast: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetMovieFromCast ({imdbCode}) in {elapsedMs} milliseconds.");
                    }

                    var result = wrapper?.Movies ?? new List<MovieLightJson>();
                    await ProcessTranslations(result);
                    return result;
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new List<MovieLightJson>();
            }
        }

        /// <summary>
        /// Retrieve an image url from Tmdb
        /// </summary>
        /// <param name="url">Image to retrieve</param>
        /// <returns>Image url</returns>
        public async Task<string> GetImagePathFromTmdb(string url)
        {
            return (await _tmdbService.GetClient).GetImageUrl("original", url, true).AbsoluteUri;
        }
    }
}