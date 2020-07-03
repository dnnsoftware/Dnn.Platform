// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Entities
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// Collection storing Search Results.
    /// </summary>
    [Serializable]
    public class SearchResults
    {
        public SearchResults()
        {
            this.Results = new List<SearchResult>();
        }

        /// <summary>
        /// Gets or sets total Hits found in Lucene.
        /// </summary>
        /// <remarks>This number will generally be larger than count of Results object as Results usually holds 10 items, whereas TotalHits indicates TOTAL hits in entire Lucene for the query supplied.</remarks>
        public int TotalHits { get; set; }

        /// <summary>
        /// Gets or sets collection of Results.
        /// </summary>
        public IList<SearchResult> Results { get; set; }
    }
}
