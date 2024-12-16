using System;
using System.Windows;
using Popcorn.Enums;
using Popcorn.Helpers;
using Popcorn.Utils;

namespace Popcorn.Controls
{
    /// <summary>
    /// Interaction logic for DownloadProgress.xaml
    /// </summary>
    public partial class DownloadProgress
    {
        /// <summary>
        /// Download progress property
        /// </summary>
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress",
                typeof(double), typeof(DownloadProgress),
                new PropertyMetadata(0d, OnDownloadProgressChanged));

        /// <summary>
        /// Download rate property
        /// </summary>
        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate",
                typeof(double), typeof(DownloadProgress),
                new PropertyMetadata(0d));

        /// <summary>
        /// Media title property
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title",
                typeof(string), typeof(DownloadProgress),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Media type property
        /// </summary>
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type",
                typeof(MediaType), typeof(DownloadProgress),
                new PropertyMetadata(MediaType.Movie));

        /// <summary>
        /// Initialize a new instance of DownloadProgress
        /// </summary>
        public DownloadProgress()
        {
            InitializeComponent();
            DisplayText.Text =
                $"{LocalizationProviderHelper.GetLocalizedValue<string>("BufferingLabel")} : {Math.Round(Progress * 50d, 0)} % ({Rate} kB/s)";
        }

        /// <summary>
        /// The download progress
        /// </summary>
        public double Progress
        {
            private get { return (double) GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        /// <summary>
        /// The download rate
        /// </summary>
        public double Rate
        {
            private get { return (double) GetValue(RateProperty); }
            set { SetValue(RateProperty, value); }
        }

        /// <summary>
        /// The title
        /// </summary>
        public string Title
        {
            private get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// The type
        /// </summary>
        public MediaType Type
        {
            private get { return (MediaType) GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        /// <summary>
        /// On download progress changed
        /// </summary>
        /// <param name="d">Dependency object</param>
        /// <param name="e">Event args</param>
        private static void OnDownloadProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var downloadMovieProgress = d as DownloadProgress;
            downloadMovieProgress?.DisplayDownloadProgress();
        }

        /// <summary>
        /// Display download progress
        /// </summary>
        private void DisplayDownloadProgress()
        {
            if (Type == MediaType.Movie)
            {
                if (Progress >= Constants.MinimumMovieBuffering)
                    DisplayText.Text =
                        $"{LocalizationProviderHelper.GetLocalizedValue<string>("CurrentlyPlayingLabel")} : {Title}";
                else
                    DisplayText.Text = Rate >= 1000.0
                        ? $"{LocalizationProviderHelper.GetLocalizedValue<string>("BufferingLabel")} : {Math.Round(Progress * (100d / Constants.MinimumMovieBuffering), 0)} % ({Rate / 1000d} MB/s)"
                        : $"{LocalizationProviderHelper.GetLocalizedValue<string>("BufferingLabel")} : {Math.Round(Progress * (100d / Constants.MinimumMovieBuffering), 0)} % ({Rate} kB/s)";
            }
            else if (Type == MediaType.Show)
            {
                if (Progress >= Constants.MinimumShowBuffering)
                    DisplayText.Text =
                        $"{LocalizationProviderHelper.GetLocalizedValue<string>("CurrentlyPlayingLabel")} : {Title}";
                else
                    DisplayText.Text = Rate >= 1000.0
                        ? $"{LocalizationProviderHelper.GetLocalizedValue<string>("BufferingLabel")} : {Math.Round(Progress * (100d / Constants.MinimumShowBuffering), 0)} % ({Rate / 1000d} MB/s)"
                        : $"{LocalizationProviderHelper.GetLocalizedValue<string>("BufferingLabel")} : {Math.Round(Progress * (100d / Constants.MinimumShowBuffering), 0)} % ({Rate} kB/s)";
            }
        }
    }
}