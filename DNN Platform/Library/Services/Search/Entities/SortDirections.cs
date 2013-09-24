namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// Sorting direction to be used for Querying
    /// </summary>
    /// <remarks>Does not apply when SortFields.Relevance is specified in SearchQuery.SortField</remarks>
    public enum SortDirections
    {
        /// <summary>
        /// Sort by descending [default] order
        /// </summary>
        Descending = 0,

        /// <summary>
        /// Sort by ascending order
        /// </summary>
        Ascending = 1
    }
}
