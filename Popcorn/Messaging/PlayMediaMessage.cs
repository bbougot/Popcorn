using System;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Models.Bandwidth;

namespace Popcorn.Messaging
{
    public class PlayMediaMessage : MessageBase
    {
        /// <summary>
        /// The buffer progress
        /// </summary>
        public readonly Progress<double> BufferProgress;

        /// <summary>
        /// The buffer progress
        /// </summary>
        public readonly Progress<BandwidthRate> BandwidthRate;

        /// <summary>
        /// The media path
        /// </summary>
        public readonly string MediaPath;

        /// <summary>
        /// The playing progress
        /// </summary>
        public readonly IProgress<double> PlayingProgress;

        /// <summary>
        /// Initialize a new instance of PlayMediaMessage class
        /// </summary>
        /// <param name="mediaPath">The media path</param>
        /// <param name="bufferProgress">The buffer progress</param>
        /// <param name="bandwidthRate">The bandwidth rate</param>
        /// <param name="playingProgress">The playing progress</param>
        public PlayMediaMessage(string mediaPath, Progress<double> bufferProgress, Progress<BandwidthRate> bandwidthRate, IProgress<double> playingProgress)
        {
            MediaPath = mediaPath;
            BufferProgress = bufferProgress;
            BandwidthRate = bandwidthRate;
            PlayingProgress = playingProgress;
        }
    }
}
