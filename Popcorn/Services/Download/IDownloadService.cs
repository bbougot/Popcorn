using Popcorn.Models.Media;
using Popcorn.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using Popcorn.Enums;
using Popcorn.Models.Bandwidth;

namespace Popcorn.Services.Download
{
    /// <summary>
    /// Generic download service for torrent download
    /// </summary>
    /// <typeparam name="T"><see cref="IMediaFile"/></typeparam>
    public interface IDownloadService<in T> where T : IMediaFile
    {
        /// <summary>
        /// Download a torrent
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task Download(T media, TorrentType torrentType, MediaType mediaType, string torrentPath, int uploadLimit, int downloadLimit, IProgress<double> downloadProgress,
            IProgress<BandwidthRate> bandwidthRate, IProgress<int> nbSeeds, IProgress<int> nbPeers, Action buffered, Action cancelled,
            CancellationTokenSource cts);
    }
}