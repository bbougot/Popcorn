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
    public class RecommendationsMovieTabViewModel : MovieTabsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the RecommendationsMovieTabViewModel class.
        /// </summary>
        /// <param name="applicationService">Application state</param>
        /// <param name="movieService">Movie service</param>
        /// <param name="userService">Movie history service</param>
        public RecommendationsMovieTabViewModel(IApplicationService applicationService, IMovieService movieService,
            IUserService userService)
            : base(applicationService, movieService, userService,
                () => LocalizationProviderHelper.GetLocalizedValue<string>("RecommendationsTitleTab"))
        {
            SortBy = "seeds";
        }

        /// <summary>
        /// Load movies asynchronously
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

                StopLoadingMovies();
                Logger.Trace(
                    $"Loading page {Page}...");
                HasLoadingFailed = false;
                try
                {
                    IsLoadingMovies = true;
                    var getMoviesWatcher = new Stopwatch();
                    getMoviesWatcher.Start();
                    var seen = UserService.GetSeenMovies(Page);
                    var favorites = UserService.GetFavoritesMovies(Page);
                    var movies = seen.allMovies.Union(favorites.allMovies).Distinct().ToList();
                    var result = await MovieService
                        .GetSimilar(Page,
                            MaxMoviesPerPage,
                            Rating,
                            SortBy, movies,
                            CancellationLoadingMovies.Token, Genre);
                    getMoviesWatcher.Stop();
                    var getMoviesEllapsedTime = getMoviesWatcher.ElapsedMilliseconds;
                    if (reset && getMoviesEllapsedTime < 500)
                    {
                        // Wait for VerticalOffset to reach 0 (animation lasts 500ms)
                        await Task.Delay(500 - (int) getMoviesEllapsedTime, CancellationLoadingMovies.Token);
                    }

                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        Movies.AddRange(result.movies.Except(Movies, new MovieLightComparer()));
                        IsLoadingMovies = false;
                        IsMovieFound = Movies.Any();
                        CurrentNumberOfMovies = Movies.Count;
                        MaxNumberOfMovies = result.nbMovies == 0 ? Movies.Count : result.nbMovies;
                        UserService.SyncMovieHistory(Movies);
                    });
                }
                catch (Exception exception)
                {
                    Page--;
                    Logger.Error(
                        $"Error while loading page {Page}: {exception.Message}");
                    HasLoadingFailed = true;
                    Messenger.Default.Send(new ManageExceptionMessage(exception));
                }
                finally
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Logger.Trace(
                        $"Loaded page {Page} in {elapsedMs} milliseconds.");
                    LoadingSemaphore.Release();
                }
            });
        }
    }
}