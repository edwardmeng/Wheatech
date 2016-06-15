using System.Collections.Generic;
using System.Linq;

namespace Wheatech
{
    public class AssemblyIdentityComparer : IEqualityComparer<AssemblyIdentity>
    {
        public static readonly AssemblyIdentityComparer Default = new AssemblyIdentityComparer(AssemblyIdentityComparison.Default);
        public static readonly AssemblyIdentityComparer ShortName = new AssemblyIdentityComparer(AssemblyIdentityComparison.ShortName);
        public static readonly AssemblyIdentityComparer Version = new AssemblyIdentityComparer(AssemblyIdentityComparison.Version);
        public static readonly AssemblyIdentityComparer Culture = new AssemblyIdentityComparer(AssemblyIdentityComparison.Culture);
        public static readonly AssemblyIdentityComparer PublicKeyToken = new AssemblyIdentityComparer(AssemblyIdentityComparison.PublicKeyToken);
        public static readonly AssemblyIdentityComparer Architecture = new AssemblyIdentityComparer(AssemblyIdentityComparison.Architecture);

        private readonly AssemblyIdentityComparison _comparison;

        public AssemblyIdentityComparer()
            : this(AssemblyIdentityComparison.Default)
        {
        }

        public AssemblyIdentityComparer(AssemblyIdentityComparison comparison)
        {
            _comparison = comparison;
        }

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

        public int GetHashCode(AssemblyIdentity obj)
        {
            unchecked
            {
                int hashCode = obj.ShortName.GetHashCode();
                if (_comparison >= AssemblyIdentityComparison.Version)
                {
                    hashCode = (hashCode * 397) ^ (obj.Version != null ? obj.Version.GetHashCode() : 0);
                }
                if (_comparison >= AssemblyIdentityComparison.Culture)
                {
                    hashCode = (hashCode * 397) ^ (obj.Culture != null ? obj.Culture.GetHashCode() : 0);
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
