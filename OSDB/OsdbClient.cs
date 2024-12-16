using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NChardet;
using NetBike.Xml;
using OSDB.Backend;
using OSDB.Helpers;
using OSDB.Models;
using RestSharp;

namespace OSDB
{
    public class OsdbClient : IOsdbClient
    {
        public async Task<IList<Subtitle>> SearchSubtitlesFromImdb(string languages, string imdbId, int? season,
            int? episode)
        {
            if (string.IsNullOrEmpty(imdbId))
            {
                throw new ArgumentNullException(nameof(imdbId));
            }

            var request = new SearchSubtitlesRequest
            {
                SubLanguageId = languages,
                ImdbId = imdbId,
                Episode = episode,
                Season = season
            };

            return await SearchSubtitlesInternal(request);
        }

        public async Task<string> DownloadSubtitleToPath(string path, Subtitle subtitle, bool remote = true)
        {
            var destinationfile = Path.Combine(path, subtitle.IDSubtitleFile);
            if (remote)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (!Directory.Exists(path))
                {
                    throw new ArgumentException("path should point to a valid location");
                }

                if (File.Exists(destinationfile))
                {
                    return destinationfile;
                }

                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(subtitle.SubDownloadLink))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var content = response.Content)
                        {
                            var bytes = await content.ReadAsByteArrayAsync();
                            var decompressed = await UnZipSubtitleFileToFile(bytes);
                            await DecodeAndWriteFile(destinationfile, decompressed);
                        }
                    }
                }
            }
            else
            {
                await DecodeAndWriteFile(destinationfile,
                    File.ReadAllBytes(subtitle.SubDownloadLink));
            }

            return destinationfile;
        }

        public async Task<IEnumerable<Language>> GetSubLanguages()
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                var response =
                    await client.SendAsync(
                        new HttpRequestMessage(HttpMethod.Post, Constants.OpenSubtitlesXmlRpcEndpoint)
                        {
                            Content = new StringContent(
                                @"<?xml version=""1.0"" encoding=""UTF-8""?><methodCall><methodName>GetSubLanguages</methodName></methodCall>")
                        },
                        CancellationToken.None);
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var serializer = new XmlSerializer();
                    return BuildLanguageObject(serializer.Deserialize<Response>(stream));
                }
            }
        }
        
        private static IEnumerable<Language> BuildLanguageObject(Response response)
        {
            return response.Params.First().Param.Value.Member.Where(a => a.Value.Value != null)
                .SelectMany(a => a.Value.Value.Value).Select(member => new Language
                {
                    ISO639 = member.Value.First(a => a.Name == "ISO639").Value
                        .Value,
                    LanguageName = member.Value.First(a => a.Name == "LanguageName").Value.Value,
                    SubLanguageID = member.Value.First(a => a.Name == "SubLanguageID").Value.Value
                });
        }

        private async Task<IList<Subtitle>> SearchSubtitlesInternal(SearchSubtitlesRequest request)
        {
            var client = new RestClient(Constants.OpenSubtitlesRestApiEndpoint) {UserAgent = Constants.OsdbUa};
            var segment = "search/{imdbid}";
            if (request.Episode.HasValue)
                segment += "/{episode}";

            if (request.Season.HasValue)
                segment += "/{season}";

            var restRequest = new RestRequest(segment, Method.GET);
            restRequest.AddUrlSegment("imdbid", $"imdbid-{request.ImdbId}");
            if (request.Episode.HasValue)
                restRequest.AddUrlSegment("episode", $"episode-{request.Episode.Value.ToString()}");

            if (request.Season.HasValue)
                restRequest.AddUrlSegment("season", $"season-{request.Season.Value.ToString()}");
            var response = await client.ExecuteTaskAsync<IEnumerable<Subtitle>>(restRequest);
            return response.Data.ToList();
        }

        private async Task DecodeAndWriteFile(string destinationfile, byte[] decompressed)
        {
            using (var subFile = File.Open(destinationfile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var cdo = new CharsetDetectionObserver();
                var detector = new Detector(6);
                detector.Init(cdo);
                detector.DoIt(decompressed, decompressed.Length, false);
                detector.Done();
                var probable = detector.getProbableCharsets().FirstOrDefault();
                var enc = Encoding.GetEncoding(!string.IsNullOrEmpty(cdo.Charset)
                    ? cdo.Charset
                    : (string.IsNullOrEmpty(probable) || probable == "nomatch" ? "UTF-8" : probable));

                var str = Encoding.Convert(enc, Encoding.UTF8, decompressed);
                var output = Encoding.UTF8.GetBytes(WebUtility.HtmlDecode(Encoding.UTF8.GetString(str)));
                await subFile.WriteAsync(output, 0, output.Length);
            }
        }

        private async Task<byte[]> UnZipSubtitleFileToFile(byte[] gzipStream)
        {
            using (var compressedMs = new MemoryStream(gzipStream))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs,
                        CompressionMode.Decompress)))
                    {
                        await gzs.CopyToAsync(decompressedMs);
                    }

                    return decompressedMs.ToArray();
                }
            }
        }
    }
}