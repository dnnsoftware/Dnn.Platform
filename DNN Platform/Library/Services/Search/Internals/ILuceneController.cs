// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Analysis;
    using Lucene.Net.Documents;
    using Lucene.Net.Search;

    internal interface ILuceneController
    {
        /// <summary>
        /// Execute Search.
        /// </summary>
        /// <param name="luceneSearchContext">Search Context.</param>
        /// <returns>List of matching Documents.</returns>
        LuceneResults Search(LuceneSearchContext luceneSearchContext);

        /// <summary>
        /// Adds Lucene Document in Lucene Index.
        /// </summary>
        void Add(Document doc);

        /// <summary>
        /// Delete a Search Document from the Search Index.
        /// </summary>
        void Delete(Query query);

        /// <summary>
        /// Commits the added search documents into the search database.
        /// </summary>
        void Commit();

        /// <summary>
        /// Optimize the search index files by compacting and removing previously deleted search documents.
        /// </summary>
        /// <remarks>
        /// This is a costly operation which consumes substantial CPU and I/O resources, therefore use it
        /// judiciously. If your site has a a single server that performs both indexing and searching, then
        /// you should consider running the optimize operation after hours or over the weekend so that it
        /// does not interfere with ongoing search activities.
        /// <para>This means you should expect the size of your index to roughly triple (temporarily)
        /// during optimization. Once optimization completes, and once you call commit(), disk usage
        /// will fall back to a lower level than the starting size.</para>
        /// </remarks>
        /// <param name="doWait">Whether to run optimization on background thread or wait for optimization to finish.</param>
        /// <returns>True is optimization was scheduled to run in the background or ran to completion in foreground, false otherwise (due to
        /// that there were no deletions or the writer was not created yet).</returns>
        bool OptimizeSearchIndex(bool doWait);

        /// <summary>
        /// Returns number of total documents in the search index (including deleted ones).
        /// </summary>
        /// <returns>Number of total documents in the search index (including Deletions).</returns>
        int MaxDocsCount();

        /// <summary>
        /// Returns number of total searchable documents in the search index.
        /// </summary>
        /// <returns>Number of total searchable documents in the search index.</returns>
        int SearchbleDocsCount();

        /// <summary>
        /// Returns if the current search index has deletions.
        /// </summary>
        /// <returns>Whther the search index has deletions or not.</returns>
        bool HasDeletions();

        /// <summary>
        /// Returns current search indexs general information.
        /// </summary>
        /// <returns><see cref="SearchStatistics"/> object or null if the information can not be retrieved.</returns>
        SearchStatistics GetSearchStatistics();

        Analyzer GetCustomAnalyzer();
    }
}
