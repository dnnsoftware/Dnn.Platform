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
        /// Sort by Tag.
        /// </summary>
        Tag = 3,

        /// <summary>
        /// Sort by NumericKey (as specified in SearchDocument while indexing). The NumericKeys key-name should be specified in SearchQuery.CustomSortField
        /// </summary>
        NumericKey = 4,

        /// <summary>
        /// Sort by Keywords (as specified in SearchDocument while indexing). The Keywords key-name should be specified in SearchQuery.CustomSortField
        /// </summary>
        Keyword = 5,

        /// <summary>
        /// Specify custom numeric field for sorting. Field name should be specified in SearchQuery.CustomSortField
        /// </summary>
        /// <remarks>This option should be used when you can't any of the previous options, e.g. AuthorUserId (authorid) or TabId (tab)</remarks>
        CustomNumericField = 6,

        /// <summary>
        /// Specify custom string filed for sorting. Field name should be specified in SearchQuery.CustomSortField
        /// </summary>
        /// <remarks>This option should be used when you can't any of the previous options, e.g. authorname or UniqueKey (key)</remarks>
        CustomStringField = 7
    }
}
