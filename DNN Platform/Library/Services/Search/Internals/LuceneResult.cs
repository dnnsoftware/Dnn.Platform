﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
