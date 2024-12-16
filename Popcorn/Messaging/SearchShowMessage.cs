using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging
{
    /// <summary>
    /// Used to broadcast the search of a show
    /// </summary>
    public class SearchShowMessage : MessageBase
    {
        /// <summary>
        /// The search filter
        /// </summary>
        public readonly string Filter;

        /// <summary>
        /// Initialize a new instance of SearchShowMessage class
        /// </summary>
        /// <param name="filter">Filter use as criteria for search</param>
        public SearchShowMessage(string filter)
        {
            Filter = filter;
        }
    }
}
