using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;

namespace Wheatech
{
    /// <summary>
    /// Provides support for lazy initialization.
    /// </summary>
    /// <typeparam name="T">Specifies the type of object that is being lazily initialized.</typeparam>
    [Serializable]
    [ComVisible(false)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
    [DebuggerDisplay("ThreadSafetyMode={Mode}, IsValueCreated={IsValueCreated}, IsValueFaulted={IsValueFaulted}, Value={ValueForDebugDisplay}")]
    [DebuggerTypeProxy(typeof(LazyDebugView<>))]
    public class Lazy<T>
    {
        #region Fields

        [NonSerialized]
        private readonly Func<T> _valueFactory;

        private System.Lazy<T> _lazy;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class. 
        /// When lazy initialization occurs, the default constructor of the target type is used.
        /// </summary>
        public Lazy()
            : this(LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class. 
        /// When lazy initialization occurs, the default constructor of the target type and the specified initialization mode are used.
        /// </summary>
        /// <param name="isThreadSafe">
        /// <c>true</c> to make this instance usable concurrently by multiple threads; 
        /// <c>false</c> to make the instance usable by only one thread at a time. 
        /// </param>
        public Lazy(bool isThreadSafe)
            : this(isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class that uses the default constructor of T 
        /// and the specified thread-safety mode.
        /// </summary>
        /// <param name="mode">
        /// One of the enumeration values that specifies the thread safety mode.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="mode"/> contains an invalid value.
        /// </exception>
        public Lazy(LazyThreadSafetyMode mode)
        {
            Mode = mode;
            _lazy = CreateLazy();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class. When lazy initialization occurs, 
        /// the specified initialization function is used.
        /// </summary>
        /// <param name="valueFactory">
        /// The delegate that is invoked to produce the lazily initialized value when it is needed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueFactory"/> is <see langword="null"/>.
        /// </exception>
        public Lazy(Func<T> valueFactory)
            : this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class. 
        /// When lazy initialization occurs, the specified initialization function and initialization mode are used.
        /// </summary>
        /// <param name="valueFactory">
        /// The delegate that is invoked to produce the lazily initialized value when it is needed.
        /// </param>
        /// <param name="isThreadSafe">
        /// <c>true</c> to make this instance usable concurrently by multiple threads; 
        /// <c>false</c> to make the instance usable by only one thread at a time. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueFactory"/> is <see langword="null"/>.
        /// </exception>
        public Lazy(Func<T> valueFactory, bool isThreadSafe)
            : this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class 
        /// that uses the specified initialization function and thread-safety mode.
        /// </summary>
        /// <param name="valueFactory">
        /// The delegate that is invoked to produce the lazily initialized value when it is needed.
        /// </param>
        /// <param name="mode">
        /// One of the enumeration values that specifies the thread safety mode.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueFactory"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="mode"/> contains an invalid value.
        /// </exception>
        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
        {
            _valueFactory = valueFactory;
            Mode = mode;
            _lazy = CreateLazy();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the lazily initialized value of the current <see cref="Lazy{T}"/> instance.
        /// </summary>
        /// <value>The lazily initialized value of the current <see cref="Lazy{T}"/> instance.</value>
        /// <exception cref="MemberAccessException">
        /// The <see cref="Lazy{T}"/> instance is initialized to use the default constructor of the type 
        /// that is being lazily initialized, and permissions to access the constructor are missing.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// The <see cref="Lazy{T}"/> instance is initialized to use the default constructor of the type 
        /// that is being lazily initialized, and that type does not have a public, parameterless constructor.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The initialization function tries to access <see cref="Value"/> on this instance.
        /// </exception>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value
        {
            get
            {
                try
                {
                    return _lazy.Value;
                }
                catch (Exception)
                {
                    IsValueFaulted = true;
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether a value has been created for this <see cref="Lazy{T}"/> instance.
        /// </summary>
        /// <value><c>true</c> if a value has been created for this <see cref="Lazy{T}"/> instance; otherwise, <c>false</c>.</value>
        public bool IsValueCreated => _lazy.IsValueCreated;

        internal bool IsValueFaulted { get; private set; }

        internal T ValueForDebugDisplay => !IsValueCreated ? default(T) : Value;

        internal LazyThreadSafetyMode Mode { get; }

        #endregion

        #region Methods

        private System.Lazy<T> CreateLazy()
        {
            return _valueFactory != null ? new System.Lazy<T>(_valueFactory, Mode) : new System.Lazy<T>(Mode);
        }

        /// <summary>
        /// Resets the initialized value of the current <see cref="Lazy{T}"/> instance.
        /// </summary>
        public void Reset()
        {
            _lazy = CreateLazy();
            IsValueFaulted = false;
        }

        /// <summary>
        /// Creates and returns a string representation of the <see cref="Value"/> property for this instance.
        /// </summary>
        /// <returns>
        /// The result of calling the <see cref="ToString"/> method on the <see cref="Value"/> property for this instance, 
        /// if the value has been created (that is, if the <see cref="IsValueCreated"/> property returns true). 
        /// Otherwise, a string indicating that the value has not been created. 
        /// </returns>
        /// <exception cref="NullReferenceException">
        /// The <see cref="Value"/> property is <see langword="null"/>.
        /// </exception>
        public override string ToString()
        {
            return _lazy.ToString();
        }

        #endregion
    }

    #region LazyDebugView

    internal class LazyDebugView<T>
    {
        private readonly Lazy<T> _lazy;

        public LazyDebugView(Lazy<T> lazy)
        {
            _lazy = lazy;
        }

        public bool IsValueCreated => _lazy.IsValueCreated;

        public bool IsValueFaulted => _lazy.IsValueFaulted;

        public LazyThreadSafetyMode Mode => _lazy.Mode;

        public T Value => _lazy.ValueForDebugDisplay;
    }

    #endregion
}
