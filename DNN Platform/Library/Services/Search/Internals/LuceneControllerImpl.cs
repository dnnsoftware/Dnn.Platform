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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Search.Entities;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Vectorhighlight;
using Lucene.Net.Store;
using DotNetNuke.Instrumentation;
using System.Web;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Impl Controller class for Lucene
    /// </summary>
    /// -----------------------------------------------------------------------------
    internal class LuceneControllerImpl : ILuceneController, IDisposable
    {
        #region Constants
        private const string DefaultSearchFolder = @"App_Data\Search";
        private const string WriteLockFile = "write.lock";
        internal const int DefaultRereadTimeSpan = 30; // in seconds
        private const int DefaultSearchRetryTimes = 5;
        private const int DISPOSED = 1;
        private const int UNDISPOSED = 0;
        #endregion

        #region Private Properties

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LuceneControllerImpl));

        internal string IndexFolder { get; private set; }

        private IndexWriter _writer;
        private IndexReader _idxReader;
        private CachedReader _reader;
        private FastVectorHighlighter _fastHighlighter;
        private readonly object _writerLock = new object();
        private readonly double _readerTimeSpan; // in seconds
        private readonly int _searchRetryTimes; //search retry times if exception thrown during search process
        private readonly List<CachedReader> _oldReaders = new List<CachedReader>();
        private int _isDisposed = UNDISPOSED;

        private const string HighlightPreTag = "[b]";
        private const string HighlightPostTag = "[/b]";

        private const string HtmlPreTag = "<b>";
        private const string HtmlPostTag = "</b>";

        #region constructor
        public LuceneControllerImpl()
        {
            var hostController = HostController.Instance;

            var folder = hostController.GetString(Constants.SearchIndexFolderKey, DefaultSearchFolder);
            if (string.IsNullOrEmpty(folder)) folder = DefaultSearchFolder;
            IndexFolder = Path.Combine(Globals.ApplicationMapPath, folder);
            _readerTimeSpan = hostController.GetDouble(Constants.SearchReaderRefreshTimeKey, DefaultRereadTimeSpan);
            _searchRetryTimes = hostController.GetInteger(Constants.SearchRetryTimesKey, DefaultSearchRetryTimes);
        }

        private void CheckDisposed()
        {
            if (Thread.VolatileRead(ref _isDisposed) == DISPOSED)
                throw new ObjectDisposedException(Localization.Localization.GetExceptionMessage("LuceneControlerIsDisposed","LuceneController is disposed and cannot be used anymore"));
        }
        #endregion

        private IndexWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    lock (_writerLock)
                    {
                        if (_writer == null)
                        {
                            var lockFile = Path.Combine(IndexFolder, WriteLockFile);
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
                                        Localization.Localization.GetExceptionMessage("UnableToCreateLuceneWriter","Unable to create Lucene writer (lock file is in use). Please recycle AppPool in IIS to release lock."),
                                        e);
#pragma warning restore 0618
                                }
                            }

                            CheckDisposed();
                            var writer = new IndexWriter(FSDirectory.Open(IndexFolder),
                                GetCustomAnalyzer() ?? new SynonymAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
                            _idxReader = writer.GetReader();
                            Thread.MemoryBarrier();
                            _writer = writer;
                        }
                    }
                }
                return _writer;
            }
        }

        // made internal to be used in unit tests only; otherwise could be made private
        internal IndexSearcher GetSearcher()
        {
            if (_reader == null || MustRereadIndex)
            {
                CheckValidIndexFolder();
                UpdateLastAccessTimes();
                InstantiateReader();
            }

            return _reader.GetSearcher();
        }

        private void InstantiateReader()
        {
            IndexSearcher searcher;
            if (_idxReader != null)
            {
                //use the Reopen() method for better near-realtime when the _writer ins't null
                var newReader = _idxReader.Reopen();
                if (_idxReader != newReader)
                {
                    //_idxReader.Dispose(); -- will get disposed upon disposing the searcher
                    Interlocked.Exchange(ref _idxReader, newReader);
                }

                searcher = new IndexSearcher(_idxReader);
            }
            else
            {
                // Note: disposing the IndexSearcher instance obtained from the next
                // statement will not close the underlying reader on dispose.
                searcher = new IndexSearcher(FSDirectory.Open(IndexFolder));
            }

            var reader = new CachedReader(searcher);
            var cutoffTime = DateTime.Now - TimeSpan.FromSeconds(_readerTimeSpan*10);
            lock (((ICollection) _oldReaders).SyncRoot)
            {
                CheckDisposed();
                _oldReaders.RemoveAll(r => r.LastUsed <= cutoffTime);
                _oldReaders.Add(reader);
                Interlocked.Exchange(ref _reader, reader);
            }
        }

        private DateTime _lastReadTimeUtc;
        private DateTime _lastDirModifyTimeUtc;

        private bool MustRereadIndex
        {
            get
            {
                return (DateTime.UtcNow - _lastReadTimeUtc).TotalSeconds >= _readerTimeSpan && 
                    System.IO.Directory.Exists(IndexFolder) && 
                    System.IO.Directory.GetLastWriteTimeUtc(IndexFolder) != _lastDirModifyTimeUtc;
            }
        }

        private void UpdateLastAccessTimes()
        {
            _lastReadTimeUtc = DateTime.UtcNow;
            if (System.IO.Directory.Exists(IndexFolder))
            {
                _lastDirModifyTimeUtc = System.IO.Directory.GetLastWriteTimeUtc(IndexFolder);
            }
        }

        private void RescheduleAccessTimes()
        {
            // forces re-opening the reader within 30 seconds from now (used mainly by commit)
            var now = DateTime.UtcNow;
            if (_readerTimeSpan > DefaultRereadTimeSpan && (now - _lastReadTimeUtc).TotalSeconds > DefaultRereadTimeSpan)
            {
                _lastReadTimeUtc = now - TimeSpan.FromSeconds(_readerTimeSpan - DefaultRereadTimeSpan);
            }
        }

        private void CheckValidIndexFolder()
        {
            if (!ValidateIndexFolder())
            {
                throw new SearchIndexEmptyException(Localization.Localization.GetExceptionMessage("SearchIndexingDirectoryNoValid","Search indexing directory is either empty or does not exist"));
            }
        }

        private bool ValidateIndexFolder()
        {
            return System.IO.Directory.Exists(IndexFolder) &&
                   System.IO.Directory.GetFiles(IndexFolder, "*.*").Length > 0;
        }

        private FastVectorHighlighter FastHighlighter
        {
            get
            {
                if (_fastHighlighter == null)
                {
                    FragListBuilder fragListBuilder = new SimpleFragListBuilder();
                    FragmentsBuilder fragmentBuilder = new ScoreOrderFragmentsBuilder(
                        new[] { HighlightPreTag }, new[] { HighlightPostTag });
                    _fastHighlighter = new FastVectorHighlighter(true, true, fragListBuilder, fragmentBuilder);
                }
                return _fastHighlighter;
            }
        }

        #endregion

        public LuceneResults Search(LuceneSearchContext searchContext)
        {
            Requires.NotNull("LuceneQuery", searchContext.LuceneQuery);
            Requires.NotNull("LuceneQuery.Query", searchContext.LuceneQuery.Query);
            Requires.PropertyNotEqualTo("LuceneQuery", "PageSize", searchContext.LuceneQuery.PageSize, 0);
            Requires.PropertyNotEqualTo("LuceneQuery", "PageIndex", searchContext.LuceneQuery.PageIndex, 0);

            var luceneResults = new LuceneResults();

            //validate whether index folder is exist and contains index files, otherwise return null.
            if (!ValidateIndexFolder())
            {
                return luceneResults;
            }
            
            var highlighter = FastHighlighter;
            var fieldQuery = highlighter.GetFieldQuery(searchContext.LuceneQuery.Query);

            var maxResults = searchContext.LuceneQuery.PageIndex * searchContext.LuceneQuery.PageSize;
            var minResults = maxResults - searchContext.LuceneQuery.PageSize + 1;

            for (var i = 0; i < _searchRetryTimes; i++)
            {
                try
                {
                    var searcher = GetSearcher();
                    var searchSecurityTrimmer = new SearchSecurityTrimmer(new SearchSecurityTrimmerContext
                    {
                        Searcher = searcher,
                        SecurityChecker = searchContext.SecurityCheckerDelegate,
                        LuceneQuery = searchContext.LuceneQuery,
                        SearchQuery = searchContext.SearchQuery
                    });
                    searcher.Search(searchContext.LuceneQuery.Query, null, searchSecurityTrimmer);
                    luceneResults.TotalHits = searchSecurityTrimmer.TotalHits;

                    if (Logger.IsDebugEnabled)
                    {
                        var sb = GetSearcResultExplanation(searchContext.LuceneQuery, searchSecurityTrimmer.ScoreDocs, searcher);
                        Logger.Trace(sb);
                    }

                    //Page doesn't exist
                    if (luceneResults.TotalHits < minResults)
                        break;

                    luceneResults.Results = searchSecurityTrimmer.ScoreDocs.Select(match =>
                        new LuceneResult
                        {
                            Document = searcher.Doc(match.Doc),
                            Score = match.Score,
                            DisplayScore = GetDisplayScoreFromMatch(match.ToString()),
                            TitleSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.TitleTag, searchContext.LuceneQuery.TitleSnippetLength),
                            BodySnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.BodyTag, searchContext.LuceneQuery.BodySnippetLength),
                            DescriptionSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.DescriptionTag, searchContext.LuceneQuery.TitleSnippetLength),
                            TagSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.Tag, searchContext.LuceneQuery.TitleSnippetLength),
                            AuthorSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.AuthorNameTag, searchContext.LuceneQuery.TitleSnippetLength),
                            ContentSnippet = GetHighlightedText(highlighter, fieldQuery, searcher, match, Constants.ContentTag, searchContext.LuceneQuery.TitleSnippetLength)
                        }).ToList();
                    break;
                }
                catch (Exception ex) when (ex is IOException || ex is AlreadyClosedException)
                {
                    DisposeReaders();
                    DisposeWriter(false);

                    if (i == _searchRetryTimes - 1)
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

        private string GetHighlightedText(FastVectorHighlighter highlighter, FieldQuery fieldQuery, IndexSearcher searcher, ScoreDoc match, string tag, int length)
        {
            var s = highlighter.GetBestFragment(fieldQuery, searcher.IndexReader, match.Doc, tag, length);
            if (!string.IsNullOrEmpty(s))
            {
                s = HttpUtility.HtmlEncode(s).Replace(HighlightPreTag, HtmlPreTag).Replace(HighlightPostTag, HtmlPostTag);
            }
            return s;
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

        /// <summary>
        /// Extract the Score portion of the match.ToString()
        /// </summary>
        private string GetDisplayScoreFromMatch(string match)
        {
            var displayScore = string.Empty;
            if (!string.IsNullOrEmpty(match))
            {
                var beginPos = match.IndexOf('[');
                var endPos = match.LastIndexOf(']');
                if (beginPos > 0 && endPos > 0 && endPos > beginPos) displayScore = match.Substring(beginPos + 1, endPos - beginPos - 1);
            }

            return displayScore;
        }

        public void Add(Document doc)
        {
            Requires.NotNull("searchDocument", doc);
            if (doc.GetFields().Count > 0)
            {
                try
                {
                    Writer.AddDocument(doc);
                }
                catch (OutOfMemoryException)
                {
                    lock (_writerLock)
                    {
                        // as suggested by Lucene's doc
                        DisposeWriter();
                        Writer.AddDocument(doc);
                    }
                }
            }
        }

        public void Delete(Query query)
        {
            Requires.NotNull("luceneQuery", query);
            Writer.DeleteDocuments(query);
        }

        public void Commit()
        {
            if (_writer != null)
            {
                lock (_writerLock)
                {
                    if (_writer != null)
                    {
                        CheckDisposed();
                        _writer.Commit();
                        RescheduleAccessTimes();
                    }
                }
            }
        }

        public bool OptimizeSearchIndex(bool doWait)
        {
            var writer = _writer;
            if (writer != null && writer.HasDeletions())
            {
                if (doWait)
                {
                    Logger.Debug("Compacting Search Index - started");
                }

                CheckDisposed();
                //optimize down to "> 1 segments" for better performance than down to 1
                _writer.Optimize(4, doWait);
                
                if (doWait)
                {
                    Commit();
                    Logger.Debug("Compacting Search Index - finished");
                }

                return true;
            }

            return false;
        }

        public bool HasDeletions()
        {
            CheckDisposed();
            var searcher = GetSearcher();
            return searcher.IndexReader.HasDeletions;
        }

        public int MaxDocsCount()
        {
            CheckDisposed();
            var searcher  = GetSearcher();
            return searcher.IndexReader.MaxDoc;
        }

        public int SearchbleDocsCount()
        {
            CheckDisposed();
            var searcher = GetSearcher();
            return searcher.IndexReader.NumDocs();
        }

        // works on the computer (in a web-farm) that runs under the scheduler
        public SearchStatistics GetSearchStatistics()
        {
            CheckDisposed();
            var searcher = GetSearcher();

            var files = System.IO.Directory.GetFiles(IndexFolder, "*.*");
            var size = files.Select(name => new FileInfo(name)).Select(fInfo => fInfo.Length).Sum();

            return new SearchStatistics
                {
                    //Hack: seems that NumDocs/MaxDoc are buggy and return incorrect/swapped values
                    TotalActiveDocuments = searcher.IndexReader.NumDocs(),
                    TotalDeletedDocuments = searcher.IndexReader.NumDeletedDocs,
                    IndexLocation = IndexFolder,
                    LastModifiedOn = System.IO.Directory.GetLastWriteTimeUtc(IndexFolder),
                    IndexDbSize = size
                };
        }

        public void Dispose()
        {
            var status = Interlocked.CompareExchange(ref _isDisposed, DISPOSED, UNDISPOSED);
            if (status == UNDISPOSED)
            {
                DisposeWriter();
                DisposeReaders();
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
                            throw new ArgumentException(String.Format(
                                Localization.Localization.GetExceptionMessage("InvalidAnalyzerClass", "The class '{0}' cannot be created because it's invalid or is not an analyzer, will use default analyzer."), 
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

        private void DisposeWriter(bool commit = true)
        {
            if (_writer != null)
            {
                lock (_writerLock)
                {
                    if (_writer != null)
                    {
                        _idxReader.Dispose();
                        _idxReader = null;

                        if (commit)
                        {
                            _writer.Commit();
                        }

                        _writer.Dispose();
                        _writer = null;
                    }
                }
            }
        }

        private void DisposeReaders()
        {
            lock (((ICollection)_oldReaders).SyncRoot)
            {
                foreach (var rdr in _oldReaders)
                {
                    rdr.Dispose();
                }
                _oldReaders.Clear();
                _reader = null;
            }
        }

        class CachedReader : IDisposable
        {
            public DateTime LastUsed { get; private set; }
            private readonly IndexSearcher _searcher;

            public CachedReader(IndexSearcher searcher)
            {
                _searcher = searcher;
                UpdateLastUsed();
            }

            public IndexSearcher GetSearcher()
            {
                UpdateLastUsed();
                return _searcher;
            }

            private void UpdateLastUsed()
            {
                LastUsed = DateTime.Now;
            }

            public void Dispose()
            {
                _searcher.Dispose();
                _searcher.IndexReader.Dispose();
            }
        }
    }
}
