using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSDB.Models;

namespace OSDB
{
    public interface IOsdbClient
    {
        Task<IList<Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId, int? season, int? episode);
        Task<IEnumerable<Language>> GetSubLanguages();
        Task<string> DownloadSubtitleToPath(string path, Subtitle subtitle, bool remote = true);
    }
}