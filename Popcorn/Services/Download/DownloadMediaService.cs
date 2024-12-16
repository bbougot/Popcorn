using GalaSoft.MvvmLight.Messaging;
using Popcorn.Messaging;
using Popcorn.Models.Media;
using System;
using Popcorn.Models.Bandwidth;
using Popcorn.Services.Cache;

namespace Popcorn.Services.Download
{
    /// <summary>
    /// Media download service for torrent download
    /// </summary>
    /// <typeparam name="T"><see cref="IMediaFile"/></typeparam>
    public class DownloadMediaService<T> : DownloadService<T> where T : MediaFile
    {
        public DownloadMediaService(ICacheService cacheService) : base(cacheService)
        {
        }

        /// <summary>
        /// Action to execute when a media has been buffered
        /// </summary>
        /// <param name="media"><see cref="MediaFile"/></param>
        /// <param name="reportDownloadProgress">Download progress</param>
        /// <param name="reportBandwidthRate">The bandwidth rate</param>
        /// <param name="playingProgress">The playing progress</param>
        protected override void BroadcastMediaBuffered(T media, Progress<double> reportDownloadProgress, Progress<BandwidthRate> reportBandwidthRate, IProgress<double> playingProgress)
        {
            Messenger.Default.Send(new PlayMediaMessage(media.FilePath, reportDownloadProgress, reportBandwidthRate, playingProgress));
        }
    }
}