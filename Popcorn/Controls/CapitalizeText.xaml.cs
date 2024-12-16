using System.Threading;
using System.Windows;

namespace Popcorn.Controls
{
    /// <summary>
    /// Interaction logic for CapitalizeText.xaml
    /// </summary>
    public partial class CapitalizeText
    {
        /// <summary>
        /// Text property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                typeof(string), typeof(CapitalizeText),
                new PropertyMetadata(string.Empty, OnTextChanged));

        /// <summary>
        /// Initialize a new instance of CapitalizeText
        /// </summary>
        public CapitalizeText()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The text to capitalize
        /// </summary>
        public string Text
        {
            private get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// On text changed
        /// </summary>
        /// <param name="d">Dependency object</param>
        /// <param name="e">Event args</param>
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var capitalizeText = d as CapitalizeText;
            capitalizeText?.DisplayCapitalizedText();
        }

        /// <summary>
        /// Display capitalized text
        /// </summary>
        private void DisplayCapitalizedText()
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;

            DisplayText.Text = textInfo.ToTitleCase(Text);
        }
    }
}