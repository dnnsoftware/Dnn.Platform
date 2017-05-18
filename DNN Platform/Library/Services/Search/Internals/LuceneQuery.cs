#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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