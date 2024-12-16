using System.Collections.Generic;
using System.Threading.Tasks;
using OSDB;
using SubtitlesParser.Classes;

namespace Popcorn.Services.Subtitles
{
    /// <summary>
    /// The subtitles service
    /// </summary>
    public interface ISubtitlesService
    {
        /// <summary>
        /// Get subtitles languages
        /// </summary>
        /// <returns>Languages</returns>
        Task<IEnumerable<OSDB.Models.Language>> GetSubLanguages();

        /// <summary>
        /// Search subtitles by imdb code and languages
        /// </summary>
        /// <param name="languages">Languages</param>
        /// <param name="imdbId">Imdb code</param>
        /// <param name="season">Season number</param>
        /// <param name="episode">Episode number</param>
        /// <returns></returns>
        Task<IList<OSDB.Models.Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId, int? season, int? episode);

        /// <summary>
        /// Download a subtitle to a path
        /// </summary>
        /// <param name="path">Path to download</param>
        /// <param name="subtitle">Subtitle to download</param>
        /// <param name="remote">Is remote download path</param>
        /// <returns>Downloaded subtitle path</returns>
        Task<string> DownloadSubtitleToPath(string path, OSDB.Models.Subtitle subtitle, bool remote = true);

        /// <summary>
        /// Convert a .srt file to a .vtt file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Path to the converted vtt file</returns>
        string ConvertSrtToVtt(string path);

        /// <summary>
        /// Get captions from file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string LoadCaptions(string filePath);
    }
}
