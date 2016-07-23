using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Wheatech.Properties;

namespace Wheatech
{
    /// <summary>
    /// Provides a set of methods to resolve path.
    /// </summary>
    public static class PathUtils
    {
        internal const char appRelativeCharacter = '~';
        internal const string appRelativeCharacterString = "~/";

        private static bool IsRooted(string basepath)
        {
            return string.IsNullOrEmpty(basepath) || basepath[0] == '/' || basepath[0] == '\\';
        }

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

        // Checks if virtual path contains a protocol, which is referred to as a scheme in the
        // URI spec.
        private static bool HasScheme(string virtualPath)
        {
            // URIs have the format <scheme>:<scheme-specific-path>, e.g. mailto:user@ms.com,
            // http://server/, nettcp://server/, etc.  The <scheme> cannot contain slashes.
            // The virtualPath passed to this method may be absolute or relative. Although
            // ':' is only allowed in the <scheme-specific-path> if it is encoded, the 
            // virtual path that we're receiving here may be decoded, so it is impossible
            // for us to determine if virtualPath has a scheme.  We will be conservative
            // and err on the side of assuming it has a scheme when we cannot tell for certain.
            // To do this, we first check for ':'.  If not found, then it doesn't have a scheme.
            // If ':' is found, then as long as we find a '/' before the ':', it cannot be
            // a scheme because schemes don't contain '/'.  Otherwise, we will assume it has a 
            // scheme.
            int indexOfColon = virtualPath.IndexOf(':');
            if (indexOfColon == -1)
                return false;
            int indexOfSlash = virtualPath.IndexOf('/');
            return (indexOfSlash == -1 || indexOfColon < indexOfSlash);
        }

        /// <summary>
        /// Maps the specified virtual path to a physical path.
        /// </summary>
        /// <param name="basePath">The base path to resolve the virtual path to absolute path.</param>
        /// <param name="virtualPath">The virtual path (absolute or relative) for the current environment.</param>
        /// <returns>The physical path specified by <paramref name="virtualPath"/>.</returns>
        public static string ResolvePath(string basePath, string virtualPath)
        {
            if (HasScheme(virtualPath))
            {
                return virtualPath.StartsWith("file://", StringComparison.OrdinalIgnoreCase) ? HttpUtility.UrlDecode(new Uri(virtualPath).LocalPath) : virtualPath;
            }
            if (!IsAbsolutePhysical(virtualPath))
            {
                if (HostingEnvironment.IsHosted && AppDomain.CurrentDomain.BaseDirectory == basePath)
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
                return Reduce(Path.Combine(basePath, virtualPath));
            }
            return virtualPath;
        }

        /// <summary>
        /// Maps the specified virtual path to a physical path.
        /// </summary>
        /// <param name="virtualPath">The virtual path (absolute or relative) for the current environment.</param>
        /// <returns>The physical path specified by <paramref name="virtualPath"/>.</returns>
        public static string ResolvePath(string virtualPath)
        {
            return ResolvePath(AppDomain.CurrentDomain.BaseDirectory, virtualPath);
        }

        internal static bool VirtualPathStartsWithAppPath(string virtualPath)
        {
            return VirtualPathStartsWithVirtualPath(virtualPath, HttpRuntime.AppDomainAppVirtualPath);
        }

        private static bool VirtualPathStartsWithVirtualPath(string virtualPath1, string virtualPath2)
        {
            if (virtualPath1 == null)
            {
                throw new ArgumentNullException(nameof(virtualPath1));
            }

            if (virtualPath2 == null)
            {
                throw new ArgumentNullException(nameof(virtualPath2));
            }

            // if virtualPath1 as a string doesn't start with virtualPath2 as s string, then no for sure
            if (!StringUtils.StringStartsWithIgnoreCase(virtualPath1, virtualPath2))
            {
                return false;
            }

            int virtualPath2Length = virtualPath2.Length;

            // same length - same path
            if (virtualPath1.Length == virtualPath2Length)
            {
                return true;
            }

            // Special case for apps rooted at the root. VSWhidbey 286145
            if (virtualPath2Length == 1)
            {
                Debug.Assert(virtualPath2[0] == '/');
                return true;
            }

            // If virtualPath2 ends with a '/', it's definitely a child
            if (virtualPath2[virtualPath2Length - 1] == '/')
                return true;

            // If it doesn't, make sure the next char in virtualPath1 is a '/'.
            // e.g. /app1 vs /app11 (VSWhidbey 285038)
            if (virtualPath1[virtualPath2Length] != '/')
            {
                return false;
            }

            // passed all checks
            return true;
        }

        // If a virtual path starts with the app path, make it start with
        // ~ instead, so that it becomes application agnostic
        // E.g. /MyApp/Sub/foo.aspx --> ~/Sub/foo.aspx
        internal static string MakeVirtualPathAppRelative(string virtualPath,
            string applicationPath, bool nullIfNotInApp)
        {

            if (virtualPath == null)
                throw new ArgumentNullException(nameof(virtualPath));

            int appPathLength = applicationPath.Length;
            int virtualPathLength = virtualPath.Length;

            // If virtualPath is the same as the app path, but without the ending slash,
            // treat it as if it were truly the app path (VSWhidbey 495949)
            if (virtualPathLength == appPathLength - 1)
            {
                if (StringUtils.StringStartsWithIgnoreCase(applicationPath, virtualPath))
                    return appRelativeCharacterString;
            }

            if (!VirtualPathStartsWithVirtualPath(virtualPath, applicationPath))
            {
                // If it doesn't start with the app path, return either null or the input path
                if (nullIfNotInApp)
                    return null;
                else
                    return virtualPath;
            }

            // If they are the same, just return "~/"
            if (virtualPathLength == appPathLength)
                return appRelativeCharacterString;

            // Special case for apps rooted at the root:
            if (appPathLength == 1)
            {
                return appRelativeCharacter + virtualPath;
            }

            return appRelativeCharacter + virtualPath.Substring(appPathLength - 1);
        }

