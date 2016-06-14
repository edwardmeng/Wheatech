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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as AliasNameAttribute;
            return other != null && string.Equals(AliasNameValue, other.AliasNameValue);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (AliasNameValue != null ? AliasNameValue.GetHashCode() : 0);
            }
        }

        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}
