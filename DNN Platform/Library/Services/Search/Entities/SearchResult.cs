// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Entities
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Search.Internals;

    /// <summary>
    /// Search Result to be displayed to end user
    ///
    /// Inherited "Body" property from SearchDocument may be purposefully left empty for performance purposes.
    /// </summary>
    [Serializable]
    public class SearchResult : SearchDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult"/> class.
        /// Empty Constructor.
        /// </summary>
        public SearchResult()
        {
            this.Tags = new string[0];
            this.NumericKeys = new Dictionary<string, int>();
            this.Keywords = new Dictionary<string, string>();
            this.SearchContext = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets time when Content was last modified (in friendly format).
        /// </summary>
        public string DisplayModifiedTime
        {
            get { return DateUtils.CalculateDateForDisplay(this.ModifiedTimeUtc); }
        }

        /// <summary>
        /// Gets or sets highlighted snippet from document.
        /// </summary>
        public string Snippet { get; set; }

        /// <summary>
        /// Gets or sets optional: Display Name of the Author.
        /// </summary>
        /// <remarks>This may be different form current Display Name when Index was run prior to change in Display Name.</remarks>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets lucene's original Score. The score of this document for the query.
        /// </summary>
        /// <remarks>This field may not be reliable as most of the time it contains Nan. Use DisplayScore instead.</remarks>
        public float Score { get; set; }

        /// <summary>
        /// Gets or sets lucene's original Score in String format, e.g. 1.45678 or 0.87642. The score of this document for the query.
        /// </summary>
        /// <remarks>This field is more reliable than the float version of Score.</remarks>
        public string DisplayScore { get; set; }

        /// <summary>
        /// Gets or sets context information such as the type of module that initiated the search can be stored here.
        /// <remarks>This is key-value pair, e.g. "SearchSource","SiteSearch"</remarks>
        /// </summary>
        public IDictionary<string, string> SearchContext { get; set; }
    }
}
