using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using Wheatech.Properties;

namespace Wheatech.Collection
{
    /// <summary>
    ///     Represents a generic collection of key/value pairs that are ordered independently of the key and value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
    {
        #region Fields

        /// <summary>
        ///     The default initial capacity.
        /// </summary>
        private const int DefaultInitialCapacity = 0;

        /// <summary>
        ///     The key type name.
        /// </summary>
        private static readonly string KeyTypeName = typeof(TKey).FullName;

        /// <summary>
        ///     The value type name.
        /// </summary>
        private static readonly string ValueTypeName = typeof(TValue).FullName;

        /// <summary>
        ///     Is value type a reference type.
        /// </summary>
        private static readonly bool IsValueTypeReferenceType = !typeof(ValueType).IsAssignableFrom(typeof(TValue));

        /// <summary>
        ///     The dictionary.
        /// </summary>
        private Dictionary<TKey, TValue> _dictionary;

        /// <summary>
        ///     The list.
        /// </summary>
        private List<KeyValuePair<TKey, TValue>> _list;

        /// <summary>
        ///     The comparer.
        /// </summary>
        private readonly IEqualityComparer<TKey> _comparer;

        /// <summary>
        ///     The sync root.
        /// </summary>
        private object _syncRoot;

        /// <summary>
        ///     The initial capacity.
        /// </summary>
        private readonly int _initialCapacity;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}" /> class.
        /// </summary>
        public OrderedDictionary()
            : this(DefaultInitialCapacity)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}" /> class using the specified comparer.
        /// </summary>
        /// <param name="comparer">
        ///     The <see cref="IEqualityComparer{TKey}"> IEqualityComparer&lt;TKey&gt; </see> to use when comparing keys, or <null /> to
        ///     use the default <see cref="EqualityComparer{TKey}"> EqualityComparer&lt;TKey&gt; </see> for the type of the key.
        /// </param>
        public OrderedDictionary(IEqualityComparer<TKey> comparer)
            : this(DefaultInitialCapacity, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}" /> class using the specified initial
        ///     capacity and comparer.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the
        ///     <see cref="OrderedDictionary{TKey,TValue}"></see> collection can contain.
        /// </param>
        /// <param name="comparer">
        ///     The <see cref="IEqualityComparer{TKey}"/> to use when comparing keys, or null to
        ///     use the default <see cref="EqualityComparer{TKey}"/> for the type of the key.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="capacity" /> is less than 0.
        /// </exception>
        public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer = null)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), string.Format(CultureInfo.CurrentCulture, Strings.ArgumentOutOfRange_NeedNonNegNum, nameof(capacity)));
            }

            _initialCapacity = capacity;
            _comparer = comparer;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets a value indicating whether the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection is read-only.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> is read-only; otherwise,
        ///     <see langword="false" />. The default is <see langword="false" />.
        /// </value>
        /// <remarks>
        ///     A collection that is read-only does not allow the addition, removal, or modification of elements after the
        ///     collection is created.
        ///     <para>
        ///         A collection that is read-only is simply a collection with a wrapper that prevents modification of the
        ///         collection; therefore, if changes are made to the underlying collection, the read-only collection reflects
        ///         those changes.
        ///     </para>
        /// </remarks>
        public bool IsReadOnly => false;

        /// <summary>
        ///     Gets an <see cref="System.Collections.Generic.ICollection{TKey}"/> object containing the
        ///     keys in the <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <value>
        ///     An <see cref="System.Collections.Generic.ICollection{TKey}"/> object containing the keys
        ///     in the <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </value>
        /// <remarks>
        ///     The returned <see cref="System.Collections.Generic.ICollection{TKey}"/> object is not
        ///     a static copy; instead, the collection refers back to the keys in the original
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>. Therefore, changes to the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> continue to be reflected in the
        ///     key collection.
        /// </remarks>
        public ICollection<TKey> Keys => Dictionary.Keys;

        /// <summary>
        ///     Gets an <see cref="ICollection{TValue}"/> object containing the values in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <value>
        ///     An <see cref="ICollection{TValue}"/> object containing the values in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </value>
        /// <remarks>
        ///     The returned <see cref="ICollection{TValue}"/> object is not a static copy; instead,
        ///     the collection refers back to the values in the original
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>. Therefore, changes to the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> continue to be reflected in the
        ///     value collection.
        /// </remarks>
        public ICollection<TValue> Values => List.Select(x => x.Value).ToArray();

        /// <summary>
        ///     Gets the number of key/values pairs contained in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </summary>
        /// <value>
        ///     The number of key/value pairs contained in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </value>
        public int Count => List.Count;

        /// <summary>
        ///     Gets a value indicating whether the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> has a fixed size.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> has a fixed size; otherwise,
        ///     <see langword="false" />. The default is <see langword="false" />.
        /// </value>
        bool IDictionary.IsFixedSize => false;

        /// <summary>
        ///     Gets an <see cref="ICollection" /> object containing the keys in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <value>
        ///     An <see cref="ICollection" /> object containing the keys in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </value>
        /// <remarks>
        ///     The returned <see cref="ICollection" /> object is not a static copy; instead, the collection refers back to
        ///     the keys in the original <see cref="OrderedDictionary{TKey,TValue}"/>.
        ///     Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     continue to be reflected in the key collection.
        /// </remarks>
        ICollection IDictionary.Keys => (ICollection) Keys;

        /// <summary>
        ///     Gets an <see cref="ICollection" /> object containing the values in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </summary>
        /// <value>
        ///     An <see cref="ICollection" /> object containing the values in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </value>
        /// <remarks>
        ///     The returned <see cref="ICollection" /> object is not a static copy; instead, the <see cref="ICollection" />
        ///     refers back to the values in the original
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection. Therefore, changes
        ///     to the <see cref="OrderedDictionary{TKey,TValue}"/> continue to be reflected
        ///     in the <see cref="ICollection" />.
        /// </remarks>
        ICollection IDictionary.Values => (ICollection) Values;

        /// <summary>
        ///     Gets a value indicating whether access to the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> object is synchronized
        ///     (thread-safe).
        /// </summary>
        /// <value>This method always returns false.</value>
        bool ICollection.IsSynchronized => false;

        /// <summary>
        ///     Gets an object that can be used to synchronize access to the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> object.
        /// </summary>
        /// <value>
        ///     An object that can be used to synchronize access to the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> object.
        /// </value>
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }

                return _syncRoot;
            }
        }

        /// <summary>
        ///     Gets the dictionary object that stores the keys and values.
        /// </summary>
        /// <value>
        ///     The dictionary object that stores the keys and values for the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </value>
        /// <remarks>Accessing this property will create the dictionary object if necessary.</remarks>
        private Dictionary<TKey, TValue> Dictionary => _dictionary ?? (_dictionary = new Dictionary<TKey, TValue>(_initialCapacity, _comparer));

        /// <summary>
        ///     Gets the list object that stores the key/value pairs.
        /// </summary>
        /// <value>
        ///     The list object that stores the key/value pairs for the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </value>
        /// <remarks>Accessing this property will create the list object if necessary.</remarks>
        private List<KeyValuePair<TKey, TValue>> List => _list ?? (_list = new List<KeyValuePair<TKey, TValue>>(_initialCapacity));

        #endregion

        #region Indexers

        /// <summary>
        ///     Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <returns>The value of the item at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0.<br />
        ///     -or-<br />
        ///     Index is equal to or greater than <see cref="Count" />.
        /// </exception>
        public TValue this[int index]
        {
            get { return List[index].Value; }
            set
            {
                if (index >= Count || index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), Strings.ArgumentOutOfRange_Index);
                }

                var key = List[index].Key;
                List[index] = new KeyValuePair<TKey, TValue>(key, value);
                Dictionary[key] = value;
            }
        }

        /// <summary>
        ///     Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        ///     The value associated with the specified key. If the specified key is not found, attempting to get it returns
        ///     <null />, and attempting to set it creates a new element using the specified key.
        /// </returns>
        public TValue this[TKey key]
        {
            get { return Dictionary[key]; }
            set
            {
                if (Dictionary.ContainsKey(key))
                {
                    Dictionary[key] = value;
                    List[IndexOfKey(key)] = new KeyValuePair<TKey, TValue>(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <returns>The value of the item at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0.<br />
        ///     -or-<br />
        ///     Index is equal to or greater than <see cref="Count" />.
        /// </exception>
        object IOrderedDictionary.this[int index]
        {
            get { return this[index]; }

            set { this[index] = ConvertToValueType(value); }
        }

        /// <summary>
        ///     Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        ///     The value associated with the specified key. If the specified key is not found, attempting to get it returns
        ///     <null />, and attempting to set it creates a new element using the specified key.
        /// </returns>
        object IDictionary.this[object key]
        {
            get { return this[ConvertToKeyType(key)]; }

            set { this[ConvertToKeyType(key)] = ConvertToValueType(value); }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds an entry with the specified key and value into the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection with the lowest
        ///     available index.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. This value can be <null />.</param>
        /// <returns>The index of the newly added entry.</returns>
        /// <remarks>
        ///     A key cannot be <null />, but a value can be.
        ///     <para>
        ///         You can also use the <see cref="OrderedDictionary{TKey,TValue}.Item(TKey)" /> property to add new elements by
        ///         setting the value of a key that does not exist in the
        ///         <see cref="OrderedDictionary{TKey,TValue}"/> collection; however, if the
        ///         specified key already exists in the
        ///         <see cref="OrderedDictionary{TKey,TValue}"/>, setting the
        ///         <see cref="OrderedDictionary{TKey,TValue}.Item(TKey)" /> property overwrites the old value. In contrast, the
        ///         <see cref="Add" /> method does not modify existing elements.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is <null />.</exception>
        /// <exception cref="ArgumentException">
        ///     An element with the same key already exists in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </exception>
        public int Add(TKey key, TValue value)
        {
            Dictionary.Add(key, value);
            List.Add(new KeyValuePair<TKey, TValue>(key, value));
            return Count - 1;
        }

        /// <summary>
        ///     Removes the entry with the specified key from the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the key was found and the corresponding element was removed; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool Remove(TKey key)
        {
            if (null == key)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var index = IndexOfKey(key);
            if (index >= 0)
            {
                if (Dictionary.Remove(key))
                {
                    List.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Inserts a new entry into the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     collection with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">
        ///     The value of the entry to add. The value can be <null /> if the type of the values in the
        ///     dictionary is a reference type.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0.<br />
        ///     -or-<br />
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is <null />.</exception>
        /// <exception cref="ArgumentException">
        ///     An element with the same key already exists in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </exception>
        public void Insert(int index, TKey key, TValue value)
        {
            if (index > Count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Dictionary.Add(key, value);
            List.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        ///     Removes the entry at the specified index from the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </summary>
        /// <param name="index">The zero-based index of the entry to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0.<br />
        ///     -or-<br />
        ///     Index is equal to or greater than <see cref="Count" />.
        /// </exception>
        public void RemoveAt(int index)
        {
            if (index >= Count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), Strings.ArgumentOutOfRange_Index);
            }

            var key = List[index].Key;
            List.RemoveAt(index);
            Dictionary.Remove(key);
        }

        /// <summary>
        ///     Removes all elements from the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     collection.
        /// </summary>
        /// <remarks>The capacity is not changed as a result of calling this method.</remarks>
        public void Clear()
        {
            Dictionary.Clear();
            List.Clear();
        }

        /// <summary>
        ///     Determines whether the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     collection contains a specific key.
        /// </summary>
        /// <param name="key">
        ///     The key to locate in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection contains an element
        ///     with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is <null />.</exception>
        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        /// <summary>
        ///     Returns the zero-based index of the specified key in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">
        ///     The key to locate in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </param>
        /// <returns>
        ///     The zero-based index of <paramref name="key" />, if <paramref name="key" /> is found in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>; otherwise, -1.
        /// </returns>
        /// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
        public int IndexOfKey(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            for (var index = 0; index < List.Count; index++)
            {
                var entry = List[index];
                var next = entry.Key;
                if (null != _comparer)
                {
                    if (_comparer.Equals(next, key))
                    {
                        return index;
                    }
                }
                else if (next.Equals(key))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of <paramref name="value" />. This parameter can be passed
        ///     uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> contains an element with the
        ///     specified key; otherwise, <see langword="false" />.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

        /// <summary>
        ///     The IOrderedDictionary.GetEnumerator() implementation.
        /// </summary>
        /// <returns>
        ///     The <see cref="IDictionaryEnumerator" />.
        /// </returns>
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        ///     The IDictionary.GetEnumerator() implementation.
        /// </summary>
        /// <returns>
        ///     The <see cref="IDictionaryEnumerator" />.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        ///     The IEnumerable.GetEnumerator() implementation.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerator" />.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

        /// <summary>
        ///     The <see cref="IEnumerable{T}.GetEnumerator()"/> implementation.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerator" />.
        /// </returns>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => List.GetEnumerator();

        /// <summary>
        ///     Inserts a new entry into the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     collection with the specified key and value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">
        ///     The value of the entry to add. The value can be <null /> if the type of the values in the
        ///     dictionary is a reference type.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0.<br />
        ///     -or-<br />
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key" /> is <null />.<br />
        ///     -or-<br />
        ///     <paramref name="value" /> is <null />, and the value type of the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> is a value type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The key type of the <see cref="OrderedDictionary{TKey,TValue}"/> is not in
        ///     the inheritance hierarchy of <paramref name="key" />.<br />
        ///     -or-<br />
        ///     The value type of the <see cref="OrderedDictionary{TKey,TValue}"/> is not in
        ///     the inheritance hierarchy of <paramref name="value" />.<br />
        ///     -or-<br />
        ///     An element with the same key already exists in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </exception>
        void IOrderedDictionary.Insert(int index, object key, object value) => Insert(index, ConvertToKeyType(key), ConvertToValueType(value));

        /// <summary>
        ///     Adds an entry with the specified key and value into the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection with the lowest
        ///     available index.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. This value can be <null />.</param>
        /// <remarks>
        ///     A key cannot be <null />, but a value can be.
        ///     <para>
        ///         You can also use the <see cref="OrderedDictionary{TKey,TValue}.Item(TKey)" /> property to add new elements by
        ///         setting the value of a key that does not exist in the
        ///         <see cref="OrderedDictionary{TKey,TValue}"/> collection; however, if the
        ///         specified key already exists in the
        ///         <see cref="OrderedDictionary{TKey,TValue}"/>, setting the
        ///         <see cref="OrderedDictionary{TKey,TValue}.Item(TKey)" /> property overwrites the old value. In contrast, the
        ///         <see cref="Add" /> method does not modify existing elements.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is <null />.</exception>
        /// <exception cref="ArgumentException">
        ///     An element with the same key already exists in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </exception>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => Add(key, value);

        /// <summary>
        ///     Adds an entry with the specified key and value into the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection with the lowest
        ///     available index.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. This value can be <null />.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key" /> is <null />.<br />
        ///     -or-<br />
        ///     <paramref name="value" /> is <null />, and the value type of the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> is a value type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The key type of the <see cref="OrderedDictionary{TKey,TValue}"/> is not in
        ///     the inheritance hierarchy of <paramref name="key" />.<br />
        ///     -or-<br />
        ///     The value type of the <see cref="OrderedDictionary{TKey,TValue}"/> is not in
        ///     the inheritance hierarchy of <paramref name="value" />.
        /// </exception>
        void IDictionary.Add(object key, object value) => Add(ConvertToKeyType(key), ConvertToValueType(value));

        /// <summary>
        ///     Determines whether the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     collection contains a specific key.
        /// </summary>
        /// <param name="key">
        ///     The key to locate in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection contains an element
        ///     with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is <null />.</exception>
        /// <exception cref="ArgumentException">
        ///     The key type of the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> is not in the inheritance
        ///     hierarchy of <paramref name="key" />.
        /// </exception>
        bool IDictionary.Contains(object key) => ContainsKey(ConvertToKeyType(key));

        /// <summary>
        ///     Removes the entry with the specified key from the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> collection.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        void IDictionary.Remove(object key) => Remove(ConvertToKeyType(key));

        /// <summary>
        ///     Copies the elements of the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     elements to a one-dimensional Array object at the specified index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="Array" /> object that is the destination of the
        ///     <see cref="KeyValuePair{TKey,TValue}" /> objects copied from the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>. The <see cref="Array" /> must
        ///     have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <remarks>
        ///     The <see cref="ICollection.CopyTo" /> method preserves the order of the elements in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </remarks>
        void ICollection.CopyTo(Array array, int index) => ((ICollection) List).CopyTo(array, index);

        /// <summary>
        ///     Adds the specified value to the <see cref="OrderedDictionary{TKey,TValue}"/>
        ///     with the specified key.
        /// </summary>
        /// <param name="item">
        ///     The <see cref="KeyValuePair{TKey,TValue}"/> structure representing
        ///     the key and value to add to the <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <summary>
        ///     Determines whether the <see cref="OrderedDictionary{TKey,TValue}"/> contains
        ///     a specific key and value.
        /// </summary>
        /// <param name="item">
        ///     The <see cref="KeyValuePair{TKey,TValue}"/> structure to locate in
        ///     the <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is found in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>) Dictionary).Contains(item);

        /// <summary>
        ///     Copies the elements of the <see cref="OrderedDictionary{TKey,TValue}"/> to an
        ///     array of type <see cref="KeyValuePair{TKey,TValue}" />, starting at the specified index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional array of type
        ///     <see cref="KeyValuePair{TKey,TValue}"/> that is the destination of the
        ///     <see cref="KeyValuePair{TKey,TValue}"/> elements copied from the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>. The array must have zero-based
        ///     indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>) Dictionary).CopyTo(array, arrayIndex);

        /// <summary>
        ///     Removes a key and value from the dictionary.
        /// </summary>
        /// <param name="item">
        ///     The <see cref="KeyValuePair{TKey,TValue}"/> structure representing
        ///     the key and value to remove from the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the key and value represented by <paramref name="item" /> is successfully found and
        ///     removed; otherwise, <see langword="false" />. This method returns <see langword="false" /> if
        ///     <paramref name="item" /> is not found in the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/>.
        /// </returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        /// <summary>
        ///     Converts the object passed as a key to the key type of the dictionary.
        /// </summary>
        /// <param name="keyObject">The key object to check.</param>
        /// <returns>The key object, cast as the key type of the dictionary.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="keyObject" /> is <null />.</exception>
        /// <exception cref="ArgumentException">
        ///     The key type of the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> is not in the inheritance
        ///     hierarchy of <paramref name="keyObject" />.
        /// </exception>
        private static TKey ConvertToKeyType(object keyObject)
        {
            if (keyObject == null)
            {
                throw new ArgumentNullException(nameof(keyObject));
            }

            if (keyObject is TKey)
            {
                return (TKey) keyObject;
            }

            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Arg_WrongType, keyObject, KeyTypeName), nameof(keyObject));
        }

        /// <summary>
        ///     Converts the object passed as a value to the value type of the dictionary.
        /// </summary>
        /// <param name="valueObject">The object to convert to the value type of the dictionary.</param>
        /// <returns>The value object, converted to the value type of the dictionary.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="valueObject" /> is <null />, and the value type of the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> is a value type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The value type of the
        ///     <see cref="OrderedDictionary{TKey,TValue}"/> is not in the inheritance
        ///     hierarchy of <paramref name="valueObject" />.
        /// </exception>
        private static TValue ConvertToValueType(object valueObject)
        {
            if (null == valueObject)
            {
                if (IsValueTypeReferenceType)
                {
                    return default(TValue);
                }

                throw new ArgumentNullException(nameof(valueObject));
            }

            if (valueObject is TValue)
            {
                return (TValue) valueObject;
            }

            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Arg_WrongType, valueObject, ValueTypeName), nameof(valueObject));
        }

        #endregion
    }
}