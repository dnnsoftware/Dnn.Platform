// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;
    using Moq;
    using NUnit.Framework;

    using Directory = System.IO.Directory;

    /// <summary>
    ///  Testing various aspects of LuceneController.
    /// </summary>
    [TestFixture]
    public class LuceneControllerTests
    {
        private const string SearchIndexFolder = @"App_Data\LuceneTests";
        private const string WriteLockFile = "write.lock";
        private const string Line1 = "the quick brown fox jumps over the lazy dog";
        private const string Line2 = "the quick gold fox jumped over the lazy black dog";
        private const string Line3 = "the quick fox jumps over the black dog";
        private const string Line4 = "the red fox jumped over the lazy dark gray dog";
        private const string Line_Chinese = "这里是中文的内容";

        private const string SearchKeyword_Line1 = "fox";
        private const string SearchKeyword_Chinese = "中文";

        private const string EmptyCustomAnalyzer = "";
        private const string InvalidCustomAnalyzer = "Lucene.Net.Analysis.Cn.ChineseInvalidAnalyzer";
        private const string ValidCustomAnalyzer = "Lucene.Net.Analysis.Cn.ChineseAnalyzer, Lucene.Net.Contrib.Analyzers";
        private const int DefaultSearchRetryTimes = 5;

        // Arrange
        private const int TotalTestDocs2Create = 5;
        private const string ContentFieldName = "content";
        private readonly double _readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;

        private Mock<IHostController> _mockHostController;
        private LuceneControllerImpl _luceneController;
        private Mock<CachingProvider> _cachingProvider;
        private Mock<ISearchHelper> _mockSearchHelper;
        private Mock<SearchQuery> _mockSearchQuery;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            this._cachingProvider = MockComponentProvider.CreateDataCacheProvider();

            this._mockHostController = new Mock<IHostController>();
            this._mockHostController.Setup(c => c.GetString(Constants.SearchIndexFolderKey, It.IsAny<string>())).Returns(SearchIndexFolder);
            this._mockHostController.Setup(c => c.GetDouble(Constants.SearchReaderRefreshTimeKey, It.IsAny<double>())).Returns(this._readerStaleTimeSpan);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchMinLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMinLen);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchMaxLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMaxLen);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchRetryTimesKey, It.IsAny<int>())).Returns(DefaultSearchRetryTimes);
            HostController.RegisterInstance(this._mockHostController.Object);

            this._mockSearchHelper = new Mock<ISearchHelper>();
            this._mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(this.GetSynonymsCallBack);
            this._mockSearchHelper.Setup(c => c.GetSearchStopWords(It.IsAny<int>(), It.IsAny<string>())).Returns(new SearchStopWords());
            this._mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            this._mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool retainSpace) => html);
            SearchHelper.SetTestableInstance(this._mockSearchHelper.Object);

            this._mockSearchQuery = new Mock<SearchQuery>();

            this.DeleteIndexFolder();
            this.CreateNewLuceneControllerInstance();
        }

        [TearDown]
        public void TearDown()
        {
            this._luceneController.Dispose();
            this.DeleteIndexFolder();
            SearchHelper.ClearInstance();
        }

        [Test]
        public void LuceneController_SearchFolderIsAsExpected()
        {
            var inf1 = new DirectoryInfo(SearchIndexFolder);
            var inf2 = new DirectoryInfo(this._luceneController.IndexFolder);
            Assert.AreEqual(inf1.FullName, inf2.FullName);
        }

        [Test]
        public void LuceneController_Add_Throws_On_Null_Document()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this._luceneController.Add(null));
        }

        public void LuceneController_Add_Throws_On_Null_Query()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this._luceneController.Delete(null));
        }

        [Test]
        public void LuceneController_Add_Empty_FiledsCollection_DoesNot_Create_Index()
        {
            // Arrange

            // Act
            this._luceneController.Add(new Document());
            this._luceneController.Commit();

            var numFiles = 0;
            this.DeleteIndexFolder();

            Assert.AreEqual(0, numFiles);
        }

        [Test]
        public void LuceneController_GetsHighlightedDesc()
        {
            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            this._luceneController.Add(doc);
            this._luceneController.Commit();

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

            // Assert
            Assert.AreEqual(1, hits.Results.Count());
            Assert.AreEqual("brown <b>fox</b> jumps over the lazy dog", hits.Results.ElementAt(0).ContentSnippet);
        }

        [Test]
        public void LuceneController_HighlightedDescHtmlEncodesOutput()
        {
            // Arrange
            const string fieldName = "content";
            const string fieldValue = "<script src='fox' type='text/javascript'></script>";
            const string expectedResult = " src=&#39;<b>fox</b>&#39; type=&#39;text/javascript&#39;&gt;&lt;/script&gt;";

            // Note that we mustn't get " src='<b>fox</b>' type='text/javascript'></script>" as this causes browser rendering issues

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            this._luceneController.Add(doc);
            this._luceneController.Commit();

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

            // Assert
            Assert.AreEqual(1, hits.Results.Count());
            Assert.AreEqual(expectedResult, hits.Results.ElementAt(0).ContentSnippet);
        }

        [Test]
        public void LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit()
        {
            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);
            this._luceneController.Add(doc);

            // DONOT commit here to enable testing near-realtime of search writer
            // _luceneController.Commit();
            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

            // Assert
            Assert.AreEqual(1, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Returns_Correct_Total_Hits()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")) }));

            // Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(4, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_1_Result_Returns_1_Record_But_More_TotalHits()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 1 }));

            // Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(1, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex1_PageSize4()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));

            // Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(4, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex4_PageSize1()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));

            // Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(4, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_NonExisting_PageNumbers_Returns_No_Record()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this._luceneController.Search(this.CreateSearchContext(
                new LuceneQuery
                {
                    Query = new TermQuery(new Term(Constants.ContentTag, "fox")),
                    PageIndex = 5,
                    PageSize = 10,
                }));

            // Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(0, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_PagIndex2_PageSize1_Returns_2nd_Record_Only()
        {
            // Arrange
            this.AddStandardDocs();

            var query = new LuceneQuery
            {
                Query = new TermQuery(new Term(Constants.ContentTag, "quick")),
                PageIndex = 2,
                PageSize = 1,
            };

            var hits = this._luceneController.Search(this.CreateSearchContext(query));

            // Assert
            Assert.AreEqual(3, hits.TotalHits);
            Assert.AreEqual(1, hits.Results.Count());

            // for some reason, this search's docs have scoring as
            // Line1=0.3125, Line1=0.3125, Line2=0.3125, Line2=0.3750
            Assert.AreEqual(Line1, hits.Results.ElementAt(0).Document.GetField(Constants.ContentTag).StringValue);
        }

        [Test]
        public void LuceneController_NumericRangeCheck()
        {
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            this._luceneController.Add(doc1);

            // Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
            this._luceneController.Add(doc2);

            // Add third numeric field
            var doc3 = new Document();
            doc3.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(3));
            this._luceneController.Add(doc3);

            // Add fourth numeric field
            var doc4 = new Document();
            doc4.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(4));
            this._luceneController.Add(doc4);

            this._luceneController.Commit();

            var query = NumericRangeQuery.NewIntRange(fieldName, 2, 3, true, true);
            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.AreEqual(2, hits.Results.Count());
        }

        [Test]
        public void LuceneController_DateRangeCheck()
        {
            // Arrange
            const string fieldName = "content";
            var dates = new List<DateTime> { DateTime.Now.AddYears(-3), DateTime.Now.AddYears(-2), DateTime.Now.AddYears(-1), DateTime.Now };

            // Act
            foreach (var date in dates)
            {
                var doc = new Document();
                doc.Add(new NumericField(fieldName, Field.Store.YES, true).SetLongValue(long.Parse(date.ToString(Constants.DateTimeFormat))));
                this._luceneController.Add(doc);
            }

            this._luceneController.Commit();

            var futureTime = DateTime.Now.AddMinutes(1).ToString(Constants.DateTimeFormat);
            var query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(futureTime), long.Parse(futureTime), true, true);

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.AreEqual(0, hits.Results.Count());

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-1).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.AreEqual(1, hits.Results.Count());

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-368).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.AreEqual(2, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_LuceneQuery()
        {
            Assert.Throws<ArgumentNullException>(() => this._luceneController.Search(this.CreateSearchContext(null)));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_Query()
        {
            Assert.Throws<ArgumentNullException>(() => this._luceneController.Search(this.CreateSearchContext(new LuceneQuery())));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageSize()
        {
            Assert.Throws<ArgumentException>(() => this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageSize = 0 })));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageIndex()
        {
            Assert.Throws<ArgumentException>(() => this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageIndex = 0 })));
        }

        [Test]
        [TestCase(EmptyCustomAnalyzer)]
        [TestCase(InvalidCustomAnalyzer)]
        [TestCase(ValidCustomAnalyzer)]
        public void LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(string customAlalyzer = "")
        {
            this._mockHostController.Setup(c => c.GetString(Constants.SearchCustomAnalyzer, It.IsAny<string>())).Returns(customAlalyzer);

            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line_Chinese;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            this._luceneController.Add(doc);
            this._luceneController.Commit();

            var analyzer = this._luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Chinese);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

            // Assert
            if (customAlalyzer == ValidCustomAnalyzer)
            {
                Assert.AreEqual(1, hits.Results.Count());
                Assert.AreEqual(Line_Chinese.Replace(SearchKeyword_Chinese, string.Format("<b>{0}</b>", SearchKeyword_Chinese)), hits.Results.ElementAt(0).ContentSnippet);
            }
            else
            {
                Assert.AreEqual(0, hits.Results.Count());
            }
        }

        [Test]
        [TestCase(EmptyCustomAnalyzer)]
        [TestCase(InvalidCustomAnalyzer)]
        [TestCase(ValidCustomAnalyzer)]
        public void LuceneController_Search_With_English_Chars_And_Custom_Analyzer(string customAlalyzer = "")
        {
            this._mockHostController.Setup(c => c.GetString(Constants.SearchCustomAnalyzer, It.IsAny<string>())).Returns(customAlalyzer);

            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            this._luceneController.Add(doc);
            this._luceneController.Commit();

            var analyzer = this._luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Line1);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = this._luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

            // Assert
            Assert.AreEqual(1, hits.Results.Count());
            Assert.AreEqual("brown <b>fox</b> jumps over the lazy dog", hits.Results.ElementAt(0).ContentSnippet);
        }

        [Test]
        public void LuceneController_Search_Single_FuzzyQuery()
        {
            // Arrange
            string[] docs =
            {
                "fuzzy",
                "wuzzy",
                };
            const string keyword = "wuzza";

            this.AddLinesAsSearchDocs(docs);

            // Act
            var luceneQuery = new LuceneQuery { Query = new FuzzyQuery(new Term(Constants.ContentTag, keyword)) };
            var previews = this._luceneController.Search(this.CreateSearchContext(luceneQuery));

            // Assert
            Assert.AreEqual(2, previews.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Double_FuzzyQuery()
        {
            // Arrange
            string[] docs =
            {
                "home",
                "homez", // note home and homes could be returned by PorterFilter
                "fuzzy",
                "wuzzy",
                };

            string[] keywords =
            {
                "wuzza",
                "homy",
                };

            this.AddLinesAsSearchDocs(docs);

            // Act
            var finalQuery = new BooleanQuery();
            foreach (var keyword in keywords)
            {
                finalQuery.Add(new FuzzyQuery(new Term(Constants.ContentTag, keyword)), Occur.SHOULD);
            }

            var luceneQuery = new LuceneQuery { Query = finalQuery };
            var previews = this._luceneController.Search(this.CreateSearchContext(luceneQuery));

            // Assert
            Assert.AreEqual(3, previews.Results.Count());
        }

        [Test]
        public void LuceneController_Throws_SearchIndexEmptyException_WhenNoDataInSearch()
        {
            Assert.Throws<SearchIndexEmptyException>(() => { var r = this._luceneController.GetSearcher(); });
        }

        [Test]
        public void LuceneController_ReaderNotChangedBeforeTimeSpanElapsed()
        {
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            this._luceneController.Add(doc1);
            this._luceneController.Commit();

            var reader = this._luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this._readerStaleTimeSpan / 2));

            Assert.AreSame(reader, this._luceneController.GetSearcher());
        }

        [Test]
        public void LuceneController_ReaderNotChangedIfNoIndexUpdated()
        {
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            this._luceneController.Add(doc1);
            this._luceneController.Commit();

            var reader = this._luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this._readerStaleTimeSpan * 1.1));

            Assert.AreSame(reader, this._luceneController.GetSearcher());
        }

        [Test]
        public void LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed()
        {
            // Arrange
            const string fieldName = "content";

            // Act

            // Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            this._luceneController.Add(doc1);
            this._luceneController.Commit();

            var reader = this._luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this._readerStaleTimeSpan * 1.1));

            // Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
            this._luceneController.Add(doc2);

            // var lastAcccess = Directory.GetLastWriteTime(_luceneController.IndexFolder);
            // Directory.SetLastWriteTime(_luceneController.IndexFolder, lastAcccess + TimeSpan.FromSeconds(1));
            Assert.AreNotSame(reader, this._luceneController.GetSearcher());
        }

        [Test]
        public void LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController()
        {
            // Arrange
            const string fieldName = "content";
            var lockFile = Path.Combine(SearchIndexFolder, WriteLockFile);
            if (!Directory.Exists(SearchIndexFolder))
            {
                Directory.CreateDirectory(SearchIndexFolder);
            }

            if (!File.Exists(lockFile))
            {
                File.Create(lockFile).Close();
            }

            // Act
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));

            // Assert
            Assert.True(File.Exists(lockFile));
            Assert.DoesNotThrow(() => this._luceneController.Add(doc1));
        }

        [Test]
        public void LuceneController_LockFileCanBeObtainedByOnlySingleController()
        {
            // Arrange
            const string fieldName = "content";
            var lockFile = Path.Combine(SearchIndexFolder, WriteLockFile);

            // Act
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            this._luceneController.Add(doc1);

            // create another controller then try to access the already locked index by the first one
            var secondController = new LuceneControllerImpl();

            // Assert
            Assert.True(File.Exists(lockFile));
            Assert.Throws<SearchException>(() => secondController.Add(doc1));
        }

        [Test]
        public void LuceneController_DocumentMaxAndCountAreCorrect()
        {
            this.AddTestDocs();

            Assert.AreEqual(TotalTestDocs2Create, this._luceneController.MaxDocsCount());
            Assert.AreEqual(TotalTestDocs2Create, this._luceneController.SearchbleDocsCount());
        }

        [Test]
        public void LuceneController_TestDeleteBeforeOptimize()
        {
            // Arrange
            this.AddTestDocs();
            var delCount = this.DeleteTestDocs();

            Assert.IsTrue(this._luceneController.HasDeletions());
            Assert.AreEqual(TotalTestDocs2Create, this._luceneController.MaxDocsCount());
            Assert.AreEqual(TotalTestDocs2Create - delCount, this._luceneController.SearchbleDocsCount());
        }

        [Test]
        public void LuceneController_TestDeleteAfterOptimize()
        {
            // Arrange
            this.AddTestDocs();
            var delCount = this.DeleteTestDocs();

            this._luceneController.OptimizeSearchIndex(true);

            Assert.AreEqual(TotalTestDocs2Create, this._luceneController.MaxDocsCount());
            Assert.AreEqual(TotalTestDocs2Create - delCount, this._luceneController.SearchbleDocsCount());
        }

        [Test]
        public void LuceneController_TestGetSearchStatistics()
        {
            // Arrange
            var addedCount = this.AddTestDocs();
            var delCount = this.DeleteTestDocs();
            var statistics = this._luceneController.GetSearchStatistics();

            Assert.IsNotNull(statistics);
            Assert.AreEqual(statistics.TotalDeletedDocuments, delCount);
            Assert.AreEqual(statistics.TotalActiveDocuments, addedCount - delCount);
        }

        [Test]
        public void SearchController_LuceneControllerReaderIsNotNullWhenWriterIsNull()
        {
            // Arrange
            this.AddTestDocs();
            this.CreateNewLuceneControllerInstance(); // to force a new reader for the next assertion

            // Assert
            Assert.IsNotNull(this._luceneController.GetSearcher());
        }

        private void CreateNewLuceneControllerInstance()
        {
            if (this._luceneController != null)
            {
                LuceneController.ClearInstance();
                this._luceneController.Dispose();
            }

            this._luceneController = new LuceneControllerImpl();
            LuceneController.SetTestableInstance(this._luceneController);
        }

        private IList<string> GetSynonymsCallBack(int portalId, string cultureCode, string term)
        {
            var synonyms = new List<string>();
            if (term == "fox")
            {
                synonyms.Add("wolf");
            }

            return synonyms;
        }

        private void DeleteIndexFolder()
        {
            try
            {
                if (Directory.Exists(SearchIndexFolder))
                {
                    Directory.Delete(SearchIndexFolder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Adds standarad SearchDocs in Lucene Index.
        /// </summary>
        private void AddStandardDocs()
        {
            string[] lines =
            {
                Line1, Line2, Line3, Line4,
                };

            this.AddLinesAsSearchDocs(lines);
        }

        private void AddLinesAsSearchDocs(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                var field = new Field(Constants.ContentTag, line, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                var doc = new Document();
                doc.Add(field);
                this._luceneController.Add(doc);
            }

            this._luceneController.Commit();
        }

        private int AddTestDocs()
        {
            // Act
            for (var i = 0; i < TotalTestDocs2Create; i++)
            {
                var doc = new Document();

                // format to "D#" because LengthFilter will not consider words of length < 3 or > 255 characters in length (defaults)
                doc.Add(new Field(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen), Field.Store.YES, Field.Index.ANALYZED));
                this._luceneController.Add(doc);
            }

            this._luceneController.Commit();
            return TotalTestDocs2Create;
        }

        private int DeleteTestDocs()
        {
            // Act
            // delete odd docs => [1, 3]
            var delCount = 0;
            for (var i = 1; i < TotalTestDocs2Create; i += 2)
            {
                // format to "D#" because LengthFilter will not consider the defaults for these values
                this._luceneController.Delete(new TermQuery(new Term(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen))));
                delCount++;
            }

            this._luceneController.Commit();
            return delCount;
        }

        private LuceneSearchContext CreateSearchContext(LuceneQuery luceneQuery)
        {
            return new LuceneSearchContext { LuceneQuery = luceneQuery, SearchQuery = this._mockSearchQuery.Object };
        }
    }
}
