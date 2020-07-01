// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Entities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Criteria to search for.
    /// </summary>
    /// <remarks>This object should be passed to SearchController to search for Content. KeyWords and PortalId must be specified.</remarks>
    [Serializable]
    public class SearchQuery
    {
        public SearchQuery()
        {
            this.Tags = new string[0];
            this.PortalIds = new int[0];
            this.SearchTypeIds = new int[0];
            this.ModuleDefIds = new int[0];
            this.TitleSnippetLength = 60;
            this.BodySnippetLength = 100;
            this.PageSize = 10;
            this.PageIndex = 1;
            this.SearchContext = new Dictionary<string, string>();
            this.CustomKeywords = new Dictionary<string, string>();
            this.NumericKeys = new Dictionary<string, int>();
        }

        /// <summary>
        /// Gets or sets a key to uniquely identify a document in the Index.
        /// </summary>
        public string UniqueKey { get; set; }

        /// <summary>
        /// Gets or sets keywords to search for.
        /// </summary>
        public string KeyWords { get; set; }

        /// <summary>
        /// Gets or sets a collection of Portal Ids of the Site to perform Search upon. This field must be specified or else Portal 0 will be searched by default.
        /// </summary>
        /// <remarks>Search cannot be executed across Sites.</remarks>
        public IEnumerable<int> PortalIds { get; set; }

        /// <summary>
        /// Gets or sets a collection of Search Type Ids that should be searched upon [Optional].
        /// </summary>
        public IEnumerable<int> SearchTypeIds { get; set; }

        /// <summary>
        /// Gets or sets a collection of Module Def Ids that should be searched upon [Optional]. Match is performed only when a SearchTypeId for Module Search Crawler Id.
        /// </summary>
        public IEnumerable<int> ModuleDefIds { get; set; }

        /// <summary>
        /// Gets or sets module Id to restrict Search. Value > 0 is used only.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Gets or sets role Id to restrict Search. Value > 0 is used only.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Gets or sets tab Id to restrict Search. Value > 0 is used only.
        /// </summary>
        public int TabId { get; set; }

        /// <summary>
        /// Gets or sets locale to restrict Search to. This field can be left empty for single language site.
        /// </summary>
        /// <remarks>E.g. A value en-US or nl-NL can specified to restrict search to a single locale .</remarks>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets begin Date of the time when Content was last modified (in Utc). This field is optional.
        /// </summary>
        public DateTime BeginModifiedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets end Date of the time when Content was last modified (in Utc). This field is optional.
        /// </summary>
        public DateTime EndModifiedTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets restrict search to specific tags. This field is optional.
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets page Index for the result, e.g. pageIndex=1 and pageSize=10 indicates first 10 hits. Default value is 1.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets page size of the search result. Default value is 10.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets maximum length of highlighted title field in the results.
        /// </summary>
        public int TitleSnippetLength { get; set; }

        /// <summary>
        /// Gets or sets maximum length of highlighted title field in the results.
        /// </summary>
        public int BodySnippetLength { get; set; }

        /// <summary>
        /// Gets or sets culture Code associated with the content.
        /// </summary>
        /// <remarks>Culture-Neutral content is always returned even though this value is specfied.</remarks>
        public string CultureCode { get; set; }

        /// <summary>
        /// Gets or sets sort option of the search result. This field is optional.
        /// </summary>
        public SortFields SortField { get; set; }

        /// <summary>
        /// Gets or sets sort option of the search result. This field is optional.
        /// </summary>
        public SortDirections SortDirection { get; set; }

        /// <summary>
        /// Gets or sets name of the custom sort field, works with SortFields.CustomNumericField or SortFields.CustomStringField option.
        /// </summary>
        /// <remarks> Enum SortFields can be used to sort on Relevance, LastModified and Title. Additional fields such as one provided under
        /// SearchDocument.Keywords, SearchDocument.NumericKeys or Tags can be specified.
        /// can be specified by using CustomSortField property. One needs to ensure that the field name is a valid one.
        /// </remarks>
        /// <example>authorid or authorname.</example>
        public string CustomSortField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether set this to true to perform perform WildCard Search.
        /// </summary>
        /// <remarks>This property is not respected when Keywords contain special boolean phrases "~", "*", "\"", "\'", "and", "or", "+", "-".
        /// When this is enabled, an additional OR is performed, e.g. (keyword OR keyword*). It adds asterisk at then end to find any words starting with the keyword.
        /// There can be performance implications with this setting turned on.</remarks>
        public bool WildCardSearch { get; set; }

        /// <summary>
        /// Gets or sets context information such as the type of module that initiated the search can be stored here.
        /// <remarks>This is key-value pair, e.g. "SearchSource","SiteSearch"</remarks>
        /// </summary>
        public IDictionary<string, string> SearchContext { get; set; }

        /// <summary>
        /// Gets or sets restrict search to specific Keywords. This field is optional. Lookup is done in the SearchDocument.Keywords collection.
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "AliasName","something".</remarks>
        /// <remarks>An empty dictionary means this is property is not used.</remarks>
        public IDictionary<string, string> CustomKeywords { get; set; }

        /// <summary>
        /// Gets or sets restrict search to specific NumericKeys. This field is optional.
        /// </summary>
        /// <remarks>This is key-value pair, e.g. "ItemId","888".</remarks>
        /// <remarks>An empty dictionary means this is property is not used.</remarks>
        public IDictionary<string, int> NumericKeys { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether set this to true to allow search in word.
        /// </summary>
        /// <remarks>When host setting "Search_AllowLeadingWildcard" set to true, it will always allow search in word but ignore this value.</remarks>
        public bool AllowLeadingWildcard { get; set; }
    }
}
