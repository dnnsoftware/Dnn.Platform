// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Analysis;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using Lucene.Net.Search.Vectorhighlight;
    using Lucene.Net.Store;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Impl Controller class for Lucene.
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class LuceneControllerImpl : ILuceneController, IDisposable
    {
        internal const int DefaultRereadTimeSpan = 30; // in seconds
        private const string DefaultSearchFolder = @"App_Data\Search";
        private const string WriteLockFile = "write.lock";
        private const int DefaultSearchRetryTimes = 5;
        private const int DISPOSED = 1;
        private const int UNDISPOSED = 0;
        private const string HighlightPreTag = "[b]";
        private const string HighlightPostTag = "[/b]";

        private const string HtmlPreTag = "<b>";
        private const string HtmlPostTag = "</b>";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LuceneControllerImpl));
        private readonly object _writerLock = new object();
        private readonly double _readerTimeSpan; // in seconds
        private readonly int _searchRetryTimes; // search retry times if exception thrown during search process
        private readonly List<CachedReader> _oldReaders = new List<CachedReader>();

        private IndexWriter _writer;
        private IndexReader _idxReader;
        private CachedReader _reader;
        private FastVectorHighlighter _fastHighlighter;
        private int _isDisposed = UNDISPOSED;

        private DateTime _lastReadTimeUtc;
        private DateTime _lastDirModifyTimeUtc;

        public LuceneControllerImpl()
        {
            var hostController = HostController.Instance;

            var folder = hostController.GetString(Constants.SearchIndexFolderKey, DefaultSearchFolder);
            if (string.IsNullOrEmpty(folder))
            {
                folder = DefaultSearchFolder;
            }

            this.IndexFolder = Path.Combine(Globals.ApplicationMapPath, folder);
            this._readerTimeSpan = hostController.GetDouble(Constants.SearchReaderRefreshTimeKey, DefaultRereadTimeSpan);
            this._searchRetryTimes = hostController.GetInteger(Constants.SearchRetryTimesKey, DefaultSearchRetryTimes);
        }

        internal string IndexFolder { get; private set; }

        private IndexWriter Writer
        {
            get
            {
                if (this._writer == null)
                {
                    lock (this._writerLock)
                    {
                        if (this._writer == null)
                        {
                            var lockFile = Path.Combine(this.IndexFolder, WriteLockFile);
                            if (File.Exists(lockFile))
                            {
                                try
                                {
                                    // if we successd in deleting the file, move on and create a new writer; otherwise,
                                    // the writer is locked by another instance (e.g., another server in a webfarm).
                                    File.Delete(lockFile);
                                }
                                catch (IOException e)
                                {
#pragma warning disable 0618
                                    throw new SearchException(
                                        Localization.GetExceptionMessage("UnableToCreateLuceneWriter", "Unable to create Lucene writer (lock file is in use). Please recycle AppPool in IIS to release lock."),
                                        e, new SearchItemInfo());
#pragma warning restore 0618
                                }
                            }

                            this.CheckDisposed();
                            var writer = new IndexWriter(
                                FSDirectory.Open(this.IndexFolder),
                                this.GetCustomAnalyzer() ?? new SynonymAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
                            this._idxReader = writer.GetReader();
                            Thread.MemoryBarrier();
                            this._writer = writer;
                        }
                    }
                }

                return this._writer;
            }
        }

        private bool MustRereadIndex
        {
            get
            {
                return (DateTime.UtcNow - this._lastReadTimeUtc).TotalSeconds >= this._readerTimeSpan &&
                    System.IO.Directory.Exists(this.IndexFolder) &&
                    System.IO.Directory.GetLastWriteTimeUtc(this.IndexFolder) != this._lastDirModifyTimeUtc;
            }
        }

        private FastVectorHighlighter FastHighlighter
        {
            get
            {
                if (this._fastHighlighter == null)
                {
                    FragListBuilder fragListBuilder = new SimpleFragListBuilder();
                    FragmentsBuilder fragmentBuilder = new ScoreOrderFragmentsBuilder(
                        new[] { HighlightPreTag }, new[] { HighlightPostTag });
                    this._fastHighlighter = new FastVectorHighlighter(true, true, fragListBuilder, fragmentBuilder);
                }

                return this._fastHighlighter;
            }
        }

        public LuceneResults Search(LuceneSearchContext searchContext)
        {
            Requires.NotNull("LuceneQuery", searchContext.LuceneQuery);
            Requires.NotNull("LuceneQuery.Query", searchContext.LuceneQuery.Query);
            Requires.PropertyNotEqualTo("LuceneQuery", "PageSize", searchContext.LuceneQuery.PageSize, 0);
            Requires.PropertyNotEqualTo("LuceneQuery", "PageIndex", searchContext.LuceneQuery.PageIndex, 0);

            var luceneResults = new LuceneResults();

            // validate whether index folder is exist and contains index files, otherwise return null.
            if (!this.ValidateIndexFolder())
            {
                return luceneResults;
            }

            var highlighter = this.FastHighlighter;
            var fieldQuery = highlighter.GetFieldQuery(searchContext.LuceneQuery.Query);

            var maxResults = searchContext.LuceneQuery.PageIndex * searchContext.LuceneQuery.PageSize;
            var minResults = maxResults - searchContext.LuceneQuery.PageSize + 1;

            for (var i = 0; i < this._searchRetryTimes; i++)
            {
                try
                {
                    var searcher = this.GetSearcher();
                    var searchSecurityTrimmer = new SearchSecurityTrimmer(new SearchSecurityTrimmerContext
                    {
                        Searcher = searcher,
                        SecurityChecker = searchContext.SecurityCheckerDelegate,
                        LuceneQuery = searchContext.LuceneQuery,
                        SearchQuery = searchContext.SearchQuery,
                    });
                    searcher.Search(searchContext.LuceneQuery.Query, null, searchSecurityTrimmer);
                    luceneResults.TotalHits = searchSecurityTrimmer.TotalHits;

                    if (Logger.IsDebugEnabled)
                    {
                        var sb = GetSearcResultExplanation(searchContext.LuceneQuery, searchSecurityTrimmer.ScoreDocs, searcher);
                        Logger.Trace(sb);
                    }

                    // Page doesn't exist
                    if (luceneResults.TotalHits < minResults)
                    {
                        break;
                    }

                    luceneResults.Results = searchSecurityTrimmer.ScoreDocs.Select(match =>
                        new LuceneResult
                        {
                            Document = searcher.Doc(match.Doc),
                            Score = match.Score,
                            DisplayScore = this.GetDisplayScoreFromMatch(match.ToString()),
                            TitleSnippet = this.GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.TitleTag, searchContext.LuceneQuery.TitleSnippetLength),
                            BodySnippet = this.GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.BodyTag, searchContext.LuceneQuery.BodySnippetLength),
                            DescriptionSnippet = this.GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.DescriptionTag, searchContext.LuceneQuery.TitleSnippetLength),
                            TagSnippet = this.GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.Tag, searchContext.LuceneQuery.TitleSnippetLength),
                            AuthorSnippet = this.GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.AuthorNameTag, searchContext.LuceneQuery.TitleSnippetLength),
                            ContentSnippet = this.GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.ContentTag, searchContext.LuceneQuery.TitleSnippetLength),
                        }).ToList();
                    break;
                }
                catch (Exception ex) when (ex is IOException || ex is AlreadyClosedException)
                {
                    this.DisposeReaders();
                    this.DisposeWriter(false);

                    if (i == this._searchRetryTimes - 1)
                    {
                        throw;
                    }

                    Logger.Error(ex);
                    Logger.Error($"Search Index Folder Is Not Available: {ex.Message}, Retry {i + 1} time(s).");
                    Thread.Sleep(100);
                }
            }

            return luceneResults;
        }

        public void Add(Document doc)
        {
            Requires.NotNull("searchDocument", doc);
            if (doc.GetFields().Count > 0)
            {
                try
                {
                    this.Writer.AddDocument(doc);
                }
                catch (OutOfMemoryException)
                {
                    lock (this._writerLock)
                    {
                        // as suggested by Lucene's doc
                        this.DisposeWriter();
                        this.Writer.AddDocument(doc);
                    }
                }
            }
        }

        public void Delete(Query query)
        {
            Requires.NotNull("luceneQuery", query);
            this.Writer.DeleteDocuments(query);
        }

        public void Commit()
        {
            if (this._writer != null)
            {
                lock (this._writerLock)
                {
                    if (this._writer != null)
                    {
                        this.CheckDisposed();
                        this._writer.Commit();
                        this.RescheduleAccessTimes();
                    }
                }
            }
        }

        public bool OptimizeSearchIndex(bool doWait)
        {
            var writer = this._writer;
            if (writer != null && writer.HasDeletions())
            {
                if (doWait)
                {
                    Logger.Debug("Compacting Search Index - started");
                }

                this.CheckDisposed();

                // optimize down to "> 1 segments" for better performance than down to 1
                this._writer.Optimize(4, doWait);

                if (doWait)
                {
                    this.Commit();
                    Logger.Debug("Compacting Search Index - finished");
                }

                return true;
            }

            return false;
        }

        public bool HasDeletions()
        {
            this.CheckDisposed();
            var searcher = this.GetSearcher();
            return searcher.IndexReader.HasDeletions;
        }

        public int MaxDocsCount()
        {
            this.CheckDisposed();
            var searcher = this.GetSearcher();
            return searcher.IndexReader.MaxDoc;
        }

        public int SearchbleDocsCount()
        {
            this.CheckDisposed();
            var searcher = this.GetSearcher();
            return searcher.IndexReader.NumDocs();
        }

        // works on the computer (in a web-farm) that runs under the scheduler
        public SearchStatistics GetSearchStatistics()
        {
            this.CheckDisposed();
            var searcher = this.GetSearcher();

            var files = System.IO.Directory.GetFiles(this.IndexFolder, "*.*");
            var size = files.Select(name => new FileInfo(name)).Select(fInfo => fInfo.Length).Sum();

            return new SearchStatistics
            {
                // Hack: seems that NumDocs/MaxDoc are buggy and return incorrect/swapped values
                TotalActiveDocuments = searcher.IndexReader.NumDocs(),
                TotalDeletedDocuments = searcher.IndexReader.NumDeletedDocs,
                IndexLocation = this.IndexFolder,
                LastModifiedOn = System.IO.Directory.GetLastWriteTimeUtc(this.IndexFolder),
                IndexDbSize = size,
            };
        }

        public void Dispose()
        {
            var status = Interlocked.CompareExchange(ref this._isDisposed, DISPOSED, UNDISPOSED);
            if (status == UNDISPOSED)
            {
                this.DisposeWriter();
                this.DisposeReaders();
            }
        }

        public Analyzer GetCustomAnalyzer()
        {
            var analyzer = DataCache.GetCache<Analyzer>("Search_CustomAnalyzer");
            if (analyzer == null)
            {
                var customAnalyzerType = HostController.Instance.GetString("Search_CustomAnalyzer", string.Empty);

                if (!string.IsNullOrEmpty(customAnalyzerType))
                {
                    try
                    {
                        var analyzerType = Reflection.CreateType(customAnalyzerType);
                        analyzer = Reflection.CreateInstance(analyzerType) as Analyzer;
                        if (analyzer == null)
                        {
                            throw new ArgumentException(string.Format(
                                Localization.GetExceptionMessage("InvalidAnalyzerClass", "The class '{0}' cannot be created because it's invalid or is not an analyzer, will use default analyzer."),
                                customAnalyzerType));
                        }

                        DataCache.SetCache("Search_CustomAnalyzer", analyzer);
                        return analyzer;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }

            return analyzer;
        }

        // made internal to be used in unit tests only; otherwise could be made private
        internal IndexSearcher GetSearcher()
        {
            if (this._reader == null || this.MustRereadIndex)
            {
                this.CheckValidIndexFolder();
                this.UpdateLastAccessTimes();
                this.InstantiateReader();
            }

            return this._reader.GetSearcher();
        }

        private static StringBuilder GetSearcResultExplanation(LuceneQuery luceneQuery, IEnumerable<ScoreDoc> scoreDocs, IndexSearcher searcher)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Query: " + luceneQuery.Query.ToString());
            foreach (var match in scoreDocs)
            {
                var explanation = searcher.Explain(luceneQuery.Query, match.Doc);
                sb.AppendLine("-------------------");
                var doc = searcher.Doc(match.Doc);
                sb.AppendLine(doc.Get(Constants.TitleTag));
                sb.AppendLine(explanation.ToString());
            }

            return sb;
        }

        private void CheckDisposed()
        {
            if (Thread.VolatileRead(ref this._isDisposed) == DISPOSED)
            {
                throw new ObjectDisposedException(Localization.GetExceptionMessage("LuceneControlerIsDisposed", "LuceneController is disposed and cannot be used anymore"));
            }
        }

        private void InstantiateReader()
        {
            IndexSearcher searcher;
            if (this._idxReader != null)
            {
                // use the Reopen() method for better near-realtime when the _writer ins't null
                var newReader = this._idxReader.Reopen();
                if (this._idxReader != newReader)
                {
                    // _idxReader.Dispose(); -- will get disposed upon disposing the searcher
                    Interlocked.Exchange(ref this._idxReader, newReader);
                }

                searcher = new IndexSearcher(this._idxReader);
            }
            else
            {
                // Note: disposing the IndexSearcher instance obtained from the next
                // statement will not close the underlying reader on dispose.
                searcher = new IndexSearcher(FSDirectory.Open(this.IndexFolder));
            }

            var reader = new CachedReader(searcher);
            var cutoffTime = DateTime.Now - TimeSpan.FromSeconds(this._readerTimeSpan * 10);
            lock (((ICollection)this._oldReaders).SyncRoot)
            {
                this.CheckDisposed();
                this._oldReaders.RemoveAll(r => r.LastUsed <= cutoffTime);
                this._oldReaders.Add(reader);
                Interlocked.Exchange(ref this._reader, reader);
            }
        }

        private void UpdateLastAccessTimes()
        {
            this._lastReadTimeUtc = DateTime.UtcNow;
            if (System.IO.Directory.Exists(this.IndexFolder))
            {
                this._lastDirModifyTimeUtc = System.IO.Directory.GetLastWriteTimeUtc(this.IndexFolder);
            }
        }

        private void RescheduleAccessTimes()
        {
            // forces re-opening the reader within 30 seconds from now (used mainly by commit)
            var now = DateTime.UtcNow;
            if (this._readerTimeSpan > DefaultRereadTimeSpan && (now - this._lastReadTimeUtc).TotalSeconds > DefaultRereadTimeSpan)
            {
                this._lastReadTimeUtc = now - TimeSpan.FromSeconds(this._readerTimeSpan - DefaultRereadTimeSpan);
            }
        }

        private void CheckValidIndexFolder()
        {
            if (!this.ValidateIndexFolder())
            {
                throw new SearchIndexEmptyException(Localization.GetExceptionMessage("SearchIndexingDirectoryNoValid", "Search indexing directory is either empty or does not exist"));
            }
        }

        private bool ValidateIndexFolder()
        {
            return System.IO.Directory.Exists(this.IndexFolder) &&
                   System.IO.Directory.GetFiles(this.IndexFolder, "*.*").Length > 0;
        }

        private string GetHighlightedText(FastVectorHighlighter highlighter, FieldQuery fieldQuery, IndexSearcher searcher, ScoreDoc match, string tag, int length)
        {
            var s = highlighter.GetBestFragment(fieldQuery, searcher.IndexReader, match.Doc, tag, length);
            if (!string.IsNullOrEmpty(s))
            {
                s = HttpUtility.HtmlEncode(s).Replace(HighlightPreTag, HtmlPreTag).Replace(HighlightPostTag, HtmlPostTag);
            }

            return s;
        }

        /// <summary>
        /// Extract the Score portion of the match.ToString().
        /// </summary>
        private string GetDisplayScoreFromMatch(string match)
        {
            var displayScore = string.Empty;
            if (!string.IsNullOrEmpty(match))
            {
                var beginPos = match.IndexOf('[');
                var endPos = match.LastIndexOf(']');
                if (beginPos > 0 && endPos > 0 && endPos > beginPos)
                {
                    displayScore = match.Substring(beginPos + 1, endPos - beginPos - 1);
                }
            }

            return displayScore;
        }

        private void DisposeWriter(bool commit = true)
        {
            if (this._writer != null)
            {
                lock (this._writerLock)
                {
                    if (this._writer != null)
                    {
                        this._idxReader.Dispose();
                        this._idxReader = null;

                        if (commit)
                        {
                            this._writer.Commit();
                        }

                        this._writer.Dispose();
                        this._writer = null;
                    }
                }
            }
        }

        private void DisposeReaders()
        {
            lock (((ICollection)this._oldReaders).SyncRoot)
            {
                foreach (var rdr in this._oldReaders)
                {
                    rdr.Dispose();
                }

                this._oldReaders.Clear();
                this._reader = null;
            }
        }

        private class CachedReader : IDisposable
        {
            private readonly IndexSearcher _searcher;

            public CachedReader(IndexSearcher searcher)
            {
                this._searcher = searcher;
                this.UpdateLastUsed();
            }

            public DateTime LastUsed { get; private set; }

            public IndexSearcher GetSearcher()
            {
                this.UpdateLastUsed();
                return this._searcher;
            }

            public void Dispose()
            {
                this._searcher.Dispose();
                this._searcher.IndexReader.Dispose();
            }

            private void UpdateLastUsed()
            {
                this.LastUsed = DateTime.Now;
            }
        }
    }
}
