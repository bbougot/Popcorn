using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Popcorn.Models.Shows
{
    [DataContract]
    public class ShowLightResponse
    {
        [DataMember(Name = "totalShows")]
        public int TotalShows { get; set; }

        [DataMember(Name = "shows")]
        public List<ShowLightJson> Shows { get; set; }
    }
}
