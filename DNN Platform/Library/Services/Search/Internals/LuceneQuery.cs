﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Services.Search.Entities;

using Lucene.Net.Search;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// Lucene Specific Query Object to be passed into Lucene for Search
    /// </summary>
    internal class LuceneQuery
    {
        /// <summary>
        /// Lucene's original Query Object
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// Lucene's original Sort Object. Default is by Relevance
        /// </summary>
        public Sort Sort { get; set; }

        /// <summary>
        /// Page Index for the result, e.g. pageIndex=1 and pageSize=10 indicates first 10 hits. Default value is 1
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size of the search result. Default value is 10.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Maximum length of highlighted title field in the results
        /// </summary>
        public int TitleSnippetLength { get; set; }

        /// <summary>
        /// Maximum length of highlighted title field in the results
        /// </summary>
        public int BodySnippetLength { get; set; }

        #region constructor

        public LuceneQuery()
        {
            PageIndex = 1;
            TitleSnippetLength = 60;
            BodySnippetLength = 100;
            PageSize = 10;
            Sort = Sort.RELEVANCE;
        }

        #endregion 
    }
}
