namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// Sorting criteria to be used for Querying
    /// </summary>
    public enum SortFields
    {
        /// <summary>
        /// Sort by Relevance [default]. Most relevant come first, SortDirection is ignored.
        /// </summary>
        Relevance = 0,

        /// <summary>
        /// Sort by DateTime Modified. Latest come first
        /// </summary>
        LastModified = 1,
        
        /// <summary>
        /// Sort by Title. 
        /// </summary>
        Title = 2,

        /// <summary>
        /// Specify custom numeric field for sorting. Field name should be specified in SearchQuery.CustomSortField
        /// </summary>
        CustomNumericField = 3,

        /// <summary>
        /// Specify custom string filed for sorting. Field name should be specified in SearchQuery.CustomSortField
        /// </summary>
        CustomStringField = 4
    }
}
