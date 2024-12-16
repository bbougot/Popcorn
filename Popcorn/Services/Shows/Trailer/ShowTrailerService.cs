using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Shows;
using Popcorn.Services.Shows.Show;
using Popcorn.Exceptions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;
using Popcorn.Enums;

namespace Popcorn.Services.Shows.Trailer
{
    public class ShowTrailerService : IShowTrailerService
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The service used to interact with shows
        /// </summary>
        private IShowService ShowService { get; }

        /// <summary>
        /// Initializes a new instance of the ShowTrailerService class.
        /// </summary>
        /// <param name="showService">Show service</param>
        public ShowTrailerService(IShowService showService)
        {
            ShowService = showService;
        }

        /// <summary>
        /// Load movie's trailer asynchronously
        /// </summary>
        /// <param name="show">The show</param>
        /// <param name="ct">Cancellation token</param>
        public async Task LoadTrailerAsync(ShowJson show, CancellationToken ct)
        {
            var timeoutPolicy =
                Policy.TimeoutAsync(Constants.DefaultRequestTimeoutInSecond, TimeoutStrategy.Optimistic);
            try
            {
                await timeoutPolicy.ExecuteAsync(async cancellation =>
                {
                    try
                    {
                        var trailer = await ShowService.GetShowTrailerAsync(show, cancellation);
                        if (!ct.IsCancellationRequested && string.IsNullOrEmpty(trailer))
                        {
                            Logger.Error(
                                $"Failed loading show's trailer: {show.Title}");
                            Messenger.Default.Send(
                                new ManageExceptionMessage(
                                    new TrailerNotAvailableException(
                                        LocalizationProviderHelper.GetLocalizedValue<string>("TrailerNotAvailable"))));
                            Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show));
                            return;
                        }

                        if (!ct.IsCancellationRequested)
                        {
                            Uri uriResult;
                            bool result = Uri.TryCreate(trailer, UriKind.Absolute, out uriResult)
                                          && (uriResult.Scheme == Uri.UriSchemeHttp ||
                                              uriResult.Scheme == Uri.UriSchemeHttps);
                            if (result)
                            {
                                var client = new HttpClient();
                                try
                                {
                                    using (var response = await client.GetAsync(uriResult,
                                        HttpCompletionOption.ResponseHeadersRead, ct))
                                    {
                                        if (response == null || response.StatusCode != HttpStatusCode.OK)
                                        {
                                            Logger.Error(
                                                $"Failed loading show's trailer: {show.Title}");
                                            Messenger.Default.Send(
                                                new ManageExceptionMessage(
                                                    new TrailerNotAvailableException(
                                                        LocalizationProviderHelper.GetLocalizedValue<string>(
                                                            "TrailerNotAvailable"))));
                                            Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show));
                                            return;
                                        }
                                    }
                                }
                                catch (WebException)
                                {
                                    Logger.Error(
                                        $"Failed loading show's trailer: {show.Title}");
                                    Messenger.Default.Send(
                                        new ManageExceptionMessage(
                                            new TrailerNotAvailableException(
                                                LocalizationProviderHelper.GetLocalizedValue<string>(
                                                    "TrailerNotAvailable"))));
                                    Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show));
                                    return;
                                }
                            }
                            else
                            {
                                Logger.Error(
                                    $"Failed loading show's trailer: {show.Title}");
                                Messenger.Default.Send(
                                    new ManageExceptionMessage(
                                        new TrailerNotAvailableException(
                                            LocalizationProviderHelper
                                                .GetLocalizedValue<string>("TrailerNotAvailable"))));
                                Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show));
                                return;
                            }

                            Logger.Info(
                                $"Show's trailer loaded: {show.Title}");
                            Messenger.Default.Send(new PlayTrailerMessage(trailer, show.Title,
                                () => { Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show)); },
                                MediaType.Show));
                        }
                    }
                    catch (Exception exception) when (exception is TaskCanceledException)
                    {
                        Logger.Debug(
                            "LoadTrailerAsync cancelled.");
                        Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show));
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(
                            $"LoadTrailerAsync: {exception.Message}");
                        Messenger.Default.Send(
                            new ManageExceptionMessage(
                                new TrailerNotAvailableException(
                                    LocalizationProviderHelper.GetLocalizedValue<string>(
                                        "TrailerNotAvailable"))));
                        Messenger.Default.Send(new StopPlayingTrailerMessage(MediaType.Show));
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