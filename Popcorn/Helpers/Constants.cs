using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using Popcorn.Extensions;

namespace Popcorn.Helpers
{
    /// <summary>
    /// Constants of the project
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// App version
        /// </summary>
        public const string AppVersion = "1.0.6";

        /// <summary>
        /// Copyright
        /// </summary>
        public static readonly string Copyright = "Copyright Popcorn © 2019-" + DateTime.Now.Year;

        /// <summary>
        /// Popcorn Api
        /// </summary>
        public const string PopcornApi = "https://workstation.ns1.name/api";

        /// <summary>
        /// Client ID for TMDb
        /// </summary>
        public const string TmDbClientId = "8a0ab80688ed6a0d1e8ae75ba1c3a307";

        /// <summary>
        /// Path to the FFmpeg shared libs
        /// </summary>
        public static string FFmpegPath => $@"{new Uri(Assembly.GetExecutingAssembly().GetPath())
            .OriginalString}\FFmpeg";

        /// <summary>
        /// In percentage, the minimum of buffering before we can actually start playing the movie
        /// </summary>
        public static double MinimumMovieBuffering
        {
            get
            {
                try
                {
                    return double.Parse((ConfigurationManager.GetSection("settings") as NameValueCollection)["MinimumMovieBuffering"]);
                }
                catch (Exception)
                {
                    return 3d;
                }
            }
        }

        /// <summary>
        /// In percentage, the minimum of buffering before we can actually start playing the episode
        /// </summary>
        public static double MinimumShowBuffering
        {
            get
            {
                try
                {
                    return double.Parse((ConfigurationManager.GetSection("settings") as NameValueCollection)["MinimumShowBuffering"]);
                }
                catch (Exception)
                {
                    return 5d;
                }
            }
        }

        /// <summary>
        /// The maximum number of movies per page to load from the API
        /// </summary>
        public const int MaxMoviesPerPage = 20;

        /// <summary>
        /// The maximum number of shows per page to load from the API
        /// </summary>
        public const int MaxShowsPerPage = 20;

        /// <summary>
        /// Default request timeout
        /// </summary>
        public const int DefaultRequestTimeoutInSecond = 15;
    }
}