using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using Polly;
using OSDB;
using Popcorn.Helpers;
using Popcorn.Messaging;
using Popcorn.Utils;
using Popcorn.Exceptions;
using SubtitlesParser.Classes;

namespace Popcorn.Services.Subtitles
{
    /// <summary>
    /// The subtitles service
    /// </summary>
    public class SubtitlesService : ISubtitlesService
    {
        private readonly IOsdbClient _client;

        public SubtitlesService()
        {
            _client = new OsdbClient();
        }

        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Get subtitles languages
        /// </summary>
        /// <returns>Languages</returns>
        public async Task<IEnumerable<OSDB.Models.Language>> GetSubLanguages()
        {
            var retryGetSubLanguagesPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            try
            {
                return await retryGetSubLanguagesPolicy.ExecuteAsync(async () =>
                {
                    var osdb = new OsdbClient();
                    return await osdb.GetSubLanguages();
                });
            }
            catch (Exception)
            {
                Messenger.Default.Send(new ManageExceptionMessage(new PopcornException(LocalizationProviderHelper.GetLocalizedValue<string>("OpenSubtitlesNotAvailable"))));
                return new List<OSDB.Models.Language>();
            }
        }

        /// <summary>
        /// Search subtitles by imdb code and languages
        /// </summary>
        /// <param name="languages">Languages</param>
        /// <param name="imdbId">Imdb code</param>
        /// <param name="season">Season number</param>
        /// <param name="episode">Episode number</param>
        /// <returns>Subtitles</returns>
        public async Task<IList<OSDB.Models.Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId, int? season, int? episode)
        {
            var retrySearchSubtitlesFromImdbPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            return await retrySearchSubtitlesFromImdbPolicy.ExecuteAsync(async () => await _client.SearchSubtitlesFromImdb(languages, imdbId, season, episode));
        }

        /// <summary>
        /// Download a subtitle to a path
        /// </summary>
        /// <param name="path">Path to download</param>
        /// <param name="subtitle">Subtitle to download</param>
        /// <param name="remote">Is remote download path</param>
        /// <returns>Downloaded subtitle path</returns>
        public async Task<string> DownloadSubtitleToPath(string path, OSDB.Models.Subtitle subtitle, bool remote = true)
        {
            var retryDownloadSubtitleToPathPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            return await retryDownloadSubtitleToPathPolicy.ExecuteAsync(async () => await _client.DownloadSubtitleToPath(path, subtitle, remote));
        }

        /// <summary>
        /// Get captions from local path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string LoadCaptions(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return string.Empty;
            try
            {
                var parser = new SubtitlesParser.Classes.Parsers.SubParser();
                using (var fileStream = File.OpenRead(filePath))
                {
                    var lines = parser.ParseStream(fileStream, Encoding.UTF8);
                    var file = $@"{Path.GetDirectoryName(filePath)}\{Guid.NewGuid()}.srt";
                    using (var srtFile = new StreamWriter(file, false, Encoding.UTF8))
                    {
                        var count = 1;
                        foreach (var line in lines)
                        {
                            if (line.StartTime <= 0 || line.EndTime <= 0)
                            {
                                continue;
                            }

                            srtFile.WriteLine(count);
                            srtFile.WriteLine(
                                $"{TimeSpan.FromMilliseconds(line.StartTime).ToString("hh\\:mm\\:ss\\,fff")} --> {TimeSpan.FromMilliseconds(line.EndTime).ToString("hh\\:mm\\:ss\\,fff")}");
                            foreach (var item in line.Lines)
                            {
                                srtFile.WriteLine(item);
                            }

                            srtFile.WriteLine();
                            count++;
                        }
                    }

                    return file;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert a .srt file to a .vtt file
        /// </summary>
        /// <param name="sFilePath">Path to the file</param>
        /// <returns>Path to the converted vtt file</returns>
        public string ConvertSrtToVtt(string sFilePath)
        {
            try
            {
                var path = Path.ChangeExtension(sFilePath, "vtt");
                using (var strReader = new StreamReader(sFilePath, Encoding.UTF8))
                using (var strWriter = new StreamWriter(File.Create(path), Encoding.UTF8))
                {
                    var rgxDialogNumber = new Regex(@"^\d+$");
                    var rgxTimeFrame = new Regex(@"(\d\d:\d\d:\d\d,\d\d\d) --> (\d\d:\d\d:\d\d,\d\d\d)");

                    // Write starting line for the WebVTT file
                    strWriter.WriteLine("WEBVTT");
                    strWriter.WriteLine("");

                    // Handle each line of the SRT file
                    string sLine;
                    while ((sLine = strReader.ReadLine()) != null)
                    {
                        // We only care about lines that aren't just an integer (aka ignore dialog id number lines)
                        if (rgxDialogNumber.IsMatch(sLine))
                            continue;

                        // If the line is a time frame line, reformat and output the time frame
                        Match match = rgxTimeFrame.Match(sLine);
                        if (match.Success)
                        {
                            sLine = sLine.Replace(',', '.'); // Simply replace the comma in the time with a period
                        }

                        strWriter.WriteLine(sLine); // Write out the line
                    }

                    return path;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }
    }
}