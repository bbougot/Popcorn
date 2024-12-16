using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Popcorn.Models.Movie
{
    [DataContract]
    public class MovieResponse
    {
        [DataMember(Name = "totalMovies")]
        public int TotalMovies { get; set; }

        [DataMember(Name = "movies")]
        public List<MovieJson> Movies { get; set; }
    }
}
