namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// Sorting criteria to be used for Querying
    /// </summary>
    public enum SortFields
    {
        /// <summary>
        /// Sort by Relevance [default]. Most relevant come first.
        /// </summary>
        Relevance = 0,

        /// <summary>
        /// Sort by DateTime Modified. Latest come first
        /// </summary>
        LastModified = 1
    }
}
