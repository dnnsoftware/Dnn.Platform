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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using Directory = System.IO.Directory;

    /// <summary> Testing various aspects of LuceneController.</summary>
    [TestFixture]
    public class LuceneControllerTests
    {
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
        private readonly double readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;

        private Mock<IHostController> mockHostController;
        private LuceneControllerImpl luceneController;
        private Mock<CachingProvider> cachingProvider;
        private Mock<ISearchHelper> mockSearchHelper;
        private Mock<SearchQuery> mockSearchQuery;
        private FakeServiceProvider serviceProvider;

        private string SearchIndexFolder => this.mockHostController.Object.GetString(Constants.SearchIndexFolderKey, string.Empty);

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            this.cachingProvider = MockComponentProvider.CreateDataCacheProvider();

            this.MockHostController();

            this.mockSearchHelper = new Mock<ISearchHelper>();
            this.mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(this.GetSynonymsCallBack);
            this.mockSearchHelper.Setup(c => c.GetSearchStopWords(It.IsAny<int>(), It.IsAny<string>())).Returns(new SearchStopWords());
            this.mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            this.mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool retainSpace) => html);
            SearchHelper.SetTestableInstance(this.mockSearchHelper.Object);

            this.mockSearchQuery = new Mock<SearchQuery>();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.cachingProvider.Object);
                    services.AddSingleton(this.mockHostController.Object);
                    services.AddSingleton((IHostSettingsService)this.mockHostController.Object);
                    services.AddSingleton(this.mockSearchHelper.Object);
                    services.AddSingleton<IApplicationStatusInfo>(new ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
                });

            this.DeleteIndexFolder();
            this.CreateNewLuceneControllerInstance();
        }

        [TearDown]
        public void TearDown()
        {
            LuceneController.ClearInstance();
            this.luceneController.Dispose();
            this.DeleteIndexFolder();
            SearchHelper.ClearInstance();
            this.serviceProvider.Dispose();

            this.mockHostController = null;
            this.luceneController = null;
            this.cachingProvider = null;
            this.mockSearchHelper = null;
            this.mockSearchQuery = null;
        }

        private void MockHostController()
        {
            this.mockHostController = new Mock<IHostController>();

            this.mockHostController.Setup(c => c.GetString(Constants.SearchIndexFolderKey, It.IsAny<string>())).Returns(@"App_Data\LuceneTests" + DateTime.UtcNow.Ticks);
            this.mockHostController.Setup(c => c.GetDouble(Constants.SearchReaderRefreshTimeKey, It.IsAny<double>())).Returns(this.readerStaleTimeSpan);
            this.mockHostController.Setup(c => c.GetInteger(Constants.SearchMinLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMinLen);
            this.mockHostController.Setup(c => c.GetInteger(Constants.SearchMaxLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMaxLen);
            this.mockHostController.Setup(c => c.GetInteger(Constants.SearchRetryTimesKey, It.IsAny<int>())).Returns(DefaultSearchRetryTimes);
            this.mockHostController.As<IHostSettingsService>();
        }

        [Test]
        public void LuceneController_SearchFolderIsAsExpected()
        {
            var searchIndexFolder = this.mockHostController.Object.GetString(Constants.SearchIndexFolderKey, this.SearchIndexFolder);
            var inf1 = new DirectoryInfo(searchIndexFolder);
            var inf2 = new DirectoryInfo(this.luceneController.IndexFolder);
            Assert.That(inf2.Name, Is.EqualTo(inf1.Name));
        }

        [Test]
        public void LuceneController_Add_Throws_On_Null_Document()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Add(null));
        }

        public void LuceneController_Add_Throws_On_Null_Query()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Delete(null));
        }

        [Test]
        public void LuceneController_Add_Empty_FiledsCollection_DoesNot_Create_Index()
        {
            // Arrange

            // Act
            this.luceneController.Add(new Document());
            this.luceneController.Commit();

            var numFiles = 0;
            this.DeleteIndexFolder();

            Assert.That(numFiles, Is.EqualTo(0));
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

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.Results.Count(), Is.EqualTo(1));
                Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo("brown <b>fox</b> jumps over the lazy dog"));
            });
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

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.Results.Count(), Is.EqualTo(1));
                Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo(expectedResult));
            });
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
            this.luceneController.Add(doc);

            // DONOT commit here to enable testing near-realtime of search writer
            // _luceneController.Commit();
            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

            // Assert
            Assert.That(hits.Results.Count(), Is.EqualTo(1));
        }

        [Test]
        public void LuceneController_Search_Returns_Correct_Total_Hits()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")) }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(4));
            });
        }

        [Test]
        public void LuceneController_Search_Request_For_1_Result_Returns_1_Record_But_More_TotalHits()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 1 }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(1));
            });
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex1_PageSize4()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(4));
            });
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex4_PageSize1()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(4));
            });
        }

        [Test]
        public void LuceneController_Search_Request_For_NonExisting_PageNumbers_Returns_No_Record()
        {
            // Arrange
            this.AddStandardDocs();

            var hits = this.luceneController.Search(this.CreateSearchContext(
                new LuceneQuery
                {
                    Query = new TermQuery(new Term(Constants.ContentTag, "fox")),
                    PageIndex = 5,
                    PageSize = 10,
                }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(4));
                Assert.That(hits.Results.Count(), Is.EqualTo(0));
            });
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

            var hits = this.luceneController.Search(this.CreateSearchContext(query));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.TotalHits, Is.EqualTo(3));
                Assert.That(hits.Results.Count(), Is.EqualTo(1));

                // for some reason, this search's docs have scoring as
                // Line1=0.3125, Line1=0.3125, Line2=0.3125, Line2=0.3750
                Assert.That(hits.Results.ElementAt(0).Document.GetField(Constants.ContentTag).StringValue, Is.EqualTo(Line1));
            });
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
            this.luceneController.Add(doc1);

            // Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
            this.luceneController.Add(doc2);

            // Add third numeric field
            var doc3 = new Document();
            doc3.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(3));
            this.luceneController.Add(doc3);

            // Add fourth numeric field
            var doc4 = new Document();
            doc4.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(4));
            this.luceneController.Add(doc4);

            this.luceneController.Commit();

            var query = NumericRangeQuery.NewIntRange(fieldName, 2, 3, true, true);
            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.That(hits.Results.Count(), Is.EqualTo(2));
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
                this.luceneController.Add(doc);
            }

            this.luceneController.Commit();

            var futureTime = DateTime.Now.AddMinutes(1).ToString(Constants.DateTimeFormat);
            var query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(futureTime), long.Parse(futureTime), true, true);

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.That(hits.Results.Count(), Is.EqualTo(0));

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-1).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.That(hits.Results.Count(), Is.EqualTo(1));

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-368).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.That(hits.Results.Count(), Is.EqualTo(2));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_LuceneQuery()
        {
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Search(this.CreateSearchContext(null)));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_Query()
        {
            Assert.Throws<ArgumentNullException>(() => this.luceneController.Search(this.CreateSearchContext(new LuceneQuery())));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageSize()
        {
            Assert.Throws<ArgumentException>(() => this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageSize = 0 })));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageIndex()
        {
            Assert.Throws<ArgumentException>(() => this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageIndex = 0 })));
        }

        [Test]
        [TestCase(EmptyCustomAnalyzer)]
        [TestCase(InvalidCustomAnalyzer)]
        [TestCase(ValidCustomAnalyzer)]
        public void LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(string customAlalyzer = "")
        {
            this.mockHostController.Setup(controller => controller.GetString(Constants.SearchCustomAnalyzer, It.IsAny<string>())).Returns(customAlalyzer);

            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line_Chinese;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var analyzer = this.luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Chinese);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

            // Assert
            if (customAlalyzer == ValidCustomAnalyzer)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(hits.Results.Count(), Is.EqualTo(1));
                    Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo(Line_Chinese.Replace(SearchKeyword_Chinese, string.Format("<b>{0}</b>", SearchKeyword_Chinese))));
                });
            }
            else
            {
                Assert.That(hits.Results.Count(), Is.EqualTo(0));
            }
        }

        [Test]
        [TestCase(EmptyCustomAnalyzer)]
        [TestCase(InvalidCustomAnalyzer)]
        [TestCase(ValidCustomAnalyzer)]
        public void LuceneController_Search_With_English_Chars_And_Custom_Analyzer(string customAlalyzer = "")
        {
            this.mockHostController.Setup(c => c.GetString(Constants.SearchCustomAnalyzer, It.IsAny<string>())).Returns(customAlalyzer);

            // Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            // Act
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            this.luceneController.Add(doc);
            this.luceneController.Commit();

            var analyzer = this.luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Line1);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = this.luceneController.Search(this.CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(hits.Results.Count(), Is.EqualTo(1));
                Assert.That(hits.Results.ElementAt(0).ContentSnippet, Is.EqualTo("brown <b>fox</b> jumps over the lazy dog"));
            });
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
            var previews = this.luceneController.Search(this.CreateSearchContext(luceneQuery));

            // Assert
            Assert.That(previews.Results.Count(), Is.EqualTo(2));
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
            var previews = this.luceneController.Search(this.CreateSearchContext(luceneQuery));

            // Assert
            Assert.That(previews.Results.Count(), Is.EqualTo(3));
        }

        [Test]
        public void LuceneController_Throws_SearchIndexEmptyException_WhenNoDataInSearch()
        {
            Assert.Throws<SearchIndexEmptyException>(() => { var r = this.luceneController.GetSearcher(); });
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
            this.luceneController.Add(doc1);
            this.luceneController.Commit();

            var reader = this.luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this.readerStaleTimeSpan / 2));

            Assert.That(this.luceneController.GetSearcher(), Is.SameAs(reader));
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
            this.luceneController.Add(doc1);
            this.luceneController.Commit();

            var reader = this.luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this.readerStaleTimeSpan * 1.1));

            Assert.That(this.luceneController.GetSearcher(), Is.SameAs(reader));
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
            this.luceneController.Add(doc1);
            this.luceneController.Commit();

            var reader = this.luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(this.readerStaleTimeSpan * 1.1));

            // Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
            this.luceneController.Add(doc2);

            // var lastAcccess = Directory.GetLastWriteTime(_luceneController.IndexFolder);
            // Directory.SetLastWriteTime(_luceneController.IndexFolder, lastAcccess + TimeSpan.FromSeconds(1));
            Assert.That(this.luceneController.GetSearcher(), Is.Not.SameAs(reader));
        }

        [Test]
        public void LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController()
        {
            // Arrange
            const string fieldName = "content";
            var searchIndexFolder = this.mockHostController.Object.GetString(Constants.SearchIndexFolderKey, this.SearchIndexFolder);
            var lockFile = Path.Combine(searchIndexFolder, WriteLockFile);
            if (!Directory.Exists(searchIndexFolder))
            {
                Directory.CreateDirectory(searchIndexFolder);
            }

            if (!File.Exists(lockFile))
            {
                File.Create(lockFile).Close();
            }

            // Act
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));

            // Assert
            Assert.That(File.Exists(lockFile), Is.True);
            Assert.DoesNotThrow(() => this.luceneController.Add(doc1));
        }

        [Test]
        public void LuceneController_LockFileCanBeObtainedByOnlySingleController()
        {
            // Arrange
            const string fieldName = "content";
            var lockFile = Path.Combine(this.luceneController.IndexFolder, WriteLockFile);

            // Act
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            this.luceneController.Add(doc1);

            // create another controller then try to access the already locked index by the first one
            var secondController = new LuceneControllerImpl();

            // Assert
            Assert.That(File.Exists(lockFile), Is.True);
            Assert.Throws<SearchException>(() => secondController.Add(doc1));
        }

        [Test]
        public void LuceneController_DocumentMaxAndCountAreCorrect()
        {
            this.AddTestDocs();

            Assert.Multiple(() =>
            {
                Assert.That(this.luceneController.MaxDocsCount(), Is.EqualTo(TotalTestDocs2Create));
                Assert.That(this.luceneController.SearchbleDocsCount(), Is.EqualTo(TotalTestDocs2Create));
            });
        }

        [Test]
        public void LuceneController_TestDeleteBeforeOptimize()
        {
            // Arrange
            this.AddTestDocs();
            var delCount = this.DeleteTestDocs();

            Assert.Multiple(() =>
            {
                Assert.That(this.luceneController.HasDeletions(), Is.True);
                Assert.That(this.luceneController.MaxDocsCount(), Is.EqualTo(TotalTestDocs2Create));
                Assert.That(this.luceneController.SearchbleDocsCount(), Is.EqualTo(TotalTestDocs2Create - delCount));
            });
        }

        [Test]
        public void LuceneController_TestDeleteAfterOptimize()
        {
            // Arrange
            this.AddTestDocs();
            var delCount = this.DeleteTestDocs();

            this.luceneController.OptimizeSearchIndex(true);

            Assert.Multiple(() =>
            {
                Assert.That(this.luceneController.MaxDocsCount(), Is.EqualTo(TotalTestDocs2Create));
                Assert.That(this.luceneController.SearchbleDocsCount(), Is.EqualTo(TotalTestDocs2Create - delCount));
            });
        }

        [Test]
        public void LuceneController_TestGetSearchStatistics()
        {
            // Arrange
            var addedCount = this.AddTestDocs();
            var delCount = this.DeleteTestDocs();
            var statistics = this.luceneController.GetSearchStatistics();

            Assert.Multiple(() =>
            {
                Assert.That(statistics, Is.Not.Null);
                Assert.That(delCount, Is.EqualTo(statistics.TotalDeletedDocuments));
                Assert.That(addedCount - delCount, Is.EqualTo(statistics.TotalActiveDocuments));
            });
        }

        [Test]
        public void SearchController_LuceneControllerReaderIsNotNullWhenWriterIsNull()
        {
            // Arrange
            this.AddTestDocs();
            this.CreateNewLuceneControllerInstance(); // to force a new reader for the next assertion

            // Assert
            Assert.That(this.luceneController.GetSearcher(), Is.Not.Null);
        }

        private void CreateNewLuceneControllerInstance()
        {
            if (this.luceneController != null)
            {
                LuceneController.ClearInstance();
                this.luceneController.Dispose();
            }

            this.luceneController = new LuceneControllerImpl();
            LuceneController.SetTestableInstance(this.luceneController);
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
                if (Directory.Exists(this.SearchIndexFolder))
                {
                    Directory.Delete(this.SearchIndexFolder, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>Adds standarad SearchDocs in Lucene Index.</summary>
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
                this.luceneController.Add(doc);
            }

            this.luceneController.Commit();
        }

        private int AddTestDocs()
        {
            // Act
            for (var i = 0; i < TotalTestDocs2Create; i++)
            {
                var doc = new Document();

                // format to "D#" because LengthFilter will not consider words of length < 3 or > 255 characters in length (defaults)
                doc.Add(new Field(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen), Field.Store.YES, Field.Index.ANALYZED));
                this.luceneController.Add(doc);
            }

            this.luceneController.Commit();
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
                this.luceneController.Delete(new TermQuery(new Term(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen))));
                delCount++;
            }

            this.luceneController.Commit();
            return delCount;
        }

        private LuceneSearchContext CreateSearchContext(LuceneQuery luceneQuery)
        {
            return new LuceneSearchContext { LuceneQuery = luceneQuery, SearchQuery = this.mockSearchQuery.Object };
        }
    }
}
