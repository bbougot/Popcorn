using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Popcorn.Controls
{
    /// <summary>
    /// Interaction logic for GenresLabel.xaml
    /// </summary>
    public partial class GenresLabel
    {
        /// <summary>
        /// Genres property
        /// </summary>
        public static readonly DependencyProperty GenresProperty =
            DependencyProperty.Register("Genres",
                typeof(IEnumerable<string>), typeof(GenresLabel),
                new PropertyMetadata(null, OnGenresChanged));

        /// <summary>
        /// Initialize a new instance of GenresLabel
        /// </summary>
        public GenresLabel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The movie runtime
        /// </summary>
        public IEnumerable<string> Genres
        {
            private get { return (IEnumerable<string>) GetValue(GenresProperty); }
            set { SetValue(GenresProperty, value); }
        }

        /// <summary>
        /// On genres changed
        /// </summary>
        /// <param name="d">Dependency object</param>
        /// <param name="e">Event args</param>
        private static void OnGenresChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var movieGenres = d as GenresLabel;
            movieGenres?.DisplayMovieGenres();
        }

        /// <summary>
        /// Display movie genres
        /// </summary>
        private void DisplayMovieGenres()
        {
            var index = 0;
            if (Genres == null)
                return;

            DisplayText.Text = string.Empty;
            foreach (var genre in Genres)
            {
                index++;
                DisplayText.Text += FirstCharToUpper(genre);
                // Add the comma at the end of each genre.
                if (index != Genres.Count())
                    DisplayText.Text += ", ";
            }
        }

        /// <summary>
        /// Make first letter of a string upper case
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>First letter upper cased string</returns>
        private static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input is empty");
            return input.First().ToString().ToUpper(CultureInfo.InvariantCulture) + input.Substring(1);
        }
    }
}