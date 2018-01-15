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

using DotNetNuke.Services.Search.Entities;

using Lucene.Net.Documents;
using Lucene.Net.Search;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// Lucene Specific Query Result
    /// </summary>
    internal class LuceneResult
    {
        /// <summary>
        /// Lucene's original Document Object
        /// </summary>
        public Document Document { get; set; }

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
        /// Highlighted Title Snippet. This may be empty for synonym based search
        /// </summary>
        public string TitleSnippet { get; set; }

        /// <summary>
        /// Highlighted Body Snippet. This may be empty for synonym based search
        /// </summary>
        public string BodySnippet { get; set; }

        /// <summary>
        /// Highlighted Description Snippet. This may be empty for synonym based search
        /// </summary>
        public string DescriptionSnippet { get; set; }

        /// <summary>
        /// Highlighted Tag Snippet. This may be empty for synonym based search
        /// </summary>
        public string TagSnippet { get; set; }

        /// <summary>
        /// Highlighted Author Snippet. This may be empty for synonym based search
        /// </summary>
        public string AuthorSnippet { get; set; }

        /// <summary>
        /// Highlighted Content Snippet. This may be empty for synonym based search
        /// </summary>
        public string ContentSnippet { get; set; }
    }
}