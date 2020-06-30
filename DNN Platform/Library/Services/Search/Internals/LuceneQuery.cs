// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Search;

    /// <summary>
    /// Lucene Specific Query Object to be passed into Lucene for Search.
    /// </summary>
    internal class LuceneQuery
    {
        public LuceneQuery()
        {
            this.PageIndex = 1;
            this.TitleSnippetLength = 60;
            this.BodySnippetLength = 100;
            this.PageSize = 10;
            this.Sort = Sort.RELEVANCE;
        }

        /// <summary>
        /// Gets or sets lucene's original Query Object.
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// Gets or sets lucene's original Sort Object. Default is by Relevance.
        /// </summary>
        public Sort Sort { get; set; }

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
    }
}
