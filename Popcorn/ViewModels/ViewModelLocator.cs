using System.Diagnostics.CodeAnalysis;
using Enterwell.Clients.Wpf.Notifications;
using GalaSoft.MvvmLight.Ioc;
using Popcorn.Services.Application;
using Popcorn.Services.Cache;
using Popcorn.Services.Genres;
using Popcorn.Services.Movies.Movie;
using Popcorn.Services.Movies.Trailer;
using Popcorn.Services.Shows.Show;
using Popcorn.Services.Subtitles;
using Popcorn.Services.User;
using Popcorn.ViewModels.Pages.Home;
using Popcorn.ViewModels.Pages.Home.Movie;
using Popcorn.ViewModels.Pages.Home.Movie.Details;
using Popcorn.ViewModels.Pages.Home.Show;
using Popcorn.ViewModels.Pages.Home.Show.Details;
using Popcorn.ViewModels.Windows;
using Popcorn.Services.Shows.Trailer;
using Popcorn.Services.Tmdb;
using Popcorn.ViewModels.Pages.Home.Cast;
using Popcorn.ViewModels.Pages.Home.Settings;
using Popcorn.ViewModels.Pages.Home.Settings.About;
using Popcorn.ViewModels.Pages.Home.Settings.ApplicationSettings;
using Popcorn.ViewModels.Pages.Home.Settings.Help;

namespace Popcorn.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public static void Setup()
        {
            #region Services

            var tmdbService = new TmdbService();
            var movieService = new MovieService(tmdbService);
            var showService = new ShowService(tmdbService);
            SimpleIoc.Default.Register<IUserService>(() => new UserService(movieService, showService));
            SimpleIoc.Default.Register<IMovieService>(() => movieService);
            SimpleIoc.Default.Register<IShowService>(() => showService);
            SimpleIoc.Default.Register<IMovieTrailerService, MovieTrailerService>();
            SimpleIoc.Default.Register<IShowTrailerService, ShowTrailerService>();
            SimpleIoc.Default.Register<IApplicationService, ApplicationService>();
            SimpleIoc.Default.Register<ISubtitlesService, SubtitlesService>();
            SimpleIoc.Default.Register<IGenreService, GenreService>();
            SimpleIoc.Default.Register<ICacheService, CacheService>();
            SimpleIoc.Default.Register<NotificationMessageManager>();

            #endregion

            #region ViewModels

            SimpleIoc.Default.Register<WindowViewModel>();
            SimpleIoc.Default.Register<PagesViewModel>();

            SimpleIoc.Default.Register<MoviePageViewModel>();
            SimpleIoc.Default.Register<MovieDetailsViewModel>();

            SimpleIoc.Default.Register<ShowPageViewModel>();
            SimpleIoc.Default.Register<ShowDetailsViewModel>();

            SimpleIoc.Default.Register<CastViewModel>();

            SimpleIoc.Default.Register<SettingsPageViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<ApplicationSettingsViewModel>();
            SimpleIoc.Default.Register<HelpViewModel>();

            #endregion
        }

        /// <summary>
        /// Gets the Window property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public WindowViewModel Window => SimpleIoc.Default.GetInstance<WindowViewModel>();

        /// <summary>
        /// Gets the Pages property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public PagesViewModel Pages => SimpleIoc.Default.GetInstance<PagesViewModel>();

        /// <summary>
        /// Gets the MovieDetails property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MovieDetailsViewModel MovieDetails => SimpleIoc.Default.GetInstance<MovieDetailsViewModel>();

        /// <summary>
        /// Gets the ShowDetails property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ShowDetailsViewModel ShowDetails => SimpleIoc.Default.GetInstance<ShowDetailsViewModel>();

        /// <summary>
        /// Gets the Cast property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public CastViewModel Cast => SimpleIoc.Default.GetInstance<CastViewModel>();

        /// <summary>
        /// Gets the SettingsPage property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsPageViewModel SettingsPage
            => SimpleIoc.Default.GetInstance<SettingsPageViewModel>();

        /// <summary>
        /// Gets the About property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public AboutViewModel About
            => SimpleIoc.Default.GetInstance<AboutViewModel>();

        /// <summary>
        /// Gets the About property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public HelpViewModel Help
            => SimpleIoc.Default.GetInstance<HelpViewModel>();

        /// <summary>
        /// Gets the ApplicationSettings property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public ApplicationSettingsViewModel ApplicationSettings
            => SimpleIoc.Default.GetInstance<ApplicationSettingsViewModel>();

        /// <summary>
        /// Gets the NotificationMessageManager property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public NotificationMessageManager Manager
            => SimpleIoc.Default.GetInstance<NotificationMessageManager>();
    }
}