#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;
using System.Data;

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    /// <summary>
    ///  Testing various aspects of SearchController
    /// </summary>
    [TestFixture]
    public class SearchHelperTests
    {

        #region constants

        private const int OtherSearchTypeId = 2;
        private const string TermDNN = "DNN";
        private const string TermDotNetNuke = "DotnetNuke";
        private const string TermLaptop = "Laptop";
        private const string TermNotebook = "Notebook";
        private const string TermJump = "Jump";
        private const string TermLeap = "Leap";
        private const string TermHop = "Hop";
        private const int PortalId0 = 0;
        private const string CultureEnUs = "en-US";

        #endregion

        #region Private Properties

        private Mock<DataProvider> _dataProvider;
        private Mock<CachingProvider> _cachingProvider;
        private SearchHelperImpl _searchHelper;

        #endregion

        #region Set Up

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            _cachingProvider = MockComponentProvider.CreateDataCacheProvider();
            _dataProvider = MockComponentProvider.CreateDataProvider();
            SetupDataProvider();
            _searchHelper = new SearchHelperImpl();
            DataCache.ClearCache();
        }

        #endregion

        #region Private Methods

        private void SetupDataProvider()
        {
            //Standard DataProvider Path for Logging
            _dataProvider.Setup(d => d.GetProviderPath()).Returns("");

            _dataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);

            _dataProvider.Setup(d => d.GetPortalSettings(It.IsAny<int>(), It.IsAny<string>())).Returns<int, string>(GetPortalSettingsCallBack);

            var dataTable = new DataTable("SynonymsGroups");
            var pkId = dataTable.Columns.Add("SynonymsGroupID", typeof(int));
            dataTable.Columns.Add("SynonymsTags", typeof(string));
            dataTable.Columns.Add("CreatedByUserID", typeof(int));
            dataTable.Columns.Add("CreatedOnDate", typeof(DateTime));
            dataTable.Columns.Add("LastModifiedByUserID", typeof(int));
            dataTable.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            dataTable.Columns.Add("PortalID", typeof(int));

            dataTable.PrimaryKey = new[] { pkId };

            //Create some test data
            var now = DateTime.Now;
            dataTable.Rows.Add(1, string.Join(",", new[] { TermDNN, TermDotNetNuke }), 1, now, 1, now, 0);
            dataTable.Rows.Add(2, string.Join(",", new[] { TermLaptop, TermNotebook }), 1, now, 1, now, 0);
            dataTable.Rows.Add(3, string.Join(",", new[] { TermJump, TermLeap, TermHop }), 1, now, 1, now, 0);

            _dataProvider.Setup(x => x.GetAllSynonymsGroups(0, It.IsAny<string>())).Returns(dataTable.CreateDataReader());
        }

        private IDataReader GetPortalSettingsCallBack(int portalId, string culture)
        {
            var table = new DataTable("PortalSettings");

            var cols = new string[]
			           	{
							"SettingName","SettingValue","CreatedByUserID","CreatedOnDate","LastModifiedByUserID","LastModifiedOnDate","CultureCode"
			           	};

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            table.Rows.Add("SearchAdminInitialization", "true", "-1", DateTime.Now, "-1", DateTime.Now, CultureEnUs);

            return table.CreateDataReader();
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
			           		"UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "SuperTabId", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode"
			           	};

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            var homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null, "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@change.me", "en-US", "-8", "58", "Portals/0", null, homePage.ToString(), null, null, "57", "56", "-1", "-1", "7", "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }

        #endregion

        #region Synonyms Tests

        [Test]
        public void SearchHelper_GetSynonyms_Two_Terms_Returns_Correct_Results()
        {
            //Arrange            

            //Act
            var synonyms = _searchHelper.GetSynonyms(PortalId0, CultureEnUs,TermDNN).ToArray();

            //Assert
            Assert.AreEqual(1, synonyms.Count());
            Assert.AreEqual(TermDotNetNuke.ToLower(), synonyms[0]);
        }

        [Test]
        public void SearchHelper_GetSynonyms_Three_Terms_Returns_Correct_Results()
        {
            //Arrange            

            //Act
            var synonyms = _searchHelper.GetSynonyms(PortalId0, CultureEnUs, TermHop).ToArray();

            //Assert
            Assert.AreEqual(2, synonyms.Count());
            Assert.AreEqual(TermJump.ToLower(), synonyms[0]);
            Assert.AreEqual(TermLeap.ToLower(), synonyms[1]);
        }

        #endregion

        #region Rephrasing Search Text Tests

        [Test]
        public void SearchHelper_Rephrase_NoWildCardButExact_1()
        {
            //Arrange            
            const string inPhrase = "\"brown fox\"";
            const string expected = inPhrase;

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, false);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardButExact_2()
        {
            //Arrange            
            const string inPhrase = "\"brown fox\" -\"(Lazy) dog\" jumps";
            const string expected = inPhrase;

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, false);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardNoExact()
        {
            //Arrange            
            const string inPhrase = "(brown) OR (fox AND dog) +jumbs";
            const string expected = inPhrase;

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, false);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_0()
        {
            //Arrange            
            const string inPhrase = "fox dog";
            const string expected = "(fox OR fox*) (dog OR dog*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_1()
        {
            //Arrange            
            const string inPhrase = "(lazy-dog)";
            const string expected = "(lazy-dog OR lazy-dog*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_2()
        {
            //Arrange            
            const string inPhrase = "fox (dog)";
            const string expected = "(fox OR fox*) (dog OR dog*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_3()
        {
            //Arrange            
            const string inPhrase = "(dog) fox";
            const string expected = "(dog OR dog*) (fox OR fox*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_4()
        {
            //Arrange            
            const string inPhrase = "brown fox (lazy) dog";
            const string expected = "(brown OR brown*) (fox OR fox*) (lazy OR lazy*) (dog OR dog*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_5()
        {
            //Arrange            
            const string inPhrase = "(brown fox) lazy dog";
            const string expected = "((brown OR brown*) (fox OR fox*)) (lazy OR lazy*) (dog OR dog*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_6()
        {
            //Arrange            
            const string inPhrase = "brown fox (lazy dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) (dog OR dog*))";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_7()
        {
            //Arrange            
            const string inPhrase = "brown fox (lazy AND dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) AND (dog OR dog*))";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_8()
        {
            //Arrange            
            const string inPhrase = "brown fox (lazy and dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) (and OR and*) (dog OR dog*))";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_1()
        {
            //Arrange            
            const string inPhrase = "\"brown fox\"";
            const string expected = inPhrase;

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_2()
        {
            //Arrange            
            const string inPhrase = "\"brown fox\" dog";
            const string expected = "\"brown fox\" (dog OR dog*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_3()
        {
            //Arrange            
            const string inPhrase = "The +\"brown fox\" -\"Lazy dog\" jumps";
            const string expected = "(The OR The*) +\"brown fox\" -\"Lazy dog\" (jumps OR jumps*)";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithTilde_4()
        {
            //Arrange            
            const string inPhrase = "dog jump~2";
            const string expected = "(dog OR dog*) jump~2";

            //Act
            var analyzed = _searchHelper.RephraseSearchText(inPhrase, true);

            //Assert
            Assert.AreEqual(expected, analyzed);
        }

        //[Test]
        //public void SearchHelper_Rephrase_##()
        //{
        //    //Arrange            
        //    const string inPhrase = "";
        //    const string expected = "";

        //    //Act
        //    var analyzed = _searchHelper.AnalyzeSearchText(inPhrase, false);

        //    //Assert
        //    Assert.AreEqual(expected, analyzed);
        //}

        #endregion
    }
}