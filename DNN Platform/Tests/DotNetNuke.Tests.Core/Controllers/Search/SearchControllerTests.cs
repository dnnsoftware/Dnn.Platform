#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Internal;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search.Controllers;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;
using Constants = DotNetNuke.Services.Search.Internals.Constants;
using DotNetNuke.Entities.Controllers;

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    /// <summary>
    ///  Testing various aspects of SearchController
    /// </summary>
    [TestFixture]
    public class SearchControllerTests
    {

        #region Constants
        private const int ModuleSearchTypeId = 1;
        private const int TabSearchTypeId = 2;
        private const int DocumentSearchTypeId = 3;
        private const int UrlSearchTypeId = 4;
        private const int OtherSearchTypeId = 5;
        private const int UnknownSearchTypeId = 6;
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
        private const int NumericValue200 = 200;
        private const int NumericValue1000 = 1000;
        private const string KeyWord1Name = "keyword1";
        private const string KeyWord1Value = "value1";
        private const string KeyWord2Name = "keyword2";
        private const string KeyWord2Value = "value2";
        private const string Line1 = "the quick brown fox jumps over the lazy dog";
        private const string Line2 = "the quick gold fox jumped over the lazy black dog";
        private const string Line3 = "the quick fox jumps over the black dog";
        private const string Line4 = "the red fox jumped over the lazy dark gray dog";

        private const string SearchIndexFolder = @"App_Data\SearchTests";
        private readonly double _readerStaleTimeSpan = TimeSpan.FromMilliseconds(100).TotalSeconds;
        #endregion

        #region Private Properties

        private Mock<IHostController> _mockHostController;
        private Mock<CachingProvider> _mockCachingProvider;
        private Mock<DataProvider> _mockDataProvider;
        private Mock<ILocaleController> _mockLocaleController;
        private Mock<ISearchHelper> _mockSearchHelper;
        private Mock<IUserController> _mockUserController;

        private SearchControllerImpl _searchController;
        private IInternalSearchController _internalSearchController;
        private LuceneControllerImpl _luceneController;

        #endregion

        #region Set Up

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.ResetContainer();

            _mockDataProvider = MockComponentProvider.CreateDataProvider();
            _mockLocaleController = MockComponentProvider.CreateLocaleController();
            _mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();

            _mockUserController = new Mock<IUserController>();
            _mockHostController = new Mock<IHostController>();
            _mockSearchHelper = new Mock<ISearchHelper>();

            SetupDataProvider();
            SetupHostController();
            SetupSearchHelper();
            SetupLocaleController();

            _mockUserController.Setup(c => c.GetUserById(It.IsAny<int>(), It.IsAny<int>())).Returns((int portalId, int userId) => GetUserByIdCallback(portalId, userId));
            TestableUserController.SetTestableInstance(_mockUserController.Object);

            DeleteIndexFolder();
            InternalSearchController.SetTestableInstance(new InternalSearchControllerImpl());

            _internalSearchController = InternalSearchController.Instance;
            _searchController = new SearchControllerImpl();
            CreateNewLuceneControllerInstance();
        }

        [TearDown]
        public void TearDown()
        {
            _luceneController.Dispose();
            DeleteIndexFolder();
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

        private void SetupHostController()
        {
            _mockHostController.Setup(c => c.GetString(Constants.SearchIndexFolderKey, It.IsAny<string>())).Returns(SearchIndexFolder);
            _mockHostController.Setup(c => c.GetDouble(Constants.SearchReaderRefreshTimeKey, It.IsAny<double>())).Returns(_readerStaleTimeSpan);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchTitleBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchTitleBoost);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchTagBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchTagBoost);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchContentBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchKeywordBoost);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchDescriptionBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchDescriptionBoost);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchAuthorBoostSetting, It.IsAny<int>())).Returns(Constants.DefaultSearchAuthorBoost);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchMinLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMinLen);
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchMaxLengthKey, It.IsAny<int>())).Returns(Constants.DefaultMaxLen);
            HostController.RegisterInstance(_mockHostController.Object);
        }

        private void SetupLocaleController()
        {
            _mockLocaleController.Setup(l => l.GetLocale(It.IsAny<string>())).Returns(new Locale { LanguageId = Null.NullInteger, Code = string.Empty });
            _mockLocaleController.Setup(l => l.GetLocale(CultureEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            _mockLocaleController.Setup(l => l.GetLocale(CultureEnCa)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa });
            _mockLocaleController.Setup(l => l.GetLocale(CultureItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt });
            _mockLocaleController.Setup(l => l.GetLocale(CultureEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs });

            _mockLocaleController.Setup(l => l.GetLocale(It.IsAny<int>())).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            _mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnUs)).Returns(new Locale { LanguageId = LanguageIdEnUs, Code = CultureEnUs });
            _mockLocaleController.Setup(l => l.GetLocale(LanguageIdEnFr)).Returns(new Locale { LanguageId = LanguageIdEnFr, Code = CultureEnCa });
            _mockLocaleController.Setup(l => l.GetLocale(LanguageIdItIt)).Returns(new Locale { LanguageId = LanguageIdItIt, Code = CultureItIt });
            _mockLocaleController.Setup(l => l.GetLocale(LanguageIdEsEs)).Returns(new Locale { LanguageId = LanguageIdEsEs, Code = CultureEsEs });
        }

        private void SetupDataProvider()
        {
            //Standard DataProvider Path for Logging
            _mockDataProvider.Setup(d => d.GetProviderPath()).Returns("");

            DataTableReader searchTypes = null;
            _mockDataProvider.Setup(ds => ds.GetAllSearchTypes())
                     .Callback(() => searchTypes = GetAllSearchTypes().CreateDataReader())
                     .Returns(() => searchTypes);

            _mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);
        }

        private IDataReader GetPortalsCallBack(string culture)
        {
            return GetPortalCallBack(PortalId0, CultureEnUs);
        }

        private IDataReader GetPortalCallBack(int portalId, string culture)
        {
            DataTable table = new DataTable("Portal");

            var cols = new string[]
			           	{
			           		"PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate", "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
			           		"HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId", "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor", "ProcessorUserId",
			           		"ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset", "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
			           		"UserTabId", "SearchTabId", "SuperTabId", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode"
			           	};

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            var homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null, "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@change.me", "en-US", "-8", "58", "Portals/0", null, homePage.ToString(), null, null, "57", "56", "7", "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }

        private void SetupSearchHelper()
        {
            _mockSearchHelper.Setup(c => c.GetSearchMinMaxLength()).Returns(new Tuple<int, int>(Constants.DefaultMinLen, Constants.DefaultMaxLen));
            _mockSearchHelper.Setup(c => c.GetSynonyms(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns<int, string, string>(GetSynonymsCallBack);
            _mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>()))
                              .Returns((string name) => new SearchType { SearchTypeId = 0, SearchTypeName = name });
            _mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>())).Returns<string>(GetSearchTypeByNameCallback);
            _mockSearchHelper.Setup(x => x.GetSearchTypes()).Returns(GetSearchTypes());
            /*_mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEnUs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEnUs,
                    StopWords = "the,over",
                });*/
            _mockSearchHelper.Setup(x => x.GetSearchStopWords(0, CultureEsEs)).Returns(
                new SearchStopWords
                {
                    PortalId = 0,
                    CultureCode = CultureEsEs,
                    StopWords = "los,de,el",
                });
            _mockSearchHelper.Setup(x => x.RephraseSearchText(It.IsAny<string>(), It.IsAny<bool>())).Returns<string, bool>(new SearchHelperImpl().RephraseSearchText);
            SearchHelper.SetTestableInstance(_mockSearchHelper.Object);
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
                synonyms.Add("wolf");

            return synonyms;
        }

        private UserInfo GetUserByIdCallback(int portalId, int userId)
        {
            if (portalId == PortalId12 && userId == StandardAuthorId)
                return new UserInfo { UserID = userId, DisplayName = StandardAuthorDisplayName };

            return null;
        }

        private DataTable GetAllSearchTypes()
        {
            var dtSearchTypes = new DataTable("SearchTypes");
            var pkId = dtSearchTypes.Columns.Add("SearchTypeId", typeof(int));
            dtSearchTypes.Columns.Add("SearchTypeName", typeof(string));
            dtSearchTypes.Columns.Add("SearchResultClass", typeof(string));
            dtSearchTypes.PrimaryKey = new[] { pkId };

            //Create default Crawler
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
                    new SearchType {SearchTypeId = ModuleSearchTypeId, SearchTypeName = ModuleSearchTypeName, SearchResultClass = FakeResultControllerClass},
                    new SearchType {SearchTypeId = TabSearchTypeId, SearchTypeName = TabSearchTypeName, SearchResultClass = FakeResultControllerClass},
                    new SearchType {SearchTypeId = OtherSearchTypeId, SearchTypeName = OtherSearchTypeName, SearchResultClass = FakeResultControllerClass},
                    new SearchType {SearchTypeId = DocumentSearchTypeId, SearchTypeName = DocumentSearchTypeName, SearchResultClass = NoPermissionFakeResultControllerClass},
                    new SearchType {SearchTypeId = UrlSearchTypeId, SearchTypeName = UrlSearchTypeName, SearchResultClass = FakeResultControllerClass}
                };

            return searchTypes;
        }

        private void DeleteIndexFolder()
        {
            try
            {
                if (Directory.Exists(SearchIndexFolder))
                    Directory.Delete(SearchIndexFolder, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Returns few SearchDocs
        /// </summary>
        private IEnumerable<SearchDocument> GetStandardSearchDocs(int searchTypeId = ModuleSearchTypeId)
        {
            var searchDocs = new List<SearchDocument> {
                new SearchDocument { Tags = new List<string> { Tag0, Tag1, TagOldest, Tag0WithSpace }, Title = Line1 },
                new SearchDocument { Tags = new List<string> { Tag1, Tag2, TagNeutral }, Title = Line2 },
                new SearchDocument { Tags = new List<string> { Tag2, Tag3, TagIt }, Title = Line3, CultureCode = CultureItIt },
                new SearchDocument { Tags = new List<string> { Tag3, Tag4, TagLatest }, Title = Line4, CultureCode = CultureEnCa },
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
        /// Adds standarad SearchDocs in Lucene Index
        /// </summary>
        /// <returns>Number of dcuments added</returns>
        private int AddStandardSearchDocs(int searchTypeId = ModuleSearchTypeId)
        {
            var docs = GetStandardSearchDocs(searchTypeId).ToArray();
            _internalSearchController.AddSearchDocuments(docs);
            return docs.Length;
        }

        private int AddDocumentsWithNumericKeys(int searchTypeId = OtherSearchTypeId)
        {
             var doc1 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key1",
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
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue200 } },
            };
            var doc3 = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key3",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                PortalId = PortalId12,
                NumericKeys = new Dictionary<string, int>() { { NumericKey1, NumericValue1000 } },
            };

            var docs = new List<SearchDocument>() {doc1, doc2, doc3};

            _internalSearchController.AddSearchDocuments(docs);

            return docs.Count;
        }

        private int AddDocuments(IList<string> titles, string body,  int searchTypeId = OtherSearchTypeId)
        {
            var count = 0;
            foreach (var doc in titles.Select(title => new SearchDocument
                                                           {
                                                               Title = title,
                                                               UniqueKey = Guid.NewGuid().ToString(),
                                                               Body = body,
                                                               SearchTypeId = OtherSearchTypeId,
                                                               ModifiedTimeUtc = DateTime.UtcNow,
                                                               PortalId = PortalId12
                                                           }))
            {
                _internalSearchController.AddSearchDocument(doc);
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
                PortalId = PortalId12
            }))
            {
                _internalSearchController.AddSearchDocument(doc);
                count++;
            }

            return count;
        }

        private void AddLinesAsSearchDocs(IList<string> lines, int searchTypeId = OtherSearchTypeId)
        {
            var now = DateTime.UtcNow - TimeSpan.FromSeconds(lines.Count());
            var i = 0;

            _internalSearchController.AddSearchDocuments(
                lines.Select(line =>
                    new SearchDocument
                    {
                        Title = line,
                        UniqueKey = Guid.NewGuid().ToString(),
                        SearchTypeId = searchTypeId,
                        ModifiedTimeUtc = now.AddSeconds(i++)
                    }).ToList());
        }

        private SearchResults SearchForKeyword(string keyword, int searchTypeId = OtherSearchTypeId)
        {
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { searchTypeId } };
            return _searchController.SiteSearch(query);
        }

        private SearchResults SearchForKeywordWithWildCard(string keyword, int searchTypeId = OtherSearchTypeId)
        {
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { searchTypeId }, WildCardSearch = true };
            return _searchController.SiteSearch(query);
        }

        private SearchResults SearchForKeywordInModule(string keyword, int searchTypeId = ModuleSearchTypeId)
        {
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { searchTypeId } };
            return _searchController.SiteSearch(query);
        }

        private string StipEllipses(string text)
        {
            return text.Replace("...", "").Trim();
        }

        /// <summary>
        /// Executes function proc on a separate thread respecting the given timeout value.
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="proc">The function to execute.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <returns>R</returns>
        /// <remarks>From: http://stackoverflow.com/questions/9460661/implementing-regex-timeout-in-net-4</remarks>
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

        #endregion

        #region Search Tests

        [Test]
        public void SearchController_Search_Throws_On_Null_Query()
        {
            //Arrange

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => _searchController.SiteSearch(null));
        }

        [Test]
        public void SearchController_Search_Throws_On_Empty_TypeId_Collection()
        {
            //Arrange

            //Act, Assert
            Assert.Throws<ArgumentException>(() => _searchController.SiteSearch(new SearchQuery { KeyWords = "word" }));
        }

        [Test]
        public void SearchController_AddSearchDcoumet_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing()
        {
            //Arrange
            var document = new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "<<Click here for the complete city listing by a... ", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act, Assert
            _internalSearchController.AddSearchDocument(document);
        }

        [Test]
        public void SearchController_AddSearchDcoumet_Regex_Does_Not_Sleep_On_Bad_Text_During_Alt_Text_Parsing2()
        {
            //Arrange
            var document = new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "<<Click here for the complete city listing by a... ", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act, Assert
            Assert.DoesNotThrow(() => ExecuteWithTimeout(
                    () =>
                    {
                        _internalSearchController.AddSearchDocument(document);
                        return false;
                    }, TimeSpan.FromSeconds(1)));
        }

        #endregion

        #region Add and Search Tests

        [Test]
        public void SearchController_Added_Item_IsRetrieved()
        {
            //Arrange
            var doc = new SearchDocument { UniqueKey = "key01", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc);

            var result = SearchForKeyword("hello");

            //Assert
            Assert.AreEqual(1, result.Results.Count);
            Assert.AreEqual(result.Results[0].UniqueKey, doc.UniqueKey);
            Assert.AreEqual(result.Results[0].Title, doc.Title);
        }

        [Test]
        public void SearchController_EnsureIndexIsAppended_When_Index_Is_NotDeleted_InBetween()
        {
            //Arrange
            string[] docs = {
                Line1,
                Line2,
                };

            //Act

            //Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = Guid.NewGuid().ToString(), SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            _internalSearchController.AddSearchDocument(doc1);

            //first luceneQuery
            var query1 = new SearchQuery { KeyWords = "fox", SearchTypeIds = new List<int> { OtherSearchTypeId } };
            var search1 = _searchController.SiteSearch(query1);

            //Assert
            Assert.AreEqual(1, search1.Results.Count);

            //Add second document
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = Guid.NewGuid().ToString(), SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            _internalSearchController.AddSearchDocument(doc2);
            CreateNewLuceneControllerInstance(); // to force a new reader for the next assertion

            //second luceneQuery
            var query2 = new SearchQuery { KeyWords = "fox", SearchTypeIds = new List<int> { OtherSearchTypeId } };
            var search2 = _searchController.SiteSearch(query2);

            //Assert
            Assert.AreEqual(2, search2.Results.Count);
        }

        [Test]
        public void SearchController_Getsearch_TwoTermsSearch()
        {
            //Arrange
            string[] docs = {
                Line1,
                Line2,
                Line3,
                Line4
                };

            AddLinesAsSearchDocs(docs);

            //Act
            var search = SearchForKeyword("fox jumps");

            //Assert
            Assert.AreEqual(docs.Length, search.Results.Count);
            //Assert.AreEqual("brown <b>fox jumps</b> over the lazy dog ", search.Results[0].Snippet);
            //Assert.AreEqual("quick <b>fox jumps</b> over the black dog ", search.Results[1].Snippet);
        }

        [Test]
        public void SearchController_GetResult_TwoTermsSearch()
        {
            //Arrange
            string[] docs = {
                Line1,
                Line2,
                Line3,
                Line4
                };

            AddLinesAsSearchDocs(docs);

            //Act
            var search = SearchForKeyword("fox jumps");

            //Assert
            Assert.AreEqual(docs.Length, search.Results.Count);
            //Assert.AreEqual("brown <b>fox jumps</b> over the lazy dog ", search.Results[0].Snippet);
            //Assert.AreEqual("quick <b>fox jumps</b> over the black dog ", search.Results[1].Snippet);
        }

        [Test]
        public void SearchController_GetResult_PortalIdSearch()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, PortalIds = new List<int> { PortalId0 } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_SearchTypeIdSearch()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
        }


        [Test]
        public void SearchController_SearchFindsAnalyzedVeryLongWords()
        {
            //Arrange
            //const string fieldName = Constants.ContentTag;
            const string veryLongWord = // 107 characters
                "NowIsTheTimeForAllGoodMenToComeToTheAidOfTheirCountryalsoIsTheTimeForAllGoodMenToComeToTheAidOfTheirCountry";

            var doc = new SearchDocument
            {
                Title = veryLongWord,
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = ModuleSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow,
                ModuleId = 1,
                ModuleDefId = 1
            };
            _internalSearchController.AddSearchDocument(doc);

            //Act
            var query = new SearchQuery { KeyWords = veryLongWord, SearchTypeIds = new List<int> { ModuleSearchTypeId } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual("<b>" + veryLongWord + "</b>", StipEllipses(search.Results[0].Snippet).Trim());
        }
        #endregion

        #region Security Trimming Tests

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsNoResultsWhenHavingNoPermission()
        {
            //Arrange
            AddStandardSearchDocs(DocumentSearchTypeId);

            //Act
            var result = SearchForKeyword("fox", DocumentSearchTypeId);

            //Assert
            // by default AuthorUserId = 0 which have no permission, so this passes
            Assert.AreEqual(0, result.Results.Count);
        }

        /// <summary>
        /// Sets up some data for testing security trimming.
        /// In the tests below, the users will have access to the follwoing documents
        /// { 6, 7, 8, 9, 16, 17, 18, 19, 26, 27, 28, 29, ..., etc. }
        /// The tests check that pagination qith various page sizes returns the proper groupings
        /// </summary>
        private void SetupSecurityTrimmingDocs(int totalDocs, int searchType = DocumentSearchTypeId)
        {
            var docModifyTime = DateTime.UtcNow - TimeSpan.FromSeconds(totalDocs);
            for (var i = 0; i < totalDocs; i++)
            {
                _internalSearchController.AddSearchDocument(new SearchDocument
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

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1A()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 4,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 6, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1B()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 6,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 12, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9, 16, 17 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1C()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 12, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9, 16, 17, 18, 19 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1D()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId; // user should have access to some documnets here
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 100,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(4 * 3, result.TotalHits);
            Assert.AreEqual(4 * 3, result.Results.Count);
            Assert.AreEqual(new[] { 6, 7, 8, 9, 16, 17, 18, 19, 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1E()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = TabSearchTypeId; // user should have access to all documnets here
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 1,
                PageSize = 10,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(30, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(Enumerable.Range(0, 10).ToArray(), ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage1F()
        {
            //Arrange
            const int maxDocs = 100;
            const int stype = TabSearchTypeId; // user should have access to all documnets here
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 10,
                PageSize = 10,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs, result.TotalHits);
            Assert.AreEqual(query.PageSize, result.Results.Count);
            Assert.AreEqual(Enumerable.Range(90, 10).ToArray(), ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2A()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 5,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(5, result.Results.Count);
            Assert.AreEqual(new[] { 17, 18, 19, 26, 27 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2B()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 6,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(6, result.Results.Count);
            Assert.AreEqual(new[] { 18, 19, 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage2C()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var query = new SearchQuery
            {
                PageIndex = 2,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(query);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(4, result.Results.Count);
            Assert.AreEqual(new[] { 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3A()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 4,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(4, result.Results.Count);
            Assert.AreEqual(new[] { 26, 27, 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3B()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 5,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(2, result.Results.Count);
            Assert.AreEqual(new[] { 28, 29 }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage3C()
        {
            //Arrange
            const int maxDocs = 30;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 3,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 18, result.TotalHits);
            Assert.AreEqual(0, result.Results.Count);
            Assert.AreEqual(new int[] { }, ids);
        }

        [Test]
        public void SearchController_SecurityTrimmedTest_ReturnsExpectedResultsForPage5()
        {
            //Arrange
            const int maxDocs = 100;
            const int stype = DocumentSearchTypeId;
            SetupSecurityTrimmingDocs(maxDocs, stype);

            //Act
            var queryPg3 = new SearchQuery
            {
                PageIndex = 5,
                PageSize = 8,
                KeyWords = "fox",
                SearchTypeIds = new[] { stype }
            };

            var result = _searchController.SiteSearch(queryPg3);
            var ids = result.Results.Select(doc => doc.AuthorUserId).ToArray();

            //Assert
            Assert.AreEqual(maxDocs - 10 * 6, result.TotalHits);
            Assert.AreEqual(8, result.Results.Count);
            Assert.AreEqual(new int[] { 86, 87, 88, 89, 96, 97, 98, 99 }, ids);
        }

        #endregion

        #region Supplied Data Tests

        [Test]
        public void SearchController_GetResult_Returns_Correct_SuppliedData_When_Optionals_Are_Supplied()
        {
            //Arrange
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
                Keywords = keywords
            };
            _internalSearchController.AddSearchDocument(doc);

            //run luceneQuery on common keyword between both the docs
            var search = SearchForKeywordInModule("Title");

            //Assert
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
            //Arrange
            var modifiedDateTime = DateTime.UtcNow;
            var doc = new SearchDocument
            {
                Title = "Title",
                UniqueKey = "key",
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = modifiedDateTime,
            };
            _internalSearchController.AddSearchDocument(doc);

            var search = SearchForKeyword("Title");

            //Assert -
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(0, search.Results[0].PortalId);
            Assert.AreEqual(0, search.Results[0].TabId);
            Assert.AreEqual(0, search.Results[0].ModuleDefId);
            Assert.AreEqual(0, search.Results[0].ModuleId);
            Assert.AreEqual(OtherSearchTypeId, search.Results[0].SearchTypeId);
            Assert.AreEqual(null, search.Results[0].Description);
            Assert.AreEqual(null, search.Results[0].Body);
            Assert.AreEqual(0, search.Results[0].AuthorUserId);
            Assert.AreEqual(0, search.Results[0].RoleId);
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
            //Arrange
            string[] docs = {
                Line1,
                Line2,
                Line3,
                Line4
                };
            AddLinesAsSearchDocs(docs);

            //Act
            var searches = SearchForKeyword("fox");

            //Assert
            Assert.AreEqual(docs.Length, searches.Results.Count);
            Assert.IsTrue(new[]
                {
                  "brown <b>fox</b> jumps over the lazy dog",
                  "quick <b>fox</b> jumps over the black dog",
                  "gold <b>fox</b> jumped over the lazy black dog",
                  "e red <b>fox</b> jumped over the lazy dark gray dog",
                }.SequenceEqual(searches.Results.Select(r => StipEllipses(r.Snippet))));
        }

        [Test]
        public void SearchController_CorrectDocumentCultureIsUsedAtIndexing()
        {
            //Arrange
            // assign a culture that is different than the current one
            var isNonEnglishEnv = Thread.CurrentThread.CurrentCulture.Name != CultureEsEs;
            string cultureCode, title, searchWord;

            //Act
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

            _internalSearchController.AddSearchDocument(
                new SearchDocument
                {
                    Title = title,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = OtherSearchTypeId,
                    ModifiedTimeUtc = DateTime.UtcNow,
                    CultureCode = cultureCode
                });
            _internalSearchController.Commit();

            var searches = SearchForKeyword(searchWord);

            //Assert
            Assert.AreEqual(1, searches.TotalHits);
            Assert.AreEqual(cultureCode, searches.Results[0].CultureCode);
        }

        #endregion

        #region DateRange Tests

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Ignores_When_Only_BeginDate_Specified()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, BeginModifiedTimeUtc = DateTime.Now };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Resturns_Scoped_Results_When_BeginDate_Is_After_End_Date()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                BeginModifiedTimeUtc = DateTime.Now,
                EndModifiedTimeUtc = DateTime.Now.AddSeconds(-1)
            };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TimeRangeSearch_Resturns_Scoped_Results_When_Both_Dates_Specified()
        {
            //Arrange
            var added = AddStandardSearchDocs();
            var stypeIds = new List<int> { ModuleSearchTypeId };
            var utcNow = DateTime.UtcNow.AddDays(1);
            const SortFields sfield = SortFields.LastModified; 

            //Act and Assert - just a bit later
            var query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddSeconds(1), EndModifiedTimeUtc = utcNow.AddDays(1) };
            var search = _searchController.SiteSearch(query);
            Assert.AreEqual(0, search.Results.Count);

            //Act and Assert - 10 day
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-10), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = _searchController.SiteSearch(query);
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(Line4, search.Results[0].Title);

            //Act and Assert - 1 year or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-368), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = _searchController.SiteSearch(query);
            Assert.AreEqual(2, search.Results.Count);
            Assert.AreEqual(Line4, search.Results[0].Title);
            Assert.AreEqual(Line3, search.Results[1].Title);

            //Act and Assert - 2 years or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-800), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = _searchController.SiteSearch(query);
            Assert.AreEqual(3, search.Results.Count);
            Assert.AreEqual(Line4, search.Results[0].Title);
            Assert.AreEqual(Line3, search.Results[1].Title);
            Assert.AreEqual(Line2, search.Results[2].Title);

            //Act and Assert - 3 years or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-1200), EndModifiedTimeUtc = utcNow.AddDays(1) };
            search = _searchController.SiteSearch(query);
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(Line4, search.Results[0].Title);
            Assert.AreEqual(Line3, search.Results[1].Title);
            Assert.AreEqual(Line2, search.Results[2].Title);
            Assert.AreEqual(Line1, search.Results[3].Title);

            //Act and Assert - 2 to 3 years or so
            query = new SearchQuery { SearchTypeIds = stypeIds, SortField = sfield, BeginModifiedTimeUtc = utcNow.AddDays(-1200), EndModifiedTimeUtc = utcNow.AddDays(-800) };
            search = _searchController.SiteSearch(query);
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(Line1, search.Results[0].Title);
        }

        #endregion

        #region Tag Tests

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_Returns_Single_Result()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag0 } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(1, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_With_Space_Returns_Single_Result()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag0WithSpace } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(1, search.Results.Count);
        }


        [Test]
        public void SearchController_GetResult_TagSearch_Lowercase_Search_Returns_PropercaseTag_Single_Result()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { TagNeutral.ToLower() } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(1, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Single_Tag_Returns_Two_Results()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag1 } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(2, search.Results.Count);
            Assert.AreEqual(Tag0, search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(Tag1, search.Results[0].Tags.ElementAt(1));
            Assert.AreEqual(Tag1, search.Results[1].Tags.ElementAt(0));
            Assert.AreEqual(Tag2, search.Results[1].Tags.ElementAt(1));
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Two_Tags_Returns_Nothing()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag0, Tag4 } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(0, search.Results.Count);
        }

        [Test]
        public void SearchController_GetResult_TagSearch_Two_Tags_Returns_Single_Results()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery { SearchTypeIds = new List<int> { ModuleSearchTypeId }, Tags = new List<string> { Tag1, Tag2 } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(1, search.Results.Count);
            Assert.AreEqual(Tag1, search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(Tag2, search.Results[0].Tags.ElementAt(1));
        }

        [Test]
        public void SearchController_GetResult_TagSearch_With_Vowel_Tags_Returns_Data()
        {
            //Arrange
            const string keyword = "awesome";
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { TagTootsie } };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, Tags = new List<string> { TagTootsie } };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(1, search.Results.Count);
        }

        #endregion

        #region Sort Tests

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_CustomNumericField_Is_Specified_And_CustomSortField_Is_Not()
        {
            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomNumericField
            };

            _searchController.SiteSearch(query);
         }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_CustomStringField_Is_Specified_And_CustomSortField_Is_Not()
        {
            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomStringField
            };

            _searchController.SiteSearch(query);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_NumericKey_Is_Specified_And_CustomSortField_Is_Not()
        {
            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.NumericKey
            };

            _searchController.SiteSearch(query);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_GetResult_Throws_When_Keyword_Is_Specified_And_CustomSortField_Is_Not()
        {
            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Keyword
            };

            _searchController.SiteSearch(query);
        }
        
        [Test]
        public void SearchController_GetResult_Sorty_By_Date_Returns_Latest_Docs_First()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
                {
                    SearchTypeIds = new List<int> { ModuleSearchTypeId },
                    SortField = SortFields.LastModified
                };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(Tag3, search.Results[0].Tags.ElementAt(0));
            Assert.AreEqual(Tag4, search.Results[0].Tags.ElementAt(1));
            Assert.AreEqual(TagLatest, search.Results[0].Tags.ElementAt(2));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Date_Ascending_Returns_Earliest_Docs_First()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.LastModified,
                SortDirection = SortDirections.Ascending
            };
            var search = _searchController.SiteSearch(query);

            //Assert
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
            var added = AddDocumentsWithNumericKeys();

            //Act
            var query = new SearchQuery
            {
                KeyWords = "Title",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.NumericKey,
                SortDirection = SortDirections.Ascending,
                CustomSortField = NumericKey1
            };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.Greater(search.Results[1].NumericKeys[NumericKey1], search.Results[0].NumericKeys[NumericKey1]);
            Assert.Greater(search.Results[2].NumericKeys[NumericKey1], search.Results[1].NumericKeys[NumericKey1]);
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_NumericKeys_Descending_Returns_Bigger_Numbers_First()
        {
            var added = AddDocumentsWithNumericKeys();

            //Act
            var query = new SearchQuery
            {
                KeyWords = "Title",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.NumericKey,
                SortDirection = SortDirections.Descending,
                CustomSortField = NumericKey1
            };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.Greater(search.Results[0].NumericKeys[NumericKey1], search.Results[1].NumericKeys[NumericKey1]);
            Assert.Greater(search.Results[1].NumericKeys[NumericKey1], search.Results[2].NumericKeys[NumericKey1]);
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Title_Ascending_Returns_Alphabetic_Ascending()
        {
            var titles = new List<string> {"cat", "ant", "dog", "antelope", "zebra", "yellow", " "};

            var added = AddDocuments(titles, "animal");

            //Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Title,
                SortDirection = SortDirections.Ascending
            };
            var search = _searchController.SiteSearch(query);

            //Assert
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

            var added = AddDocuments(titles, "animal");

            //Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Title,
                SortDirection = SortDirections.Descending
            };
            var search = _searchController.SiteSearch(query);

            //Assert
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

            var added = AddDocumentsWithKeywords(titles, "animal");

            //Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Keyword,
                SortDirection = SortDirections.Ascending,
                CustomSortField = KeyWord1Name
            };
            var search = _searchController.SiteSearch(query);

            //Assert
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

            var added = AddDocumentsWithKeywords(titles, "animal");

            //Act
            var query = new SearchQuery
            {
                KeyWords = "animal",
                SearchTypeIds = new List<int> { OtherSearchTypeId },
                SortField = SortFields.Keyword,
                SortDirection = SortDirections.Descending,
                CustomSortField = KeyWord1Name
            };
            var search = _searchController.SiteSearch(query);

            //Assert
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
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomStringField,
                SortDirection = SortDirections.Descending,
                CustomSortField = "unknown"
            };
            _searchController.SiteSearch(query);
        }


        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_StringField_In_Ascending_Order_Does_Not_Throw()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomStringField,
                SortDirection = SortDirections.Ascending,
                CustomSortField = "unknown"
            };
            _searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_NumericField_In_Descending_Order_Does_Not_Throw()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomNumericField,
                SortDirection = SortDirections.Descending,
                CustomSortField = "unknown"
            };
            _searchController.SiteSearch(query);
        }


        [Test]
        public void SearchController_GetResult_Sort_By_Unknown_NumericField_In_Ascending_Order_Does_Not_Throw()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.CustomNumericField,
                SortDirection = SortDirections.Ascending,
                CustomSortField = "unknown"
            };
            _searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Relevance_Returns_TopHit_Docs_First()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
                {
                    SearchTypeIds = new List<int> { ModuleSearchTypeId },
                    SortField = SortFields.Relevance,
                    KeyWords = "brown OR fox"
                };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Snippet.Contains("brown") && search.Results[0].Snippet.Contains("dog"));
        }

        [Test]
        public void SearchController_GetResult_Sorty_By_Relevance_Ascending_Does_Not_Change_Sequence_Of_Results()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
            {
                SearchTypeIds = new List<int> { ModuleSearchTypeId },
                SortField = SortFields.Relevance,
                SortDirection = SortDirections.Ascending,
                KeyWords = "brown OR fox"
            };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(added, search.Results.Count);
            Assert.AreEqual(true, search.Results[0].Snippet.Contains("brown") && search.Results[0].Snippet.Contains("dog"));
        }

        #endregion

        #region Locale Tests

        [Test]
        public void SearchController_GetResult_By_Locale_Returns_Specific_And_Neutral_Locales()
        {
            //Arrange
            AddStandardSearchDocs();

            //Act
            var query = new SearchQuery
                {
                    SearchTypeIds = new List<int> { ModuleSearchTypeId },
                    SortField = SortFields.LastModified,
                    CultureCode = CultureItIt
                };
            var search = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(3, search.Results.Count);
            Assert.AreEqual(Line3, search.Results[0].Title);
            Assert.AreEqual(Line2, search.Results[1].Title);
            Assert.AreEqual(Line1, search.Results[2].Title);
        }


        #endregion

        #region Add and Delete Tests
        [Test]
        public void SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_Content_With_Same_Key()
        {
            //Arrange
            string[] docs = {
                Line1,
                Line2,
                };
            const string docKey = "key1";

            //Act

            //Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            _internalSearchController.AddSearchDocument(doc1);

            //Add second document with same key
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            _internalSearchController.AddSearchDocument(doc2);

            //run luceneQuery on common keyword between both the docs
            var search = SearchForKeyword("fox");

            //Assert - there should just be one entry - first one must have been removed.
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

            Assert.DoesNotThrow(() => _internalSearchController.AddSearchDocument(doc));
        }

        [Test]
        public void SearchController_Add_Does_Not_Throw_On_Empty_Title()
        {
            var doc = new SearchDocument
            {
                UniqueKey = Guid.NewGuid().ToString(),
                SearchTypeId = OtherSearchTypeId,
                ModifiedTimeUtc = DateTime.UtcNow
            };

            Assert.DoesNotThrow(() => _internalSearchController.AddSearchDocument(doc));
        }

        #endregion

        #region IsActive Tests
        [Test]
        public void SearchController_EnsureOldDocument_Deleted_Upon_Second_Index_When_IsActive_Is_False()
        {
            //Arrange
            string[] docs = {
                Line1,
                Line2,
                };
            const string docKey = "key1";

            //Act

            //Add first document
            var doc1 = new SearchDocument { Title = docs[0], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            _internalSearchController.AddSearchDocument(doc1);

            //Add second document with same key
            var doc2 = new SearchDocument { Title = docs[1], UniqueKey = docKey, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, IsActive = false };
            _internalSearchController.AddSearchDocument(doc2);

            //run luceneQuery on common keyword between both the docs
            var search = SearchForKeyword("fox");

            //Assert - there should not be any record.
            Assert.AreEqual(0, search.Results.Count);
        }

        #endregion

        #region accents tests
        // Note: these tests needs to pass through the analyzer which is utilized
        //       in SearchControllerImpl but not LuceneControllerImpl.

        [Test]
        public void SearchController_SearchFindsAccentedAndNonAccentedWords()
        {
            //Arrange
            string[] lines = {
                "zèbre or panthère",
                "zebre without accent",
                "panthere without accent"
                };

            AddLinesAsSearchDocs(lines);

            //Act
            var searches1 = SearchForKeyword("zèbre");
            var searches2 = SearchForKeyword("zebre");

            //Assert
            Assert.AreEqual(2, searches1.TotalHits);
            Assert.AreEqual("<b>z&#232;bre</b> or panth&#232;re", StipEllipses(searches1.Results[0].Snippet).Trim());
            Assert.AreEqual("<b>zebre</b> without accent", StipEllipses(searches1.Results[1].Snippet).Trim());

            Assert.AreEqual(2, searches2.TotalHits);
            Assert.AreEqual("<b>z&#232;bre</b> or panth&#232;re", StipEllipses(searches2.Results[0].Snippet).Trim());
            Assert.AreEqual("<b>zebre</b> without accent", StipEllipses(searches2.Results[1].Snippet).Trim());
        }

        [Test]
        public void SearchController_PorterFilterTest()
        {
            //Arrange
            string[] lines = {
                "field1_value",
                "field2_value",
                };

            AddLinesAsSearchDocs(lines);

            //Act
            var search1 = SearchForKeyword(lines[0]);
            var search2 = SearchForKeyword("\"" + lines[1] + "\"");

            //Assert
            Assert.AreEqual(1, search1.TotalHits);
            Assert.AreEqual(1, search2.TotalHits);

            Assert.AreEqual("<b>" + lines[0] + "</b>", StipEllipses(search1.Results[0].Snippet).Trim());
            Assert.AreEqual("<b>" + lines[1] + "</b>", StipEllipses(search2.Results[0].Snippet).Trim());
        }

        [Test]
        public void SearchController_SearchFindsStemmedWords()
        {
            //Arrange
            string[] lines = {
                "I ride my bike to work",
                "All team are riding their bikes",
                "The boy rides his bike to school",
                "This sentence is missing the bike ri... word"
                };

            AddLinesAsSearchDocs(lines);

            //Act
            var search = SearchForKeyword("ride");

            //Assert
            Assert.AreEqual(3, search.TotalHits);
            Assert.AreEqual("I <b>ride</b> my bike to work", StipEllipses(search.Results[0].Snippet));
            Assert.AreEqual("m are <b>riding</b> their bikes", StipEllipses(search.Results[1].Snippet));
            Assert.AreEqual("e boy <b>rides</b> his bike to school", StipEllipses(search.Results[2].Snippet));
        }

        #endregion

        #region synonyms tests

        [Test]
        public void SearchController_Search_Synonym_Works()
        {
            //Arrange
            var added = AddStandardSearchDocs();

            //Act
            var search = SearchForKeywordInModule("wolf");

            //Assert
            Assert.AreEqual(added, search.TotalHits);
            Assert.AreEqual("brown <b>fox</b> jumps over the lazy dog", StipEllipses(search.Results[0].Snippet));
            Assert.AreEqual("quick <b>fox</b> jumps over the black dog", StipEllipses(search.Results[1].Snippet));
            Assert.AreEqual("gold <b>fox</b> jumped over the lazy black dog", StipEllipses(search.Results[2].Snippet));
            Assert.AreEqual("e red <b>fox</b> jumped over the lazy dark gray dog", StipEllipses(search.Results[3].Snippet));
        }
        #endregion

        #region Field Boosting Tests
        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Body()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "Hello World" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "cow is gone" };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.Commit();


            var result = SearchForKeyword("cow");

            //Assert
            Assert.AreEqual(result.TotalHits, 2);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Body_Regardless_Of_Document_Sequence()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "cow is gone" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "cow is gone", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.Commit();


            var result = SearchForKeyword("cow");

            //Assert
            Assert.AreEqual(result.TotalHits, 2);
            Assert.AreEqual(doc3.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc1.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_Title_Ranked_Higher_Than_Tag()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "cow", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "Hello World" };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { "cow" } };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "I'm here", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "random text" };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.Commit();


            var result = SearchForKeyword("cow");

            //Assert
            Assert.AreEqual(result.TotalHits, 2);
            Console.WriteLine("first score: {0}  {1}", result.Results[0].UniqueKey, result.Results[0].DisplayScore);
            Console.WriteLine("second score: {0}  {1}", result.Results[1].UniqueKey, result.Results[1].DisplayScore);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_RankingTest_With_Vowel()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "tootsie", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Tags = new List<string> { "tootsie" } };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Keywords = new Dictionary<string, string>() { { KeyWord1Name, "tootsie" } } };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Description = "tootsie" };
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = "Hello World", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, Body = "hello tootsie" };



            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.AddSearchDocument(doc4);
            _internalSearchController.AddSearchDocument(doc5);

            _internalSearchController.Commit();


            var result = SearchForKeyword("tootsie");

            //Assert
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

        #endregion

        #region FileName Tests
        [Test]
        public void SearchController_FileNameTest_With_WildCard()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);

            _internalSearchController.Commit();

            var result = SearchForKeywordWithWildCard("file");

            //Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        public void SearchController_Full_FileNameTest_Without_WildCard()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);

            _internalSearchController.Commit();

            var result = SearchForKeywordWithWildCard("file.ext");

            //Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }

        [Test]
        public void SearchController_Full_FileNameTest_With_WildCard()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "file.ext", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);

            _internalSearchController.Commit();

            var result = SearchForKeyword("file.ext");

            //Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }
        #endregion

        #region folder scope search tests

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
                                { "friday.gif", "My<Images/My<DNN/" }
                               };


            foreach (var file in allFiles)
            {
                var doc = new SearchDocument
                {
                    Title = file.Key,
                    UniqueKey = Guid.NewGuid().ToString(),
                    SearchTypeId = OtherSearchTypeId,
                    ModifiedTimeUtc = DateTime.UtcNow,
                    Keywords = new Dictionary<string, string> { { "folderName", file.Value.ToLower() } }
                };
                _internalSearchController.AddSearchDocument(doc);
            }

            _internalSearchController.Commit();
        }

        [Test]
        public void SearchController_Scope_By_FolderName()
        {
            //Arrange
            AddFoldersAndFiles();
            
            //Act
            var result1 = SearchForKeyword("kw-folderName:Images/*");
            var result2 = SearchForKeyword("kw-folderName:Images/DNN/*");
            var result3 = SearchForKeywordWithWildCard("kw-folderName:Images/* AND spacer");

            //Assert
            Assert.AreEqual(5, result1.TotalHits);
            Assert.AreEqual(2, result2.TotalHits);
            Assert.AreEqual(1, result3.TotalHits);
        }

        [Test]
        public void SearchController_Scope_By_FolderName_With_Spaces()
        {
            //Arrange
            AddFoldersAndFiles();

            //Act - Space is replaced by <
            var query1 = new SearchQuery {KeyWords = "kw-folderName:Images/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = false };
            var query2 = new SearchQuery { KeyWords = "kw-folderName:my<Images/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = true };
            var query3 = new SearchQuery { KeyWords = "kw-folderName:my<Images/my<dnn/*", SearchTypeIds = new[] { OtherSearchTypeId }, WildCardSearch = true };
            var result1 = _searchController.SiteSearch(query1);
            var result2 = _searchController.SiteSearch(query2);
            var result3 = _searchController.SiteSearch(query3);

            //Assert
            Assert.AreEqual(5, result1.TotalHits);
            Assert.AreEqual(5, result2.TotalHits);
            Assert.AreEqual(2, result3.TotalHits);
        }

        #endregion

        #region EmailAddress Tests
        [Test]
        public void SearchController_EmailTest_With_WildCard()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "email@domain.com", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);

            _internalSearchController.Commit();

            var result = SearchForKeywordWithWildCard("email@");

            //Assert
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
        }
        #endregion

        #region search type id test
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleDefinitionId()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Use_Of_Module_Search_Type_Requires_ModuleId()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleDefinitionId()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Add_Non_Module_Type_SearchId_Should_Not_Provide_ModuleId()
        {
            //Arrange
            var doc1 = new SearchDocument { UniqueKey = "key01", Title = "awesome", SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleId = HtmlModuleId };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Search_For_ModuleId_Must_Have_Only_One_Search_Type_Id_Specified()
        {
            //Arrange
            const string keyword = "awesome";

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { DocumentSearchTypeId, OtherSearchTypeId }, ModuleId = IdeasModuleId };

            //Act
            _searchController.SiteSearch(query);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SearchController_Search_For_ModuleId_Must_Have_Only_Module_Search_Type_Id_Specified()
        {
            //Arrange
            const string keyword = "awesome";

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, OtherSearchTypeId }, ModuleId = IdeasModuleId };

            //Act
            _searchController.SiteSearch(query);
        }

        [Test]
        public void SearchController_Search_For_Unknown_SearchTypeId_Does_Not_Throw_Exception()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = UnknownSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };
            _internalSearchController.AddSearchDocument(doc1);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { UnknownSearchTypeId } };

            //Act
            var result = _searchController.SiteSearch(query);
            Assert.AreEqual(0, result.TotalHits); // 0 due to security trimming
            Assert.AreEqual(0, result.Results.Count);
        }


        #endregion

        #region group id test

        [Test]
        public void SearchController_Search_For_GroupId_Zero_Ignores_GroupId()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId0 };
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, RoleId = 0};

            //Act
            var result = _searchController.SiteSearch(query);
            Assert.AreEqual(2, result.TotalHits);
        }

        [Test]
        public void SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId532 };
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId }, RoleId = RoleId731 };

            //Act
            var result = _searchController.SiteSearch(query);
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(RoleId731, result.Results[0].RoleId);
        }

        [Test]
        public void SearchController_Search_For_GroupId_Returns_Records_With_GroupIds_Only_Even_In_Multi_SearchTypeId()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId731 };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, RoleId = RoleId532 };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
         
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { OtherSearchTypeId, ModuleSearchTypeId }, RoleId = RoleId731 };

            //Act
            var result = _searchController.SiteSearch(query);
            Assert.AreEqual(1, result.TotalHits);
            Assert.AreEqual(RoleId731, result.Results[0].RoleId);
        }

        #endregion

        #region Module Definitions Test

        [Test]
        public void SearchController_Search_For_Two_ModuleDefinitions_Returns_Two_Only()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(2, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }

        [Test]
        public void SearchController_Search_For_ModuleId_Returns_from_that_module_Only()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.AddSearchDocument(doc4);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId }, ModuleId = IdeasModuleId };

            var result = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(2, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
        }
        #endregion

        #region module and non-module search type tests

        [Test]
        public void SearchController_Search_For_Module_Search_Type_And_Multiple_ModuleDefIds_Should_Return_From_Those_ModuleDefinitions_Only()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.AddSearchDocument(doc4);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(3, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
            Assert.AreEqual(doc3.UniqueKey, result.Results[2].UniqueKey);
        }

        [Test]
        public void SearchController_Search_For_Multiple_Search_Types_Should_Return_Result_from_All_Sources()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = IdeasModuleDefId, ModuleId = IdeasModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = keyword, SearchTypeId = OtherSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.AddSearchDocument(doc4);
            _internalSearchController.AddSearchDocument(doc5);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, OtherSearchTypeId }, ModuleDefIds = new[] { IdeasModuleDefId, AnswersModuleDefId } };

            var result = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(4, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
            Assert.AreEqual(doc3.UniqueKey, result.Results[2].UniqueKey);
            Assert.AreEqual(doc5.UniqueKey, result.Results[3].UniqueKey);
        }

        [Test]
        public void SearchController_Search_For_ModuleSearchTypeId_With_Two_ModuleDefinitions_And_OtherSearchTypeId_Returns_Correct_Results()
        {
            //Arrange
            const string keyword = "awesome";

            var doc1 = new SearchDocument { UniqueKey = "key01", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId, ModuleId = HtmlModuleId };
            var doc2 = new SearchDocument { UniqueKey = "key02", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = HtmlModuleDefId, ModuleId = HtmlModuleId };
            var doc3 = new SearchDocument { UniqueKey = "key03", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = AnswersModuleDefId, ModuleId = AnswersModuleId };
            var doc4 = new SearchDocument { UniqueKey = "key04", Title = keyword, SearchTypeId = ModuleSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow, ModuleDefId = BlogsoduleDefId, ModuleId = BlogsModuleId };
            var doc5 = new SearchDocument { UniqueKey = "key05", Title = keyword, SearchTypeId = TabSearchTypeId, ModifiedTimeUtc = DateTime.UtcNow };

            //Act
            _internalSearchController.AddSearchDocument(doc1);
            _internalSearchController.AddSearchDocument(doc2);
            _internalSearchController.AddSearchDocument(doc3);
            _internalSearchController.AddSearchDocument(doc4);
            _internalSearchController.AddSearchDocument(doc5);

            var query = new SearchQuery { KeyWords = keyword, SearchTypeIds = new[] { ModuleSearchTypeId, TabSearchTypeId }, ModuleDefIds = new[] { HtmlModuleDefId } };

            var result = _searchController.SiteSearch(query);

            //Assert
            Assert.AreEqual(3, result.TotalHits);
            Assert.AreEqual(doc1.UniqueKey, result.Results[0].UniqueKey);
            Assert.AreEqual(doc2.UniqueKey, result.Results[1].UniqueKey);
            Assert.AreEqual(doc5.UniqueKey, result.Results[2].UniqueKey);
        }

        #endregion
    }
}
