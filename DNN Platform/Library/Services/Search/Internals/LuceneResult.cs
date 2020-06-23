// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Documents;
    using Lucene.Net.Search;

    /// <summary>
    /// Lucene Specific Query Result.
    /// </summary>
    internal class LuceneResult
    {
        /// <summary>
        /// Gets or sets lucene's original Document Object.
        /// </summary>
        public Document Document { get; set; }

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
        /// Gets or sets highlighted Title Snippet. This may be empty for synonym based search.
        /// </summary>
        public string TitleSnippet { get; set; }

        /// <summary>
        /// Gets or sets highlighted Body Snippet. This may be empty for synonym based search.
        /// </summary>
        public string BodySnippet { get; set; }

        /// <summary>
        /// Gets or sets highlighted Description Snippet. This may be empty for synonym based search.
        /// </summary>
        public string DescriptionSnippet { get; set; }

        /// <summary>
        /// Gets or sets highlighted Tag Snippet. This may be empty for synonym based search.
        /// </summary>
        public string TagSnippet { get; set; }

        /// <summary>
        /// Gets or sets highlighted Author Snippet. This may be empty for synonym based search.
        /// </summary>
        public string AuthorSnippet { get; set; }

        /// <summary>
        /// Gets or sets highlighted Content Snippet. This may be empty for synonym based search.
        /// </summary>
        public string ContentSnippet { get; set; }
    }
}
