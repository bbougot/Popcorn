using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Comparers;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Shows;
using Popcorn.Services.Application;
using Popcorn.Services.Shows.Show;
using Popcorn.Services.User;

namespace Popcorn.ViewModels.Pages.Home.Show.Tabs
{
    public class FavoritesShowTabViewModel : ShowTabsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the FavoritesMovieTabViewModel class.
        /// </summary>
        /// <param name="applicationService">Application state</param>
        /// <param name="showService">Show service</param>
        /// <param name="userService">User service</param>
        public FavoritesShowTabViewModel(IApplicationService applicationService, IShowService showService,
            IUserService userService)
            : base(applicationService, showService, userService,
                () => LocalizationProviderHelper.GetLocalizedValue<string>("FavoritesTitleTab"))
        {
        }

        /// <summary>
        /// Load movies asynchronously
        /// </summary>
        public override async Task LoadShowsAsync(bool reset = false)
        {
            await LoadingSemaphore.WaitAsync(CancellationLoadingShows.Token);
            StopLoadingShows();
            if (reset)
            {
                Shows.Clear();
                Page = 0;
                VerticalScroll = 0d;
            }

            var watch = Stopwatch.StartNew();
            Page++;
            if (Page > 1 && Shows.Count == MaxNumberOfShows && !NeedSync)
            {
                Page--;
                LoadingSemaphore.Release();
                return;
            }

            Logger.Trace(
                $"Loading shows favorite page {Page}...");
            HasLoadingFailed = false;
            try
            {
                IsLoadingShows = true;
                var imdbIds = UserService.GetFavoritesShows(Page);

                var showsToDelete = Shows.Select(a => a.ImdbId).Except(imdbIds.allShows);
                var showsToAdd = imdbIds.shows.Except(Shows.Select(a => a.ImdbId));

                foreach (var movie in showsToDelete.ToList())
                {
                    Shows.Remove(Shows.FirstOrDefault(a => a.ImdbId == movie));
                }

                var shows = showsToAdd.ToList();
                var showsToAddAndToOrder = new List<ShowLightJson>();
                try
                {
                    var showByIds = await ShowService.GetShowsByIds(shows, CancellationLoadingShows.Token);
                    foreach (var show in showByIds.movies)
                    {
                        if ((Genre == null || show.Genres.Contains(Genre.EnglishName)) &&
                            show.Rating.Percentage >= Rating * 10)
                        {
                            showsToAddAndToOrder.Add(show);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                foreach (var show in showsToAddAndToOrder.Except(Shows.ToList(), new ShowLightComparer()))
                {
                    var pair = Shows
                        .Select((value, index) => new {value, index})
                        .FirstOrDefault(x => string.CompareOrdinal(x.value.Title, show.Title) > 0);
                    if (pair == null)
                    {
                        Shows.Add(show);
                    }
                    else
                    {
                        Shows.Insert(pair.index, show);
                    }
                }

                IsLoadingShows = false;
                IsShowFound = Shows.Any();
                CurrentNumberOfShows = Shows.Count;
                MaxNumberOfShows = imdbIds.nbShows;
                UserService.SyncShowHistory(Shows);
            }
            catch (Exception exception)
            {
                Page--;
                Logger.Error(
                    $"Error while loading shows favorite page {Page}: {exception.Message}");
                HasLoadingFailed = true;
                Messenger.Default.Send(new ManageExceptionMessage(exception));
            }
            finally
            {
                NeedSync = false;
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Logger.Trace(
                    $"Loaded shows favorite page {Page} in {elapsedMs} milliseconds.");
                LoadingSemaphore.Release();
            }
        }
    }
}