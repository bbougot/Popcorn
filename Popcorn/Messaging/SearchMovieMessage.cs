using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging
{
    /// <summary>
    /// Used to broadcast the search of a movie
    /// </summary>
    public class SearchMovieMessage : MessageBase
    {
        /// <summary>
        /// The search filter
        /// </summary>
        public readonly string Filter;

        /// <summary>
        /// Initialize a new instance of SearchMovieMessage class
        /// </summary>
        /// <param name="filter">Filter use as criteria for search</param>
        public SearchMovieMessage(string filter)
        {
            Filter = filter;
        }
    }
}