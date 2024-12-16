using System.Runtime.Serialization;

namespace Popcorn.Models.Torrent.Show
{
    [DataContract]
    public class TorrentShowNodeJson
    {
        [DataMember(Name = "torrent_0")]
        public TorrentShowJson Torrent_0 { get; set; }

        [DataMember(Name = "torrent_480p")]
        public TorrentShowJson Torrent_480p { get; set; }

        [DataMember(Name = "torrent_720p")]
        public TorrentShowJson Torrent_720p { get; set; }

        [DataMember(Name = "torrent_1080p")]
        public TorrentShowJson Torrent_1080p { get; set; }
    }
}
