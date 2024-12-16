using GalaSoft.MvvmLight;

namespace Popcorn.Models.Localization
{
    /// <summary>
    /// Custom language which is not yet represented into the application
    /// </summary>
    public sealed class CustomLanguage : ObservableObject, ILanguage
    {
        /// <summary>
        /// Language's name
        /// </summary>
        public string LocalizedName { get; set; }

        /// <summary>
        /// English language's name
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// Language's culture
        /// </summary>
        public string Culture { get; set; }
    }
}