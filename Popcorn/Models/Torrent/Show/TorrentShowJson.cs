using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace Popcorn.Models.Torrent.Show
{
    [DataContract]
    public class TorrentShowJson : ObservableObject, ITorrent
    {
        private int? _seeds;

        private int? _peers;

        private string _quality;

        [DataMember(Name = "provider")]
        public string Provider { get; set; }

        [DataMember(Name = "peers")]
        public int? Peers
        {
            get => _peers;
            set { Set(() => Peers, ref _peers, value); }
        }

        [DataMember(Name = "seeds")]
        public int? Seeds
        {
            get => _seeds;
            set { Set(() => Seeds, ref _seeds, value); }
        }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        public string Size { get; set; }

        public string Quality
        {
            get => _quality;
            set { Set(() => Quality, ref _quality, value); }
        }
    }
}