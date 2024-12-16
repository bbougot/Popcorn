using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;

namespace Popcorn.Helpers
{
    public static class DownloadFileHelper
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Downloads a file from a specified Internet address.
        /// </summary>
        /// <param name="remotePath">Internet address of the file to download.</param>
        /// <param name="localPath">
        /// Local file name where to store the content of the download, if null a temporary file name will
        /// be generated.
        /// </param>
        public static async Task<Tuple<string, string, Exception>> DownloadFileTaskAsync(string remotePath,
            string localPath = null)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                if (remotePath == null)
                {
                    Logger.Debug("DownloadFileTaskAsync (null remote path): skipping");
                    throw new ArgumentNullException(nameof(remotePath));
                }

                if (localPath == null)
                {
                    Logger.Debug(
                        $"DownloadFileTaskAsync (null local path): generating a temporary file name for {remotePath}");
                    localPath = Path.GetTempFileName();
                }

                if (File.Exists(localPath))
                {
                    var fileInfo = new FileInfo(localPath).Length;
                    if (fileInfo != 0)
                        return new Tuple<string, string, Exception>(remotePath, localPath, null);
                }

                var directory = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(HttpMethod.Get, remotePath))
                        {
                            using (
                                Stream contentStream =
                                        await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                                    stream = new FileStream(localPath, FileMode.Create, FileAccess.Write,
                                        FileShare.None, 4096, true))
                            {
                                await contentStream.CopyToAsync(stream);
                            }
                        }
                    }
                }
                catch (Exception exception) when (exception is TaskCanceledException)
                {
                    watch.Stop();
                    Logger.Debug(
                        "DownloadFileTaskAsync cancelled.");
                    return new Tuple<string, string, Exception>(remotePath, null, exception);
                }
                catch (ObjectDisposedException exception)
                {
                    watch.Stop();
                    Logger.Debug(
                        $"DownloadFileTaskAsync (can't cancel download, it has finished previously): {remotePath}");
                    return new Tuple<string, string, Exception>(remotePath, null, exception);
                }
                catch (WebException exception)
                {
                    watch.Stop();
                    Logger.Error($"DownloadFileTaskAsync: {exception.Message}");
                    return new Tuple<string, string, Exception>(remotePath, null, exception);
                }
            }
            catch (Exception ex)
            {
                watch.Stop();
                Logger.Error(
                    $"DownloadFileTaskAsync (download failed): {remotePath} Additional informations : {ex.Message}");
                return new Tuple<string, string, Exception>(remotePath, null, ex);
            }
            finally
            {
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Logger.Trace($"DownloadFileTaskAsync (downloaded in {elapsedMs} ms): {remotePath}");
            }

            return new Tuple<string, string, Exception>(remotePath, localPath, null);
        }
    }
}