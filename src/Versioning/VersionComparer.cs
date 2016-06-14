using System;
using System.Collections.Generic;

namespace Wheatech
{
    /// <summary>
    /// An comparer for SemanticVersion type.
    /// </summary>
    public sealed class VersionComparer : IEqualityComparer<Version>, IComparer<Version>
    {
        #region Fields

        /// <summary>
        /// A default comparer that compares metadata as strings.
        /// </summary>
        public static readonly VersionComparer Default = new VersionComparer(VersionComparison.Default);

        /// <summary>
        /// A comparer that uses only the version numbers.
        /// </summary>
        public static readonly VersionComparer Version = new VersionComparer(VersionComparison.Version);

        /// <summary>
        /// Compares versions without comparing the metadata.
        /// </summary>
        public static readonly VersionComparer VersionRelease = new VersionComparer(VersionComparison.VersionRelease);

        /// <summary>
        /// A version comparer that follows SemVer 2.0.0 rules.
        /// </summary>
        public static readonly VersionComparer VersionReleaseMetadata = new VersionComparer(VersionComparison.VersionReleaseMetadata);

        private readonly VersionComparison _mode;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a VersionComparer using the default mode.
        /// </summary>
        public VersionComparer()
        {
            _mode = VersionComparison.Default;
        }

        /// <summary>
        /// Creates a VersionComparer that respects the given comparison mode.
        /// </summary>
        /// <param name="versionComparison">comparison mode</param>
        public VersionComparer(VersionComparison versionComparison)
        {
            _mode = versionComparison;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines if both versions are equal.
        /// </summary>
        public bool Equals(Version x, Version y)
        {
            return Compare(x, y) == 0;
        }

        /// <summary>
        /// Compares the given versions using the VersionComparison mode.
        /// </summary>
        public static int Compare(Version version1, Version version2, VersionComparison versionComparison)
        {
            var comparer = new VersionComparer(versionComparison);
            return comparer.Compare(version1, version2);
        }

        /// <summary>
        /// Gives a hash code based on the normalized version string.
        /// </summary>
        public int GetHashCode(Version version)
        {
            if (ReferenceEquals(version, null)) return 0;
            var hashCode = version.Major.GetHashCode();
            hashCode = (hashCode * 397) ^ version.Minor.GetHashCode();
            hashCode = (hashCode * 397) ^ version.Patch.GetHashCode();
            hashCode = (hashCode * 397) ^ version.Revision.GetHashCode();
            if ((_mode == VersionComparison.Default
                 || _mode == VersionComparison.VersionRelease
                 || _mode == VersionComparison.VersionReleaseMetadata) && version.IsPrerelease)
            {
                hashCode = (hashCode * 397) ^ version.Release.GetHashCode();
            }
            if (_mode == VersionComparison.VersionReleaseMetadata && version.HasMetadata)
            {
                hashCode = (hashCode * 397) ^ version.Metadata.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Compare versions.
        /// </summary>
        public int Compare(Version x, Version y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(y, null)) return 1;
            if (ReferenceEquals(x, null)) return -1;
            // compare version
            var result = x.Major.CompareTo(y.Major);
            if (result != 0) return result;

            result = x.Minor.CompareTo(y.Minor);
            if (result != 0) return result;

            result = x.Patch.CompareTo(y.Patch);
            if (result != 0) return result;

            result = x.Revision.CompareTo(y.Revision);
            if (result != 0) return result;

            if (_mode == VersionComparison.Version) return 0;

            // compare release labels
            if (x.IsPrerelease && !y.IsPrerelease) return -1;

            if (!x.IsPrerelease && y.IsPrerelease) return 1;

            if (x.IsPrerelease && y.IsPrerelease)
            {
                for (int i = 0, length = Math.Max(x.ReleaseLabels.Length, y.ReleaseLabels.Length); i < length; i++)
                {
                    if (i >= x.ReleaseLabels.Length) return -1;
                    if (i >= y.ReleaseLabels.Length) return 1;
                    result = CompareRelease(x.ReleaseLabels[i], y.ReleaseLabels[i]);
                    if (result != 0) return result;
                }
            }

            // compare the metadata
            if (_mode == VersionComparison.VersionReleaseMetadata)
            {
                result = StringComparer.OrdinalIgnoreCase.Compare(x.Metadata ?? string.Empty, y.Metadata ?? string.Empty);
                if (result != 0) return result;
            }

            return 0;
        }

        private int CompareRelease(string version1, string version2)
        {
            int version1Num, version2Num;

            // check if the identifiers are numeric
            var v1IsNumeric = int.TryParse(version1, out version1Num);
            var v2IsNumeric = int.TryParse(version2, out version2Num);

            // if both are numeric compare them as numbers
            if (v1IsNumeric && v2IsNumeric)
            {
                return version1Num.CompareTo(version2Num);
            }
            if (v1IsNumeric || v2IsNumeric)
            {
                // numeric labels come before alpha labels
                return v1IsNumeric ? -1 : 1;
            }
            // Ignoring 2.0.0 case sensitive compare. Everything will be compared case insensitively as 2.0.1 specifies.
            return StringComparer.OrdinalIgnoreCase.Compare(version1, version2);
        }

        #endregion
    }
}
