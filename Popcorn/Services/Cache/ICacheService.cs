using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popcorn.Services.Cache
{
    public interface ICacheService
    {
        string Assets { get; }
        string MovieDownloads { get; }
        string DropFilesDownloads { get; }
        string ShowDownloads { get; }
        string MovieTorrentDownloads { get; }
        string Subtitles { get; }
        string PopcornTemp { get; }
    }
}
