using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Popcorn.Models.Genres;
using Popcorn.Models.Shows;
using Popcorn.Models.User;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using Popcorn.Services.Tmdb;
using Popcorn.Exceptions;
using Popcorn.Helpers;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;
using RestSharp;
using VideoLibrary;

namespace Popcorn.Services.Shows.Show
{
    public class ShowService : IShowService
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// <see cref="ITmdbService"/>
        /// </summary>
        private readonly ITmdbService _tmdbService;

        /// <summary>
        /// Change the culture of TMDb
        /// </summary>
        /// <param name="language">Language to set</param>
        public async Task ChangeTmdbLanguage(Language language)
        {
            (await _tmdbService.GetClient).DefaultLanguage = language.Culture;
        }

        /// <summary>
        /// <see cref="RestClient"/>
        /// </summary>
        private readonly RestClient _restClient;

        /// <summary>
        /// Constructor
        /// </summary>
        public ShowService(ITmdbService tmdbService)
        {
            _tmdbService = tmdbService;
            _restClient = new RestClient(Constants.PopcornApi)
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };
        }

        /// <summary>
        /// Get show by its Imdb code
        /// </summary>
        /// <param name="imdbId">Show's Imdb code</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The show</returns>
        public async Task<ShowJson> GetShowAsync(string imdbId, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var request = new RestRequest("/{segment}/{show}", Method.GET);
                    request.AddUrlSegment("segment", "shows");
                    request.AddUrlSegment("show", imdbId);
                    var show = new ShowJson();
                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        show = JsonConvert.DeserializeObject<ShowJson>(response.Content);
                        var shows = await (await _tmdbService.GetClient).SearchTvShowAsync(show.Title, cancellationToken: cancellation);
                        if (shows.Results.Any())
                        {
                            foreach (var tvShow in shows.Results)
                            {
                                try
                                {
                                    var result =
                                        await (await _tmdbService.GetClient).GetTvShowExternalIdsAsync(tvShow.Id, cancellationToken: cancellation);
                                    if (result.ImdbId == show.ImdbId)
                                    {
                                        show.TmdbId = result.Id;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex);
                                }
                            }
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetShowAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetShowAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetShowAsync ({imdbId}) in {elapsedMs} milliseconds.");
                    }

                    return show;
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Get show light by its Imdb code
        /// </summary>
        /// <param name="imdbId">Show's Imdb code</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The show</returns>
        public async Task<ShowLightJson> GetShowLightAsync(string imdbId, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var request = new RestRequest("/{segment}/light/{show}", Method.GET);
                    request.AddUrlSegment("segment", "shows");
                    request.AddUrlSegment("show", imdbId);
                    var show = new ShowLightJson();
                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        show = JsonConvert.DeserializeObject<ShowLightJson>(response.Content);
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetShowLightAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetShowLightAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetShowLightAsync ({imdbId}) in {elapsedMs} milliseconds.");
                    }

                    return show;
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Get shows by ids
        /// </summary>
        /// <param name="imdbIds">The imdbIds of the shows, split by comma</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Shows</returns>
        public async Task<(IEnumerable<ShowLightJson> movies, int nbMovies)> GetShowsByIds(IList<string> imdbIds,
            CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                return await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    var watch = Stopwatch.StartNew();
                    var wrapper = new ShowLightResponse();
                    var request = new RestRequest("/{segment}/{subsegment}", Method.POST);
                    request.AddUrlSegment("segment", "shows");
                    request.AddUrlSegment("subsegment", "ids");
                    request.AddJsonBody(imdbIds);

                    try
                    {
                        var response = await _restClient.ExecuteTaskAsync(request, cancellation);
                        if (response.ErrorException != null)
                            throw response.ErrorException;

                        wrapper = JsonConvert.DeserializeObject<ShowLightResponse>(response.Content);
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetShowsByIds cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetShowsByIds: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetShowsByIds ({string.Join(",", imdbIds)}) in {elapsedMs} milliseconds.");
                    }

                    var result = wrapper?.Shows ?? new List<ShowLightJson>();
                    var nbResult = wrapper?.TotalShows ?? 0;
                    return (result, nbResult);
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return (new List<ShowLightJson>(), 0);
            }
        }

        /// <summary>
        /// Get popular shows by page
        /// </summary>
        /// <param name="page">Page to return</param>
        /// <param name="limit">The maximum number of shows to return</param>
        /// <param name="ct">Cancellation token</param>
        /// <param name="genre">The genre to filter</param>
        /// <param name="sortBy">The sort</param>
        /// <param name="ratingFilter">Used to filter by rating</param>
        /// <returns>Popular shows and the number of shows found</returns>
        public async Task<(IEnumerable<ShowLightJson> shows, int nbShows)> GetShowsAsync(int page,
            int limit,
            double ratingFilter,
            string sortBy,
            CancellationToken ct,
            GenreJson genre = null)
        {
            return await SearchShowsAsync(string.Empty,
                page,
                limit,
                genre,
                ratingFilter,
                sortBy,
                ct);
        }

        /// <summary>
        /// Search shows by criteria
        /// </summary>
        /// <param name="criteria">Criteria used for search</param>
        /// <param name="page">Page to return</param>
        /// <param name="limit">The maximum number of movies to return</param>
        /// <param name="genre">The genre to filter</param>
        /// <param name="ratingFilter">Used to filter by rating</param>
        /// <param name="sortBy">Sort by</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Searched shows and the number of movies found</returns>
        public async Task<(IEnumerable<ShowLightJson> shows, int nbShows)> SearchShowsAsync(string criteria,
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
                    var wrapper = new ShowLightResponse();
                    if (limit < 1 || limit > 50)
                        limit = Constants.MaxShowsPerPage;

                    if (page < 1)
                        page = 1;

                    var request = new RestRequest("/{segment}", Method.GET);
                    request.AddUrlSegment("segment", "shows");
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

                        wrapper = JsonConvert.DeserializeObject<ShowLightResponse>(response.Content);
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "SearchShowsAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"SearchShowsAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"SearchShowsAsync ({criteria}, {page}, {limit}) in {elapsedMs} milliseconds.");
                    }

                    var result = wrapper?.Shows ?? new List<ShowLightJson>();
                    var nbResult = wrapper?.TotalShows ?? 0;
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
        /// Get the link to the youtube trailer of a show
        /// </summary>
        /// <param name="show">The show</param>
        /// <param name="ct">Used to cancel loading trailer</param>
        /// <returns>Video trailer</returns>
        public async Task<string> GetShowTrailerAsync(ShowJson show, CancellationToken ct)
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
                        var tmdbVideos = await (await _tmdbService.GetClient).GetTvShowVideosAsync(show.TmdbId);
                        if (tmdbVideos != null && tmdbVideos.Results.Any())
                        {
                            var trailer = tmdbVideos.Results.FirstOrDefault();
                            using var service = Client.For(YouTube.Default);
                            var videos = (await service
                                    .GetAllVideosAsync("https://youtube.com/watch?v=" + trailer.Key))
                                .ToList();
                            if (videos.Any())
                            {
                                var settings = SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();
                                var maxRes = settings.DefaultHdQuality ? 1080 : 720;
                                uri =
                                    await videos
                                        .Where(a => !a.Is3D && a.Resolution <= maxRes &&
                                                    a.Format == VideoFormat.Mp4 &&
                                                    a.AudioBitrate > 0)
                                        .Aggregate((i1, i2) => i1.Resolution > i2.Resolution ? i1 : i2)
                                        .GetUriAsync();
                            }
                        }
                        else
                        {
                            throw new PopcornException("No trailer found.");
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException ||
                                                      exception is OperationCanceledException)
                    {
                        Logger.Debug(
                            "GetShowTrailerAsync cancelled.");
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetShowTrailerAsync: {exception.Message}");
                        throw;
                    }
                    finally
                    {
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        Logger.Trace(
                            $"GetShowTrailerAsync ({show.ImdbId}) in {elapsedMs} milliseconds.");
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
    }
}