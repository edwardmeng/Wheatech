using System;
using System.Globalization;
using Wheatech.Properties;
using static System.String;

namespace Wheatech
{
    internal sealed class VersionComparator : IVersionComparator
    {
        #region Fields

        private readonly Version _version;
        private readonly VersionFloatBehavior _floatBehavior;
        private readonly string _releasePrefix;
        private readonly VersionOperator _operator;
        internal string _originalString;

        #endregion

        #region Constructors
        public VersionComparator(Version version)
            : this(version, VersionOperator.Equal)
        {
        }

        public VersionComparator(Version version, VersionOperator @operator)
            : this(version, VersionFloatBehavior.None, @operator)
        {
        }

        public VersionComparator(Version version, VersionFloatBehavior floatBehavior)
            : this(version, floatBehavior, VersionOperator.Equal)
        {
        }

        public VersionComparator(Version version, VersionFloatBehavior floatBehavior, VersionOperator @operator)
            : this(version, floatBehavior, null, @operator)
        {
        }

        public VersionComparator(Version version, VersionFloatBehavior floatBehavior, string releasePrefix)
            : this(version, floatBehavior, releasePrefix, VersionOperator.Equal)
        {

        }

        public VersionComparator(Version version, VersionFloatBehavior floatBehavior, string releasePrefix, VersionOperator @operator)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            _version = version;
            _floatBehavior = floatBehavior;
            _releasePrefix = releasePrefix;
            _operator = @operator;
            if (_releasePrefix == null && version.IsPrerelease)
            {
                // use the actual label if one was not given
                _releasePrefix = version.Release;
            }
        }

        #endregion

        #region Properties

        public Version Version => _version;

        public VersionFloatBehavior FloatBehavior => _floatBehavior;

        public string ReleasePrefix => _releasePrefix;

        public VersionOperator Operator => _operator;

        #endregion

        #region Methods

