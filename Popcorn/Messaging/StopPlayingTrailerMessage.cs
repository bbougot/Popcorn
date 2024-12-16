using GalaSoft.MvvmLight.Messaging;
using Popcorn.Enums;
using Popcorn.Utils;

namespace Popcorn.Messaging
{
    /// <summary>
    /// Used to broadcast the stop of a trailer playing
    /// </summary>
    public class StopPlayingTrailerMessage : MessageBase
    {
        public MediaType Type { get; set; }

        public StopPlayingTrailerMessage(MediaType type)
        {
            Type = type;
        }
    }
}