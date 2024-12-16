using System;

namespace Popcorn.Models.Bandwidth
{
    public class BandwidthRate
    {
        public double DownloadRate { get; set; }
        public double UploadRate { get; set; }
        public TimeSpan ETA { get; set; }
    }
}
