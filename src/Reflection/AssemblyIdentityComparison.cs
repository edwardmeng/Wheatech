namespace Wheatech
{
    /// <summary>
    /// Assembly identity comparison modes.
    /// </summary>
    public enum AssemblyIdentityComparison
    {
        /// <summary>
        /// Uses only the assembly short name
        /// </summary>
        ShortName,

        /// <summary>
        /// Uses only the assembly short name and version.
        /// </summary>
        Version,

        /// <summary>
        /// Uses only the assembly short name, version and culture.
        /// </summary>
        Culture,

        /// <summary>
        /// Uses only the assembly short name, version, culture and public key token.
        /// </summary>
        PublicKeyToken,

        /// <summary>
        /// Uses only the assembly short name, version, culture, public key token and processor architecture.
        /// </summary>
        Architecture,

        /// <summary>
        /// Compares all informations.
        /// </summary>
        Default
    }
}
