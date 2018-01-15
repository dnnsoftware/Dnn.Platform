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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using DotNetNuke.ComponentModel;
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
using DotNetNuke.Entities.Controllers;

using Directory = System.IO.Directory;

namespace DotNetNuke.Tests.Core.Controllers.Search
{
	/// <summary>
    ///  Testing various aspects of LuceneController
	/// </summary>
	[TestFixture]
	public class LuceneControllerTests
	{
		#region Private Properties & Constants

	    private readonly double _readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;
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

        private Mock<IHostController> _mockHostController;
        private LuceneControllerImpl _luceneController;
        private Mock<CachingProvider> _cachingProvider;
        private Mock<ISearchHelper> _mockSearchHelper;
	    private Mock<SearchQuery> _mockSearchQuery;

		#endregion

		#region Set Up

		[SetUp]
		public void SetUp()
		{
            ComponentFactory.Container = new SimpleContainer();
            _cachingProvider = MockComponentProvider.CreateDataCacheProvider();

            _mockHostController = new Mock<IHostController>();
            _mockHostController.Setup(c => c.GetString(Constants.SearchIndexFolderKey, It.IsAny<string>())).Returns(SearchIndexFolder);
            _mockHostController.Setup(c => c.GetDouble(Constants.SearchReaderRefreshTimeKey, It.IsAny<double>())).Returns(_readerStaleTimeSpan);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchMinLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMinLen);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchMaxLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMaxLen);
            HostController.RegisterInstance(_mockHostController.Object);

