using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Wheatech.ComponentModel
{
    /// <summary>
    /// Provides an extension of <see cref="PropertyDescriptor"/> that represents a enumeration member.
    /// </summary>
    public sealed class EnumerationMemberDescriptor : MemberDescriptor
    {
        #region Fields

        private readonly FieldInfo _field;
        private readonly System.Lazy<int> _order;
        private readonly System.Lazy<string> _displayName;
        private readonly System.Lazy<string> _description;
        private readonly System.Lazy<string> _category;
        private readonly System.Lazy<string> _prompt;
        private readonly System.Lazy<string> _aliasName;
        private ResourceManager _resourceManager;

        #endregion

        internal EnumerationMemberDescriptor(Type enumerationType, FieldInfo field)
            : base(field.Name, Attribute.GetCustomAttributes(field))
        {
            EnumerationType = enumerationType;
            _field = field;
            _displayName = new System.Lazy<string>(GetDisplayName);
            _order = new System.Lazy<int>(GetOrder);
            _description = new System.Lazy<string>(GetDescription);
            _category = new System.Lazy<string>(GetCategory);
            _prompt = new System.Lazy<string>(GetPrompt);
            _aliasName = new System.Lazy<string>(GetAliasName);
        }

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this enumeration member should be localized, 
        /// as specified in the <see cref="LocalizableAttribute"/>.
        /// </summary>
        /// <value><c>true</c> if the member is marked with the <see cref="LocalizableAttribute"/> set to <c>true</c>; otherwise, <c>false</c>.</value>
        public bool Localizable => LocalizableAttribute.Yes.Equals(Attributes[typeof(LocalizableAttribute)]);

        /// <summary>
        /// Gets a value indicating whether this enumeration member is obsoleted, 
        /// as specified in the <see cref="ObsoleteAttribute"/>.
        /// </summary>
        /// <value><c>true</c> if the member is marked with the <see cref="ObsoleteAttribute"/>; otherwise, <c>false</c>.</value>
        public bool Obsolete => Attributes[typeof(ObsoleteAttribute)] != null;

        /// <summary>
        /// Gets the type of the enumeration.
        /// </summary>
        /// <value>
        /// The type of the enumeration.
        /// </value>
        public Type EnumerationType { get; }

        /// <summary>
        /// Gets the order in which the enumeration members are sorted,
        /// as specified in the <see cref="DisplayAttribute.Order"/>.
        /// </summary>
        /// <value>
        /// The order to be sorted.
        /// If there is no <see cref="DisplayAttribute"/>, 
        /// the property value is set to the underlying value of the enumeration member.
        /// </value>
        public int Order => _order.Value;

        /// <summary>
        /// Gets the description of the enumeration member, 
        /// as specified in the <see cref="DescriptionAttribute" /> or <see cref="DisplayAttribute.Description"/>.
        /// </summary>
        /// <returns>
        /// The description of the enumeration member. 
        /// If there is no <see cref="DescriptionAttribute" /> or <see cref="DisplayAttribute"/>, 
        /// the property value is set to the default, which is an empty string ("").
        /// </returns>
        public override string Description => _description.Value;

        /// <summary>
        /// Gets the name to display for the enumeration member, as specified in the <see cref="DisplayAttribute" />.
        /// </summary>
        /// <returns>
        /// The name to display for the enumeration member.
        /// </returns>
        public override string DisplayName => _displayName.Value;

        /// <summary>
        /// Gets the name of the category to which the member belongs, 
        /// as specified in the <see cref="CategoryAttribute" /> or <see cref="DisplayAttribute.GroupName"/>.
        /// </summary>
        /// <returns>
        /// The name of the category to which the member belongs. 
        /// If there is no <see cref="CategoryAttribute" /> or <see cref="DisplayAttribute"/>, 
        /// the category name is set to the default category, Misc.
        /// </returns>
        public override string Category => _category.Value;

        /// <summary>
        /// Gets a value that will be used to set the watermark for prompts.
        /// </summary>
        /// <value>A value that will be used to display a watermark.</value>
        public string Prompt => _prompt.Value;

        /// <summary>
        /// Gets the alias name for the enumeration member, as specified in the <see cref="AliasNameAttribute"/>.
        /// </summary>
        /// <returns>
        /// The alias name for the enumeration member.
        /// </returns>
        public string AliasName => _aliasName.Value;

        /// <summary>
        /// Gets the value of the enumeration member.
        /// </summary>
        /// <value>The value of the enumeration member.</value>
        public Enum Value => (Enum)_field.GetValue(null);

        /// <summary>
        /// Gets the underlying value of the enumeration member.
        /// </summary>
        /// <value>The underlying value of the enumeration member.</value>
        public object Data => _field.GetRawConstantValue();

        #endregion

        #region Methods

        private string GetLocalizeString(string resourceKey)
        {
            if (_resourceManager == null)
            {
                _resourceManager = new ResourceManager(EnumerationType);
            }
            try
            {
                return _resourceManager.GetString(resourceKey);
            }
            catch (MissingManifestResourceException)
            {
                return resourceKey;
            }
        }

        private string GetDisplayName()
        {
            string display = null;
            bool isRootAttribute = false;
            var displayNameAttribute = Attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
            if (displayNameAttribute != null && !displayNameAttribute.IsDefaultAttribute())
            {
                display = displayNameAttribute.DisplayName;
                isRootAttribute = displayNameAttribute.GetType() == typeof(DisplayNameAttribute);
            }
            if (string.IsNullOrEmpty(display))
            {
                var displayAttribute = Attributes[typeof(DisplayAttribute)] as DisplayAttribute;
                if (displayAttribute != null)
                {
                    display = displayAttribute.GetName();
                    isRootAttribute = true;
                    if (Localizable && displayAttribute.ResourceType != null && !string.IsNullOrEmpty(display))
                    {
                        return display;
                    }
                }
            }
            if (Localizable && !string.IsNullOrEmpty(display) && isRootAttribute)
            {
                display = GetLocalizeString(display);
            }
            return !string.IsNullOrEmpty(display) ? display : Name;
        }

        private string GetAliasName()
        {
            string aliasName = null;
            var aliasNameAttribute = Attributes[typeof(AliasNameAttribute)] as AliasNameAttribute;
            if (aliasNameAttribute != null && !aliasNameAttribute.IsDefaultAttribute())
            {
                aliasName = aliasNameAttribute.AliasName;
                if (Localizable && !string.IsNullOrEmpty(aliasName) && aliasNameAttribute.GetType() == typeof(AliasNameAttribute))
                {
                    return GetLocalizeString(aliasName);
                }
            }
            return string.IsNullOrEmpty(aliasName) ? Name : aliasName;
        }

        private int GetOrder()
        {
            return ((DisplayAttribute)Attributes[typeof(DisplayAttribute)])?.GetOrder() ?? ((IConvertible)Value).ToInt32(CultureInfo.InvariantCulture);
        }

        private string GetDescription()
        {
            string description = null;
            var descriptionAttribute = Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
            if (descriptionAttribute != null && !descriptionAttribute.IsDefaultAttribute())
            {
                description = descriptionAttribute.Description;
            }
            if (string.IsNullOrEmpty(description))
            {
                var displayAttribute = Attributes[typeof(DisplayAttribute)] as DisplayAttribute;
                if (displayAttribute != null)
                {
                    description = displayAttribute.GetDescription();
                    if (Localizable && displayAttribute.ResourceType != null && !string.IsNullOrEmpty(description))
                    {
                        return description;
                    }
                }
            }
            if (Localizable && !string.IsNullOrEmpty(description))
            {
                description = GetLocalizeString(description);
            }
            return description;
        }

        private string GetCategory()
        {
            string category = null;
            var descriptionAttribute = Attributes[typeof(CategoryAttribute)] as CategoryAttribute;
            if (descriptionAttribute != null && !descriptionAttribute.IsDefaultAttribute())
            {
                category = descriptionAttribute.Category;
            }
            if (string.IsNullOrEmpty(category))
            {
                var displayAttribute = Attributes[typeof(DisplayAttribute)] as DisplayAttribute;
                if (displayAttribute != null)
                {
                    category = displayAttribute.GetGroupName();
                    if (Localizable && displayAttribute.ResourceType != null && !string.IsNullOrEmpty(category))
                    {
                        return category;
                    }
                }
            }
            if (Localizable && !string.IsNullOrEmpty(category))
            {
                category = GetLocalizeString(category);
            }
            return category;
        }

        private string GetPrompt()
        {
            return ((DisplayAttribute)Attributes[typeof(DisplayAttribute)])?.GetPrompt();
        }

        #endregion
    }
}
