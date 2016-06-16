using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wheatech.Properties;
using static System.String;

namespace Wheatech
{
    /// <summary>
    /// A semantic version implementation, it is compatible with revision.
    /// </summary>
    [Serializable]
    public sealed class Version : IFormattable, IComparable, IComparable<Version>, IEquatable<Version>
    {
        #region Fields

        private readonly string[] _releaseLabels;
        private readonly string _metadata;
        private readonly System.Version _version;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> using <see cref="Version.Parse(string)"/>
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <exception cref="ArgumentException"><paramref name="version"/> is null, empty string or invalid format.</exception>
        public Version(string version)
            : this(Parse(version))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> from an existing <see cref="Version"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="version"/> is null.</exception>
        public Version(Version version)
            : this(version?.Major ?? 0, version?.Minor ?? 0, version?.Patch ?? 0, version?.ReleaseLabels ?? Enumerable.Empty<string>(), version?.Metadata)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> X.Y.Z
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> or <paramref name="minor"/> is negative number.</exception>
        public Version(int major, int minor, int patch)
            : this(major, minor, patch, Enumerable.Empty<string>(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> X.Y.Z-alpha
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <param name="releaseLabel">Prerelease label</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> or <paramref name="minor"/> is negative number.</exception>
        public Version(int major, int minor, int patch, string releaseLabel)
            : this(major, minor, patch, releaseLabel, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> X.Y.Z-alpha+build01
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <param name="releaseLabel">Prerelease label</param>
        /// <param name="metadata">Build metadata</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> or <paramref name="minor"/> is negative number.</exception>
        public Version(int major, int minor, int patch, string releaseLabel, string metadata)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> X.Y.Z-alpha.1.2+build01
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <param name="releaseLabels">Release labels that have been split by the dot separator</param>
        /// <param name="metadata">Build metadata</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> or <paramref name="minor"/> is negative number.</exception>
        public Version(int major, int minor, int patch, IEnumerable<string> releaseLabels, string metadata)
            : this(new System.Version(major, minor, patch, 0), releaseLabels, metadata)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> from a .NET Version with additional release labels, build metadata.
        /// </summary>
        /// <param name="version">Version numbers</param>
        /// <param name="releaseLabel">prerelease labels</param>
        /// <param name="metadata">Build metadata</param>
        /// <exception cref="ArgumentNullException"><paramref name="version"/> is null.</exception>
        public Version(System.Version version, string releaseLabel = null, string metadata = null)
            : this(version, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> W.X.Y.Z
        /// </summary>
        /// <param name="major">W.x.y.z</param>
        /// <param name="minor">w.X.y.z</param>
        /// <param name="patch">w.x.Y.z</param>
        /// <param name="revision">w.x.y.Z</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> or <paramref name="minor"/> is negative number.</exception>
        public Version(int major, int minor, int patch, int revision)
            : this(major, minor, patch, revision, Enumerable.Empty<string>(), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> W.X.Y.Z-alpha+build01
        /// </summary>
        /// <param name="major">W.x.y.z</param>
        /// <param name="minor">w.X.y.z</param>
        /// <param name="patch">w.x.Y.z</param>
        /// <param name="revision">w.x.y.Z</param>
        /// <param name="releaseLabel">Prerelease label</param>
        /// <param name="metadata">Build metadata</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> or <paramref name="minor"/> is negative number.</exception>
        public Version(int major, int minor, int patch, int revision, string releaseLabel, string metadata)
            : this(major, minor, patch, revision, ParseReleaseLabels(releaseLabel), metadata)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> W.X.Y.Z-alpha.1+build01
        /// </summary>
        /// <param name="major">W.x.y.z</param>
        /// <param name="minor">w.X.y.z</param>
        /// <param name="patch">w.x.Y.z</param>
        /// <param name="revision">w.x.y.Z</param>
        /// <param name="releaseLabels">Prerelease labels</param>
        /// <param name="metadata">Build metadata</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="major"/> or <paramref name="minor"/> is negative number.</exception>
        public Version(int major, int minor, int patch, int revision, IEnumerable<string> releaseLabels, string metadata)
            : this(new System.Version(major, minor, patch, revision), releaseLabels, metadata)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> from a .NET Version with additional release labels, build metadata.
        /// </summary>
        /// <param name="version">Version numbers</param>
        /// <param name="releaseLabels">prerelease labels</param>
        /// <param name="metadata">Build metadata</param>
        /// <exception cref="ArgumentNullException"><paramref name="version"/> is null.</exception>
        public Version(System.Version version, IEnumerable<string> releaseLabels, string metadata)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (version.Build < 0 || version.Revision < 0)
            {
                version = new System.Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
            }
            _version = version;
            _metadata = metadata;

            if (releaseLabels != null)
            {
                // enumerate the list
                _releaseLabels = releaseLabels.ToArray();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Major version X (X.y.z)
        /// </summary>
        public int Major => _version.Major;

        /// <summary>
        /// Minor version Y (x.Y.z)
        /// </summary>
        public int Minor => _version.Minor;

        /// <summary>
        /// Patch version Z (x.y.Z)
        /// </summary>
        public int Patch => _version.Build;

        /// <summary>
        /// Revision version R (x.y.z.R)
        /// </summary>
        public int Revision => _version.Revision;

        /// <summary>
        /// A collection of pre-release labels attached to the version.
        /// </summary>
        public string[] ReleaseLabels => _releaseLabels ?? new string[0];

        /// <summary>
        /// Build metadata attached to the version.
        /// </summary>
        public string Metadata => _metadata;

        /// <summary>
        /// The full pre-release label for the version.
        /// </summary>
        public string Release => _releaseLabels != null ? Join(".", _releaseLabels) : Empty;

        /// <summary>
        /// True if pre-release labels exist for the version.
        /// </summary>
        public bool IsPrerelease => _releaseLabels != null && _releaseLabels.Length > 0 && !IsNullOrEmpty(_releaseLabels[0]);

        /// <summary>
        /// True if metadata exists for the version.
        /// </summary>
        public bool HasMetadata => !IsNullOrEmpty(Metadata);

        #endregion

        #region Format

        /// <summary>
        /// Gives a normalized representation of the version.
        /// </summary>
        public override string ToString()
        {
            return ToString("N", new VersionFormatter());
        }

        /// <summary>
        /// Converts the value of the current Version object to its equivalent string representation using the specified format and culture-specific format information.
        /// </summary>
        /// <param name="format">A standard or custom date and time format string.</param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>A string representation of value of the current Version object as specified by format and provider.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string formattedString;

            if (formatProvider == null
                || !TryFormatter(format, formatProvider, out formattedString))
            {
                formattedString = ToString();
            }

            return formattedString;
        }

        private bool TryFormatter(string format, IFormatProvider formatProvider, out string formattedString)
        {
            var formatted = false;
            formattedString = null;

            var formatter = formatProvider?.GetFormat(typeof(Version)) as ICustomFormatter;
            if (formatter != null)
            {
                formatted = true;
                formattedString = formatter.Format(format, this, formatProvider);
            }

            return formatted;
        }

        #endregion

        #region Compare

        /// <summary>
        /// Compares the value of this instance to a specified object that contains a specified <see cref="Version"/> value, 
        /// and returns an integer that indicates whether this instance is less than, the same as, or greater than the specified <see cref="Version"/> value.
        /// </summary>
        /// <param name="value">A boxed object to compare, or null.</param>
        /// <returns>A signed number indicating the relative values of this instance and value.</returns>
        public int CompareTo(object value)
        {
            return CompareTo(value as Version);
        }

        /// <summary>
        /// Compares the value of this instance to a specified <see cref="Version"/> value and returns an integer 
        /// that indicates whether this instance is less than, the same as, or greater than the specified <see cref="Version"/> value.
        /// </summary>
        /// <param name="value">The object to compare to the current instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and the value parameter.</returns>
        public int CompareTo(Version value)
        {
            return CompareTo(value, VersionComparison.Default);
        }

        /// <summary>
        /// Compares NuGetVersion objects using the given comparison mode.
        /// </summary>
        public int CompareTo(Version other, VersionComparison versionComparison)
        {
            var comparer = new VersionComparer(versionComparison);
            return comparer.Compare(this, other);
        }

        private static int Compare(Version version1, Version version2)
        {
            var comparer = new VersionComparer();
            return comparer.Compare(version1, version2);
        }

        #endregion

        #region Equals

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return VersionComparer.Default.GetHashCode(this);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="value">The object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is an instance of <see cref="Version"/> and equals the value of this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object value)
        {
            return Equals(value as Version);
        }

        /// <summary>
        /// Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="Version"/> instance.
        /// </summary>
        /// <param name="value">The object to compare to this instance.</param>
        /// <returns><c>true</c> if the <paramref name="value"/> parameter equals the value of this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Version value)
        {
            return Equals(value, VersionComparison.Default);
        }

        /// <summary>
        /// Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="Version"/> instance based on the given comparison mode.
        /// </summary>
        /// <param name="value">The object to compare to this instance.</param>
        /// <param name="versionComparison">One of the enumeration values that specifies how the versions will be compared.</param>
        /// <returns><c>true</c> if the <paramref name="value"/> parameter equals the value of this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Version value, VersionComparison versionComparison)
        {
            return CompareTo(value, versionComparison) == 0;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Version"/> values are equal.
        /// </summary>
        /// <param name="version1">The first value to compare.</param>
        /// <param name="version2">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="version1"/> and <paramref name="version2"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Version version1, Version version2)
        {
            return Compare(version1, version2) == 0;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="Version"/> objects have different values.
        /// </summary>
        /// <param name="version1">The first value to compare.</param>
        /// <param name="version2">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="version1"/> and <paramref name="version2"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Version version1, Version version2)
        {
            return Compare(version1, version2) != 0;
        }

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Version"/> is less than another specified <see cref="Version"/>.
        /// </summary>
        /// <param name="version1">The first value to compare.</param>
        /// <param name="version2">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="version1"/> is less than <paramref name="version2"/>; otherwise, <c>false</c>.</returns>
        public static bool operator <(Version version1, Version version2)
        {
            return Compare(version1, version2) < 0;
        }

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Version"/> is less than or equal to another specified <see cref="Version"/>.
        /// </summary>
        /// <param name="version1">The first value to compare.</param>
        /// <param name="version2">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="version1"/> is less than or equal to <paramref name="version2"/>; otherwise, <c>false</c>.</returns>
        public static bool operator <=(Version version1, Version version2)
        {
            return Compare(version1, version2) <= 0;
        }

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Version"/> is greater than another specified <see cref="Version"/>.
        /// </summary>
        /// <param name="version1">The first value to compare.</param>
        /// <param name="version2">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="version1"/> is greater than <paramref name="version2"/>; otherwise, <c>false</c>.</returns>
        public static bool operator >(Version version1, Version version2)
        {
            return Compare(version1, version2) > 0;
        }

        /// <summary>
        /// Returns a value indicating whether a specified <see cref="Version"/> is greater than or equal to another specified <see cref="Version"/>.
        /// </summary>
        /// <param name="version1">The first value to compare.</param>
        /// <param name="version2">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="version1"/> is greater than or equal to <paramref name="version2"/>; otherwise, <c>false</c>.</returns>
        public static bool operator >=(Version version1, Version version2)
        {
            return Compare(version1, version2) >= 0;
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="System.Version"/> to a <see cref="Version"/>.
        /// </summary>
        /// <param name="version">The <see cref="System.Version"/> to convert.</param>
        public static implicit operator Version(System.Version version)
        {
            return version == null ? null : new Version(version);
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="Version"/> to a <see cref="System.Version"/>.
        /// </summary>
        /// <param name="version">The value to convert.</param>
        public static explicit operator System.Version(Version version)
        {
            return version?._version;
        }

        #endregion

        #region Parse

        private static IEnumerable<string> ParseReleaseLabels(string releaseLabels)
        {
            return !IsNullOrEmpty(releaseLabels) ? releaseLabels.Split('.') : null;
        }

        /// <summary>
        /// Parse the version string into version/release/build
        /// The goal of this code is to take the most direct and optimized path
        /// to parsing and validating a semver. Regex would be much cleaner, but
        /// due to the number of versions created in NuGet Regex is too slow.
        /// </summary>
        private static Tuple<string, string[], string> ParseSections(string value)
        {
            string versionString = null;
            string[] releaseLabels = null;
            string buildMetadata = null;

            var dashPos = -1;
            var plusPos = -1;

            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var end = (i == chars.Length - 1);

                if (dashPos < 0)
                {
                    if (end || chars[i] == '-' || chars[i] == '+')
                    {
                        var endPos = i + (end ? 1 : 0);
                        versionString = value.Substring(0, endPos);

                        dashPos = i;

                        if (chars[i] == '+') plusPos = i;
                    }
                }
                else if (plusPos < 0)
                {
                    if (end || chars[i] == '+')
                    {
                        var start = dashPos + 1;
                        var endPos = i + (end ? 1 : 0);
                        var releaseLabel = value.Substring(start, endPos - start);

                        releaseLabels = releaseLabel.Split('.');

                        plusPos = i;
                    }
                }
                else if (end)
                {
                    var start = plusPos + 1;
                    var endPos = i + 1;
                    buildMetadata = value.Substring(start, endPos - start);
                }
            }

            return new Tuple<string, string[], string>(versionString, releaseLabels, buildMetadata);
        }

        private static bool IsLetterOrDigitOrDash(char c)
        {
            var x = (int)c;
            // "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-"
            return (x >= 48 && x <= 57) || (x >= 65 && x <= 90) || (x >= 97 && x <= 122) || x == 45;
        }

        private static bool IsValidPart(char[] chars, bool allowLeadingZeros)
        {
            bool result = chars.Length != 0;

            // 0 is fine, but 00 is not. 
            // 0A counts as an alpha numeric string where zeros are not counted
            if (!allowLeadingZeros && chars.Length > 1 && chars[0] == '0' && chars.All(char.IsDigit))
            {
                // no leading zeros in labels allowed
                result = false;
            }
            else
            {
                result &= chars.All(IsLetterOrDigitOrDash);
            }

            return result;
        }

        /// <summary>
        /// Parse a version string
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="version">The instance that will contain the parsed value. If the method returns <c>true</c>, result contains a valid version. If the method returns <c>false</c>, result is null.</param>
        /// <returns>false if the version is not a strict semver</returns>
        public static bool TryParse(string value, out Version version)
        {
            version = null;

            if (value != null)
            {
                var sections = ParseSections(value.Trim());

                // null indicates the string did not meet the rules
                if (sections != null)
                {
                    // validate the version string
                    var parts = sections.Item1.Split('.');
                    System.Version systemVersion;
                    if (System.Version.TryParse(sections.Item1 + (parts.Length == 1 ? ".0" : null), out systemVersion))
                    {
                        // leading zeros are not allowed
                        if (parts.Any(part => !IsValidPart(part.ToCharArray(), false)))
                        {
                            return false;
                        }

                        // labels
                        if (sections.Item2 != null && !sections.Item2.All(s => IsValidPart(s.ToCharArray(), false)))
                        {
                            return false;
                        }

                        // build metadata
                        if (sections.Item3 != null && !sections.Item3.Split('.').All(p => IsValidPart(p.ToCharArray(), true)))
                        {
                            return false;
                        }

                        version = new Version(systemVersion, sections.Item2, sections.Item3 ?? Empty);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Parses a version string using strict version rules.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> is null, empty string or invalid format.</exception>
        public static Version Parse(string value)
        {
            if (IsNullOrEmpty(value))
            {
                throw new ArgumentException(Strings.Argument_Cannot_Be_Null_Or_Empty, nameof(value));
            }
            Version ver;
            if (!TryParse(value, out ver))
            {
                throw new ArgumentException(Format(CultureInfo.CurrentCulture, Strings.InvalidVersion, value), nameof(value));
            }
            return ver;
        }

        #endregion
    }
}
