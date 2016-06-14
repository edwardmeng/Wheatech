using System;
using System.Globalization;
using System.Text;

namespace Wheatech
{
    /// <summary>
    /// Provides culture-specific information about the format of version values.
    /// </summary>
    public class VersionFormatter : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// Converts the value of a specified <see cref="Version"/> to an equivalent string representation using specified format and culture-specific formatting information.
        /// </summary>
        /// <param name="format">A format string containing formatting specifications.</param>
        /// <param name="arg">An object to format.</param>
        /// <param name="formatProvider">An object that supplies format information about the current instance.</param>
        /// <returns>The string representation of the value of <paramref name="arg"/>, formatted as specified by <paramref name="format"/> and <paramref name="formatProvider"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="arg"/> is null.</exception>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg));
            if (string.IsNullOrEmpty(format)) return string.Empty;
            var version = arg as Version;
            if (version == null) return null;
            // single char identifiers
            if (format.Length == 1) return Format(format[0], version);
            var sb = new StringBuilder(format.Length);
            for (var i = 0; i < format.Length; i++)
            {
                sb.Append(Format(format[i], version));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns an object of the specified type that provides a version formatting service.
        /// </summary>
        /// <param name="formatType">The type of the required formatting service.</param>
        /// <returns>The current object, if <paramref name="formatType"/> is the same as the type of <see cref="ICustomFormatter"/> or <see cref="Version"/> ; otherwise, null.</returns>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) || formatType == typeof(Version) ? this : null;
        }

        private static string GetNormalizedString(Version version)
        {
            var sb = new StringBuilder();

            sb.Append(Format('V', version));

            if (version.IsPrerelease)
            {
                sb.Append('-');
                sb.Append(version.Release);
            }

            if (version.HasMetadata)
            {
                sb.Append('+');
                sb.Append(version.Metadata);
            }

            return sb.ToString();
        }

        private static string Format(char c, Version version)
        {
            switch (c)
            {
                case 'N':
                    return GetNormalizedString(version);
                case 'R':
                    return version.Release;
                case 'M':
                    return version.Metadata;
                case 'V':
                    return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}{3}", version.Major, version.Minor, version.Patch,
                        version.Revision > 0 ? String.Format(CultureInfo.InvariantCulture, ".{0}", version.Revision) : null);
                case 'x':
                    return string.Format(CultureInfo.InvariantCulture, "{0}", version.Major);
                case 'y':
                    return string.Format(CultureInfo.InvariantCulture, "{0}", version.Minor);
                case 'z':
                    return string.Format(CultureInfo.InvariantCulture, "{0}", version.Patch);
                case 'r':
                    return string.Format(CultureInfo.InvariantCulture, "{0}", version.Revision);
            }
            return c.ToString();
        }
    }
}
