using System.Runtime.Serialization;

namespace Popcorn.Models.Image
{
    [DataContract]
    public class ImageShowJson
    {
        [DataMember(Name = "poster")]
        public string Poster { get; set; }

        [DataMember(Name = "banner")]
        public string Banner { get; set; }
    }
}
