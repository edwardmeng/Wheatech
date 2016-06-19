using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Wheatech.Properties;

namespace Wheatech
{
    /// <summary>
    /// Describes an assembly's identity information.
    /// </summary>
    [Serializable]
    public sealed class AssemblyIdentity : ISerializable, IDeserializationCallback
    {
        #region Fields

        private string _originalString;
        private string _shortName;
        private System.Version _version;
        private byte[] _publicKeyToken;
        private CultureInfo _culture;
        private ProcessorArchitecture _processorArchitecture;
        private readonly SerializationInfo _serializationInfo;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyIdentity"/> by using the specified assembly string.
        /// </summary>
        /// <param name="assemblyString">The assembly string.</param>
        public AssemblyIdentity(string assemblyString)
            : this(Parse(assemblyString))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyIdentity"/> by using the specified assembly name, version, culture, public key token and processor architecture.
        /// </summary>
        /// <param name="shortName">The short name of assembly.</param>
        /// <param name="version">The version of assembly.</param>
        /// <param name="culture">The culture of assembly.</param>
        /// <param name="publicKeyToken">The public key token of assembly.</param>
        /// <param name="architecture">The processor architecture of assembly.</param>
        public AssemblyIdentity(string shortName, System.Version version = null, CultureInfo culture = null, byte[] publicKeyToken = null,
            ProcessorArchitecture architecture = ProcessorArchitecture.None)
        {
            if (string.IsNullOrEmpty(shortName))
            {
                throw new ArgumentException(Strings.Argument_Cannot_Be_Null_Or_Empty, nameof(shortName));
            }
            _shortName = shortName;
            _version = version;
            _culture = culture;
            _publicKeyToken = publicKeyToken;
            _processorArchitecture = architecture;
        }

        internal AssemblyIdentity(SerializationInfo info, StreamingContext context)
        {
            _serializationInfo = info;
        }

