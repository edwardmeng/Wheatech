using System;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace Wheatech
{
    public static class PathUtils
    {
        private static bool IsAbsolutePhysical(string path)
        {
            if (path == null || path.Length < 3)
            {
                return false;
            }
            return path[1] == ':' && IsDirectorySeparatorChar(path[2]) || IsUncSharePath(path);
        }

        private static bool IsDirectorySeparatorChar(char ch)
        {
            return ch == Path.AltDirectorySeparatorChar || ch == Path.DirectorySeparatorChar;
        }

        private static bool IsUncSharePath(string path)
        {
            return path.Length > 2 && IsDirectorySeparatorChar(path[0]) && IsDirectorySeparatorChar(path[1]);
        }

        private static bool IsAppRelative(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Length < 2) return false;
            return path[0] == '~' && IsDirectorySeparatorChar(path[1]);
        }

        private static bool IsUri(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var colonIndex = path.IndexOf(":", StringComparison.Ordinal);
            if (colonIndex == -1) return false;
            if (path.Length < colonIndex + 3 || !IsDirectorySeparatorChar(path[colonIndex + 1]) || !IsDirectorySeparatorChar(path[colonIndex + 2])) return false;
            var scheme = path.Substring(0, colonIndex).Trim().ToLower();
            return scheme == "http" || scheme == "https" || scheme == "ftp" || scheme == "file";
        }

        /// <summary>
        /// Maps the specified virtual path to a physical path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (absolute or relative) for the current environment.</param>
        /// <returns>The physical path specified by <paramref name="virtualPath"/>.</returns>
        public static string ResolvePath(string virtualPath)
        {
            if (IsUri(virtualPath))
            {
                return virtualPath.StartsWith("file://", StringComparison.OrdinalIgnoreCase) ? HttpUtility.UrlDecode(new Uri(virtualPath).LocalPath) : virtualPath;
            }
            if (!IsAbsolutePhysical(virtualPath))
            {
                if (HostingEnvironment.IsHosted)
                {
                    try
                    {
                        return HostingEnvironment.MapPath(virtualPath);
                    }
                    catch
                    {
                    }
                }
                if (IsAppRelative(virtualPath))
                {
                    virtualPath = virtualPath.Substring(2);
                }
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, virtualPath);
            }
            return virtualPath;
        }
    }
}
