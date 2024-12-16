using System.Runtime.Serialization;

namespace Popcorn.Models.Rating
{
    [DataContract]
    public class RatingJson
    {
        [DataMember(Name = "percentage")]
        public int? Percentage { get; set; }

        [DataMember(Name = "watching")]
        public int? Watching { get; set; }

        [DataMember(Name = "votes")]
        public int? Votes { get; set; }

        [DataMember(Name = "loved")]
        public int? Loved { get; set; }

        [DataMember(Name = "hated")]
        public int? Hated { get; set; }
    }
}
