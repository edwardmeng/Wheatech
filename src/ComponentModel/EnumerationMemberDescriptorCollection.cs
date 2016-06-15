using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Wheatech.Properties;

namespace Wheatech.ComponentModel
{
    /// <summary>
    /// Represents a collection of <see cref="EnumerationMemberDescriptor"/> objects.
    /// </summary>
    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public class EnumerationMemberDescriptorCollection : IEnumerable<EnumerationMemberDescriptor>
    {
        #region Fields

        private readonly List<EnumerationMemberDescriptor> _descriptors;
        private HybridDictionary _caseSensitiveDisplayDictionary;
        private HybridDictionary _caseInsensitiveDisplayDictionary;
        private HybridDictionary _caseSensitiveNameDictionary;
        private HybridDictionary _caseInsensitiveNameDictionary;

        #endregion

        internal EnumerationMemberDescriptorCollection(IEnumerable<EnumerationMemberDescriptor> descriptors)
        {
            _descriptors = descriptors != null
                               ? new List<EnumerationMemberDescriptor>(descriptors.OrderBy(descriptor => descriptor.Order))
                               : new List<EnumerationMemberDescriptor>();
        }

        /// <summary>
        /// Returns the <see cref="EnumerationMemberDescriptor"/> with the specified name, 
        /// using a <see cref="Boolean"/> to indicate whether to ignore case.
        /// </summary>
        /// <param name="name">The name of the <see cref="EnumerationMemberDescriptor"/> to return from the collection. </param>
        /// <param name="ignoreCase"><c>true</c> if you want to ignore the case of the enumeration name; otherwise, <c>false</c>. </param>
        /// <returns>A <see cref="EnumerationMemberDescriptor"/> with the specified name, or null if the enumeration does not exist.</returns>
        public virtual EnumerationMemberDescriptor FindByName(string name, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(Strings.Argument_Cannot_Be_Null_Or_Empty, nameof(name));
            return (EnumerationMemberDescriptor)(ignoreCase ? GetCaseInsensitiveNameDictionary() : GetCaseSensitiveNameDictionary())[name];
        }

        /// <summary>
        /// Returns the <see cref="EnumerationMemberDescriptor"/> with the specified display text,
        /// using a <see cref="Boolean"/> to indicate whether to ignore case.
        /// </summary>
        /// <param name="display">The display text of the <see cref="EnumerationMemberDescriptor"/> to return from the collection.</param>
        /// <param name="ignoreCase"><c>true</c> if you want to ignore the case of the enumeration display text; otherwise, <c>false</c>. </param>
        /// <returns>A <see cref="EnumerationMemberDescriptor"/> with the specified display text, or null if the enumeration does not exist.</returns>
        public virtual EnumerationMemberDescriptor FindByDisplay(string display, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(display)) throw new ArgumentException(Strings.Argument_Cannot_Be_Null_Or_Empty, nameof(display));
            return (EnumerationMemberDescriptor)(ignoreCase ? GetCaseInsensitiveDisplayDictionary() : GetCaseSensitiveDisplayDictionary())[display];
        }

        private HybridDictionary GetCaseSensitiveNameDictionary()
        {
            if (_caseSensitiveNameDictionary == null)
            {
                _caseSensitiveNameDictionary = new HybridDictionary();
                foreach (var descriptor in _descriptors)
                {
                    _caseSensitiveNameDictionary.Add(descriptor.Name, descriptor);
                }
            }
            return _caseSensitiveNameDictionary;
        }

        private HybridDictionary GetCaseInsensitiveNameDictionary()
        {
            if (_caseInsensitiveNameDictionary == null)
            {
                _caseInsensitiveNameDictionary = new HybridDictionary(true);
                foreach (var descriptor in _descriptors)
                {
                    if (!_caseInsensitiveNameDictionary.Contains(descriptor.Name))
                    {
                        _caseInsensitiveNameDictionary.Add(descriptor.Name, descriptor);
                    }
                }
            }
            return _caseInsensitiveNameDictionary;
        }

        private HybridDictionary GetCaseSensitiveDisplayDictionary()
        {
            if (_caseSensitiveDisplayDictionary == null)
            {
                _caseSensitiveDisplayDictionary = new HybridDictionary();
                foreach (var descriptor in _descriptors)
                {
                    if (!_caseSensitiveDisplayDictionary.Contains(descriptor.DisplayName))
                    {
                        _caseSensitiveDisplayDictionary.Add(descriptor.DisplayName, descriptor);
                    }
                }
            }
            return _caseSensitiveDisplayDictionary;
        }

        private HybridDictionary GetCaseInsensitiveDisplayDictionary()
        {
            if (_caseInsensitiveDisplayDictionary == null)
            {
                _caseInsensitiveDisplayDictionary = new HybridDictionary(true);
                foreach (var descriptor in _descriptors)
                {
                    if (!_caseInsensitiveDisplayDictionary.Contains(descriptor.DisplayName))
                    {
                        _caseInsensitiveDisplayDictionary.Add(descriptor.DisplayName, descriptor);
                    }
                }
            }
            return _caseInsensitiveDisplayDictionary;
        }

        /// <summary>
        /// Gets the number of member descriptors in the collection.
        /// </summary>
        /// <value>The number of member descriptors in the collection.</value>
        public int Count => _descriptors.Count;

        /// <summary>
        /// Gets or sets the <see cref="EnumerationMemberDescriptor"/> with the specified name.
        /// </summary>
        /// <param name="name">The name of the <see cref="EnumerationMemberDescriptor"/> to get from the collection. </param>
        /// <returns>The <see cref="EnumerationMemberDescriptor"/> with the specified name, or null if the property does not exist.</returns>
        public virtual EnumerationMemberDescriptor this[string name] => FindByName(name, false);

        /// <summary>
        /// Gets the <see cref="EnumerationMemberDescriptor"/> at the specified index number.
        /// </summary>
        /// <param name="index">The zero-based index of the <see cref="EnumerationMemberDescriptor"/> to get. </param>
        /// <returns>The <see cref="EnumerationMemberDescriptor"/> with the specified index number.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// The <paramref name="index"/> parameter is not a valid index for <see cref="this[int]"/>. 
        /// </exception>
        public virtual EnumerationMemberDescriptor this[int index]
        {
            get
            {
                if (index < 0 || index >= _descriptors.Count) throw new IndexOutOfRangeException();
                return _descriptors[index];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}" /> that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<EnumerationMemberDescriptor> IEnumerable<EnumerationMemberDescriptor>.GetEnumerator() => _descriptors.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator() => ((IEnumerable<EnumerationMemberDescriptor>)this).GetEnumerator();
    }
}
