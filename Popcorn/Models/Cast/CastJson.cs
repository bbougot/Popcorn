using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace Popcorn.Models.Cast
{
    [DataContract]
    public class CastJson : ObservableObject
    {
        private string _characterName;
        private string _name;
        private string _smallImage;
        private string _imdbCode;

        [DataMember(Name = "name")]
        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        [DataMember(Name = "characterName")]
        public string CharacterName
        {
            get => _characterName;
            set { Set(() => CharacterName, ref _characterName, value); }
        }

        [DataMember(Name = "smallImage")]
        public string SmallImage
        {
            get => _smallImage;
            set { Set(() => SmallImage, ref _smallImage, value); }
        }

        [DataMember(Name = "imdbCode")]
        public string ImdbCode
        {
            get => _imdbCode;
            set { Set(() => ImdbCode, ref _imdbCode, value); }
        }
    }
}