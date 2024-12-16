using System;
using System.IO;
using System.Linq;
using System.Reflection;
using GalaSoft.MvvmLight.Ioc;
using NLog;
using Popcorn.Services.Cache;

namespace Popcorn.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static readonly ICacheService _cacheService;

        /// <summary>
        /// Extract Embedded resource files to a given path
        /// </summary>
        /// <param name="embeddedFileName">Name of the embedded resource file</param>
        /// <param name="destinationPath">Path and file to export resource to</param>
        public static void ExtractResource(string embeddedFileName, string destinationPath)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            using (var resourceToSave = currentAssembly.GetManifestResourceStream(embeddedFileName))
            {
                File.WriteAllBytes(destinationPath, ReadFully(resourceToSave));
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get directory size
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static long GetDirectorySize(string folderPath)
        {
            try
            {
                var di = new DirectoryInfo(folderPath);
                return di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return 0L;
            }
        }

        static FileHelper()
        {
            _cacheService = SimpleIoc.Default.GetInstance<ICacheService>();
        }

        /// <summary>
        /// Create download folders
        /// </summary>
        public static void CreateFolders()
        {
            if (!Directory.Exists(_cacheService.Assets))
            {
                Directory.CreateDirectory(_cacheService.Assets);
            }

            if (!Directory.Exists(_cacheService.Subtitles))
            {
                Directory.CreateDirectory(_cacheService.Subtitles);
            }

            if (!Directory.Exists(_cacheService.MovieDownloads))
            {
                Directory.CreateDirectory(_cacheService.MovieDownloads);
            }

            if (!Directory.Exists(_cacheService.DropFilesDownloads))
            {
                Directory.CreateDirectory(_cacheService.DropFilesDownloads);
            }

            if (!Directory.Exists(_cacheService.ShowDownloads))
            {
                Directory.CreateDirectory(_cacheService.ShowDownloads);
            }

            if (!Directory.Exists(_cacheService.MovieTorrentDownloads))
            {
                Directory.CreateDirectory(_cacheService.MovieTorrentDownloads);
            }
        }

        /// <summary>
        /// Clear download folders
        /// </summary>
        public static void ClearFolders(bool removeAlsoAssets = false)
        {
            if (removeAlsoAssets && Directory.Exists(_cacheService.Assets))
            {
                DeleteFolder(_cacheService.Assets);
            }

            if (Directory.Exists(_cacheService.Subtitles))
            {
                DeleteFolder(_cacheService.Subtitles);
            }

            if (Directory.Exists(_cacheService.MovieDownloads))
            {
                DeleteFolder(_cacheService.MovieDownloads);
            }

            if (Directory.Exists(_cacheService.DropFilesDownloads))
            {
                DeleteFolder(_cacheService.DropFilesDownloads);
            }

            if (Directory.Exists(_cacheService.ShowDownloads))
            {
                DeleteFolder(_cacheService.ShowDownloads);
            }

            if (Directory.Exists(_cacheService.MovieTorrentDownloads))
            {
                DeleteFolder(_cacheService.MovieTorrentDownloads);
            }
        }

        /// <summary>
        /// Delete folder and subfolders
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFolder(string path)
        {
            foreach (
                var filePath in Directory.GetFiles(path, "*.*",
                    SearchOption.AllDirectories)
            )
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error while deleting file: {ex.Message}.");
                }
            }

            try
            {
                var directory = new DirectoryInfo(path);
                var directories = directory.GetDirectories();

                foreach (var folder in directories)
                    Directory.Delete(folder.FullName, true);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while deleting folder: {ex.Message}.");
            }
        }
    }
}