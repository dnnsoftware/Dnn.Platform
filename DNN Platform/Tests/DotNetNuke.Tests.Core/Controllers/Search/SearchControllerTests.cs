// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Lucene.Net.Documents;
    using Moq;
    using NUnit.Framework;

    using Constants = DotNetNuke.Services.Search.Internals.Constants;

    /// <summary>
    ///  Testing various aspects of SearchController.
    /// </summary>
    [TestFixture]
    public class SearchControllerTests
    {
        private const int ModuleSearchTypeId = (int)SearchTypeIds.ModuleSearchTypeId;
        private const int TabSearchTypeId = (int)SearchTypeIds.TabSearchTypeId;
        private const int DocumentSearchTypeId = (int)SearchTypeIds.DocumentSearchTypeId;
        private const int UrlSearchTypeId = (int)SearchTypeIds.UrlSearchTypeId;
        private const int OtherSearchTypeId = (int)SearchTypeIds.OtherSearchTypeId;
        private const int UnknownSearchTypeId = (int)SearchTypeIds.UnknownSearchTypeId;
        private const int PortalId0 = 0;
        private const int PortalId12 = 12;
        private const int IdeasModuleDefId = 201;
        private const int BlogsoduleDefId = 202;
        private const int AnswersModuleDefId = 203;
        private const int HtmlModuleDefId = 20;
        private const int HtmlModuleId = 25;
        private const int IdeasModuleId = 301;
        private const int BlogsModuleId = 302;
        private const int AnswersModuleId = 303;
        private const int RoleId731 = 731;
        private const int RoleId532 = 532;
        private const int RoleId0 = 0;
        private const string Tag0 = "tag0";
        private const string Tag0WithSpace = "tag0 hello";
        private const string Tag1 = "tag1";
        private const string Tag2 = "tag2";
        private const string Tag3 = "tag3";
        private const string Tag4 = "tag4";
        private const string TagTootsie = "tootsie";
        private const string TagLatest = "latest";
        private const string TagOldest = "oldest";
        private const string TagIt = "IT";
        private const string TagNeutral = "Neutral";
        private const string ModuleSearchTypeName = "module";
        private const string OtherSearchTypeName = "other";
        private const string TabSearchTypeName = "tab";
        private const string DocumentSearchTypeName = "document";
        private const string UrlSearchTypeName = "url";
        private const string ModuleResultControllerClass = "DotNetNuke.Services.Search.Crawlers.ModuleResultController, DotNetNuke";
        private const string FakeResultControllerClass = "DotNetNuke.Tests.Core.Controllers.Search.FakeResultController, DotNetNuke.Tests.Core";
        private const string NoPermissionFakeResultControllerClass = "DotNetNuke.Tests.Core.Controllers.Search.NoPermissionFakeResultController, DotNetNuke.Tests.Core";
        private const string CultureEnUs = "en-US";
        private const string CultureEnCa = "en-CA";
        private const string CultureItIt = "it-IT";
        private const string CultureEsEs = "es-ES";
        private const int LanguageIdEnUs = 1;
        private const int LanguageIdEnFr = 2;
        private const int LanguageIdItIt = 3;
        private const int LanguageIdEsEs = 4;
        private const int StandardAuthorId = 55;
        private const int StandardRoleId = 66;
        private const string StandardAuthorDisplayName = "Standard User";
        private const int StandardTabId = 99;
        private const string StandardPermission = "!Translator (en-US);![4];[5];[6];Administrators;ContentEditorRole;";
        private const string StandardQueryString = "cid=1";
        private const string NumericKey1 = "numerickey1";
        private const string NumericKey2 = "numerickey2";
        private const int NumericValue1 = 77777;
        private const int NumericValue2 = 55555;
        private const int NumericValue50 = 50;
        private const int NumericValue100 = 100;
        private const int NumericValue200 = 200;
        private const int NumericValue500 = 500;
        private const int NumericValue1000 = 1000;
        private const string KeyWord1Name = "keyword1";
        private const string KeyWord1Value = "value1";
        private const string KeyWord2Name = "keyword2";
        private const string KeyWord2Value = "value2";
        private const string KeyWord3Value = "value3";
        private const string KeyWord4Value = "value4";
        private const string KeyWord5Value = "value5";
        private const string Line1 = "The quick brown fox jumps over the lazy dog";
        private const string Line2 = "The quick gold fox jumped over the lazy black dog";
        private const string Line3 = "the quick fox jumps over the black dog - Italian";
        private const string Line4 = "the red fox jumped over the lazy dark gray dog";
        private const string Line5 = "the quick fox jumps over the white dog - los de el Espana";

        private const int CustomBoost = 80;

        private const string SearchIndexFolder = @"App_Data\SearchTests";
        private const int DefaultSearchRetryTimes = 5;
        private readonly double _readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;
        private Mock<IHostController> _mockHostController;
        private Mock<CachingProvider> _mockCachingProvider;
        private Mock<DataProvider> _mockDataProvider;
        private Mock<ILocaleController> _mockLocaleController;
        private Mock<ISearchHelper> _mockSearchHelper;
        private Mock<IUserController> _mockUserController;

        private SearchControllerImpl _searchController;
        private IInternalSearchController _internalSearchController;
        private LuceneControllerImpl _luceneController;

        public enum SearchTypeIds
        {
            ModuleSearchTypeId = 1,
            TabSearchTypeId,
            DocumentSearchTypeId,
            UrlSearchTypeId,
            OtherSearchTypeId,
            UnknownSearchTypeId,
        }

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.ResetContainer();

            this._mockDataProvider = MockComponentProvider.CreateDataProvider();
            this._mockLocaleController = MockComponentProvider.CreateLocaleController();
            this._mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();

            this._mockUserController = new Mock<IUserController>();
            this._mockHostController = new Mock<IHostController>();
            this._mockSearchHelper = new Mock<ISearchHelper>();

            this.SetupDataProvider();
            this.SetupHostController();
            this.SetupSearchHelper();
            this.SetupLocaleController();

            this._mockUserController.Setup(c => c.GetUserById(It.IsAny<int>(), It.IsAny<int>())).Returns((int portalId, int userId) => this.GetUserByIdCallback(portalId, userId));
            UserController.SetTestableInstance(this._mockUserController.Object);

            this.CreateNewLuceneControllerInstance();
        }

        [TearDown]
        public void TearDown()
        {
            this._luceneController.Dispose();
            this.DeleteIndexFolder();
            InternalSearchController.ClearInstance();
            UserController.ClearInstance();
            SearchHelper.ClearInstance();
            LuceneController.ClearInstance();
            this._luceneController = null;
        }

        [Test]
        public void SearchController_Search_Throws_On_Null_Query()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this._searchController.SiteSearch(null));
        }

        [Test]
        public void SearchController_Search_Throws_On_Empty_TypeId_Collection()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this._searchController.SiteSearch(new SearchQuery { KeyWords = "word" }));
        }

        [Test]
        public void SearchController_AddSearchDcoumet_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing()
        {
            // Arrange
            var document = new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "<<Click here for the complete city listing by a... ", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act, Assert
            this._internalSearchController.AddSearchDocument(document);
        }

        [Test]
        public void SearchController_AddSearchDcoumet_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing2()
        {
            // Arrange
            var document = new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "<<Click here for the complete city listing by a... ", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act, Assert
            Assert.DoesNotThrow(() => ExecuteWithTimeout(
                    () =>
                    {
                        this._internalSearchController.AddSearchDocument(document);
                        return false;
                    }, TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void SearchController_Added_Item_IsRetrieved()
        {
            // Arrange
            var doc = new SearchDocument { UniqueKey = "key01", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc);

            var result = this.SearchForKeyword("hello");

            // Assert
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(result.Results[0].UniqueKey, doc.UniqueKey);
            Assert.AreEqual(result.Results[0].Title, doc.Title);
        }

        [Test]
        public void SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween()
        {
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                };

            // Act

            // Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = Guid.NewGuid().ToString(), SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this._internalSearchController.AddSearchDocument(doc1);

            // first luceneQuery
            var query1 = new SearchQuery { KeyWords = "fox", SearchTypeIds = new List<int> { OtherSearchTypeId } };
            var search1 = this._searchController.SiteSearch(query1);

            // Assert
            Assert.AreEqual(1, search1.Results.Count);

            // Add second document
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = Guid.NewGuid().ToString(), SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this._internalSearchController.AddSearchDocument(doc2);
            this.CreateNewLuceneControllerInstance(); // to force a new reader for the next assertion

            // second luceneQuery
            var query2 = new SearchQuery { KeyWords = "fox", SearchTypeIds = new List<int> { OtherSearchTypeId } };
            var search2 = this._searchController.SiteSearch(query2);

            // Assert
            Assert.AreEqual(2, search2.Results.Count);
        }

        [Test]
        public void SearchController_Getsearch_TwoTermsSearch()
        {
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                Line3,
                Line4,
                Line5,
                };

            this.AddLinesAsSearchDocs(docs);

            // Act
            var search = this.SearchForKeyword("fox jumps");

            // Assert
            Assert.AreEqual(docs.Length, search.Results.Count);

            // Assert.AreEqual("brown <b>fox jumps</b> over the lazy dog ", search.Results[0].Snippet);
            // Assert.AreEqual("quick <b>fox jumps</b> over the black dog ", search.Results[1].Snippet);
        }

        [Test]
        public void SearchController_GetResult_TwoTermsSearch()
        {
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                Line3,
                Line4,
                Line5,
                };

            this.AddLinesAsSearchDocs(docs);

            // Act
            var search = this.SearchForKeyword("fox jumps");

            // Assert
            Assert.AreEqual(docs.Length, search.Results.Count);

            // Assert.AreEqual("brown <b>fox jumps</b> over the lazy dog ", search.Results[0].Snippet);
            // Assert.AreEqual("quick <b>fox jumps</b> over the black dog ", search.Results[1].Snippet);
        }

        [Test]
        public void SearchController_GetResult_PortalIdSearch()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, PortalIds = new List<int> { PortalId0 } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_SearchTypeIdSearch()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
        }

        [Test]
        public void SearchController_SearchFindsAnalyzedVeryLongWords()
        {
            // Arrange
            // const string fieldName = Constants.ContentTag;
            const string veryLongWord = // 107 characters
                "NowIsTheTimeForAllGoodMenToComeToTheAidOfTheirCountryalsoIsTheTimeForAllGoodMenToComeToTheAidOfTheirCountry";

            var doc = new SearchDocument
            {
                Title = veryLongWord,
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = ModuleSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                ModuleId = 1,
                ModuleDefId = 1,
            };
            this._internalSearchController.AddSearchDocument(doc);

            // Act
            var query = new SearchQuery { KeyWords = veryLongWord, SearchTypeIds = new List<int> { ModuleSearchTypeId } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual("<b>" + veryLongWord + "</b>", this.StipEllipses(search.Results[0].Snippet).Trim());
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsNoResultsWhenHavingNoPermission()
        {
            // Arrange
            this.AddStandardSearchDocs(DocumentSearchTypeId);

            // Act
            var result = this.SearchForKeyword("fox", DocumentSearchTypeId);

            // Assert
            // by default AuthorUserId = 0 which have no permission, so this passes
            Assert.AreEqual(0, result.Results.Count);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1A()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 4,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1B()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 6,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9, 16, 17 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1C()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9, 16, 17, 18, 19 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1D()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId; // user should have access to some documnets here
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 100,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(4 * 3, result.TotalHits);
            Assert.AreEqual(4 * 3, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9, 16, 17, 18, 19, 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1E()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = TabSearchTypeId; // user should have access to all documnets here
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 10,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).Skip(1).ToArray();

            // Assert
            Assert.AreEqual(maxDocs, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(Enumerable.Range(1, 9).ToArray(), ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1F()
        {
            // Arrange
            const int maxDocs = 100;
            const int stype = TabSearchTypeId; // user should have access to all documnets here
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 10,
                PageSize = 10,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(Enumerable.Range(90, 10).ToArray(), ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2A()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 5,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(5, result.Results.Count);
            Assert.AreEqual(new[] { 17, 18, 19, 26, 27 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2B()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 6,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(6, result.Results.Count);
            Assert.AreEqual(new[] { 18, 19, 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2C()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(4, result.Results.Count);
            Assert.AreEqual(new[] { 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3A()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 4,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(queryPg3.PageSize, result.Results.Count);
            Assert.AreEqual(new[] { 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3B()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 5,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(2, result.Results.Count);
            Assert.AreEqual(new[] { 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3C()
        {
            // Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(0, result.Results.Count);
            Assert.AreEqual(new int[] { }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage5()
        {
            // Arrange
            const int maxDocs = 100;
            const int stype = DocumentSearchTypeId;
            this.SetupSecurityTrimmingDocs(maxDocs, stype);

            // Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 5,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype },
            };

            var result = this._searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            // Assert
            Assert.AreEqual(maxDocs - (10 * 6), result.TotalHits);
            Assert.AreEqual(queryPg3.PageSize, result.Results.Count);
            Assert.AreEqual(new int[] { 86, 87, 88, 89, 96, 97, 98, 99 }, ids);
        }

        [Test]
        public void SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied()
        {
            // Arrange
            var modifiedDateTime = DateTime.UtcNow;
            var numericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue1 }, { NumericKey2, NumericValue2 } };
            var keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord1Value }, { KeyWord2Name, KeyWord2Value } };
            var tags = new List<string> { Tag1, Tag2 };
            var doc = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key",
                SearchTypeId = ModuleSearchTypeId,
                ModifiedTimeUtc = modifiedDateTime,
                PortalId = PortalId12,
                TabId = StandardTabId,
                ModuleDefId = HtmlModuleDefId,
                ModuleId = HtmlModuleId,
                Description = "Description",
                Body = "Body",
                AuthorUserId = StandardAuthorId,
                RoleId = StandardRoleId,
                Permissions = StandardPermission,
                QueryString = StandardQueryString,
                Tags = tags,
                NumericKeys = numericKeys,
                Keywords = keywords,
            };
            this._internalSearchController.AddSearchDocument(doc);

            // run luceneQuery on common keyword between both the docs
            var search = this.SearchForKeywordInModule("Title");

            // Assert
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(PortalId12, search.Results[0].PortalId);
            Assert.AreEqual(StandardTabId, search.Results[0].TabId);
            Assert.AreEqual(HtmlModuleDefId, search.Results[0].ModuleDefId);
            Assert.AreEqual(HtmlModuleId, search.Results[0].ModuleId);
            Assert.AreEqual(ModuleSearchTypeId, search.Results[0].SearchTypeId);
            Assert.AreEqual("Description", search.Results[0].Description);
            Assert.AreEqual("Body", search.Results[0].Body);
            Assert.AreEqual(StandardAuthorId, search.Results[0].AuthorUserId);
            Assert.AreEqual(StandardRoleId, search.Results[0].RoleId);
            Assert.AreEqual(modifiedDateTime.ToString(Constants.DateTimeFormat), search.Results[0].ModifiedTimeUtc.ToString(Constants.DateTimeFormat));
            Assert.AreEqual(StandardPermission, search.Results[0].Permissions);
            Assert.AreEqual(StandardQueryString, search.Results[0].QueryString);
            Assert.AreEqual(StandardAuthorDisplayName, search.Results[0].AuthorName);
            Assert.AreEqual(tags.Count, search.Results[0].Tags.Count());
            Assert.AreEqual(tags[0], search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(tags[1], search.Results[0].Tags.ElementAt(1));
            Assert.AreEqual(numericKeys.Count, search.Results[0].NumericKeys.Count);
            Assert.AreEqual(numericKeys[NumericKey1], search.Results[0].NumericKeys[NumericKey1]);
            Assert.AreEqual(numericKeys[NumericKey2], search.Results[0].NumericKeys[NumericKey2]);
            Assert.AreEqual(keywords.Count, search.Results[0].Keywords.Count);
            Assert.AreEqual(keywords[KeyWord1Name], search.Results[0].Keywords[KeyWord1Name]);
            Assert.AreEqual(keywords[KeyWord2Name], search.Results[0].Keywords[KeyWord2Name]);
        }

        [Test]
        public void SearchController_GetResult_Returns_EmptyData_When_Optionals_Are_Not_Supplied()
        {
            // Arrange
            var modifiedDateTime = DateTime.UtcNow;
            var doc = new SearchDocument
            {
                PortalId = PortalId0,
                Title = "Title",
                UniqueKey = "key",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = modifiedDateTime,
            };
            this._internalSearchController.AddSearchDocument(doc);

            var search = this.SearchForKeyword("Title");

            // Assert -
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(PortalId0, search.Results[0].PortalId);
            Assert.AreEqual(0, search.Results[0].TabId);
            Assert.AreEqual(0, search.Results[0].ModuleDefId);
            Assert.AreEqual(0, search.Results[0].ModuleId);
            Assert.AreEqual(OtherSearchTypeId, search.Results[0].SearchTypeId);
            Assert.AreEqual(null, search.Results[0].Description);
            Assert.AreEqual(null, search.Results[0].Body);
            Assert.AreEqual(0, search.Results[0].AuthorUserId);
            Assert.AreEqual(-1, search.Results[0].RoleId);
            Assert.AreEqual(modifiedDateTime.ToString(Constants.DateTimeFormat), search.Results[0].ModifiedTimeUtc.ToString(Constants.DateTimeFormat));
            Assert.AreEqual(null, search.Results[0].Permissions);
            Assert.AreEqual(null, search.Results[0].QueryString);
            Assert.AreEqual(null, search.Results[0].AuthorName);
            Assert.AreEqual(0, search.Results[0].Tags.Count());
            Assert.AreEqual(0, search.Results[0].NumericKeys.Count);
            Assert.AreEqual(0, search.Results[0].Keywords.Count);
        }

        [Test]
        public void SearchController_GetsHighlightedDesc()
        {
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                Line3,
                Line4,
                Line5,
                };
            this.AddLinesAsSearchDocs(docs);

            // Act
            var search = this.SearchForKeyword("fox");

            // Assert
            Assert.AreEqual(docs.Length, search.Results.Count);
            Assert.IsTrue(
                new[]
                {
                  "brown <b>fox</b> jumps over the lazy dog",
                  "quick <b>fox</b> jumps over the black dog - Italian",
                  "gold <b>fox</b> jumped over the lazy black dog",
                  "e red <b>fox</b> jumped over the lazy dark gray dog",
                  "quick <b>fox</b> jumps over the white dog - los de el Espana",
                }.SequenceEqual(search.Results.Select(r => this.StipEllipses(r.Snippet))),
                "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Snippet)));
        }

        [Test]
        public void SearchController_CorrectDocumentCultureIsUsedAtIndexing()
        {
            // Arrange
            // assign a culture that is different than the current one
            var isNonEnglishEnv = Thread.CurrentThread.CurrentCulture.Name != CultureEsEs;
            string cultureCode, title, searchWord;

            // Act
            if (isNonEnglishEnv)
            {
                cultureCode = CultureEsEs;
                searchWord = "zorro";
                title = "los rápidos saltos de zorro sobre el perro negro";
            }
            else
            {
                cultureCode = CultureEnUs;
                searchWord = "fox";
                title = Line3;
            }

            this._internalSearchController.AddSearchDocument(
                new SearchDocument
                {
                    Title = title,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = OtherSearchTypeId,
                    ModifiedTimeUtc = DateTime.UtcNow,
                    CultureCode = cultureCode,
                });
            this._internalSearchController.Commit();

            var searches = this.SearchForKeyword(searchWord);

            // Assert
            Assert.AreEqual(1, searches.TotalHits);
            Assert.AreEqual(cultureCode, searches.Results[0].CultureCode);
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Ignores_When_Only_BeginDate_Specified()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, BeginModifiedTimeUtc = DateTime.Now };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Resturns_Scoped_Results_When_BeginDate_Is_After_End_Date()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                BeginModifiedTimeUtc = DateTime.Now,
                EndModifiedTimeUtc = DateTime.Now.AddSeconds(-1),
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Resturns_Scoped_Results_When_Both_Dates_Specified()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();
            var stypeIds = new List<int> { ModuleSearchTypeId };
            var utcNow = DateTime.UtcNow.AddDays(1);
            const SortFields sfield = SortFields.LastModified;

            // Act and Assert - just a bit later
            var query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddSeconds(1), EndModifiedTimeUtc = utcNow.AddDays(1) };
            var search = this._searchController.SiteSearch(query);
            Assert.AreEqual(0, search.Results.Count);

            // Act and Assert - 10 day
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-10), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this._searchController.SiteSearch(query);
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(Line5, search.Results[0].Title);

            // Act and Assert - 1 year or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-368), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this._searchController.SiteSearch(query);
            Assert.AreEqual(2, search.Results.Count);
            Assert.AreEqual(Line5, search.Results[0].Title);
            Assert.AreEqual(Line4, search.Results[1].Title);

            // Act and Assert - 2 years or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-800), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this._searchController.SiteSearch(query);
            Assert.AreEqual(3, search.Results.Count);
            Assert.AreEqual(Line5, search.Results[0].Title);
            Assert.AreEqual(Line4, search.Results[1].Title);
            Assert.AreEqual(Line3, search.Results[2].Title);

            // Act and Assert - 3 years or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-1200), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = this._searchController.SiteSearch(query);
            Assert.AreEqual(4, search.Results.Count);
            Assert.AreEqual(Line5, search.Results[0].Title);
            Assert.AreEqual(Line4, search.Results[1].Title);
            Assert.AreEqual(Line3, search.Results[2].Title);
            Assert.AreEqual(Line2, search.Results[3].Title);

            // Act and Assert - 2 to 3 years or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-1200), EndModifiedTimeUtc = utcNow.AddDays(-800) };
            search = this._searchController.SiteSearch(query);
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(Line2, search.Results[0].Title);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_Returns_Single_Result()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag0 } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_With_Space_Returns_Single_Result()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag0WithSpace } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Lowercase_Search_Returns_PropercaseTag_Single_Result()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { TagNeutral.ToLowerInvariant() } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_Returns_Two_Results()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag1 } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(2, search.Results.Count);
            Assert.AreEqual(Tag0, search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(Tag1, search.Results[0].Tags.ElementAt(1));
            Assert.AreEqual(Tag1, search.Results[1].Tags.ElementAt(0));
            Assert.AreEqual(Tag2, search.Results[1].Tags.ElementAt(1));
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Two_Tags_Returns_Nothing()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag0, Tag4 } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(0, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Two_Tags_Returns_Single_Results()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag1, Tag2 } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(Tag1, search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(Tag2, search.Results[0].Tags.ElementAt(1));
        }

        [Test]
        public void SearchController_GetResult_TagSearch_With_Vowel_Tags_Returns_Data()
        {
            // Arrange
            const string keyword = "awesome";
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { TagTootsie } };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, Tags = new List<string> { TagTootsie } };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_CustomNumericField_Is_Specified_And_CustomSortField_Is_Not()
        {
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomNumericField,
            };

            this._searchController.SiteSearch(query);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_CustomStringField_Is_Specified_And_CustomSortField_Is_Not()
        {
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomStringField,
            };

            this._searchController.SiteSearch(query);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_NumericKey_Is_Specified_And_CustomSortField_Is_Not()
        {
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.NumericKey,
            };

            this._searchController.SiteSearch(query);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_Keyword_Is_Specified_And_CustomSortField_Is_Not()
        {
            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Keyword,
            };

            this._searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Date_Returns_Latest_Docs_First()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.LastModified,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count, "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));

            Assert.AreEqual(Tag3, search.Results[1].Tags.ElementAt(0));
            Assert.AreEqual(Tag4, search.Results[1].Tags.ElementAt(1));
            Assert.AreEqual(TagLatest, search.Results[1].Tags.ElementAt(2));

            Assert.AreEqual(Tag2, search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(Tag3, search.Results[0].Tags.ElementAt(1));
            Assert.AreEqual(TagIt.ToLowerInvariant(), search.Results[0].Tags.ElementAt(2));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Date_Ascending_Returns_Earliest_Docs_First()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.LastModified,
                SortDirection = SortDirections.Ascending,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.Greater(search.Results[1].DisplayModifiedTime, search.Results[0].DisplayModifiedTime);
            Assert.Greater(search.Results[2].DisplayModifiedTime, search.Results[1].DisplayModifiedTime);
            Assert.Greater(search.Results[3].DisplayModifiedTime, search.Results[2].DisplayModifiedTime);

            Assert.AreEqual(Tag0, search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(Tag1, search.Results[0].Tags.ElementAt(1));
            Assert.AreEqual(TagOldest, search.Results[0].Tags.ElementAt(2));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_NumericKeys_Ascending_Returns_Smaller_Numers_First()
        {
            var added = this.AddDocumentsWithNumericKeys();

            // Act
            var query = new SearchQuery
            {
                KeyWords = "Title",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.NumericKey,
                SortDirection = SortDirections.Ascending,
                CustomSortField = NumericKey1,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.Greater(search.Results[1].NumericKeys[NumericKey1], search.Results[0].NumericKeys[NumericKey1]);
            Assert.Greater(search.Results[2].NumericKeys[NumericKey1], search.Results[1].NumericKeys[NumericKey1]);
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_NumericKeys_Descending_Returns_Bigger_Numbers_First()
        {
            var added = this.AddDocumentsWithNumericKeys();

            // Act
            var query = new SearchQuery
            {
                KeyWords = "Title",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.NumericKey,
                SortDirection = SortDirections.Descending,
                CustomSortField = NumericKey1,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.Greater(search.Results[0].NumericKeys[NumericKey1], search.Results[1].NumericKeys[NumericKey1]);
            Assert.Greater(search.Results[1].NumericKeys[NumericKey1], search.Results[2].NumericKeys[NumericKey1]);
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Title_Ascending_Returns_Alphabetic_Ascending()
        {
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

            var added = this.AddDocuments(titles, "animal");

            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Title,
                SortDirection = SortDirections.Ascending,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);

            var count = 0;
            foreach (var title in titles.OrderBy(s => s))
            {
                Assert.AreEqual(title, search.Results[count++].Title);
            }
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Title_Descending_Returns_Alphabetic_Descending()
        {
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

            var added = this.AddDocuments(titles, "animal");

            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Title,
                SortDirection = SortDirections.Descending,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);

            var count = 0;
            foreach (var title in titles.OrderByDescending(s => s))
            {
                Assert.AreEqual(title, search.Results[count++].Title);
            }
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Keyword_Ascending_Returns_Alphabetic_Ascending()
        {
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

            var added = this.AddDocumentsWithKeywords(titles, "animal");

            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Keyword,
                SortDirection = SortDirections.Ascending,
                CustomSortField = KeyWord1Name,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);

            var count = 0;
            foreach (var title in titles.OrderBy(s => s))
            {
                Assert.AreEqual(title, search.Results[count++].Keywords[KeyWord1Name]);
            }
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Keyword_Descending_Returns_Alphabetic_Descending()
        {
            var titles = new List<string> { "cat", "ant", "dog", "antelope", "zebra", "yellow", " " };

            var added = this.AddDocumentsWithKeywords(titles, "animal");

            // Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Keyword,
                SortDirection = SortDirections.Descending,
                CustomSortField = KeyWord1Name,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);

            var count = 0;
            foreach (var title in titles.OrderByDescending(s => s))
            {
                Assert.AreEqual(title, search.Results[count++].Keywords[KeyWord1Name]);
            }
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_StringField_In_Descending_Order_Does_Not_Throw()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomStringField,
                SortDirection = SortDirections.Descending,
                CustomSortField = "unknown",
            };
            this._searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_StringField_In_Ascending_Order_Does_Not_Throw()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomStringField,
                SortDirection = SortDirections.Ascending,
                CustomSortField = "unknown",
            };
            this._searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_NumericField_In_Descending_Order_Does_Not_Throw()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomNumericField,
                SortDirection = SortDirections.Descending,
                CustomSortField = "unknown",
            };
            this._searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_NumericField_In_Ascending_Order_Does_Not_Throw()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomNumericField,
                SortDirection = SortDirections.Ascending,
                CustomSortField = "unknown",
            };
            this._searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Relevance_Returns_TopHit_Docs_First()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Relevance,
                KeyWords = "brown OR fox",
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Snippet.Contains("brown") && search.Results[0].Snippet.Contains("dog"));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_RelevanceAndTitleKeyword_Returns_TopHit_Docs_First()
        {
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchTitleBoostSetting, It.IsAny<int>())).Returns(CustomBoost);

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();
            this.CreateNewLuceneControllerInstance(true);

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Body.Contains("Hello1"));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_RelevanceAndSubjectKeyword_Returns_TopHit_Docs_First()
        {
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchContentBoostSetting, It.IsAny<int>())).Returns(CustomBoost);
            this.CreateNewLuceneControllerInstance(true);

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Body.Contains("Hello2"));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_RelevanceAndCommentKeyword_Returns_TopHit_Docs_First()
        {
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchDescriptionBoostSetting, It.IsAny<int>())).Returns(CustomBoost);
            this.CreateNewLuceneControllerInstance(true);

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Body.Contains("Hello3"));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_RelevanceAndAuthorKeyword_Returns_TopHit_Docs_First()
        {
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchAuthorBoostSetting, It.IsAny<int>())).Returns(CustomBoost);
            this.CreateNewLuceneControllerInstance(true);

            // Arrange
            var added = this.AddSearchDocsForCustomBoost();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Relevance,
                KeyWords = "Hello",
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Body.Contains("Hello4"));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Relevance_Ascending_Does_Not_Change_Sequence_Of_Results()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Relevance,
                SortDirection = SortDirections.Ascending,
                KeyWords = "brown OR fox",
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Snippet.Contains("brown") && search.Results[0].Snippet.Contains("dog"));
        }

        [Test]
        public void SearchController_GetResult_By_Locale_Returns_Specific_And_Neutral_Locales()
        {
            // Arrange
            this.AddStandardSearchDocs();

            // Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.LastModified,
                CultureCode = CultureItIt,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(2, search.Results.Count, "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));
            Assert.AreEqual(Line3, search.Results[0].Title);
            Assert.AreEqual(Line1, search.Results[1].Title);
        }

        [Test]
        public void SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_Content_With_Same_Key()
        {
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                };
            const string docKey = "key1";

            // Act

            // Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this._internalSearchController.AddSearchDocument(doc1);

            // Add second document with same key
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this._internalSearchController.AddSearchDocument(doc2);

            // run luceneQuery on common keyword between both the docs
            var search = this.SearchForKeyword("fox");

            // Assert - there should just be one entry - first one must have been removed.
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(docs[1], search.Results[0].Title);
        }

        [Test]
        public void SearchController_Add_Does_Not_Throw_On_Empty_Url()
        {
            var doc = new SearchDocument
            {
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = OtherSearchTypeId,
                Title = " ",
                ModifiedTimeUtc = DateTime.UtcNow,
                QueryString = "?foo=bar",
            };

            Assert.DoesNotThrow(() => this._internalSearchController.AddSearchDocument(doc));
        }

        [Test]
        public void SearchController_Add_Does_Not_Throw_On_Empty_Title()
        {
            var doc = new SearchDocument
            {
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
            };

            Assert.DoesNotThrow(() => this._internalSearchController.AddSearchDocument(doc));
        }

        [Test]
        public void SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_When_IsActive_Is_False()
        {
            // Arrange
            string[] docs =
            {
                Line1,
                Line2,
                };
            const string docKey = "key1";

            // Act

            // Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this._internalSearchController.AddSearchDocument(doc1);

            // Add second document with same key
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, IsActive = false };
            this._internalSearchController.AddSearchDocument(doc2);

            // run luceneQuery on common keyword between both the docs
            var search = this.SearchForKeyword("fox");

            // Assert - there should not be any record.
            Assert.AreEqual(0, search.Results.Count);
        }

        // Note: these tests needs to pass through the analyzer which is utilized
        //       in SearchControllerImpl but not LuceneControllerImpl.
        [Test]
        public void SearchController_SearchFindsAccentedAndNonAccentedWords()
        {
            // Arrange
            string[] lines =
            {
                "zèbre or panthère",
                "zebre without accent",
                "panthere without accent",
                };

            this.AddLinesAsSearchDocs(lines);

            // Act
            var searches1 = this.SearchForKeyword("zèbre");
            var searches2 = this.SearchForKeyword("zebre");

            // Assert
            Assert.AreEqual(2, searches1.TotalHits);
            Assert.AreEqual("<b>z&#232;bre</b> or panth&#232;re", this.StipEllipses(searches1.Results[0].Snippet).Trim());
            Assert.AreEqual("<b>zebre</b> without accent", this.StipEllipses(searches1.Results[1].Snippet).Trim());

            Assert.AreEqual(2, searches2.TotalHits);
            Assert.AreEqual("<b>z&#232;bre</b> or panth&#232;re", this.StipEllipses(searches2.Results[0].Snippet).Trim());
            Assert.AreEqual("<b>zebre</b> without accent", this.StipEllipses(searches2.Results[1].Snippet).Trim());
        }

        [Test]
        public void SearchController_PorterFilterTest()
        {
            // Arrange
            string[] lines =
            {
                "field1_value",
                "field2_value",
                };

            this.AddLinesAsSearchDocs(lines);

            // Act
            var search1 = this.SearchForKeyword(lines[0]);
            var search2 = this.SearchForKeyword("\"" + lines[1] + "\"");

            // Assert
            Assert.AreEqual(1, search1.TotalHits);
            Assert.AreEqual(1, search2.TotalHits);

            Assert.AreEqual("<b>" + lines[0] + "</b>", this.StipEllipses(search1.Results[0].Snippet).Trim());
            Assert.AreEqual("<b>" + lines[1] + "</b>", this.StipEllipses(search2.Results[0].Snippet).Trim());
        }

        [Test]
        public void SearchController_SearchFindsStemmedWords()
        {
            // Arrange
            string[] lines =
            {
                "I ride my bike to work",
                "All team are riding their bikes",
                "The boy rides his bike to school",
                "This sentence is missing the bike ri... word",
                };

            this.AddLinesAsSearchDocs(lines);

            // Act
            var search = this.SearchForKeyword("ride");

            // Assert
            Assert.AreEqual(3, search.TotalHits);
            Assert.AreEqual("I <b>ride</b> my bike to work", this.StipEllipses(search.Results[0].Snippet));
            Assert.AreEqual("m are <b>riding</b> their bikes", this.StipEllipses(search.Results[1].Snippet));
            Assert.AreEqual("e boy <b>rides</b> his bike to school", this.StipEllipses(search.Results[2].Snippet));
        }

        [Test]
        public void SearchController_Search_Synonym_Works()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();

            // Act
            var search = this.SearchForKeywordInModule("wolf");

            // Assert
            Assert.AreEqual(added, search.TotalHits);

            var snippets = search.Results.Select(result => this.StipEllipses(result.Snippet)).OrderBy(s => s).ToArray();
            Assert.AreEqual("brown <b>fox</b> jumps over the lazy dog", snippets[0]);
            Assert.AreEqual("e red <b>fox</b> jumped over the lazy dark gray dog", snippets[1]);
            Assert.AreEqual("gold <b>fox</b> jumped over the lazy black dog", snippets[2]);
            Assert.AreEqual("quick <b>fox</b> jumps over the black dog - Italian", snippets[3]);
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Body()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "Hello World" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "cow is gone" };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.Commit();

            var result = this.SearchForKeyword("cow");

            // Assert
            Assert.AreEqual(result.TotalHits, 2);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "cow is gone" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.Commit();

            var result = this.SearchForKeyword("cow");

            // Assert
            Assert.AreEqual(result.TotalHits, 2);
            Assert.AreEqual(doc3.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc1.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Tag()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "Hello World" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { "cow", "hello", "world" } };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.Commit();
            var result = this.SearchForKeyword("cow");

            // Assert
            Assert.AreEqual(result.TotalHits, 2);
            Console.WriteLine("first score: {0}  {1}", result.Results[0].UniqueKey, result.Results[0].DisplayScore);
            Console.WriteLine("second score: {0}  {1}", result.Results[1].UniqueKey, result.Results[1].DisplayScore);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_RankingTest_With_Vowel()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "tootsie", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { "tootsie" } };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Keywords = new Dictionary<string, string>() { { KeyWord1Name, "tootsie" } } };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Description = "tootsie" };
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "hello tootsie" };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.AddSearchDocument(doc4);
            this._internalSearchController.AddSearchDocument(doc5);

            this._internalSearchController.Commit();

            var result = this.SearchForKeyword("tootsie");

            // Assert
            Assert.AreEqual(5, result.TotalHits);
            foreach (var searchResult in result.Results)
            {
                Console.WriteLine("{0} score: {1}", searchResult.UniqueKey, searchResult.DisplayScore);
            }

            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
            Assert.AreEqual(doc3.UniqueKey, result.Results[2].UniqueKey);
            Assert.AreEqual(doc4.UniqueKey, result.Results[3].UniqueKey);
        }

        [Test]
        public void SearchController_FileNameTest_With_WildCard()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);

            this._internalSearchController.Commit();

            var result = this.SearchForKeywordWithWildCard("file");

            // Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        public void SearchController_Full_FileNameTest_Without_WildCard()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);

            this._internalSearchController.Commit();

            var result = this.SearchForKeywordWithWildCard("file.ext");

            // Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        public void SearchController_Full_FileNameTest_With_WildCard()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);

            this._internalSearchController.Commit();

            var result = this.SearchForKeyword("file.ext");

            // Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        public void SearchController_Scope_By_FolderName()
        {
            // Arrange
            this.AddFoldersAndFiles();

            // Act
            var result1 = this.SearchForKeyword("kw-folderName:Images/*");
            var result2 = this.SearchForKeyword("kw-folderName:Images/DNN/*");
            var result3 = this.SearchForKeywordWithWildCard("kw-folderName:Images/* AND spacer");

            // Assert
            Assert.AreEqual(5, result1.TotalHits);
            Assert.AreEqual(2, result2.TotalHits);
            Assert.AreEqual(1, result3.TotalHits);
        }

        [Test]
        public void SearchController_Scope_By_FolderName_With_Spaces()
        {
            // Arrange
            this.AddFoldersAndFiles();

            // Act - Space is replaced by <
            var query1 = new SearchQuery { KeyWords = "kw-folderName:Images/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = false };
            var query2 = new SearchQuery { KeyWords = "kw-folderName:my<Images/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = true };
            var query3 = new SearchQuery { KeyWords = "kw-folderName:my<Images/my<dnn/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = true };
            var result1 = this._searchController.SiteSearch(query1);
            var result2 = this._searchController.SiteSearch(query2);
            var result3 = this._searchController.SiteSearch(query3);

            // Assert
            Assert.AreEqual(5, result1.TotalHits);
            Assert.AreEqual(5, result2.TotalHits);
            Assert.AreEqual(2, result3.TotalHits);
        }

        [Test]
        public void SearchController_EmailTest_With_WildCard()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "email@domain.com", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);

            this._internalSearchController.Commit();

            var result = this.SearchForKeywordWithWildCard("email@");

            // Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleDefinitionId()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleId()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleDefinitionId()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleId()
        {
            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleId = HtmlModuleId };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Search_For_ModuleId_Must_Have_Only_One_Search_Type_Id_Specified()
        {
            // Arrange
            const string keyword = "awesome";

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { DocumentSearchTypeId, OtherSearchTypeId }, ModuleId = IdeasModuleId };

            // Act
            this._searchController.SiteSearch(query);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Search_For_ModuleId_Must_Have_Only_Module_Search_Type_Id_Specified()
        {
            // Arrange
            const string keyword = "awesome";

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, OtherSearchTypeId }, ModuleId = IdeasModuleId };

            // Act
            this._searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_Search_For_Unknown_SearchTypeId_Does_Not_Throw_Exception()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = UnknownSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            this._internalSearchController.AddSearchDocument(doc1);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { UnknownSearchTypeId } };

            // Act
            var result = this._searchController.SiteSearch(query);
            Assert.AreEqual(0, result.TotalHits); // 0 due to security trimming
            Assert.AreEqual(0, result.Results.Count);
        }

        [Test]
        public void SearchController_Search_For_GroupId_Zero_Ignores_GroupId()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId0 };
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, RoleId = 0 };

            // Act
            var result = this._searchController.SiteSearch(query);
            Assert.AreEqual(2, result.TotalHits);
        }

        [Test]
        public void SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId532 };
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, RoleId = RoleId731 };

            // Act
            var result = this._searchController.SiteSearch(query);
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(RoleId731, result.Results[0].RoleId);
        }

        [Test]
        public void SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId532 };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };

            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId, ModuleSearchTypeId }, RoleId = RoleId731 };

            // Act
            var result = this._searchController.SiteSearch(query);
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(RoleId731, result.Results[0].RoleId);
        }

        [Test]
        public void SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(2, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_Search_For_ModuleId_Returns_from_that_module_Only()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.AddSearchDocument(doc4);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId }, ModuleId = IdeasModuleId };

            var result = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(2, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.AddSearchDocument(doc4);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(3, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
            Assert.AreEqual(doc3.UniqueKey, result.Results[2].UniqueKey);
        }

        [Test]
        public void SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.AddSearchDocument(doc4);
            this._internalSearchController.AddSearchDocument(doc5);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, OtherSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(4, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
            Assert.AreEqual(doc3.UniqueKey, result.Results[2].UniqueKey);
            Assert.AreEqual(doc5.UniqueKey, result.Results[3].UniqueKey);
        }

        [Test]
        public void SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results()
        {
            // Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId, ModuleId = HtmlModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId, ModuleId = HtmlModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = keyword, SearchTypeId = TabSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.AddSearchDocument(doc4);
            this._internalSearchController.AddSearchDocument(doc5);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, TabSearchTypeId }, ModuleDefIds = new[] { HtmlModuleDefId } };

            var result = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(3, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
            Assert.AreEqual(doc5.UniqueKey, result.Results[2].UniqueKey);
        }

        [Test]
        public void SearchController_GetResult_Works_With_Custom_Numeric_Querirs()
        {
            this.AddDocumentsWithNumericKeys();

            // Act
            var query = new SearchQuery
            {
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue50 } },
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                WildCardSearch = false,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(NumericValue50, search.Results[0].NumericKeys[NumericKey1]);
        }

        [Test]
        public void SearchController_GetResult_Works_With_CustomKeyword_Querirs()
        {
            this.AddDocumentsWithKeywords();

            // Act
            var query = new SearchQuery
            {
                CustomKeywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord1Value } },
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                WildCardSearch = false,
            };
            var search = this._searchController.SiteSearch(query);

            // Assert
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(KeyWord1Value, search.Results[0].Keywords[KeyWord1Name]);
        }

        [Test]
        public void SearchController_EnableLeadingWildcard_Should_Not_Return_Results_When_Property_Is_False()
        {
            this._mockHostController.Setup(c => c.GetString("Search_AllowLeadingWildcard", It.IsAny<string>())).Returns("N");

            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.Commit();

            var result = this.SearchForKeyword("rld", OtherSearchTypeId, true, false);

            // Assert
            Assert.AreEqual(0, result.TotalHits);
        }

        [Test]
        public void SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_True()
        {
            this._mockHostController.Setup(c => c.GetString("Search_AllowLeadingWildcard", It.IsAny<string>())).Returns("N");

            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.Commit();

            var result = this.SearchForKeyword("rld", OtherSearchTypeId, true, true);

            // Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc2.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        public void SearchController_EnableLeadingWildcard_Should_Return_Results_When_Property_Is_False_But_Host_Setting_Is_True()
        {
            this._mockHostController.Setup(c => c.GetString("Search_AllowLeadingWildcard", It.IsAny<string>())).Returns("Y");

            // Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = string.Empty };

            // Act
            this._internalSearchController.AddSearchDocument(doc1);
            this._internalSearchController.AddSearchDocument(doc2);
            this._internalSearchController.AddSearchDocument(doc3);
            this._internalSearchController.Commit();

            var result = this.SearchForKeyword("rld", OtherSearchTypeId, true, false);

            // Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc2.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        public void SearchController_Search_StopWords_Works()
        {
            // Arrange
            var added = this.AddStandardSearchDocs();
            this._internalSearchController.Commit();

            // Act
            var search = this.SearchForKeywordInModule("the");

            // Assert
            // the word "the" is ignored in all languages except es-ES
            Assert.AreEqual(1, search.TotalHits, "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));

            // Act
            search = this.SearchForKeywordInModule("over");

            // Assert
            // we won't find "over" in neutral, en-US, and en-CA documents, but will find it in the es-ES and it-IT documents.
            Assert.AreEqual(2, search.TotalHits, "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));

            // Act
            search = this.SearchForKeywordInModule("los");

            // Assert
            // we won't find "los" in the es-ES document.
            Assert.AreEqual(0, search.TotalHits, "Found: " + string.Join(Environment.NewLine, search.Results.Select(r => r.Title)));
        }

        /// <summary>
        /// Executes function proc on a separate thread respecting the given timeout value.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="proc">The function to execute.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <returns>R.</returns>
        /// <remarks>From: http://stackoverflow.com/questions/9460661/implementing-regex-timeout-in-net-4.</remarks>
        private static R ExecuteWithTimeout<R>(Func<R> proc, TimeSpan timeout)
        {
            var r = default(R); // init default return value
            Exception ex = null; // records inter-thread exception

            // define a thread to wrap 'proc'
            var t = new Thread(() =>
            {
                try
                {
                    r = proc();
                }
                catch (Exception e)
                {
                    // this can get set to ThreadAbortException
                    ex = e;

                    Console.WriteLine("Exception hit");
                }
            });

            t.Start(); // start running 'proc' thread wrapper

            // from docs: "The Start method does not return until the new thread has started running."
            if (t.Join(timeout) == false)
            {
                t.Abort(); // die evil thread!

                // Abort raises the ThreadAbortException
                int i = 0;
                while ((t.Join(1) == false) && (i < 20))
                {
                    // 20 ms wait possible here
                    i++;
                }

                if (i >= 20)
                {
                    // we didn't abort, might want to log this or take some other action
                    // this can happen if you are doing something indefinitely hinky in a
                    // finally block (cause the finally be will executed before the Abort
                    // completes.
                    Console.WriteLine("Abort didn't work as expected");
                }
            }

            if (ex != null)
            {
                throw ex; // oops
            }

            return r; // ah!
        }

        private void CreateNewLuceneControllerInstance(bool reCreate = false)
        {
            InternalSearchController.SetTestableInstance(new InternalSearchControllerImpl());
            this._internalSearchController = InternalSearchController.Instance;
            this._searchController = new SearchControllerImpl();

            if (!reCreate)
            {
                this.DeleteIndexFolder();

                if (this._luceneController != null)
                {
                    LuceneController.ClearInstance();
                    this._luceneController.Dispose();
                }

                this._luceneController = new LuceneControllerImpl();
                LuceneController.SetTestableInstance(this._luceneController);
            }
        }

        private void SetupHostController()
        {
            this._mockHostController.Setup(c => c.GetString(Constants.SearchIndexFolderKey, It.IsAny<string>())).Returns(SearchIndexFolder);
            this._mockHostController.Setup(c => c.GetDouble(Constants.SearchReaderRefreshTimeKey, It.IsAny<double>())).Returns(this._readerStaleTimeSpan);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchTitleBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchTitleBoost);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchTagBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchTagBoost);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchContentBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchKeywordBoost);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchDescriptionBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchDescriptionBoost);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchAuthorBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchAuthorBoost);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchMinLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMinLen);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchMaxLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMaxLen);
            this._mockHostController.Setup(c => c.GetInteger(Constants.SearchRetryTimesKey, It.IsAny<int>())).Returns(DefaultSearchRetryTimes);
            HostController.RegisterInstance(this._mockHostController.Object);
        }

        private void SetupLocaleController()
        {
            this._mockLocaleController.Setup(l => l.GetLocale(It.IsAny<string>())).Returns(new Locale { LanguageId = -1, Code = string.Empty });
            this._mockLocaleController.Setup(l => l.GetLocale(CultureEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            this._mockLocaleController.Setup(l => l.GetLocale(CultureEnCa)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa });
            this._mockLocaleController.Setup(l => l.GetLocale(CultureItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt });
            this._mockLocaleController.Setup(l => l.GetLocale(CultureEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs });

            this._mockLocaleController.Setup(l => l.GetLocale(It.IsAny<int>())).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            this._mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            this._mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnFr)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa });
            this._mockLocaleController.Setup(l => l.GetLocale(LanguageIdItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt });
            this._mockLocaleController.Setup(l => l.GetLocale(LanguageIdEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs });
        }

        private void SetupDataProvider()
        {
            // Standard DataProvider Path for Logging
            this._mockDataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);

            DataTableReader searchTypes = null;
            this._mockDataProvider.Setup(ds => ds.GetAllSearchTypes())
                     .Callback(() => searchTypes = this.GetAllSearchTypes().CreateDataReader())
                     .Returns(() => searchTypes);

            this._mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(this.GetPortalsCallBack);
        }

        private IDataReader GetPortalsCallBack(string culture)
        {
            return this.GetPortalCallBack(PortalId0, CultureEnUs);
        }

        private IDataReader GetPortalCallBack(int portalId, string culture)
        {
            var table = new DataTable("Portal");

            var cols = new[]
                        {
                            "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate", "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
                            "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId", "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor", "ProcessorUserId",
                            "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset", "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
                            "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode",
                        };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null,
                    "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null,
                    "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0",
                    null, homePage.ToString("D"), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }

        private void SetupSearchHelper()
        {
            this._mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            this._mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(this.GetSynonymsCallBack);
            this._mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns((string name) => new SearchType { SearchTypeId = 0, SearchTypeName = name });
            this._mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns<string>(this.GetSearchTypeByNameCallback);
            this._mockSearchHelper.Setup(x => x.GetSearchTypes()).Returns(this.GetSearchTypes());
            this._mockSearchHelper.Setup(x => x.GetSearchStopWords(It.IsAny<int>(), It.IsAny<string>())).Returns(new SearchStopWords());
            this._mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEsEs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEsEs,
                    StopWords = "los,de,el",
                });
            this._mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEnUs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEnUs,
                    StopWords = "the,over",
                });
            this._mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEnCa)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEnCa,
                    StopWords = "the,over",
                });

            this._mockSearchHelper.Setup(x => x.RephraseSearchText(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns<string, bool, bool>(new SearchHelperImpl().RephraseSearchText);
            this._mockSearchHelper.Setup(x => x.StripTagsNoAttributes(It.IsAny<string>(), It.IsAny<bool>())).Returns((string html, bool retainSpace) => html);
            SearchHelper.SetTestableInstance(this._mockSearchHelper.Object);
        }

        private SearchType GetSearchTypeByNameCallback(string searchTypeName)
        {
            var searchType = new SearchType { SearchTypeName = searchTypeName, SearchTypeId = 0 };
            switch (searchTypeName)
            {
                case ModuleSearchTypeName:
                    searchType.SearchTypeId = ModuleSearchTypeId;
                    break;
                case TabSearchTypeName:
                    searchType.SearchTypeId = TabSearchTypeId;
                    break;
                case OtherSearchTypeName:
                    searchType.SearchTypeId = OtherSearchTypeId;
                    break;
                case DocumentSearchTypeName:
                    searchType.SearchTypeId = DocumentSearchTypeId;
                    break;
                case UrlSearchTypeName:
                    searchType.SearchTypeId = UrlSearchTypeId;
                    break;
            }

            return searchType;
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

        private UserInfo GetUserByIdCallback(int portalId, int userId)
        {
            if (portalId == PortalId12 && userId == StandardAuthorId)
            {
                return new UserInfo { UserID = userId, DisplayName = StandardAuthorDisplayName };
            }

            return null;
        }

        private DataTable GetAllSearchTypes()
        {
            var dtSearchTypes = new DataTable("SearchTypes");
            var pkId = dtSearchTypes.Columns.Add("SearchTypeId", typeof(int));
            dtSearchTypes.Columns.Add("SearchTypeName", typeof(string));
            dtSearchTypes.Columns.Add("SearchResultClass", typeof(string));
            dtSearchTypes.PrimaryKey = new[] { pkId };

            // Create default Crawler
            dtSearchTypes.Rows.Add(ModuleSearchTypeId, ModuleSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(TabSearchTypeId, TabSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(OtherSearchTypeId, OtherSearchTypeName, FakeResultControllerClass);
            dtSearchTypes.Rows.Add(DocumentSearchTypeId, DocumentSearchTypeName, NoPermissionFakeResultControllerClass);
            dtSearchTypes.Rows.Add(UrlSearchTypeId, UrlSearchTypeName, FakeResultControllerClass);

            return dtSearchTypes;
        }

        private IEnumerable<SearchType> GetSearchTypes()
        {
            var searchTypes = new List<SearchType>
                {
                    new SearchType { SearchTypeId = ModuleSearchTypeId, SearchTypeName = ModuleSearchTypeName, SearchResultClass = FakeResultControllerClass },
                    new SearchType { SearchTypeId = TabSearchTypeId, SearchTypeName = TabSearchTypeName, SearchResultClass = FakeResultControllerClass },
                    new SearchType { SearchTypeId = OtherSearchTypeId, SearchTypeName = OtherSearchTypeName, SearchResultClass = FakeResultControllerClass },
                    new SearchType { SearchTypeId = DocumentSearchTypeId, SearchTypeName = DocumentSearchTypeName, SearchResultClass = NoPermissionFakeResultControllerClass },
                    new SearchType { SearchTypeId = UrlSearchTypeId, SearchTypeName = UrlSearchTypeName, SearchResultClass = FakeResultControllerClass },
                };

            return searchTypes;
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
        /// Returns few SearchDocs.
        /// </summary>
        private IEnumerable<SearchDocument> GetStandardSearchDocs(int searchTypeId = ModuleSearchTypeId)
        {
            var searchDocs = new List<SearchDocument>
            {
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag0, Tag1, TagOldest, Tag0WithSpace }, Title = Line1 },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag1, Tag2, TagNeutral }, Title = Line2, CultureCode = CultureEnUs },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag2, Tag3, TagIt }, Title = Line3, CultureCode = CultureItIt },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag3, Tag4, TagLatest }, Title = Line4, CultureCode = CultureEnCa },
                new SearchDocument { PortalId = PortalId0, Tags = new List<string> { Tag2, Tag3, TagIt }, Title = Line5, CultureCode = CultureEsEs },
            };

            var now = DateTime.UtcNow.AddYears(-searchDocs.Count);
            var i = 0;

            foreach (var searchDocument in searchDocs)
            {
                searchDocument.SearchTypeId = searchTypeId;
                searchDocument.UniqueKey = Guid.NewGuid().ToString();
                searchDocument.ModuleId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleId : -1;
                searchDocument.ModuleDefId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleDefId : -1;
                searchDocument.ModifiedTimeUtc = now.AddYears(++i); // last added is the newest
            }

            return searchDocs;
        }

        private IEnumerable<SearchDocument> GetSearchDocsForCustomBoost(int searchTypeId = ModuleSearchTypeId)
        {
            var searchDocs = new List<SearchDocument>
            {
                new SearchDocument { PortalId = PortalId0, Title = Line1, Keywords = { { "title", "Hello" } }, Body = "Hello1 World" },
                new SearchDocument { PortalId = PortalId0, Title = Line2, Keywords = { { "subject", "Hello" } }, Body = "Hello2 World" },
                new SearchDocument { PortalId = PortalId0, Title = Line3, Keywords = { { "comments", "Hello" } }, Body = "Hello3 World" },
                new SearchDocument { PortalId = PortalId0, Title = Line4, Keywords = { { "authorname", "Hello" } }, Body = "Hello4 World" },
            };

            var now = DateTime.UtcNow.AddYears(-searchDocs.Count);
            var i = 0;

            foreach (var searchDocument in searchDocs)
            {
                searchDocument.SearchTypeId = searchTypeId;
                searchDocument.UniqueKey = Guid.NewGuid().ToString();
                searchDocument.ModuleId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleId : -1;
                searchDocument.ModuleDefId = (searchTypeId == ModuleSearchTypeId) ? HtmlModuleDefId : -1;
                searchDocument.ModifiedTimeUtc = now.AddYears(++i); // last added is the newest
            }

            return searchDocs;
        }

        /// <summary>
        /// Adds standarad SearchDocs in Lucene Index.
        /// </summary>
        /// <returns>Number of dcuments added.</returns>
        private int AddStandardSearchDocs(int searchTypeId = ModuleSearchTypeId)
        {
            var docs = this.GetStandardSearchDocs(searchTypeId).ToArray();
            this._internalSearchController.AddSearchDocuments(docs);
            return docs.Length;
        }

        private int AddSearchDocsForCustomBoost(int searchTypeId = ModuleSearchTypeId)
        {
            var docs = this.GetSearchDocsForCustomBoost(searchTypeId).ToArray();
            this._internalSearchController.AddSearchDocuments(docs);
            return docs.Length;
        }

        private int AddDocumentsWithNumericKeys(int searchTypeId = OtherSearchTypeId)
        {
            var doc1 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key1",
                Body = "hello",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue50 } },
            };
            var doc2 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key2",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue100 } },
            };
            var doc3 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key3",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue200 } },
            };
            var doc4 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key4",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue500 } },
            };
            var doc5 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key5",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue1000 } },
            };

            var docs = new List<SearchDocument>() { doc1, doc2, doc3, doc4, doc5 };

            this._internalSearchController.AddSearchDocuments(docs);

            return docs.Count;
        }

        private int AddDocumentsWithKeywords(int searchTypeId = OtherSearchTypeId)
        {
            var doc1 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key1",
                Body = "hello",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord1Value } },
            };
            var doc2 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key2",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord2Value } },
            };
            var doc3 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key3",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord3Value } },
            };
            var doc4 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key4",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord4Value } },
            };
            var doc5 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key5",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, KeyWord5Value } },
            };

            var docs = new List<SearchDocument>() { doc1, doc2, doc3, doc4, doc5 };

            this._internalSearchController.AddSearchDocuments(docs);

            return docs.Count;
        }

        private int AddDocuments(IList<string> titles, string body, int searchTypeId = OtherSearchTypeId)
        {
            var count = 0;
            foreach (var doc in titles.Select(title => new SearchDocument
            {
                Title = title,
                UniqueKey = Guid.NewGuid().ToString(),
                Body = body,
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
            }))
            {
                this._internalSearchController.AddSearchDocument(doc);
                count++;
            }

            return count;
        }

        private int AddDocumentsWithKeywords(IEnumerable<string> keywords, string title, int searchTypeId = OtherSearchTypeId)
        {
            var count = 0;
            foreach (var doc in keywords.Select(keyword => new SearchDocument
            {
                Title = title,
                UniqueKey = Guid.NewGuid().ToString(),
                Keywords = new Dictionary<string, string>() { { KeyWord1Name, keyword } },
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
            }))
            {
                this._internalSearchController.AddSearchDocument(doc);
                count++;
            }

            return count;
        }

        private void AddLinesAsSearchDocs(IList<string> lines, int searchTypeId = OtherSearchTypeId)
        {
            var now = DateTime.UtcNow - TimeSpan.FromSeconds(lines.Count());
            var i = 0;

            this._internalSearchController.AddSearchDocuments(
                lines.Select(line =>
                    new SearchDocument
                    {
                        Title = line,
                        UniqueKey = Guid.NewGuid().ToString(),
                        SearchTypeId = searchTypeId,
                        ModifiedTimeUtc = now.AddSeconds(i++),
                    }).ToList());
        }

        private SearchResults SearchForKeyword(string keyword, int searchTypeId = OtherSearchTypeId, bool useWildcard = false, bool allowLeadingWildcard = false)
        {
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { searchTypeId }, WildCardSearch = useWildcard, AllowLeadingWildcard = allowLeadingWildcard };
            return this._searchController.SiteSearch(query);
        }

        private SearchResults SearchForKeywordWithWildCard(string keyword, int searchTypeId = OtherSearchTypeId)
        {
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { searchTypeId }, WildCardSearch = true };
            return this._searchController.SiteSearch(query);
        }

        private SearchResults SearchForKeywordInModule(string keyword, int searchTypeId = ModuleSearchTypeId)
        {
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { searchTypeId } };
            return this._searchController.SiteSearch(query);
        }

        private string StipEllipses(string text)
        {
            return text.Replace("...", string.Empty).Trim();
        }

        /// <summary>
        /// Sets up some data for testing security trimming.
        /// In the tests below, the users will have access to the follwoing documents
        /// { 6, 7, 8, 9, 16, 17, 18, 19, 26, 27, 28, 29, ..., etc. }
        /// The tests check that pagination qith various page sizes returns the proper groupings.
        /// </summary>
        private void SetupSecurityTrimmingDocs(int totalDocs, int searchType = DocumentSearchTypeId)
        {
            var docModifyTime = DateTime.UtcNow - TimeSpan.FromSeconds(totalDocs);
            for (var i = 0; i < totalDocs; i++)
            {
                this._internalSearchController.AddSearchDocument(new SearchDocument
                {
                    AuthorUserId = i,
                    Title = "Fox and Dog",
                    Body = Line1,
                    Tags = new[] { Tag0, Tag1 },
                    SearchTypeId = searchType,
                    UniqueKey = Guid.NewGuid().ToString(),
                    ModifiedTimeUtc = docModifyTime.AddSeconds(i),
                });
            }
        }

        private void AddFoldersAndFiles()
        {
            var allFiles = new Dictionary<string, string>
                               {
                                { "Awesome-Cycles-Logo.png", "Images/" },
                                { "Banner1.jpg", "Images/" },
                                { "Banner2.jpg", "Images/" },
                                { "bike-powered.png", "Images/DNN/" },
                                { "Spacer.gif", "Images/DNN/" },
                                { "monday.png", "My<Images/" },
                                { "tuesday.jpg", "My<Images/" },
                                { "wednesday.jpg", "My<Images/" },
                                { "thursday.png", "My<Images/My<DNN/" },
                                { "friday.gif", "My<Images/My<DNN/" },
                               };

            foreach (var file in allFiles)
            {
                var doc = new SearchDocument
                {
                    Title = file.Key,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = OtherSearchTypeId,
                    ModifiedTimeUtc = DateTime.UtcNow,
                    Keywords = new Dictionary<string, string> { { "folderName", file.Value.ToLowerInvariant() } },
                };
                this._internalSearchController.AddSearchDocument(doc);
            }

            this._internalSearchController.Commit();
        }
    }
}
