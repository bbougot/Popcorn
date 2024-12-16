using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Popcorn.Events;
using Popcorn.Helpers;
using Popcorn.Models.Episode;
using Popcorn.ViewModels.Pages.Home.Show.Details;

namespace Popcorn.UserControls.Home.Show.Details
{
    /// <summary>
    /// Logique d'interaction pour ShowDetailsUserControl.xaml
    /// </summary>
    public partial class ShowDetailsUserControl : UserControl
    {
        public ShowDetailsUserControl()
        {
            InitializeComponent();
        }

        private void OnSelectedSeasonChanged(object sender, SelectedSeasonChangedEventArgs e)
        {
            var vm = DataContext as ShowDetailsViewModel;
            if (vm == null) return;

            var episodes = vm.Show.Episodes.Where(a => a.Season == e.SelectedSeasonNumber);
            EpisodesDetails.ItemsSource = new ObservableCollection<EpisodeShowJson>(episodes.OrderBy(a => a.EpisodeNumber));
        }
    }
}
