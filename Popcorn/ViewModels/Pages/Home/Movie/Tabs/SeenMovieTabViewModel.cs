using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Comparers;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Movie;
using Popcorn.Services.Application;
using Popcorn.Services.Movies.Movie;
using Popcorn.Services.User;

namespace Popcorn.ViewModels.Pages.Home.Movie.Tabs
{
    public class SeenMovieTabViewModel : MovieTabsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the SeenMovieTabViewModel class.
        /// </summary>
        /// <param name="applicationService">Application state</param>
        /// <param name="movieService">Movie service</param>
        /// <param name="userService">Movie history service</param>
        public SeenMovieTabViewModel(IApplicationService applicationService, IMovieService movieService,
            IUserService userService)
            : base(applicationService, movieService, userService,
                () => LocalizationProviderHelper.GetLocalizedValue<string>("SeenTitleTab"))
        {
        }

        /// <summary>
        /// Load movies asynchronously
        /// </summary>
        public override async Task LoadMoviesAsync(bool reset = false)
        {
            await LoadingSemaphore.WaitAsync(CancellationLoadingMovies.Token);
            StopLoadingMovies();
            if (reset)
            {
                Movies.Clear();
                Page = 0;
                VerticalScroll = 0d;
            }

            var watch = Stopwatch.StartNew();
            Page++;
            if (Page > 1 && Movies.Count == MaxNumberOfMovies && !NeedSync)
            {
                Page--;
                LoadingSemaphore.Release();
                return;
            }

            Logger.Trace(
                $"Loading movies seen page {Page}...");
            HasLoadingFailed = false;
            try
            {
                IsLoadingMovies = true;
                var imdbIds = UserService.GetSeenMovies(Page);

                var moviesToDelete = Movies.Select(a => a.ImdbId).Except(imdbIds.allMovies);
                var moviesToAdd = imdbIds.movies.Except(Movies.Select(a => a.ImdbId));

                foreach (var movie in moviesToDelete.ToList())
                {
                    Movies.Remove(Movies.FirstOrDefault(a => a.ImdbId == movie));
                }

                var movies = moviesToAdd.ToList();
                var moviesToAddAndToOrder = new List<MovieLightJson>();

                try
                {
                    if (movies.Any())
                    {
                        var movieByIds = await MovieService.GetMoviesByIds(movies, CancellationLoadingMovies.Token);
                        foreach (var movie in movieByIds.movies)
                        {
                            if ((Genre == null || movie.Genres.Contains(Genre.EnglishName)) &&
                                movie.Rating >= Rating)
                            {
                                moviesToAddAndToOrder.Add(movie);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                foreach (var movie in moviesToAddAndToOrder.Except(Movies.ToList(), new MovieLightComparer()))
                {
                    var pair = Movies
                        .Select((value, index) => new {value, index})
                        .FirstOrDefault(x => string.CompareOrdinal(x.value.Title, movie.Title) > 0);

                    if (pair == null)
                    {
                        Movies.Add(movie);
                    }
                    else
                    {
                        Movies.Insert(pair.index, movie);
                    }
                }

                IsLoadingMovies = false;
                IsMovieFound = Movies.Any();
                CurrentNumberOfMovies = Movies.Count;
                MaxNumberOfMovies = imdbIds.nbMovies;
                UserService.SyncMovieHistory(Movies);
            }
            catch (Exception exception)
            {
                Page--;
                Logger.Error(
                    $"Error while loading movies seen page {Page}: {exception.Message}");
                HasLoadingFailed = true;
                Messenger.Default.Send(new ManageExceptionMessage(exception));
            }
            finally
            {
                NeedSync = false;
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Logger.Trace(
                    $"Loaded movies seen page {Page} in {elapsedMs} milliseconds.");
                LoadingSemaphore.Release();
            }
        }
    }
}