        internal static string MakeVirtualPathAppRelative(string virtualPath)
        {
            return MakeVirtualPathAppRelative(virtualPath,
                HttpRuntime.AppDomainAppVirtualPath, false /*nullIfNotInApp*/);
        }

        // If a virtual path is app relative (i.e. starts with ~/), change it to
        // start with the actuall app path.
        // E.g. ~/Sub/foo.aspx --> /MyApp/Sub/foo.aspx
        internal static string MakeVirtualPathAppAbsolute(string virtualPath, string applicationPath)
        {

            // If the path is exactly "~", just return the app root path
            if (virtualPath.Length == 1 && virtualPath[0] == appRelativeCharacter)
                return applicationPath;

            // If the virtual path starts with "~/" or "~\", replace with the app path
            // relative (ASURT 68628)
            if (virtualPath.Length >= 2 && virtualPath[0] == appRelativeCharacter &&
                (virtualPath[1] == '/' || virtualPath[1] == '\\'))
            {

                if (applicationPath.Length > 1)
                {
                    return applicationPath + virtualPath.Substring(2);
                }
                else
                    return "/" + virtualPath.Substring(2);
            }

            // Don't allow relative paths, since they cannot be made App Absolute
            if (!IsRooted(virtualPath))
                throw new ArgumentOutOfRangeException(nameof(virtualPath));

            // Return it unchanged
            return virtualPath;
        }

        internal static string MakeVirtualPathAppAbsolute(string virtualPath)
        {
            return MakeVirtualPathAppAbsolute(virtualPath, HttpRuntime.AppDomainAppVirtualPath);
        }

        // Same as Reduce, but for a virtual path that is known to be well formed
        internal static string ReduceVirtualPath(string path)
        {
            int length = path.Length;
            int examine;

            // quickly rule out situations in which there are no . or ..

            for (examine = 0; ; examine++)
            {
                examine = path.IndexOf('.', examine);
                if (examine < 0)
                    return path;

                if ((examine == 0 || path[examine - 1] == '/')
                    && (examine + 1 == length || path[examine + 1] == '/' ||
                        (path[examine + 1] == '.' && (examine + 2 == length || path[examine + 2] == '/'))))
                    break;
            }

            // OK, we found a . or .. so process it:

            ArrayList list = new ArrayList();
            StringBuilder sb = new StringBuilder();
            examine = 0;

            for (;;)
            {
                var start = examine;
                examine = path.IndexOf('/', start + 1);

                if (examine < 0)
                    examine = length;

                if (examine - start <= 3 &&
                    (examine < 1 || path[examine - 1] == '.') &&
                    (start + 1 >= length || path[start + 1] == '.'))
                {
                    if (examine - start == 3)
                    {
                        if (list.Count == 0)
                            throw new InvalidOperationException(Strings.Cannot_exit_up_top_directory);

                        // We're about to backtrack onto a starting '~', which would yield
                        // incorrect results.  Instead, make the path App Absolute, and call
                        // Reduce on that.
                        if (list.Count == 1 && IsAppRelative(path))
                        {
                            return ReduceVirtualPath(MakeVirtualPathAppAbsolute(path));
                        }

                        sb.Length = (int)list[list.Count - 1];
                        list.RemoveRange(list.Count - 1, 1);
                    }
                }
                else
                {
                    list.Add(sb.Length);

                    sb.Append(path, start, examine - start);
                }

                if (examine == length)
                    break;
            }

            string result = sb.ToString();

            // If we end up with en empty string, turn it into either "/" or "." (VSWhidbey 289175)
            if (result.Length == 0)
            {
                if (length > 0 && path[0] == '/')
                    result = @"/";
                else
                    result = ".";
            }

            return result;
        }

        internal static string Reduce(string path)
        {
            // ignore query string
            string queryString = null;
            if (path != null)
            {
                int iqs = path.IndexOf('?');
                if (iqs >= 0)
                {
                    queryString = path.Substring(iqs);
                    path = path.Substring(0, iqs);
                }
            }

            // Take care of backslashes and duplicate slashes
            path = FixVirtualPathSlashes(path);

            path = ReduceVirtualPath(path);

            return queryString != null ? path + queryString : path;
        }

        // Change backslashes to forward slashes, and remove duplicate slashes
        internal static string FixVirtualPathSlashes(string virtualPath)
        {

            // Make sure we don't have any back slashes
            virtualPath = virtualPath.Replace('\\', '/');

            // Replace any double forward slashes
            for (;;)
            {
                string newPath = virtualPath.Replace("//", "/");

                // If it didn't do anything, we're done
                if (newPath == virtualPath)
                    break;

                // We need to loop again to take care of triple (or more) slashes (VSWhidbey 288782)
                virtualPath = newPath;
            }

            return virtualPath;
        }
    }
}
