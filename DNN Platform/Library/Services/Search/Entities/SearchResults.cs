// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// Collection storing Search Results
    /// </summary>
    [Serializable]
    public class SearchResults
    {
        /// <summary>
        /// Total Hits found in Lucene
        /// </summary>
        /// <remarks>This number will generally be larger than count of Results object as Results usually holds 10 items, whereas TotalHits indicates TOTAL hits in entire Lucene for the query supplied.</remarks>
        public int TotalHits { get; set; }

        /// <summary>
        /// Collection of Results
        /// </summary>
        public IList<SearchResult> Results { get; set; }
        
        public SearchResults()
        {
            Results = new List<SearchResult>();
        }
    }
}
