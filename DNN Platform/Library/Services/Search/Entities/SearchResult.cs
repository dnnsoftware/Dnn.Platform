#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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