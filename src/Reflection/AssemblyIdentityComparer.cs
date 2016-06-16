using System.Collections.Generic;
using System.Linq;

namespace Wheatech
{
    /// <summary>
    /// An comparer for <see cref="AssemblyIdentity"/> type.
    /// </summary>
    public class AssemblyIdentityComparer : IEqualityComparer<AssemblyIdentity>
    {
        #region Fields

        /// <summary>
        /// A default comparer that compares all informations.
        /// </summary>
        public static readonly AssemblyIdentityComparer Default = new AssemblyIdentityComparer(AssemblyIdentityComparison.Default);

        /// <summary>
        /// A comparer that uses only the assembly short name.
        /// </summary>
        public static readonly AssemblyIdentityComparer ShortName = new AssemblyIdentityComparer(AssemblyIdentityComparison.ShortName);

        /// <summary>
        /// A comparer that uses only the assembly short name and version.
        /// </summary>
        public static readonly AssemblyIdentityComparer Version = new AssemblyIdentityComparer(AssemblyIdentityComparison.Version);

        /// <summary>
        /// A comparer that uses only the assembly short name, version and culture.
        /// </summary>
        public static readonly AssemblyIdentityComparer Culture = new AssemblyIdentityComparer(AssemblyIdentityComparison.Culture);

        /// <summary>
        /// A comparer that uses only the assembly short name, version, culture and public key token.
        /// </summary>
        public static readonly AssemblyIdentityComparer PublicKeyToken = new AssemblyIdentityComparer(AssemblyIdentityComparison.PublicKeyToken);
        /// <summary>
        /// A comparer that uses only the assembly short name, version, culture, public key token and processor architecture.
        /// </summary>
        public static readonly AssemblyIdentityComparer Architecture = new AssemblyIdentityComparer(AssemblyIdentityComparison.Architecture);

        private readonly AssemblyIdentityComparison _comparison;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyIdentityComparer"/> using the default mode.
        /// </summary>
        public AssemblyIdentityComparer()
            : this(AssemblyIdentityComparison.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyIdentityComparer"/> that respects the given comparison mode.
        /// </summary>
        /// <param name="comparison">The comparison mode.</param>
        public AssemblyIdentityComparer(AssemblyIdentityComparison comparison)
        {
            _comparison = comparison;
        }

        #endregion

        /// <summary>
        /// Returns a value indicates whether two assembly identities are equal.
        /// </summary>
        /// <param name="x">An assembly identity to compare to <paramref name="y"/>.</param>
        /// <param name="y">An assembly identity to compare to <paramref name="x"/>.</param>
        /// <value>
        /// <c>true</c> if <paramref name="x"/> and <paramref name="y"/> refer to the same object, 
        /// or <paramref name="x"/> and <paramref name="y"/> are equal, 
        /// or <paramref name="x"/> and <paramref name="y"/> are null; otherwise, <c>false</c>.
        /// </value>
        public bool Equals(AssemblyIdentity x, AssemblyIdentity y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            if (_comparison >= AssemblyIdentityComparison.ShortName && !string.Equals(x.ShortName, y.ShortName))
            {
                return false;
            }
            if (_comparison >= AssemblyIdentityComparison.Version && !Equals(x.Version, y.Version))
            {
                return false;
            }
            if (_comparison >= AssemblyIdentityComparison.Culture && !Equals(x.Culture, y.Culture))
            {
                return false;
            }
            if (_comparison >= AssemblyIdentityComparison.PublicKeyToken)
            {
                if (x.PublicKeyToken == null && y.PublicKeyToken == null) return true;
                if (x.PublicKeyToken == null || y.PublicKeyToken == null) return false;
                return x.PublicKeyToken.SequenceEqual(y.PublicKeyToken);
            }
            if (_comparison >= AssemblyIdentityComparison.Architecture)
            {
                return x.Architecture == y.Architecture;
            }
            return true;
        }

        /// <summary>
        /// Gets the hash code for the specified <see cref="AssemblyIdentity"/>.
        /// </summary>
        /// <param name="obj">The <see cref="AssemblyIdentity"/> to calculate hash code.</param>
        /// <returns>A 32-bit signed hash code calculated from the value of the <paramref name="obj"/> parameter.</returns>
        public int GetHashCode(AssemblyIdentity obj)
        {
            unchecked
            {
                int hashCode = obj.ShortName.GetHashCode();
                if (_comparison >= AssemblyIdentityComparison.Version)
                {
                    hashCode = (hashCode * 397) ^ (obj.Version?.GetHashCode() ?? 0);
                }
                if (_comparison >= AssemblyIdentityComparison.Culture)
                {
                    hashCode = (hashCode * 397) ^ (obj.Culture?.GetHashCode() ?? 0);
                }
                if (_comparison >= AssemblyIdentityComparison.PublicKeyToken)
                {
                    if (obj.PublicKeyToken != null)
                    {
                        hashCode = obj.PublicKeyToken.Aggregate(hashCode, (code, x) => (hashCode * 397) ^ x.GetHashCode());
                    }
                    else
                    {
                        hashCode = (hashCode * 397) ^ 0;
                    }
                }
                if (_comparison >= AssemblyIdentityComparison.Architecture)
                {
                    hashCode = (hashCode * 397) ^ obj.Architecture.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}