        private AssemblyIdentity(AssemblyIdentity identity)
        {
            _shortName = identity.ShortName;
            _version = identity.Version;
            _culture = identity.Culture;
            _publicKeyToken = identity.PublicKeyToken;
            _processorArchitecture = identity.Architecture;
            _originalString = identity._originalString;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the simple name of the assembly.
        /// </summary>
        /// <value>The simple name of the assembly.</value>
        public string ShortName => _shortName;

        /// <summary>
        /// Gets or sets the major, minor, build, and revision numbers of the assembly.
        /// </summary>
        /// <value>An object that represents the major, minor, build, and revision numbers of the assembly.</value>
        public System.Version Version => _version;

        /// <summary>
        /// Gets the public key token, which is the last 8 bytes of the SHA-1 hash of the public key under which the application or assembly is signed.
        /// </summary>
        /// <value>A byte array that contains the public key token.</value>
        public byte[] PublicKeyToken => _publicKeyToken;

        /// <summary>
        /// Gets or sets the culture supported by the assembly.
        /// </summary>
        /// <value>An object that represents the culture supported by the assembly.</value>
        public CultureInfo Culture => _culture;

        /// <summary>
        /// Gets or sets the name of the culture associated with the assembly.
        /// </summary>
        /// <value>The culture name.</value>
        public string CultureName => _culture?.Name;

        /// <summary>
        /// Gets or sets a value that identifies the processor and bits-per-word of the platform targeted by an executable.
        /// </summary>
        /// <value>One of the enumeration values that identifies the processor and bits-per-word of the platform targeted by an executable.</value>
        public ProcessorArchitecture Architecture => _processorArchitecture;

        #endregion

        #region Methods

        /// <summary>
        /// Parses the assembly string to the equivalent assembly identity.
        /// </summary>
        /// <param name="assemblyString">The assembly string to parse.</param>
        /// <param name="identity">The instance that will contain the parsed value. If the method returns <c>true</c>, result contains a valid assembly identity. If the method returns <c>false</c>, result is null.</param>
        /// <returns><c>true</c> if the parse operation was successful; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string assemblyString, out AssemblyIdentity identity)
        {
            identity = null;
            if (string.IsNullOrWhiteSpace(assemblyString)) return false;
            var parts = assemblyString.Split(',');
            string shortName = null, versionText = null, cultureName = null, publicKeyTokenText = null, processorArchitectureText = null;
            foreach (var part in parts)
            {
                var spices = part.Split('=');
                if (spices.Length == 1)
                {
                    if (!string.IsNullOrEmpty(shortName))
                    {
                        return false;
                    }
                    shortName = spices[0].Trim();
                }
                else if (spices.Length == 2)
                {
                    switch (spices[0].Trim().ToLower())
                    {
                        case "version":
                            versionText = spices[1].Trim();
                            break;
                        case "culture":
                            cultureName = spices[1].Trim();
                            break;
                        case "publickeytoken":
                            publicKeyTokenText = spices[1].Trim();
                            break;
                        case "processorArchitecture":
                            processorArchitectureText = spices[1].Trim();
                            break;
                        default:
                            return false;
                    }
                }
            }
            if (string.IsNullOrEmpty(shortName)) return false;
            System.Version version = null;
            if (!string.IsNullOrEmpty(versionText) && !System.Version.TryParse(versionText, out version)) return false;
            CultureInfo culture;
            if (!TryParseCulture(cultureName, out culture)) return false;
            byte[] publicKeyToken;
            if (!TryParsepPublicKeyToken(publicKeyTokenText, out publicKeyToken)) return false;
            ProcessorArchitecture architecture;
            if (!TryParseArchitecture(processorArchitectureText, out architecture)) return false;
            identity = new AssemblyIdentity(shortName, version, culture, publicKeyToken, architecture) { _originalString = assemblyString };
            return true;
        }

        internal static bool TryParseCulture(string cultureName, out CultureInfo culture)
        {
            culture = null;
            if (string.IsNullOrEmpty(cultureName) || string.Equals(cultureName, "neutral", StringComparison.OrdinalIgnoreCase)) return true;
            try
            {

                culture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
            return true;
        }

        internal static bool TryParsepPublicKeyToken(string publicKeyTokenText, out byte[] publicKeyToken)
        {
            publicKeyToken = null;
            if (string.IsNullOrEmpty(publicKeyTokenText) || string.Equals(publicKeyTokenText, "null", StringComparison.OrdinalIgnoreCase)) return true;
            if (publicKeyTokenText.Length % 2 != 0) return false;
            publicKeyToken = new byte[publicKeyTokenText.Length / 2];
            for (int i = 0; i < publicKeyToken.Length; i++)
            {
                byte byteValue;
                if (!byte.TryParse(publicKeyTokenText.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byteValue))
                {
                    return false;
                }
                publicKeyToken[i] = byteValue;
            }
            return true;
        }

        internal static bool TryParseArchitecture(string processorArchitectureText, out ProcessorArchitecture architecture)
        {
            architecture = ProcessorArchitecture.None;
            return string.IsNullOrEmpty(processorArchitectureText) || Enum.TryParse(processorArchitectureText, true, out architecture);
        }

        /// <summary>
        /// Parses the assembly string to the equivalent assembly identity.
        /// </summary>
        /// <param name="assemblyString">The assembly string to parse.</param>
        /// <returns>The instance that contains the value that was parsed.</returns>
        /// <exception cref="ArgumentException"><paramref name="assemblyString"/> is null or not in a recognized format.</exception>
        public static AssemblyIdentity Parse(string assemblyString)
        {
            if (string.IsNullOrEmpty(assemblyString))
            {
                throw new ArgumentException(Strings.Argument_Cannot_Be_Null_Or_Empty, nameof(assemblyString));
            }
            AssemblyIdentity identity;
            if (!TryParse(assemblyString, out identity))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.AssemblyString_InvalidFormat, assemblyString), nameof(assemblyString));
            }
            return identity;
        }

