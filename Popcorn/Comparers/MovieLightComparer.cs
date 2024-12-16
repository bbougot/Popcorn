using System.Collections.Generic;
using Popcorn.Models.Movie;

namespace Popcorn.Comparers
{
    /// <summary>
    /// Compare two movies
    /// </summary>
    public class MovieLightComparer : IEqualityComparer<MovieLightJson>
    {
        /// <summary>
        /// Compare two movies
        /// </summary>
        /// <param name="x">First movie</param>
        /// <param name="y">Second movie</param>
        /// <returns>True if both movies are the same, false otherwise</returns>
        public bool Equals(MovieLightJson x, MovieLightJson y)
        {
            //Check whether the compared objects reference the same data.
            if (ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            return x.ImdbId == y.ImdbId;
        }

        /// <summary>
        /// Define a unique hash code for a movie
        /// </summary>
        /// <param name="movie">A movie</param>
        /// <returns>Unique hashcode</returns>
        public int GetHashCode(MovieLightJson movie)
        {
            //Check whether the object is null
            if (ReferenceEquals(movie, null)) return 0;

            //Get hash code for the Id field
            var hashId = movie.ImdbId.GetHashCode();

            return hashId;
        }
    }
}