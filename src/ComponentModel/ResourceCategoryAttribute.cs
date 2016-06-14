using System;
using System.ComponentModel;
using System.Resources;

namespace Wheatech.ComponentModel
{
    /// <summary>
    /// Represents a localized <see cref="CategoryAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ResourceCategoryAttribute : CategoryAttribute
    {
        private readonly Type _resourceType;
        private ResourceManager _resourceManager;

        static ResourceCategoryAttribute()
        {
            General = new ResourceCategoryAttribute(typeof(ResourceCategoryAttribute), "CategoryGeneral");
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="SRCategoryAttribute"/> class with the <see cref="Type"/> containing the resources and the resource name.
        /// </summary>
        /// <param name="category">The resources string name.</param>
        /// <param name="resourceType">The <see cref="Type"/> containing the resource strings.</param>
        public ResourceCategoryAttribute(Type resourceType, string category)
            : base(category)
        {
            _resourceType = resourceType;
        }

        /// <summary>
        /// Gets the type that contains the resources.
        /// </summary>
        /// <value>
        /// The type that contains the resources.
        /// </value>
        public Type ResourceType => _resourceType;

        /// <summary>
        /// Gets the localized string based on the key.
        /// </summary>
        /// <param name="value">The key to the string resources.</param>
        /// <returns>The localized string.</returns>
        protected override string GetLocalizedString(string value)
        {
            if (_resourceManager == null)
            {
                _resourceManager = new ResourceManager(_resourceType);
            }
            try
            {
                return _resourceManager.GetString(value);
            }
            catch (MissingManifestResourceException)
            {
                return value;
            }
        }

        /// <summary>
        /// Returns a localized <see cref="ResourceCategoryAttribute"/> for the General category.
        /// </summary>
        public static ResourceCategoryAttribute General { get; }
    }
}