        /// <summary>
        /// Returns a string representation of the value of this instance.
        /// </summary>
        /// <returns>A <see cref="String"/> that contains the canonical representation of the data URI instance.</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_originalString)) return _originalString;
            _originalString = _shortName;
            if (_version != null || _publicKeyToken != null)
            {
                _originalString += $", Version={_version?.ToString() ?? "0.0.0"}, Culture={_culture?.Name ?? "neutral"}, PublicKeyToken={(_publicKeyToken == null ? "null" : BitConverter.ToString(_publicKeyToken).Replace("-", ""))}";
            }
            if (_processorArchitecture != ProcessorArchitecture.None)
            {
                _originalString += $", processorArchitecture={_processorArchitecture.ToString().ToUpperInvariant()}";
            }
            return _originalString;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="AssemblyIdentity"/> object represent the same value based on the given comparison mode.
        /// </summary>
        /// <param name="other">The object to compare to this instance.</param>
        /// <param name="comparison">One of the enumeration values that specifies how the assembly identities will be compared.</param>
        /// <returns><c>true</c> if the <paramref name="other"/> parameter equals the value of this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(AssemblyIdentity other, AssemblyIdentityComparison comparison)
        {
            return new AssemblyIdentityComparer(comparison).Equals(this, other);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="AssemblyIdentity"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(AssemblyIdentity other)
        {
            return Equals(other, AssemblyIdentityComparison.Default);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="AssemblyName"/> object represent the same value based on the given comparison mode.
        /// </summary>
        /// <param name="other">The object to compare to this instance.</param>
        /// <param name="comparison">One of the enumeration values that specifies how the assembly identities will be compared.</param>
        /// <returns><c>true</c> if the <paramref name="other"/> parameter equals the value of this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(AssemblyName other, AssemblyIdentityComparison comparison)
        {
            if (other == null) return false;
            return Equals(new AssemblyIdentity(other.Name, other.Version, other.CultureInfo, other.GetPublicKeyToken()), comparison);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="AssemblyName"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(AssemblyName other)
        {
            return Equals(other, AssemblyIdentityComparison.Default);
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="AssemblyIdentity"/> that has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var identity = obj as AssemblyIdentity;
            if (identity != null) return Equals(identity);
            var name = obj as AssemblyName;
            if (name != null) return Equals(name);
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return AssemblyIdentityComparer.Default.GetHashCode(this);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (_serializationInfo == null) return;
            _shortName = _serializationInfo.GetString(nameof(ShortName));
            _version = (System.Version)_serializationInfo.GetValue(nameof(Version), typeof(System.Version));
            _publicKeyToken = (byte[])_serializationInfo.GetValue(nameof(PublicKeyToken), typeof(byte[]));
            int culture = _serializationInfo.GetInt32(nameof(Culture));
            if (culture != -1) _culture = new CultureInfo(culture);
            _originalString = _serializationInfo.GetString("OriginalString");
            _processorArchitecture = (ProcessorArchitecture)_serializationInfo.GetValue(nameof(Architecture), typeof(ProcessorArchitecture));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            info.AddValue(nameof(ShortName), ShortName);
            info.AddValue(nameof(Version), Version);
            info.AddValue(nameof(Culture), Culture?.LCID ?? -1);
            info.AddValue(nameof(PublicKeyToken), PublicKeyToken, typeof(byte[]));
            info.AddValue("OriginalString", _originalString);
            info.AddValue(nameof(Architecture), Architecture);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns a value that indicates whether two <see cref="AssemblyIdentity"/> values are equal.
        /// </summary>
        /// <param name="x">The first value to compare.</param>
        /// <param name="y">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(AssemblyIdentity x, AssemblyIdentity y)
        {
            return AssemblyIdentityComparer.Default.Equals(x, y);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="AssemblyIdentity"/> objects have different values.
        /// </summary>
        /// <param name="x">The first value to compare.</param>
        /// <param name="y">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(AssemblyIdentity x, AssemblyIdentity y)
        {
            return !AssemblyIdentityComparer.Default.Equals(x, y);
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="AssemblyIdentity"/> to a <see cref="AssemblyName"/>.
        /// </summary>
        /// <param name="identity">The <see cref="AssemblyIdentity"/> to convert.</param>
        public static implicit operator AssemblyName(AssemblyIdentity identity)
        {
            if (ReferenceEquals(identity, null)) return null;
            return new AssemblyName(identity.ToString());
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="AssemblyName"/> to a <see cref="AssemblyIdentity"/>.
        /// </summary>
        /// <param name="name">The <see cref="AssemblyName"/> to convert.</param>
        public static implicit operator AssemblyIdentity(AssemblyName name)
        {
            if (ReferenceEquals(name, null)) return null;
            return new AssemblyIdentity(name.Name, name.Version, name.CultureInfo, name.GetPublicKeyToken(), name.ProcessorArchitecture);
        }

        #endregion
    }
}
