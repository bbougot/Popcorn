using GalaSoft.MvvmLight.Messaging;
using Popcorn.Models.Episode;

namespace Popcorn.Messaging
{
    /// <summary>
    /// Download an episode of a show
    /// </summary>
    public class DownloadShowEpisodeMessage : MessageBase
    {
        /// <summary>
        /// Episode
        /// </summary>
        public readonly EpisodeShowJson Episode;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="episode">Episode</param>
        public DownloadShowEpisodeMessage(EpisodeShowJson episode)
        {
            Episode = episode;
        }
    }
}
