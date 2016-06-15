using System;

namespace Wheatech.ComponentModel
{
    /// <summary>
    /// Specifies the alias name for a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AliasNameAttribute : Attribute
    {
        /// <summary>
        /// Specifies the default value for the <see cref="AliasNameAttribute"/>. This field is read-only.
        /// </summary>
        public static readonly AliasNameAttribute Default = new AliasNameAttribute();

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasNameAttribute"/> class using the alias name.
        /// </summary>
        /// <param name="aliasName">The alias name.</param>
        public AliasNameAttribute(string aliasName)
        {
            AliasNameValue = aliasName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasNameAttribute"/> class.
        /// </summary>
        protected AliasNameAttribute()
        {
        }

        /// <summary>
        /// Gets the alias name for a field in this attribute.
        /// </summary>
        /// <returns>
        /// The alias name.
        /// </returns>
        public virtual string AliasName => AliasNameValue;

        /// <summary>
        /// Gets or sets the alias name.
        /// </summary>
        /// <returns>
        /// The alias name.
        /// </returns>
        protected string AliasNameValue { get; set; }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="value">The object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is an instance of <see cref="AliasNameAttribute"/> and equals the value of this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value)) return false;
            if (ReferenceEquals(this, value)) return true;
            var other = value as AliasNameAttribute;
            return other != null && string.Equals(AliasNameValue, other.AliasNameValue);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (AliasNameValue?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        /// Determines if this attribute is the default.
        /// </summary>
        /// <returns><c>true</c> if the attribute is the default value for this attribute class; otherwise, <c>false</c>.</returns>
        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}
