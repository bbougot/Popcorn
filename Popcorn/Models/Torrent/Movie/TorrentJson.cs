using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace Popcorn.Models.Torrent.Movie
{
    [DataContract]
    public class TorrentJson : ObservableObject, ITorrent
    {
        private string _dateUploaded;

        private int _dateUploadedUnix;

        private string _hash;

        private int? _peers;

        private string _quality;

        private int? _seeds;

        private string _size;

        private long? _sizeBytes;

        private string _url;

        [DataMember(Name = "url")]
        public string Url
        {
            get => _url;
            set { Set(() => Url, ref _url, value); }
        }

        [DataMember(Name = "hash")]
        public string Hash
        {
            get => _hash;
            set { Set(() => Hash, ref _hash, value); }
        }

        [DataMember(Name = "quality")]
        public string Quality
        {
            get => _quality;
            set { Set(() => Quality, ref _quality, value); }
        }

        [DataMember(Name = "seeds")]
        public int? Seeds
        {
            get => _seeds;
            set { Set(() => Seeds, ref _seeds, value); }
        }

        [DataMember(Name = "peers")]
        public int? Peers
        {
            get => _peers;
            set { Set(() => Peers, ref _peers, value); }
        }

        [DataMember(Name = "size")]
        public string Size
        {
            get => _size;
            set { Set(() => Size, ref _size, value); }
        }

        [DataMember(Name = "sizeBytes")]
        public long? SizeBytes
        {
            get => _sizeBytes;
            set { Set(() => SizeBytes, ref _sizeBytes, value); }
        }

        [DataMember(Name = "dateUploaded")]
        public string DateUploaded
        {
            get => _dateUploaded;
            set { Set(() => DateUploaded, ref _dateUploaded, value); }
        }

        [DataMember(Name = "dateUploadedUnix")]
        public int DateUploadedUnix
        {
            get => _dateUploadedUnix;
            set { Set(() => DateUploadedUnix, ref _dateUploadedUnix, value); }
        }
    }
}