        /// <summary>
        /// True if the given version falls into the floating range.
        /// </summary>
        public bool Match(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            int result = 0;
            switch (FloatBehavior)
            {
                case VersionFloatBehavior.None:
                    result = VersionComparer.VersionRelease.Compare(_version, version);
                    break;
                case VersionFloatBehavior.Prerelease:
                    // allow the stable version to match
                    result = VersionComparer.Version.Compare(_version, version);
                    if (result == 0)
                    {
                        if (!version.IsPrerelease)
                        {
                            result = -1;
                        }
                        else if (version.IsPrerelease && !version.Release.StartsWith(_releasePrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            result = StringComparer.OrdinalIgnoreCase.Compare(_releasePrefix, version.Release);
                        }
                    }
                    break;
                case VersionFloatBehavior.Revision:
                    if (version.IsPrerelease) return false;
                    result = new System.Version(_version.Major, _version.Minor, _version.Patch).CompareTo(new System.Version(version.Major, version.Minor, version.Patch));
                    break;
                case VersionFloatBehavior.Patch:
                    if (version.IsPrerelease) return false;
                    result = new System.Version(_version.Major, _version.Minor).CompareTo(new System.Version(version.Major, version.Minor));
                    break;
                case VersionFloatBehavior.Minor:
                    if (version.IsPrerelease) return false;
                    result = new System.Version(_version.Major, 0).CompareTo(new System.Version(version.Major, 0));
                    break;
                case VersionFloatBehavior.Major:
                    if (version.IsPrerelease) return false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (_operator)
            {
                case VersionOperator.Equal:
                    return result == 0;
                case VersionOperator.NotEqual:
                    return result != 0;
                case VersionOperator.GreaterThan:
                    return result < 0;
                case VersionOperator.GreaterThanEqual:
                    return result <= 0;
                case VersionOperator.LessThan:
                    return result > 0;
                case VersionOperator.LessThanEqual:
                    return result >= 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static VersionFloatBehavior DetermineFloatBehavior(ref string version)
        {
            var versionParts = version.Split('.').Length;
            if (versionParts == 1)
            {
                version += ".0";
                return VersionFloatBehavior.Minor;
            }
            if (versionParts == 2)
            {
                return VersionFloatBehavior.Patch;
            }
            if (versionParts == 3)
            {
                return VersionFloatBehavior.Revision;
            }
            return VersionFloatBehavior.None;
        }

        internal static bool TryParse(string versionString, VersionOperator @operator, out VersionComparator comparator)
        {
            comparator = null;
            if (versionString == null) return false;
            versionString = versionString.Trim();
            if (IsNullOrEmpty(versionString)) return false;

            if (versionString == "*")
            {
                comparator = new VersionComparator(new Version(new System.Version(0, 0)), VersionFloatBehavior.Major, @operator)
                {
                    _originalString = versionString
                };
                return true;
            }
            if (versionString.Length < 3) return false;
            var realVersion = versionString;
            if (versionString[0] == 'v' || versionString[0] == 'V')
            {
                realVersion = versionString.Substring(1);
            }
            var hyphenIndex = realVersion.IndexOf('-');
            var behavior = VersionFloatBehavior.None;
            // Has floating behavior
            if (realVersion.EndsWith(".x") || realVersion[realVersion.Length - 1] == '*')
            {
                var actualVersion = realVersion.Substring(0, realVersion.Length - 1);
                string releasePrefix = null;
                if (hyphenIndex == -1)
                {
                    actualVersion = actualVersion.TrimEnd('.');
                    behavior = DetermineFloatBehavior(ref actualVersion);
                }
                else
                {
                    behavior = VersionFloatBehavior.Prerelease;
                    releasePrefix = actualVersion.Substring(hyphenIndex + 1);
                    if (hyphenIndex == actualVersion.Length - 1) // ends with '-'
                    {
                        // remove the empty release label, the version will be release but
                        // the behavior will have to account for this
                        actualVersion = actualVersion.Substring(0, actualVersion.Length - 1);
                    }
                    if (actualVersion[actualVersion.Length - 1] == '.')
                    {
                        // ending with a . is not allowed
                        actualVersion = actualVersion.Substring(0, actualVersion.Length - 1);
                    }
                }

                Version version;
                if (Version.TryParse(actualVersion, out version))
                {
                    // there is no float range for this version
                    comparator = new VersionComparator(version, behavior, releasePrefix, @operator)
                    {
                        _originalString = versionString
                    };
                }
            }
            else
            {
                if (hyphenIndex == -1 && (@operator == VersionOperator.LessThan || @operator == VersionOperator.LessThanEqual))
                {
                    behavior = DetermineFloatBehavior(ref realVersion);
                }
                // normal version parse
                Version version;
                if (Version.TryParse(realVersion, out version))
                {
                    // there is no float range for this version
                    comparator = new VersionComparator(version, behavior, @operator)
                    {
                        _originalString = versionString
                    };
                }
            }

            return comparator != null;
        }

        /// <summary>
        /// Parse a floating version into a FloatRange
        /// </summary>
        public static bool TryParse(string comparatorString, out VersionComparator comparator)
        {
            comparator = null;
            if (comparatorString == null) return false;
            comparatorString = comparatorString.Trim();
            if (IsNullOrEmpty(comparatorString)) return false;
            VersionOperator @operator = VersionOperator.Equal;
            var versionString = comparatorString;
            if (comparatorString.StartsWith("=="))
            {
                @operator = VersionOperator.Equal;
                versionString = comparatorString.Substring(2, comparatorString.Length - 2).Trim();
            }
            else if (comparatorString.StartsWith("!="))
            {
                @operator = VersionOperator.NotEqual;
                versionString = comparatorString.Substring(2, comparatorString.Length - 2).Trim();
            }
            else if (comparatorString.StartsWith("="))
            {
                @operator = VersionOperator.Equal;
                versionString = comparatorString.Substring(1, comparatorString.Length - 1).Trim();
            }
            else if (comparatorString.StartsWith(">="))
            {
                @operator = VersionOperator.GreaterThanEqual;
                versionString = comparatorString.Substring(2, comparatorString.Length - 2).Trim();
            }
            else if (comparatorString.StartsWith(">"))
            {
                @operator = VersionOperator.GreaterThan;
                versionString = comparatorString.Substring(1, comparatorString.Length - 1).Trim();
            }
            else if (comparatorString.StartsWith("<="))
            {
                @operator = VersionOperator.LessThanEqual;
                versionString = comparatorString.Substring(2, comparatorString.Length - 2).Trim();
            }
            else if (comparatorString.StartsWith("<>"))
            {
                @operator = VersionOperator.NotEqual;
                versionString = comparatorString.Substring(2, comparatorString.Length - 2).Trim();
            }
            else if (comparatorString.StartsWith("<"))
            {
                @operator = VersionOperator.LessThan;
                versionString = comparatorString.Substring(1, comparatorString.Length - 1).Trim();
            }
            else if (comparatorString.EndsWith("+"))
            {
                @operator = VersionOperator.GreaterThanEqual;
                versionString = comparatorString.Substring(0, comparatorString.Length - 1).Trim();
            }
            else if (comparatorString.EndsWith("-"))
            {
                @operator = VersionOperator.LessThanEqual;
                versionString = comparatorString.Substring(0, comparatorString.Length - 1).Trim();
            }
            if (TryParse(versionString, @operator, out comparator))
            {
                comparator._originalString = comparatorString;
                return true;
            }
            return false;
        }

        public static VersionComparator Parse(string value)
        {
            VersionComparator ver;
            if (!TryParse(value, out ver))
            {
                throw new ArgumentException(Format(CultureInfo.CurrentCulture, Strings.InvalidVersion, value), nameof(value));
            }
            return ver;
        }

        /// <summary>
        /// Create a floating version string in the format: 1.0.0-alpha-*
        /// </summary>
        public override string ToString()
        {
            if (!IsNullOrEmpty(_originalString)) return _originalString;
            var result = Empty;
            string comparator;
            switch (Operator)
            {
                case VersionOperator.NotEqual:
                    comparator = "!=";
                    break;
                case VersionOperator.GreaterThan:
                    comparator = ">";
                    break;
                case VersionOperator.GreaterThanEqual:
                    comparator = ">=";
                    break;
                case VersionOperator.LessThan:
                    comparator = "<";
                    break;
                case VersionOperator.LessThanEqual:
                    comparator = "<=";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (_floatBehavior)
            {
                case VersionFloatBehavior.None:
                    result = Version.ToString();
                    break;
                case VersionFloatBehavior.Prerelease:
                    result = Format(new VersionFormatter(), "{0:V}-{1}*", Version, _releasePrefix);
                    break;
                case VersionFloatBehavior.Revision:
                    result = Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.*", Version.Major, Version.Minor, Version.Patch);
                    break;
                case VersionFloatBehavior.Patch:
                    result = Format(CultureInfo.InvariantCulture, "{0}.{1}.*", Version.Major, Version.Minor);
                    break;
                case VersionFloatBehavior.Minor:
                    result = Format(CultureInfo.InvariantCulture, "{0}.*", Version.Major);
                    break;
                case VersionFloatBehavior.Major:
                    result = "*";
                    break;
            }

            return comparator + result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as VersionComparator;
            return other != null && VersionComparer.Default.Equals(Version, other.Version) &&
                String.Equals(_releasePrefix, other._releasePrefix, StringComparison.OrdinalIgnoreCase) &&
                _floatBehavior == other._floatBehavior &&
                _operator == other._operator;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _version.GetHashCode();
                hashCode = (hashCode * 397) ^ (_releasePrefix != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(_releasePrefix) : 0);
                hashCode = (hashCode * 397) ^ (int)_floatBehavior;
                hashCode = (hashCode * 397) ^ (int)_operator;
                return hashCode;
            }
        }

        #endregion
    }
}
