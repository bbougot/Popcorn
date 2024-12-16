namespace Popcorn.Models.Localization
{
    /// <summary>
    /// Interface used to describe a language
    /// </summary>
    public interface ILanguage
    {
        /// <summary>
        /// Language's name in its original language
        /// </summary>
        string LocalizedName { get; }

        /// <summary>
        /// English language's name
        /// </summary>
        string EnglishName { get; }

        /// <summary>
        /// Language's culture
        /// </summary>
        string Culture { get; }
    }
}