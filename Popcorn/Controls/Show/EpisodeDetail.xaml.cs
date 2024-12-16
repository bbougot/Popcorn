using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Extensions;
using Popcorn.Messaging;
using Popcorn.Models.Episode;
using Popcorn.Models.Torrent;

namespace Popcorn.Controls.Show
{
    /// <summary>
    /// Logique d'interaction pour EpisodeDetail.xaml
    /// </summary>
    public partial class EpisodeDetail : INotifyPropertyChanged
    {
        /// <summary>
        /// Play an episode
        /// </summary>
        private ICommand _playCommand;

        /// <summary>
        /// Selected episode
        /// </summary>
        public static readonly DependencyProperty EpisodeProperty =
            DependencyProperty.Register("Episode",
                typeof(EpisodeShowJson), typeof(EpisodeDetail),
                new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var detail = dependencyObject as EpisodeDetail;
            var episode = detail?.Episode;
            if (episode == null) return;

            detail.Title.Text = episode.Title;
            detail.SeasonNumber.Text = $"Season {episode.Season}";
            detail.EpisodeNumber.Text = $"Episode {episode.EpisodeNumber}";
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var date = dtDateTime.AddSeconds(episode.FirstAired).ToLocalTime();
            detail.Duration.Text = $"Released {date.ToShortDateString()}";
            detail.Synopsis.Text = episode.Overview;
        }

        /// <summary>
        /// The selected episode
        /// </summary>
        public EpisodeShowJson Episode
        {
            get => (EpisodeShowJson) GetValue(EpisodeProperty);
            set => SetValue(EpisodeProperty, value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EpisodeDetail()
        {
            InitializeComponent();
            PlayCommand = new RelayCommand(async () =>
            {
                var torrent480P = Episode.Torrents.Torrent_480p;
                torrent480P.Quality = "480p";

                var torrent720P = Episode.Torrents.Torrent_720p;
                torrent720P.Quality = "720p";
                Episode.AvailableTorrents = new ObservableCollection<ITorrent>(new List<ITorrent>(new List<ITorrent>
                {
                    torrent480P,
                    torrent720P
                }).Where(torrent => !string.IsNullOrWhiteSpace(torrent.Url)));

                var message = new ShowDownloadSettingsDialogMessage(Episode);
                await Messenger.Default.SendAsync(message);
                if (message.Download)
                {
                    Messenger.Default.Send(new DownloadShowEpisodeMessage(Episode));
                }
            });
        }

        /// <summary>
        /// Play an episode
        /// </summary>
        public ICommand PlayCommand
        {
            get => _playCommand;
            set
            {
                _playCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Implementation of <see cref="INotifyPropertyChanged"/>
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Event of <see cref="INotifyPropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}