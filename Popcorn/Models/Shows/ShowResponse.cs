using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Popcorn.Models.Shows
{
    [DataContract]
    public class ShowResponse
    {
        [DataMember(Name = "totalShows")]
        public int TotalShows { get; set; }

        [DataMember(Name = "shows")]
        public List<ShowJson> Shows { get; set; }
    }
}
