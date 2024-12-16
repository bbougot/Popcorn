using System.Collections.Generic;

namespace Popcorn.Models.User
{
    public class User
    {
        public int DownloadLimit { get; set; }

        public int UploadLimit { get; set; }

        public bool DefaultHdQuality { get; set; }

        public string DefaultSubtitleLanguage { get; set; }

        public Language Language { get; set; }

        public List<MovieHistory> MovieHistory { get; set; }

        public List<ShowHistory> ShowHistory { get; set; }

        public string CacheLocation { get; set; }
    }
}
