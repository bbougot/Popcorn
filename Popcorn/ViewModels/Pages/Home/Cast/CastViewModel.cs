using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Popcorn.Exceptions;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Models.Movie;
using Popcorn.Services.Movies.Movie;
using TMDbLib.Objects.People;

namespace Popcorn.ViewModels.Pages.Home.Cast
{
    public class CastViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private readonly IMovieService _movieService;

        private Person _person;
        private string _mainImageUrl;
        private string _profileImageUrl;
        private bool _isLoading;
        private bool _loadingMovies;
        private ObservableCollection<MovieLightJson> _movies;

        public Person Person
        {
            get => _person;
            set => Set(ref _person, value);
        }

        public string MainImageUrl
        {
            get => _mainImageUrl;
            set => Set(ref _mainImageUrl, value);
        }

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => Set(ref _profileImageUrl, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool LoadingMovies
        {
            get => _loadingMovies;
            set => Set(ref _loadingMovies, value);
        }

        public ObservableCollection<MovieLightJson> Movies
        {
            get => _movies;
            set => Set(ref _movies, value);
        }

        public CastViewModel(IMovieService movieService)
        {
            _movieService = movieService;
            Person = new Person();
            Movies = new ObservableCollection<MovieLightJson>();
            Messenger.Default.Register<SearchCastMessage>(this, async message =>
            {
                try
                {
                    Person = new Person();
                    Movies = new ObservableCollection<MovieLightJson>();
                    IsLoading = true;
                    LoadingMovies = true;
                    ProfileImageUrl = string.Empty;
                    MainImageUrl = string.Empty;
                    Person = await _movieService.GetCast(message.Cast.ImdbCode);
                    if (Person != null)
                    {
                        if (Person.TaggedImages.Results.Any())
                        {
                            MainImageUrl = await _movieService.GetImagePathFromTmdb(
                                Person.TaggedImages.Results.Aggregate((i1, i2) => i1.Width > i2.Width ? i1 : i2)
                                    .FilePath);
                        }
                        else
                        {
                            MainImageUrl = string.Empty;
                        }

                        if (Person.Images.Profiles.Any())
                        {
                            ProfileImageUrl = await _movieService.GetImagePathFromTmdb(Person.Images.Profiles
                                .Aggregate((i1, i2) => i1.Width > i2.Width ? i1 : i2).FilePath);
                        }
                        else
                        {
                            ProfileImageUrl = string.Empty;
                        }

                        Movies = new ObservableCollection<MovieLightJson>(
                            await _movieService.GetMovieFromCast(Person.ImdbId.Substring(2), CancellationToken.None));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    Messenger.Default.Send(new NavigateToHomePageMessage());
                    Messenger.Default.Send(new ManageExceptionMessage(new PopcornException($"{LocalizationProviderHelper.GetLocalizedValue<string>("FailedLoadingLabel")} {message.Cast.Name}")));
                }
                finally
                {
                    IsLoading = false;
                    LoadingMovies = false;
                }
            });
        }
    }
}