using System.Windows;

namespace Popcorn.Controls.Show
{
    /// <summary>
    /// Logique d'interaction pour ShowNumberSummary.xaml
    /// </summary>
    public partial class ShowNumberSummary
    {
        /// <summary>
        /// Max number property
        /// </summary>
        public static readonly DependencyProperty MaxNumberOfMoviesProperty =
            DependencyProperty.Register("MaxNumberOfShows",
                typeof(double), typeof(ShowNumberSummary),
                new PropertyMetadata(0d, OnNumberOfShowsChanged));

        /// <summary>
        /// Current number property
        /// </summary>
        public static readonly DependencyProperty CurrentNumberOfMoviesProperty =
            DependencyProperty.Register("CurrentNumberOfShows",
                typeof(double), typeof(ShowNumberSummary),
                new PropertyMetadata(0d, OnNumberOfShowsChanged));

        /// <summary>
        /// Initialize a new instance of ShowNumberSummary
        /// </summary>
        public ShowNumberSummary()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The maximum number of shows
        /// </summary>
        public double MaxNumberOfShows
        {
            private get { return (double) GetValue(MaxNumberOfMoviesProperty); }
            set { SetValue(MaxNumberOfMoviesProperty, value); }
        }

        /// <summary>
        /// The current number of shows
        /// </summary>
        public double CurrentNumberOfShows
        {
            private get { return (double) GetValue(CurrentNumberOfMoviesProperty); }
            set { SetValue(CurrentNumberOfMoviesProperty, value); }
        }

        /// <summary>
        /// On number of shows changed
        /// </summary>
        /// <param name="d">Dependency object</param>
        /// <param name="e">Event args</param>
        private static void OnNumberOfShowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var showNumberSummary = d as ShowNumberSummary;
            showNumberSummary?.DisplayShowsNumberSummary();
        }

        /// <summary>
        /// Display shows summary
        /// </summary>
        private void DisplayShowsNumberSummary()
        {
            if (CurrentNumberOfShows.Equals(MaxNumberOfShows))
            {
                MaxShows.Visibility = Visibility.Collapsed;
                CurrentShows.Visibility = Visibility.Visible;

                CurrentShows.Text =
                    $"{CurrentNumberOfShows}";
            }
            else
            {
                MaxShows.Visibility = Visibility.Visible;
                CurrentShows.Visibility = Visibility.Visible;

                CurrentShows.Text =
                    $"{CurrentNumberOfShows}";
                MaxShows.Text =
                    $"{MaxNumberOfShows}";
            }
        }
    }
}