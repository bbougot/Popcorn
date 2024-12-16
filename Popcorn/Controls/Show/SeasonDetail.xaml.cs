using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Popcorn.Events;
using Popcorn.Models.Shows;

namespace Popcorn.Controls.Show
{
    /// <summary>
    /// Logique d'interaction pour SeasonDetail.xaml
    /// </summary>
    public partial class SeasonDetail
    {
        /// <summary>
        /// Selected Show
        /// </summary>
        public static readonly DependencyProperty ShowProperty =
            DependencyProperty.Register("Show",
                typeof(ShowJson), typeof(SeasonDetail),
                new PropertyMetadata(null, PropertyChangedCallback));

        /// <summary>
        /// The selected show
        /// </summary>
        public ShowJson Show
        {
            get => (ShowJson) GetValue(ShowProperty);
            set => SetValue(ShowProperty, value);
        }

        /// <summary>
        /// Raised when the selected season has changed
        /// </summary>
        public event EventHandler<SelectedSeasonChangedEventArgs> SelectedSeasonChanged;

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var seasons = dependencyObject as SeasonDetail;
            var collection = new ObservableCollection<Season>();
            if (seasons?.Show?.Episodes == null)
                return;

            var episodesBySeason =
                seasons.Show.Episodes.GroupBy(r => r.Season)
                    .ToDictionary(t => t.Key, t => t.Select(r => r).ToList());
            foreach (var nbSeason in episodesBySeason.Keys.OrderBy(a => a))
            {
                collection.Add(new Season
                {
                    Label = $"Season {nbSeason}",
                    Number = nbSeason ?? 0
                });
            }

            seasons.ComboSeasons.ItemsSource = collection;
            seasons.ComboSeasons.SelectedIndex = 0;
        }

        public SeasonDetail()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ComboSeasons.SelectedValue is Season selectedSeason)) return;
            SelectedSeasonChanged?.Invoke(this, new SelectedSeasonChangedEventArgs(selectedSeason.Number));
        }
    }

    public class Season
    {
        public int Number { get; set; }
        public string Label { get; set; }
    }
}