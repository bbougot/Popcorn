using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging
{
    public class ShowCustomSubtitleMessage : MessageBase
    {
        public string FileName { get; set; }

        public bool Error { get; set; }
    }
}
