namespace Wheatech
{
    /// <summary>
    /// Defines the contract that a class must implement in order to check whether a version is valid for a comparator.
    /// </summary>
    public interface IVersionComparator
    {
        /// <summary>
        /// Determines whether the version is a valid value for this comparator.
        /// </summary>
        /// <param name="version">The specified version to match.</param>
        /// <returns><c>true</c> if the version is a valid value; otherwise, <c>false</c>.</returns>
        bool Match(Version version);
    }
}
