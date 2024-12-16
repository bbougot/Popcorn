using Popcorn.Services.User;

namespace Popcorn.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IUserService _userService;

        public CacheService(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Directory of assets
        /// </summary>
        public string Assets => _userService.GetCacheLocationPath() + @"\Assets\";

        /// <summary>
        /// Directory of downloaded movies
        /// </summary>
        public string MovieDownloads => _userService.GetCacheLocationPath() + @"\Downloads\Movies\";

        /// <summary>
        /// Directory of dropped files
        /// </summary>
        public string DropFilesDownloads => _userService.GetCacheLocationPath() + @"\Downloads\Dropped\";

        /// <summary>
        /// Directory of downloaded shows
        /// </summary>
        public string ShowDownloads => _userService.GetCacheLocationPath() + @"\Downloads\Shows\";

        /// <summary>
        /// Directory of downloaded movie torrents
        /// </summary>
        public string MovieTorrentDownloads => _userService.GetCacheLocationPath() + @"\Torrents\Movies\";

        /// <summary>
        /// Subtitles directory
        /// </summary>
        public string Subtitles => _userService.GetCacheLocationPath() + @"\Subtitles\";

        /// <summary>
        /// Popcorn temp directory
        /// </summary>
        public string PopcornTemp => _userService.GetCacheLocationPath();
    }
}
