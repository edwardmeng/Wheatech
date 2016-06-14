namespace Wheatech
{
    internal enum VersionFloatBehavior
    {
        /// <summary>
        /// Lowest version, no float
        /// </summary>
        None,

        /// <summary>
        /// Highest matching pre-release label
        /// </summary>
        Prerelease,

        /// <summary>
        /// x.y.z.*
        /// </summary>
        Revision,

        /// <summary>
        /// x.y.*
        /// </summary>
        Patch,

        /// <summary>
        /// x.*
        /// </summary>
        Minor,

        /// <summary>
        /// *
        /// </summary>
        Major,
    }
}