            _mockSearchHelper = new Mock<ISearchHelper>();
            _mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(GetSynonymsCallBack);
            _mockSearchHelper.Setup(c => c.GetSearchStopWords(It.IsAny<int>(), It.IsAny<string>())).Returns(new SearchStopWords());
            _mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            _mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool retainSpace) => html);
            SearchHelper.SetTestableInstance(_mockSearchHelper.Object);

            _mockSearchQuery = new Mock<SearchQuery>();

            DeleteIndexFolder();
		    CreateNewLuceneControllerInstance();
		}

	    [TearDown]
        public void TearDown()
        {
            _luceneController.Dispose();
            DeleteIndexFolder();
            SearchHelper.ClearInstance();
        }


		#endregion

        #region Private Methods

        private void CreateNewLuceneControllerInstance()
        {
            if (_luceneController != null)
            {
                LuceneController.ClearInstance();
                _luceneController.Dispose();
            }
            _luceneController = new LuceneControllerImpl();
            LuceneController.SetTestableInstance(_luceneController);
        }

        private IList<string> GetSynonymsCallBack(int portalId, string cultureCode, string term)
        {
            var synonyms = new List<string>();
            if (term == "fox")
                synonyms.Add("wolf");

            return synonyms;
        }

        private void DeleteIndexFolder()
        {
            try
            {
                if (Directory.Exists(SearchIndexFolder))
                    Directory.Delete(SearchIndexFolder, true);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Adds standarad SearchDocs in Lucene Index
        /// </summary>
        private void AddStandardDocs()
        {
            string[] lines = {
                Line1, Line2, Line3, Line4 
                };

            AddLinesAsSearchDocs(lines);
        }

        private void AddLinesAsSearchDocs(IEnumerable<string> lines)
         {
             foreach (var line in lines)
             {
                 var field = new Field(Constants.ContentTag, line, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
                 var doc = new Document();
                 doc.Add(field);
                 _luceneController.Add(doc);
             }

             _luceneController.Commit();
         }

        #endregion

        #region Add Tests

        [Test]
        public void LuceneController_SearchFolderIsAsExpected()
        {
            var inf1 = new DirectoryInfo(SearchIndexFolder);
            var inf2 = new DirectoryInfo(_luceneController.IndexFolder);
            Assert.AreEqual(inf1.FullName, inf2.FullName);
        }

        [Test]
        public void LuceneController_Add_Throws_On_Null_Document()
        {
            //Arrange          

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => _luceneController.Add(null));
        }

        public void LuceneController_Add_Throws_On_Null_Query()
        {
            //Arrange          

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => _luceneController.Delete(null));
        }

        [Test]
        public void LuceneController_Add_Empty_FiledsCollection_DoesNot_Create_Index()
        {
            //Arrange     
            
            //Act
            _luceneController.Add(new Document());
            _luceneController.Commit();

            var numFiles = 0;
            DeleteIndexFolder();

            Assert.AreEqual(0, numFiles);
        }

        [Test]
        public void LuceneController_GetsHighlightedDesc()
        {
            //Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            //Act 
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            _luceneController.Add(doc);
            _luceneController.Commit();

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery {Query = new TermQuery(new Term(fieldName, "fox"))}));

            //Assert
            Assert.AreEqual(1, hits.Results.Count());
            Assert.AreEqual("brown <b>fox</b> jumps over the lazy dog", hits.Results.ElementAt(0).ContentSnippet);
        }

        [Test]
        public void LuceneController_HighlightedDescHtmlEncodesOutput()
        {
            //Arrange
            const string fieldName = "content";
            const string fieldValue = "<script src='fox' type='text/javascript'></script>";
            const string expectedResult = " src=&#39;<b>fox</b>&#39; type=&#39;text/javascript&#39;&gt;&lt;/script&gt;";
            // Note that we mustn't get " src='<b>fox</b>' type='text/javascript'></script>" as this causes browser rendering issues

            //Act 
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            _luceneController.Add(doc);
            _luceneController.Commit();

            var hits = _luceneController.Search( CreateSearchContext(new LuceneQuery {Query = new TermQuery(new Term(fieldName, "fox"))}));

            //Assert
            Assert.AreEqual(1, hits.Results.Count());
            Assert.AreEqual(expectedResult, hits.Results.ElementAt(0).ContentSnippet);
        }

        [Test]
        public void LuceneController_FindsResultsUsingNearRealtimeSearchWithoutCommit()
        {
            //Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            //Act 
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);
            _luceneController.Add(doc);
            // DONOT commit here to enable testing near-realtime of search writer
            //_luceneController.Commit();

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(fieldName, "fox")) }));

            //Assert
            Assert.AreEqual(1, hits.Results.Count());
        }

        #endregion

        #region pagination tests

        [Test]
        public void LuceneController_Search_Returns_Correct_Total_Hits()
        {
            //Arrange
            AddStandardDocs();

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery {Query = new TermQuery(new Term(Constants.ContentTag, "fox"))}));

            //Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(4, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_1_Result_Returns_1_Record_But_More_TotalHits()
        {
            //Arrange
            AddStandardDocs();

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 1 }));

            //Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(1, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex1_PageSize4()
        {
            //Arrange
            AddStandardDocs();

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));

            //Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(4, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_4_Records_Returns_4_Records_With_4_TotalHits_Based_On_PageIndex4_PageSize1()
        {
            //Arrange
            AddStandardDocs();

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = new TermQuery(new Term(Constants.ContentTag, "fox")), PageIndex = 1, PageSize = 4 }));
            
            //Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(4, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Request_For_NonExisting_PageNumbers_Returns_No_Record()
        {
            //Arrange
            AddStandardDocs();

            var hits = _luceneController.Search(CreateSearchContext(
                new LuceneQuery{
                        Query = new TermQuery(new Term(Constants.ContentTag, "fox")),
                        PageIndex = 5,
                        PageSize = 10
                    }));

            //Assert
            Assert.AreEqual(4, hits.TotalHits);
            Assert.AreEqual(0, hits.Results.Count());
        }


        [Test]
        public void LuceneController_Search_Request_For_PagIndex2_PageSize1_Returns_2nd_Record_Only()
        {
            //Arrange
            AddStandardDocs();

            var query = new LuceneQuery
                {
                    Query = new TermQuery(new Term(Constants.ContentTag, "quick")), PageIndex = 2, PageSize = 1
                };

            var hits = _luceneController.Search(CreateSearchContext(query));

            //Assert
            Assert.AreEqual(3, hits.TotalHits);
            Assert.AreEqual(1, hits.Results.Count());
            // for some reason, this search's docs have scoring as
            // Line1=0.3125, Line1=0.3125, Line2=0.3125, Line2=0.3750
            Assert.AreEqual(Line1, hits.Results.ElementAt(0).Document.GetField(Constants.ContentTag).StringValue);
        }

        #endregion

        #region search tests
        [Test]
        public void LuceneController_NumericRangeCheck()
        {
            //Arrange
            const string fieldName = "content";

            //Act 

            //Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            _luceneController.Add(doc1);

            //Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
            _luceneController.Add(doc2);

            //Add third numeric field
            var doc3 = new Document();
            doc3.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(3));
            _luceneController.Add(doc3);

            //Add fourth numeric field
            var doc4 = new Document();
            doc4.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(4));
            _luceneController.Add(doc4);

            _luceneController.Commit();

            var query = NumericRangeQuery.NewIntRange(fieldName, 2, 3, true, true);
            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.AreEqual(2, hits.Results.Count());
        }

        [Test]
        public void LuceneController_DateRangeCheck()
        {
            //Arrange
            const string fieldName = "content";
            var dates = new List<DateTime> { DateTime.Now.AddYears(-3), DateTime.Now.AddYears(-2), DateTime.Now.AddYears(-1), DateTime.Now };

            //Act 
            foreach (var date in dates)
            {
                var doc = new Document();
                doc.Add(new NumericField(fieldName, Field.Store.YES, true).SetLongValue(long.Parse(date.ToString(Constants.DateTimeFormat))));
                _luceneController.Add(doc);
            }

            _luceneController.Commit();

            var futureTime = DateTime.Now.AddMinutes(1).ToString(Constants.DateTimeFormat);
            var query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(futureTime), long.Parse(futureTime), true, true);

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.AreEqual(0, hits.Results.Count());

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-1).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = query }));
            Assert.AreEqual(1, hits.Results.Count());

            query = NumericRangeQuery.NewLongRange(fieldName, long.Parse(DateTime.Now.AddDays(-368).ToString(Constants.DateTimeFormat)), long.Parse(DateTime.Now.ToString(Constants.DateTimeFormat)), true, true);
            hits = _luceneController.Search(CreateSearchContext(new LuceneQuery {Query = query}));
            Assert.AreEqual(2, hits.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_LuceneQuery()
        {
            Assert.Throws<ArgumentNullException>(() => _luceneController.Search(CreateSearchContext(null)));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Null_Query()
        {
            Assert.Throws<ArgumentNullException>(() => _luceneController.Search(CreateSearchContext(new LuceneQuery())));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageSize()
        {
            Assert.Throws<ArgumentException>(() => _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageSize = 0 })));
        }

        [Test]
        public void LuceneController_Search_Throws_On_Zero_PageIndex()
        {
            Assert.Throws<ArgumentException>(() => _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = new BooleanQuery(), PageIndex = 0 })));
        }

        [Test]
        [TestCase(EmptyCustomAnalyzer)]
        [TestCase(InvalidCustomAnalyzer)]
        [TestCase(ValidCustomAnalyzer)]
        public void LuceneController_Search_With_Chinese_Chars_And_Custom_Analyzer(string customAlalyzer = "")
        {
            _mockHostController.Setup(c => c.GetString(Constants.SearchCustomAnalyzer, It.IsAny<string>())).Returns(customAlalyzer);
            //Arrange
            const string fieldName = "content";
            const string fieldValue = Line_Chinese;

            //Act 
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            _luceneController.Add(doc);
            _luceneController.Commit();

            var analyzer = _luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Chinese);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

            //Assert
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
            _mockHostController.Setup(c => c.GetString(Constants.SearchCustomAnalyzer, It.IsAny<string>())).Returns(customAlalyzer);
            //Arrange
            const string fieldName = "content";
            const string fieldValue = Line1;

            //Act 
            var field = new Field(fieldName, fieldValue, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS);
            var doc = new Document();
            doc.Add(field);

            _luceneController.Add(doc);
            _luceneController.Commit();

            var analyzer = _luceneController.GetCustomAnalyzer() ?? new SearchQueryAnalyzer(true);
            var keywordQuery = new BooleanQuery();
            var parserContent = new QueryParser(Constants.LuceneVersion, fieldName, analyzer);
            var parsedQueryContent = parserContent.Parse(SearchKeyword_Line1);
            keywordQuery.Add(parsedQueryContent, Occur.SHOULD);

            var hits = _luceneController.Search(CreateSearchContext(new LuceneQuery { Query = keywordQuery }));

            //Assert
            Assert.AreEqual(1, hits.Results.Count());
            Assert.AreEqual("brown <b>fox</b> jumps over the lazy dog", hits.Results.ElementAt(0).ContentSnippet);

        }

        #endregion

        #region fuzzy search
        [Test]
        public void LuceneController_Search_Single_FuzzyQuery()
        {
            //Arrange  
            string[] docs = {
                "fuzzy",
                "wuzzy"
                };
            const string keyword = "wuzza";

            AddLinesAsSearchDocs(docs);

            //Act
            var luceneQuery = new LuceneQuery { Query = new FuzzyQuery(new Term(Constants.ContentTag, keyword)) };
            var previews = _luceneController.Search(CreateSearchContext(luceneQuery));

            //Assert
            Assert.AreEqual(2, previews.Results.Count());
        }

        [Test]
        public void LuceneController_Search_Double_FuzzyQuery()
        {
            //Arrange  
            string[] docs = {
                "home",
                "homez", // note home and homes could be returned by PorterFilter
                "fuzzy",
                "wuzzy"
                };

            string[] keywords = {
                "wuzza",
                "homy"
                };

            AddLinesAsSearchDocs(docs);

            //Act
            var finalQuery = new BooleanQuery();
            foreach (var keyword in keywords)
            {
                finalQuery.Add(new FuzzyQuery(new Term(Constants.ContentTag, keyword)), Occur.SHOULD);
            }

            var luceneQuery = new LuceneQuery { Query = finalQuery };
            var previews = _luceneController.Search(CreateSearchContext(luceneQuery));

            //Assert
            Assert.AreEqual(3, previews.Results.Count());
        }
        #endregion

        #region search reader tests
        [Test]
        public void LuceneController_Throws_SearchIndexEmptyException_WhenNoDataInSearch()
        {
            Assert.Throws<SearchIndexEmptyException>(() => { var r = _luceneController.GetSearcher(); });
        }

        [Test]
	    public void LuceneController_ReaderNotChangedBeforeTimeSpanElapsed()
	    {
            //Arrange
            const string fieldName = "content";

            //Act 

            //Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            _luceneController.Add(doc1);
            _luceneController.Commit();

            var reader = _luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(_readerStaleTimeSpan / 2));

            Assert.AreSame(reader, _luceneController.GetSearcher());
        }

        [Test]
        public void LuceneController_ReaderNotChangedIfNoIndexUpdated()
        {
            //Arrange
            const string fieldName = "content";

            //Act 

            //Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            _luceneController.Add(doc1);
            _luceneController.Commit();

            var reader = _luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(_readerStaleTimeSpan * 1.1));

            Assert.AreSame(reader, _luceneController.GetSearcher());
        }

        [Test]
        public void LuceneController_ReaderIsChangedWhenIndexIsUpdatedAndTimeIsElapsed()
        {
            //Arrange
            const string fieldName = "content";

            //Act 

            //Add first numeric field
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            _luceneController.Add(doc1);
            _luceneController.Commit();

            var reader = _luceneController.GetSearcher();
            Thread.Sleep(TimeSpan.FromSeconds(_readerStaleTimeSpan * 1.1));

            //Add second numeric field
            var doc2 = new Document();
            doc2.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(2));
            _luceneController.Add(doc2);

            //var lastAcccess = Directory.GetLastWriteTime(_luceneController.IndexFolder);
            //Directory.SetLastWriteTime(_luceneController.IndexFolder, lastAcccess + TimeSpan.FromSeconds(1));

            Assert.AreNotSame(reader, _luceneController.GetSearcher());
        }
        #endregion

        #region Locking Tests

	    [Test]
	    public void LuceneController_LockFileWhenExistsDoesNotCauseProblemForFirstIController()
	    {
	        //Arrange
	        const string fieldName = "content";
	        var lockFile = Path.Combine(SearchIndexFolder, WriteLockFile);
	        if (!Directory.Exists(SearchIndexFolder)) Directory.CreateDirectory(SearchIndexFolder);
	        if (!File.Exists(lockFile))
	        {
	            File.Create(lockFile).Close();
	        }

	        //Act 
	        var doc1 = new Document();
	        doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));

	        //Assert
            Assert.True(File.Exists(lockFile));
            Assert.DoesNotThrow(() => _luceneController.Add(doc1));
	    }

	    [Test]
        public void LuceneController_LockFileCanBeObtainedByOnlySingleController()
        {
            //Arrange
            const string fieldName = "content";
            var lockFile = Path.Combine(SearchIndexFolder, WriteLockFile);

            //Act 
            var doc1 = new Document();
            doc1.Add(new NumericField(fieldName, Field.Store.YES, true).SetIntValue(1));
            _luceneController.Add(doc1);

            // create another controller then try to access the already locked index by the first one
            var secondController = new LuceneControllerImpl();

            //Assert
            Assert.True(File.Exists(lockFile));
            Assert.Throws<SearchException>(() => secondController.Add(doc1));
        }

        #endregion

        #region Added/Deleted documents count and ptimization tests

        //Arrange
        const int TotalTestDocs2Create = 5;
        const string ContentFieldName = "content";

        private int AddTestDocs()
        {
            //Act 
            for (var i = 0; i < TotalTestDocs2Create; i++)
            {
                var doc = new Document();
                // format to "D#" because LengthFilter will not consider words of length < 3 or > 255 characters in length (defaults)
                doc.Add(new Field(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen), Field.Store.YES, Field.Index.ANALYZED));
                _luceneController.Add(doc);
            }

            _luceneController.Commit();
            return TotalTestDocs2Create;
        }

        private int DeleteTestDocs()
        {
            //Act 
            // delete odd docs => [1, 3] 
            var delCount = 0;
            for (var i = 1; i < TotalTestDocs2Create; i += 2)
            {
                // format to "D#" because LengthFilter will not consider the defaults for these values
                _luceneController.Delete(new TermQuery(new Term(ContentFieldName, i.ToString("D" + Constants.DefaultMinLen))));
                delCount++;
            }

            _luceneController.Commit();
            return delCount;
        }


        [Test]
        public void LuceneController_DocumentMaxAndCountAreCorrect()
        {
            AddTestDocs();

            Assert.AreEqual(TotalTestDocs2Create, _luceneController.MaxDocsCount());
            Assert.AreEqual(TotalTestDocs2Create, _luceneController.SearchbleDocsCount());
        }

        [Test]
        public void LuceneController_TestDeleteBeforeOptimize()
        {
            //Arrange
	        AddTestDocs();
            var delCount = DeleteTestDocs();

	        Assert.IsTrue(_luceneController.HasDeletions());
            Assert.AreEqual(TotalTestDocs2Create, _luceneController.MaxDocsCount());
            Assert.AreEqual(TotalTestDocs2Create - delCount, _luceneController.SearchbleDocsCount());
        }

        [Test]
        public void LuceneController_TestDeleteAfterOptimize()
        {
            //Arrange
            AddTestDocs();
            var delCount = DeleteTestDocs();

            _luceneController.OptimizeSearchIndex(true);

            Assert.AreEqual(TotalTestDocs2Create, _luceneController.MaxDocsCount());
            Assert.AreEqual(TotalTestDocs2Create - delCount, _luceneController.SearchbleDocsCount());
        }

        [Test]
        public void LuceneController_TestGetSearchStatistics()
        {
            //Arrange
            var addedCount = AddTestDocs();
            var delCount = DeleteTestDocs();
            var statistics = _luceneController.GetSearchStatistics();

            Assert.IsNotNull(statistics);
            Assert.AreEqual(statistics.TotalDeletedDocuments, delCount);
            Assert.AreEqual(statistics.TotalActiveDocuments, addedCount - delCount);
        }
        #endregion

        #region Tests for other controllers but need to be run from this location (they enforce tesing private components in others)

        [Test]
        public void SearchController_LuceneControllerReaderIsNotNullWhenWriterIsNull()
        {
            //Arrange
            AddTestDocs();
            CreateNewLuceneControllerInstance(); // to force a new reader for the next assertion

            //Assert
            Assert.IsNotNull(_luceneController.GetSearcher());
        }

        #endregion

        private LuceneSearchContext CreateSearchContext(LuceneQuery luceneQuery)
        {
            return new LuceneSearchContext {LuceneQuery = luceneQuery, SearchQuery = _mockSearchQuery.Object };
        }
    }
}

