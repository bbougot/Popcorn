using System.Runtime.Serialization;

namespace Popcorn.Models.Trailer
{
    [DataContract]
    public class TrailerResponse
    {
        [DataMember(Name = "trailer_url")]
        public string TrailerUrl { get; set; }
    }
}
