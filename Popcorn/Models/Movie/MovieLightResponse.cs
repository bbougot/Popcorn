using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Popcorn.Models.Movie
{
    [DataContract]
    public class MovieLightResponse
    {
        [DataMember(Name = "totalMovies")]
        public int TotalMovies { get; set; }

        [DataMember(Name = "movies")]
        public List<MovieLightJson> Movies { get; set; }
    }
}
