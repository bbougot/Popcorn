using GalaSoft.MvvmLight.Messaging;
using Popcorn.Models.Movie;

namespace Popcorn.Messaging
{
    /// <summary>
    /// Used to broadcast a downloading movie message action
    /// </summary>
    public class DownloadMovieMessage : MessageBase
    {
        /// <summary>
        /// The movie to download
        /// </summary>
        public readonly MovieJson Movie;

        /// <summary>
        /// Initialize a new instance of DownloadMovieMessage class
        /// </summary>
        /// <param name="movie">The movie to download</param>
        public DownloadMovieMessage(MovieJson movie)
        {
            Movie = movie;
        }
    }
}