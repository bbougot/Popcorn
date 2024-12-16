using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Popcorn.Comparers;
using Popcorn.Extensions;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Services.Application;
using Popcorn.Services.Movies.Movie;
using Popcorn.Services.User;

namespace Popcorn.ViewModels.Pages.Home.Movie.Tabs
{
    /// <summary>
    /// The search movies tab
    /// </summary>
    public sealed class SearchMovieTabViewModel : MovieTabsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the SearchMovieTabViewModel class.
        /// </summary>
        /// <param name="applicationService">Application state</param>
        /// <param name="movieService">Movie service</param>
        /// <param name="userService">Movie history service</param>
        public SearchMovieTabViewModel(IApplicationService applicationService, IMovieService movieService,
            IUserService userService)
            : base(applicationService, movieService, userService,
                () => LocalizationProviderHelper.GetLocalizedValue<string>("SearchTitleTab"))
        {
        }

        /// <summary>
        /// The search filter
        /// </summary>
        public string SearchFilter { get; set; }

        /// <summary>
        /// Search movies asynchronously
        /// </summary>
        public override async Task LoadMoviesAsync(bool reset = false)
        {
            await LoadingSemaphore.WaitAsync(CancellationLoadingMovies.Token);
            await Task.Run(async () =>
            {
                StopLoadingMovies();
                if (reset)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Movies.Clear();
                        Page = 0;
                        VerticalScroll = 0d;
                    });
                }

                var watch = Stopwatch.StartNew();
                Page++;
                if (Page > 1 && Movies.Count == MaxNumberOfMovies)
                {
                    Page--;
                    LoadingSemaphore.Release();
                    return;
                }

                Logger.Info(
                    $"Loading search page {Page} with criteria: {SearchFilter}");
                HasLoadingFailed = false;
                try
                {
                    IsLoadingMovies = true;
                    var result =
                        await MovieService.SearchMoviesAsync(SearchFilter,
                            Page,
                            MaxMoviesPerPage,
                            Genre,
                            Rating,
                            "year",
                            CancellationLoadingMovies.Token);

                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Movies.AddRange(result.movies.Except(Movies, new MovieLightComparer()));
                        IsLoadingMovies = false;
                        IsMovieFound = Movies.Any();
                        CurrentNumberOfMovies = Movies.Count;
                        MaxNumberOfMovies = result.nbMovies;
                        UserService.SyncMovieHistory(Movies);
                    });
                }
                catch (Exception exception)
                {
                    Page--;
                    Logger.Error(
                        $"Error while loading search page {Page} with criteria {SearchFilter}: {exception.Message}");
                    HasLoadingFailed = true;
                    Messenger.Default.Send(new ManageExceptionMessage(exception));
                }
                finally
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Logger.Info(
                        $"Loaded search page {Page} with criteria {SearchFilter} in {elapsedMs} milliseconds.");
                    LoadingSemaphore.Release();
                }
            });
        }
    }
}