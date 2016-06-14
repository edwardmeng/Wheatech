using System;
using System.Globalization;
using System.Text;

namespace Wheatech
{
    public class VersionFormatter : IFormatProvider, ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null) throw new ArgumentNullException("arg");
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
