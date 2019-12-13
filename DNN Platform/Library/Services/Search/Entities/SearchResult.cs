// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// Search Result to be displayed to end user
    ///
    /// Inherited "Body" property from SearchDocument may be purposefully left empty for performance purposes.
    /// </summary>
    [Serializable]
    public class SearchResult : SearchDocument
    {
        /// <summary>
        /// Time when Content was last modified (in friendly format)
        /// </summary>
        public string DisplayModifiedTime { get { return DateUtils.CalculateDateForDisplay(ModifiedTimeUtc); } }

        /// <summary>
        /// Highlighted snippet from document
        /// </summary>
        public string Snippet { get; set; }

        /// <summary>
        /// Optional: Display Name of the Author
        /// </summary>
        /// <remarks>This may be different form current Display Name when Index was run prior to change in Display Name.</remarks>
        public string AuthorName { get; set; }

        /// <summary>
        /// Lucene's original Score. The score of this document for the query.
        /// </summary>
        /// <remarks>This field may not be reliable as most of the time it contains Nan. Use DisplayScore instead</remarks>
        public float Score { get; set; }

        /// <summary>
        /// Lucene's original Score in String format, e.g. 1.45678 or 0.87642. The score of this document for the query.
        /// </summary>
        /// <remarks>This field is more reliable than the float version of Score.</remarks>
        public string DisplayScore { get; set; }

        /// <summary>
        /// Context information such as the type of module that initiated the search can be stored here.
        /// <remarks>This is key-value pair, e.g. "SearchSource","SiteSearch"</remarks>
        /// </summary>
        public IDictionary<string, string> SearchContext { get; set; }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public SearchResult()
        {
            Tags = new string[0];
            NumericKeys = new Dictionary<string, int>();
            Keywords = new Dictionary<string, string>();
            SearchContext = new Dictionary<string, string>();
        }
    }
}
