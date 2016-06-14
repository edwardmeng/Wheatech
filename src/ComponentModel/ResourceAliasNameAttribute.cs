using System;
using System.Resources;
using static System.String;

namespace Wheatech.ComponentModel
{
    /// <summary>
    /// A customized version of <see cref="AliasNameAttribute"/> that can
    /// load the string from assembly resources instead of just a hard-wired
    /// string.
    /// </summary>
    public class ResourceAliasNameAttribute : AliasNameAttribute
    {
        private bool _resourceLoaded;

        /// <summary>
        /// Create a new instance of <see cref="ResourceAliasNameAttribute"/> where
        /// the type and name of the resource is set via properties.
        /// </summary>
        public ResourceAliasNameAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAliasNameAttribute"/> class.
        /// </summary>
        /// <param name="resourceType">Type used to locate the assembly containing the resources.</param>
        /// <param name="resourceName">Name of the entry in the resource table.</param>
        public ResourceAliasNameAttribute(Type resourceType, string resourceName)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
        }

        /// <summary>
        /// A type contained in the assembly we want to get our display name from.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Name of the string resource containing our display name.
        /// </summary>
        public string ResourceName { get; set; }


        /// <summary>
        /// Gets the alias name for a field in this attribute.
        /// </summary>
        /// <returns>
        /// The alias name.
        /// </returns>
        public override string AliasName
        {
            get
            {
                EnsureAliasNameLoaded();
                return AliasNameValue;
            }
        }

        private void EnsureAliasNameLoaded()
        {
            if (_resourceLoaded) return;

            var rm = new ResourceManager(ResourceType);
            try
            {
                AliasNameValue = rm.GetString(ResourceName);
            }
            catch (MissingManifestResourceException)
            {
                AliasNameValue = ResourceName;
            }
            if (IsNullOrEmpty(AliasNameValue)) AliasNameValue = ResourceName;
            _resourceLoaded = true;
        }
    }
}
