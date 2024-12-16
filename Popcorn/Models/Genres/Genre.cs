using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Popcorn.Models.Genres
{
    [DataContract]
    public class GenreJson
    {
        [DataMember(Name = "EnglishName")]
        public string EnglishName { get; set; }

        [DataMember(Name = "Name")]
        public string Name { get; set; }
    }

    [DataContract]
    public class GenreResponse
    {
        [DataMember(Name = "genres")]
        public List<GenreJson> Genres { get; set; }
    }
}
