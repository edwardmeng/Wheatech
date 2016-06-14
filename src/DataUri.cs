using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wheatech.Properties;

namespace Wheatech
{
    /// <summary>
    /// The data URI scheme is a uniform resource identifier (URI) scheme that provides a way to include data in-line in web pages as if they were external resources.
    /// </summary>
    /// <seealso cref="https://en.wikipedia.org/wiki/Data_URI_scheme"/>
    /// <seealso cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/data_URIs"/>
    /// <seealso cref="http://tools.ietf.org/html/rfc2397"/>
    [Serializable]
    public abstract class DataUri
    {
        #region Nested Types

        private class ParseResult
        {
            internal string MediaType;
            internal readonly NameValueCollection Parameters = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            internal string Encoding;
            internal string Data;
            private readonly bool _canThrow;
            private ParseFailureKind _failure;
            private string _failureArgumentName;
            private string _failureMessage;
            private object _failureMessageFormatArgument;
            private Exception _innerException;

            public ParseResult(bool canThrow)
            {
                _canThrow = canThrow;
                _failure = ParseFailureKind.None;
                _failureMessage = null;
                _failureMessageFormatArgument = null;
                _failureArgumentName = null;
                _innerException = null;
            }

            public void SetFailure(ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument = null,
                string failureArgumentName = null, Exception innerException = null)
            {
                _failure = failure;
                _failureMessage = failureMessageID;
                _failureMessageFormatArgument = failureMessageFormatArgument;
                _failureArgumentName = failureArgumentName;
                _innerException = innerException;
                if (_canThrow)
                {
                    throw GetParseException();
                }
            }

            public Exception GetParseException()
            {
                switch (_failure)
                {
                    case ParseFailureKind.ArgumentNull:
                        return new ArgumentNullException(_failureArgumentName, _failureMessage);

                    case ParseFailureKind.Format:
                        return new FormatException(_failureMessage);

                    case ParseFailureKind.FormatWithParameter:
                        return new FormatException(string.Format(_failureMessage, _failureMessageFormatArgument));

                    case ParseFailureKind.NativeException:
                        return _innerException;

                    case ParseFailureKind.FormatWithInnerException:
                        return new FormatException(_failureMessage, _innerException);
                }
                return new FormatException("Unrecognized data uri format.");
            }
        }

        private enum ParseFailureKind
        {
            None,
            ArgumentNull,
            Format,
            FormatWithParameter,
            NativeException,
            FormatWithInnerException
        }

        private class ReadOnlyNameValueCollection : NameValueCollection
        {
            internal ReadOnlyNameValueCollection(IEqualityComparer equalityComparer) : base(equalityComparer)
            {
            }

            internal ReadOnlyNameValueCollection(NameValueCollection value) : base(value)
            {
            }

            internal void SetReadOnly()
            {
                IsReadOnly = true;
            }
        }

        #endregion

        #region Fields

