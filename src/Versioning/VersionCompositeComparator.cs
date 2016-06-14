using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wheatech
{
    internal enum VersionCompositor
    {
        And,
        Or
    }

    internal sealed class VersionCompositeComparator : IEnumerable<IVersionComparator>, IVersionComparator
    {
        #region Fields

        private readonly List<IVersionComparator> _comparators = new List<IVersionComparator>();
        private readonly VersionCompositor _compositor;
        private string _originalString;

        #endregion

        #region Constructors

        public VersionCompositeComparator(VersionCompositeComparator comparator)
        {
            _comparators.AddRange(comparator);
            _compositor = comparator._compositor;
            _originalString = comparator._originalString;
        }

        public VersionCompositeComparator()
            : this((IEnumerable<VersionComparator>)null)
        {
        }

        public VersionCompositeComparator(params System.Version[] versions)
            : this((IEnumerable<System.Version>)versions)
        {
        }

        public VersionCompositeComparator(IEnumerable<System.Version> versions, VersionCompositor compositor = VersionCompositor.And)
            : this(versions?.Select(version => version == null ? null : new Version(version)), compositor)
        {
        }

        public VersionCompositeComparator(params Version[] versions)
            : this((IEnumerable<Version>)versions)
        {
        }

        public VersionCompositeComparator(IEnumerable<Version> versions, VersionCompositor compositor = VersionCompositor.And)
            : this(versions?.Select(version => version == null ? null : new VersionComparator(version)), compositor)
        {
        }

        public VersionCompositeComparator(params IVersionComparator[] comparators)
            : this((IEnumerable<IVersionComparator>)comparators)
        {
        }

        public VersionCompositeComparator(IEnumerable<IVersionComparator> comparators, VersionCompositor compositor = VersionCompositor.And)
            : this(comparators, compositor, null)
        {
        }

        internal VersionCompositeComparator(IEnumerable<IVersionComparator> comparators, VersionCompositor compositor, string originalString)
        {
            if (comparators != null)
            {
                foreach (var comparator in comparators)
                {
                    InternalAdd(comparator);
                }
            }
            _compositor = compositor;
            _originalString = originalString;
        }

        #endregion

        #region Properties

        public int Count => _comparators.Count;

        public IVersionComparator this[int index] => _comparators[index];

        public VersionCompositor Compositor => _compositor;

        #endregion

        #region Methods

        #region Add

        private void InternalAdd(IVersionComparator comparator)
        {
            if (comparator != null)
            {
                _comparators.Add(comparator);
            }
        }

        public void Add(IVersionComparator comparator)
        {
            if (comparator == null)
            {
                throw new ArgumentNullException(nameof(comparator));
            }
            InternalAdd(comparator);
        }

        public void Add(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }
            InternalAdd(new VersionComparator(version));
        }

        public void Add(System.Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }
            InternalAdd(new VersionComparator(new Version(version)));
        }

        #endregion

        #region Enumerable

        public IEnumerator<IVersionComparator> GetEnumerator()
        {
            return _comparators.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Satisfies

        /// <summary>
        /// Determines if an SemanticVersion meets the requirements.
        /// </summary>
        /// <param name="version">SemVer to compare.</param>
        /// <returns>True if the given version meets the version requirements.</returns>
        public bool Satisfies(Version version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            return _compositor == VersionCompositor.And
                ? _comparators.All(comparator => comparator.Satisfies(version))
                : _comparators.Any(comparator => comparator.Satisfies(version));
        }

        /// <summary>
        /// Determines if an version meets the requirements.
        /// </summary>
        /// <param name="version">Version to compare.</param>
        /// <returns>True if the given version meets the version requirements.</returns>
        public bool Satisfies(System.Version version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            return Satisfies(new Version(version));
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_originalString)) return _originalString;
            if (_compositor == VersionCompositor.And)
            {
                _originalString = string.Join(" && ", _comparators.Select(comparator =>
                 {
                     var composite = comparator as VersionCompositeComparator;
                     return composite != null && composite.Compositor == VersionCompositor.Or ? "(" + composite + ")" : comparator.ToString();
                 }));
            }
            else
            {
                _originalString = string.Join(" || ", _comparators.Select(comparator => comparator.ToString()));
            }
            return _originalString;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;
            var other = obj as VersionCompositeComparator;
            if (Compositor != other?.Compositor) return false;
            if (other._comparators.Count != _comparators.Count) return false;
            return _comparators.All(other._comparators.Contains) && other._comparators.All(_comparators.Contains);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Compositor.GetHashCode();
                foreach (var comparator in _comparators)
                {
                    hashCode = (hashCode * 397) ^ comparator.GetHashCode();
                }
                return hashCode;
            }
        }

        #endregion
    }
}
