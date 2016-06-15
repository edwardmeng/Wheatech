using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Wheatech.Properties;

namespace Wheatech
{
    [Serializable]
    public sealed class AssemblyIdentity : ISerializable, IDeserializationCallback
    {
        #region Fields

        private string _originalString;
        private string _shortName;
        private Version _version;
        private byte[] _publicKeyToken;
        private CultureInfo _culture;
        private ProcessorArchitecture _processorArchitecture;
        private readonly SerializationInfo _serializationInfo;

        #endregion

        #region Constructors

        public AssemblyIdentity(string assemblyString)
            : this(Parse(assemblyString))
        {
        }

        public AssemblyIdentity(string shortName, Version version = null, CultureInfo culture = null, byte[] publicKeyToken = null,
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

        public string ShortName => _shortName;

        public Version Version => _version;

        public byte[] PublicKeyToken => _publicKeyToken;

        public CultureInfo Culture => _culture;

        public string CultureName => _culture?.Name;

        public ProcessorArchitecture Architecture => _processorArchitecture;

        #endregion

        #region Methods

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
            Version version = null;
            if (!string.IsNullOrEmpty(versionText) && !Version.TryParse(versionText, out version)) return false;
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

        public static AssemblyIdentity Parse(string assemblyString)
        {
            if (string.IsNullOrEmpty(assemblyString))
            {
                throw new ArgumentException(Strings.Argument_Cannot_Be_Null_Or_Empty, nameof(assemblyString));
            }
            AssemblyIdentity identity;
            if (!TryParse(assemblyString, out identity))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The assembly string format is invalid: {0}", assemblyString), nameof(assemblyString));
            }
            return identity;
        }

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

        public bool Equals(AssemblyIdentity other, AssemblyIdentityComparison comparison)
        {
            return new AssemblyIdentityComparer(comparison).Equals(this, other);
        }

        public bool Equals(AssemblyIdentity other)
        {
            return Equals(other, AssemblyIdentityComparison.Default);
        }

        public bool Equals(AssemblyName other, AssemblyIdentityComparison comparison)
        {
            if (other == null) return false;
            return Equals(new AssemblyIdentity(other.Name, other.Version, other.CultureInfo, other.GetPublicKeyToken()), comparison);
        }

        public bool Equals(AssemblyName other)
        {
            return Equals(other, AssemblyIdentityComparison.Default);
        }

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

        public override int GetHashCode()
        {
            return AssemblyIdentityComparer.Default.GetHashCode(this);
        }

        public void OnDeserialization(object sender)
        {
            if (_serializationInfo == null) return;
            _shortName = _serializationInfo.GetString("Name");
            _version = (Version)_serializationInfo.GetValue("Version", typeof(Version));
            _publicKeyToken = (byte[])_serializationInfo.GetValue("PublicKeyToken", typeof(byte[]));
            int culture = _serializationInfo.GetInt32("Culture");
            if (culture != -1) _culture = new CultureInfo(culture);
            _originalString = _serializationInfo.GetString("OriginalString");
            _processorArchitecture = (ProcessorArchitecture)_serializationInfo.GetValue("Architecture", typeof(ProcessorArchitecture));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");
            info.AddValue("Name", ShortName);
            info.AddValue("Version", Version);
            info.AddValue("Culture", Culture?.LCID ?? -1);
            info.AddValue("PublicKeyToken", PublicKeyToken, typeof(byte[]));
            info.AddValue("OriginalString", _originalString);
            info.AddValue("Architecture", Architecture);
        }

        #endregion

        #region Operators

        /// <summary>
        /// ==
        /// </summary>
        public static bool operator ==(AssemblyIdentity x, AssemblyIdentity y)
        {
            return AssemblyIdentityComparer.Default.Equals(x, y);
        }

        /// <summary>
        /// !=
        /// </summary>
        public static bool operator !=(AssemblyIdentity x, AssemblyIdentity y)
        {
            return !AssemblyIdentityComparer.Default.Equals(x, y);
        }

        public static implicit operator AssemblyName(AssemblyIdentity identity)
        {
            if (ReferenceEquals(identity, null)) return null;
            return new AssemblyName(identity.ToString());
        }

        public static implicit operator AssemblyIdentity(AssemblyName name)
        {
            if (name == null) return null;
            return new AssemblyIdentity(name.Name, name.Version, name.CultureInfo, name.GetPublicKeyToken(), name.ProcessorArchitecture);
        }

        #endregion
    }
}