        private const string DefaultMediaType = "text/plain";
        private readonly string _mediaType;
        private readonly ReadOnlyNameValueCollection _parameters;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataUri"/> by using the specified media type and additional parameters.
        /// </summary>
        /// <param name="mediaType">The internet media type.</param>
        /// <param name="parameters">The additional parameters.</param>
        protected DataUri(string mediaType, NameValueCollection parameters)
        {
            _mediaType = string.IsNullOrEmpty(mediaType) ? DefaultMediaType : mediaType;
            _parameters = parameters == null ? new ReadOnlyNameValueCollection(StringComparer.InvariantCultureIgnoreCase) : new ReadOnlyNameValueCollection(parameters);
            _parameters.SetReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataUri"/> by using the specified media type.
        /// </summary>
        /// <param name="mediaType">The internet media type.</param>
        protected DataUri(string mediaType)
            : this(mediaType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataUri"/>.
        /// </summary>
        protected DataUri()
            : this(null)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an internet media type specification of the data URI. The default value is 'text/plain'.
        /// </summary>
        [DefaultValue(DefaultMediaType)]
        public string MediaType => _mediaType;

        /// <summary>
        /// Gets additional parameters of the data URI.
        /// </summary>
        public NameValueCollection Parameters => _parameters;

        /// <summary>
        /// Gets the content encoding algrithem of the data URI.
        /// </summary>
        public abstract string Encoding { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a string representation of the value of this instance.
        /// </summary>
        /// <returns>A <see cref="String"/> that contains the canonical representation of the data URI instance.</returns>
        public override string ToString()
        {
            var result = new StringBuilder("data:");
            if (!string.Equals(_mediaType, DefaultMediaType, StringComparison.InvariantCultureIgnoreCase))
            {
                result.Append(_mediaType).Append(";");
            }
            foreach (string parameterName in Parameters)
            {
                result.Append(parameterName).Append("=").Append(Parameters[parameterName]).Append(";");
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DataUri"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        protected virtual bool Equals(DataUri other)
        {
            if (!string.Equals(other.MediaType, MediaType, StringComparison.InvariantCultureIgnoreCase) || Parameters.Count != other.Parameters.Count)
            {
                return false;
            }
            foreach (string parameterName in Parameters)
            {
                if (Parameters[parameterName] != other.Parameters[parameterName])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a value that indicates whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="DataUri"/> that has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as DataUri;
            if (other == null) return false;
            return Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _mediaType != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(_mediaType) : 0;
                foreach (string parameterName in Parameters)
                {
                    hashCode = (hashCode * 397) ^ (parameterName?.GetHashCode() ?? 0);
                    var parameterValue = Parameters[parameterName];
                    hashCode = (hashCode * 397) ^ (parameterValue?.GetHashCode() ?? 0);
                }
                return hashCode;
            }
        }

        #endregion

        #region Parse

        /// <summary>
        /// Converts the string to the equivalent data URI.
        /// </summary>
        /// <param name="uriString">The string to convert.</param>
        /// <param name="uri">The instance that will contain the parsed value. If the method returns true, result contains a valid data URI. If the method returns false, result is null.</param>
        /// <returns><c>true</c> if the parse operation was successful; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string uriString, out DataUri uri)
        {
            ParseResult result = new ParseResult(false);
            if (TryParse(uriString, result) && CreateDataUri(result, out uri)) return true;
            uri = null;
            return false;
        }

        /// <summary>
        /// Converts the string to the equivalent data URI.
        /// </summary>
        /// <param name="uriString">The string to convert.</param>
        /// <returns>The instance that contains the value that was parsed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="uriString"/> is null.</exception>
        /// <exception cref="FormatException"><paramref name="uriString"/> is not in a recognized format.</exception>
        public static DataUri Parse(string uriString)
        {
            if (uriString == null)
            {
                throw new ArgumentNullException(nameof(uriString));
            }
            var result = new ParseResult(true);
            DataUri uri;
            if (!TryParse(uriString, result) || !CreateDataUri(result, out uri))
            {
                throw result.GetParseException();
            }
            return uri;
        }

        private static bool CreateDataUri(ParseResult result, out DataUri uri)
        {
            uri = null;
            if (string.IsNullOrEmpty(result.Data))
            {
                result.SetFailure(ParseFailureKind.Format, Strings.DataUri_Invalid_Format, failureArgumentName: "uriString");
                return false;
            }
            if (string.IsNullOrEmpty(result.Encoding))
            {
                uri = new TextDataUri(result.MediaType, result.Parameters, result.Data);
                return true;
            }
            if (string.Equals(result.Encoding, "base64", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    uri = new Base64DataUri(result.MediaType, result.Parameters, Convert.FromBase64String(result.Data));
                    return true;
                }
                catch (FormatException ex)
                {
                    result.SetFailure(ParseFailureKind.FormatWithInnerException, Strings.DataUri_Invalid_Format, failureArgumentName: "uriString", innerException: ex);
                    return false;
                }
                catch (Exception ex)
                {
                    result.SetFailure(ParseFailureKind.NativeException, null, innerException: ex);
                    return false;
                }
            }
            result.SetFailure(ParseFailureKind.Format, string.Format(Strings.DataUri_Unknown_Encoding, result.Encoding), failureArgumentName: "uriString");
            return false;
        }

        private static bool TryParse(string uriString, ParseResult result)
        {
            // data:[<media type>][;charset=<character set>][;base64],<data>
            if (uriString == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNull, Strings.DataUri_Cannot_Be_Null, failureArgumentName: nameof(uriString));
                return false;
            }
            uriString = uriString.Trim();
            if (uriString.Length == 0)
            {
                result.SetFailure(ParseFailureKind.Format, Strings.DataUri_Cannot_Be_Empty, failureArgumentName: nameof(uriString));
                return false;
            }
            // The data uri must be starts with 'data:'
            const string dataPrefix = "data:";
            if (!uriString.StartsWith(dataPrefix))
            {
                result.SetFailure(ParseFailureKind.Format, Strings.DataUri_Invalid_Format, failureArgumentName: nameof(uriString));
                return false;
            }
            uriString = uriString.Substring(dataPrefix.Length).Trim();
            Func<Predicate<string>, string> getUriSection = predicate =>
            {
                var semicolon = uriString.IndexOf(';');
                var comma = uriString.IndexOf(',');
                int endIndex;
                if (semicolon < 0)
                {
                    endIndex = comma;
                }
                else if (comma < 0)
                {
                    endIndex = semicolon;
                }
                else
                {
                    endIndex = Math.Min(semicolon, comma);
                }
                if (endIndex >= 0)
                {
                    string section = uriString.Substring(0, endIndex);
                    if (predicate == null || predicate(section))
                    {
                        uriString = endIndex == semicolon
                            ? uriString.Substring(semicolon + 1).Trim()
                            : uriString.Substring(comma).Trim();
                        return section;
                    }
                }
                return string.Empty;
            };
            // parse optional mimetype
            var mediaType = getUriSection(null);
            if (!string.IsNullOrEmpty(mediaType) && !Regex.IsMatch(mediaType, "^\\w+\\/\\w+$"))
            {
                result.SetFailure(ParseFailureKind.Format, string.Format(CultureInfo.CurrentCulture, Strings.DataUri_Invalid_MediaType, mediaType), failureArgumentName: nameof(uriString));
                return false;
            }
            result.MediaType = mediaType;

            string sectionString;
            while (!string.IsNullOrEmpty(sectionString = getUriSection(section => section.IndexOf('=') > 0)))
            {
                var equalIndex = sectionString.IndexOf('=');
                var parameterName = sectionString.Substring(0, equalIndex).Trim();
                if (string.IsNullOrEmpty(parameterName))
                {
                    result.SetFailure(ParseFailureKind.Format, Strings.DataUri_Invalid_Format, failureArgumentName: nameof(uriString));
                    return false;
                }
                result.Parameters.Add(parameterName, sectionString.Substring(equalIndex + 1).Trim());
            }
            // parse data section
            if (string.IsNullOrEmpty(uriString))
            {
                result.SetFailure(ParseFailureKind.Format, Strings.DataUri_Invalid_Format, failureArgumentName: nameof(uriString));
                return false;
            }
            var commaIndex = uriString.IndexOf(',');
            if (commaIndex < 0)
            {
                result.SetFailure(ParseFailureKind.Format, Strings.DataUri_Invalid_Format, failureArgumentName: nameof(uriString));
                return false;
            }
            result.Encoding = uriString.Substring(0, commaIndex).Trim();
            result.Data = uriString.Substring(commaIndex + 1).Trim();
            return true;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Indicates whether the values of two specified <see cref="DataUri"/> objects are equal.
        /// </summary>
        /// <param name="uri1">The first object to compare.</param>
        /// <param name="uri2">The second object to compare.</param>
        /// <returns><c>true</c> if <paramref name="uri1"/> and <paramref name="uri2"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DataUri uri1, DataUri uri2)
        {
            return ReferenceEquals(uri1, uri2) || (uri1 != null && uri2 != null && uri1.Equals(uri2));
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="DataUri"/> objects are not equal.
        /// </summary>
        /// <param name="uri1">The first object to compare.</param>
        /// <param name="uri2">The second object to compare.</param>
        /// <returns><c>true</c> if <paramref name="uri1"/> and <paramref name="uri2"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DataUri uri1, DataUri uri2)
        {
            return !(uri1 == uri2);
        }

        #endregion
    }

    /// <summary>
    /// The data URI scheme with base64 encoding algrithm.
    /// </summary>
    public sealed class Base64DataUri : DataUri
    {
        private readonly byte[] _content;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64DataUri"/> by using the specified media type, additional parameters and binary content.
        /// </summary>
        /// <param name="mediaType">The internet media type.</param>
        /// <param name="parameters">The additional parameters.</param>
        /// <param name="content">The binary content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
        public Base64DataUri(string mediaType, NameValueCollection parameters, byte[] content) : base(mediaType, parameters)
        {
            if (content == null) throw new ArgumentNullException(nameof(content), Strings.DataUri_Content_Cannot_Be_Null);
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64DataUri"/> by using the specified media type and binary content.
        /// </summary>
        /// <param name="mediaType">The internet media type.</param>
        /// <param name="content">The binary content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
        public Base64DataUri(string mediaType, byte[] content) : base(mediaType)
        {
            if (content == null) throw new ArgumentNullException(nameof(content), Strings.DataUri_Content_Cannot_Be_Null);
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Base64DataUri"/> by using the binary content.
        /// </summary>
        /// <param name="content">The binary content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
        public Base64DataUri(byte[] content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content), Strings.DataUri_Content_Cannot_Be_Null);
            _content = content;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the decoded binary content for the data URI.
        /// </summary>
        public byte[] Content => _content;

        /// <summary>
        /// Gets the content encoding algrithem of the data URI. The value is base64 for the <see cref="Base64DataUri"/>.
        /// </summary>
        public override string Encoding { get; } = "base64";

        #endregion

        #region Methods

        /// <summary>
        /// Returns a string representation of the value of this instance.
        /// </summary>
        /// <returns>A <see cref="String"/> that contains the canonical representation of the data URI instance.</returns>
        public override string ToString()
        {
            return base.ToString() + "base64," + Convert.ToBase64String(_content);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DataUri"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is a <see cref="Base64DataUri"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        protected override bool Equals(DataUri other)
        {
            if (!base.Equals(other)) return false;
            var uri = other as Base64DataUri;
            if (uri == null) return false;
            return Content.SequenceEqual(uri.Content);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return _content.Aggregate(base.GetHashCode(), (hashCode, b) => (hashCode * 397) ^ b.GetHashCode());
            }
        }

        #endregion
    }

    /// <summary>
    /// The data URI scheme with text content.
    /// </summary>
    public sealed class TextDataUri : DataUri
    {
        private readonly string _content;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDataUri"/> by using the specified media type, additional parameters and text content.
        /// </summary>
        /// <param name="mediaType">The internet media type.</param>
        /// <param name="parameters">The additional parameters.</param>
        /// <param name="content">The text content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
        public TextDataUri(string mediaType, NameValueCollection parameters, string content) : base(mediaType, parameters)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(Strings.DataUri_Content_Cannot_Be_Null_Or_Empty, nameof(content));
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDataUri"/> by using the specified media type and text content.
        /// </summary>
        /// <param name="mediaType">The internet media type.</param>
        /// <param name="content">The text content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
        public TextDataUri(string mediaType, string content) : base(mediaType)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(Strings.DataUri_Content_Cannot_Be_Null_Or_Empty, nameof(content));
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDataUri"/> by using the specified text content.
        /// </summary>
        /// <param name="content">The text content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is null.</exception>
        public TextDataUri(string content)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(Strings.DataUri_Content_Cannot_Be_Null_Or_Empty, nameof(content));
            _content = content;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the content for the data URI.
        /// </summary>
        public string Content => _content;

        /// <summary>
        /// Gets the content encoding algrithem of the data URI. The value is empty string for the <see cref="TextDataUri"/>.
        /// </summary>
        public override string Encoding { get; } = string.Empty;

        #endregion

        #region Methods

        /// <summary>
        /// Returns a string representation of the value of this instance.
        /// </summary>
        /// <returns>A <see cref="String"/> that contains the canonical representation of the data URI instance.</returns>
        public override string ToString()
        {
            return base.ToString() + "," + _content;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="DataUri"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is a <see cref="TextDataUri"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        protected override bool Equals(DataUri other)
        {
            if (!base.Equals(other)) return false;
            var uri = other as TextDataUri;
            if (uri == null) return false;
            return Content == uri.Content;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Content?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        #endregion
    }
}
