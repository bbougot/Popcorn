using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Polly;
using Polly.Timeout;
using Popcorn.Enums;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Movie;
using Popcorn.Services.Movies.Movie;
using Popcorn.Exceptions;

namespace Popcorn.Services.Movies.Trailer
{
    /// <summary>
    /// Manage trailer
    /// </summary>
    public sealed class MovieTrailerService : IMovieTrailerService
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The service used to interact with movies
        /// </summary>
        private IMovieService MovieService { get; }

        /// <summary>
        /// Initializes a new instance of the TrailerViewModel class.
        /// </summary>
        /// <param name="movieService">Movie service</param>
        public MovieTrailerService(IMovieService movieService)
        {
            MovieService = movieService;
        }

        /// <summary>
        /// Load movie's trailer asynchronously
        /// </summary>
        /// <param name="movie">The movie</param>
        /// <param name="ct">Cancellation token</param>
        public async Task LoadTrailerAsync(MovieJson movie, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    try
                    {
                        var trailer = await MovieService.GetMovieTrailerAsync(movie, cancellation);
                        if (!cancellation.IsCancellationRequested && string.IsNullOrEmpty(trailer))
                        {
                            Logger.Error(
                                $"Failed loading movie's trailer: {movie.Title}");
                            Messenger.Default.Send(
                                new ManageExceptionMessage(
                                    new TrailerNotAvailableException(
                                        LocalizationProviderHelper.GetLocalizedValue<string>("TrailerNotAvailable"))));
                            Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                            return;
                        }

                        if (!cancellation.IsCancellationRequested)
                        {
                            Uri uriResult;
                            bool result = Uri.TryCreate(trailer, UriKind.Absolute, out uriResult)
                                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                            if (result)
                            {
                                var client = new HttpClient();
                                try
                                {
                                    using (var response = await client.GetAsync(uriResult, HttpCompletionOption.ResponseHeadersRead,ct))
                                    {
                                        if (response == null || response.StatusCode != HttpStatusCode.OK)
                                        {
                                            Logger.Error(
                                                $"Failed loading movie's trailer: {movie.Title}");
                                            Messenger.Default.Send(
                                                new ManageExceptionMessage(
                                                    new TrailerNotAvailableException(
                                                        LocalizationProviderHelper.GetLocalizedValue<string>("TrailerNotAvailable"))));
                                            Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                                            return;
                                        }
                                    }
                                }
                                catch (WebException)
                                {
                                    Logger.Error(
                                        $"Failed loading movie's trailer: {movie.Title}");
                                    Messenger.Default.Send(
                                        new ManageExceptionMessage(
                                            new TrailerNotAvailableException(
                                                LocalizationProviderHelper.GetLocalizedValue<string>("TrailerNotAvailable"))));
                                    Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                                    return;
                                }
                            }
                            else
                            {
                                Logger.Error(
                                    $"Failed loading movie's trailer: {movie.Title}");
                                Messenger.Default.Send(
                                    new ManageExceptionMessage(
                                        new TrailerNotAvailableException(
                                            LocalizationProviderHelper.GetLocalizedValue<string>("TrailerNotAvailable"))));
                                Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                                return;
                            }

                            Logger.Info(
                                $"Movie's trailer loaded: {movie.Title}");
                            Messenger.Default.Send(new PlayTrailerMessage(trailer, movie.Title,
                                () =>
                                {
                                    Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                                }, MediaType.Movie));
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "GetMovieTrailerAsync cancelled.");
                        Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"GetMovieTrailerAsync: {exception.Message}");
                        Messenger.Default.Send(
                            new ManageExceptionMessage(
                                new TrailerNotAvailableException(
                                    LocalizationProviderHelper.GetLocalizedValue<string>(
                                        "TrailerNotAvailable"))));
                        Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Movie));
                    }
                }, ct);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}