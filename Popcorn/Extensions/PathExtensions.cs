using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Popcorn.Extensions
{
    public static class PathExtensions
    {
        private const int MaxPath = 255;

        /// <summary>
        /// Gets the short path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The short path for 8.3 support</returns>
        public static string GetShortPath(this string path)
        {
            var shortPath = new StringBuilder(MaxPath);
            GetShortPathName(path, shortPath, MaxPath);
            return shortPath.ToString();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string path,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
            int shortPathLength);

        public static string GetPath(this Assembly assembly)
        {
            return Path.GetDirectoryName(assembly.GetFileName());
        }

        public static string GetFileName(this Assembly assembly)
        {
            return assembly.CodeBase.GetPathFromUri();
        }

        public static string GetPathFromUri(this string uriString)
        {
            var uri = new Uri(Uri.EscapeUriString(uriString));
            return $"{Uri.UnescapeDataString(uri.PathAndQuery)}{Uri.UnescapeDataString(uri.Fragment)}";
        }
    }
}
