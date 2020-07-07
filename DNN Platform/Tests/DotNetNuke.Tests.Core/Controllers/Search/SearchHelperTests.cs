// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///  Testing various aspects of SearchController.
    /// </summary>
    [TestFixture]
    public class SearchHelperTests
    {
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
        private Mock<DataProvider> _dataProvider;
        private Mock<CachingProvider> _cachingProvider;
        private SearchHelperImpl _searchHelper;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            this._cachingProvider = MockComponentProvider.CreateDataCacheProvider();
            this._dataProvider = MockComponentProvider.CreateDataProvider();
            this.SetupDataProvider();
            this._searchHelper = new SearchHelperImpl();
            DataCache.ClearCache();
        }

        [Test]
        public void SearchHelper_GetSynonyms_Two_Terms_Returns_Correct_Results()
        {
            // Arrange

            // Act
            var synonyms = this._searchHelper.GetSynonyms(PortalId0, CultureEnUs, TermDNN).ToArray();

            // Assert
            Assert.AreEqual(1, synonyms.Count());
            Assert.AreEqual(TermDotNetNuke.ToLowerInvariant(), synonyms[0]);
        }

        [Test]
        public void SearchHelper_GetSynonyms_Three_Terms_Returns_Correct_Results()
        {
            // Arrange

            // Act
            var synonyms = this._searchHelper.GetSynonyms(PortalId0, CultureEnUs, TermHop).ToArray();

            // Assert
            Assert.AreEqual(2, synonyms.Count());
            Assert.AreEqual(TermJump.ToLowerInvariant(), synonyms[0]);
            Assert.AreEqual(TermLeap.ToLowerInvariant(), synonyms[1]);
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardButExact_1()
        {
            // Arrange
            const string inPhrase = "\"brown fox\"";
            const string expected = inPhrase;

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, false);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardButExact_2()
        {
            // Arrange
            const string inPhrase = "\"brown fox\" -\"(Lazy) dog\" jumps";
            const string expected = inPhrase;

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, false);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardNoExact()
        {
            // Arrange
            const string inPhrase = "(brown) OR (fox AND dog) +jumbs";
            const string expected = inPhrase;

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, false);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_0()
        {
            // Arrange
            const string inPhrase = "fox dog";
            const string expected = "(fox OR fox*) (dog OR dog*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_1()
        {
            // Arrange
            const string inPhrase = "(lazy-dog)";
            const string expected = "(lazy-dog OR lazy-dog*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_2()
        {
            // Arrange
            const string inPhrase = "fox (dog)";
            const string expected = "(fox OR fox*) (dog OR dog*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_3()
        {
            // Arrange
            const string inPhrase = "(dog) fox";
            const string expected = "(dog OR dog*) (fox OR fox*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_4()
        {
            // Arrange
            const string inPhrase = "brown fox (lazy) dog";
            const string expected = "(brown OR brown*) (fox OR fox*) (lazy OR lazy*) (dog OR dog*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_5()
        {
            // Arrange
            const string inPhrase = "(brown fox) lazy dog";
            const string expected = "((brown OR brown*) (fox OR fox*)) (lazy OR lazy*) (dog OR dog*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_6()
        {
            // Arrange
            const string inPhrase = "brown fox (lazy dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) (dog OR dog*))";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_7()
        {
            // Arrange
            const string inPhrase = "brown fox (lazy AND dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) AND (dog OR dog*))";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_8()
        {
            // Arrange
            const string inPhrase = "brown fox (lazy and dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) (and OR and*) (dog OR dog*))";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_1()
        {
            // Arrange
            const string inPhrase = "\"brown fox\"";
            const string expected = inPhrase;

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_2()
        {
            // Arrange
            const string inPhrase = "\"brown fox\" dog";
            const string expected = "\"brown fox\" (dog OR dog*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_3()
        {
            // Arrange
            const string inPhrase = "The +\"brown fox\" -\"Lazy dog\" jumps";
            const string expected = "(The OR The*) +\"brown fox\" -\"Lazy dog\" (jumps OR jumps*)";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithTilde_4()
        {
            // Arrange
            const string inPhrase = "dog jump~2";
            const string expected = "(dog OR dog*) jump~2";

            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        [Test]

        // Arrange
        [TestCase("Cäu", "(Cau OR Cau*)")]
        [TestCase("Cäutätörül", "(Cautatorul OR Cautatorul*)")]
        [TestCase("Ãbcdef", "(Abcdef OR Abcdef*)")]
        public void SearchHelper_Rephrase_AccentedCharsReplaced_Replaced(string inPhrase, string expected)
        {
            // Act
            var analyzed = this._searchHelper.RephraseSearchText(inPhrase, true);

            // Assert
            Assert.AreEqual(expected, analyzed);
        }

        private void SetupDataProvider()
        {
            // Standard DataProvider Path for Logging
            this._dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);

            this._dataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(this.GetPortalsCallBack);

            this._dataProvider.Setup(d => d.GetPortalSettings(It.IsAny<int>(), It.IsAny<string>())).Returns<int, string>(this.GetPortalSettingsCallBack);

            var dataTable = new DataTable("SynonymsGroups");
            var pkId = dataTable.Columns.Add("SynonymsGroupID", typeof(int));
            dataTable.Columns.Add("SynonymsTags", typeof(string));
            dataTable.Columns.Add("CreatedByUserID", typeof(int));
            dataTable.Columns.Add("CreatedOnDate", typeof(DateTime));
            dataTable.Columns.Add("LastModifiedByUserID", typeof(int));
            dataTable.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            dataTable.Columns.Add("PortalID", typeof(int));

            dataTable.PrimaryKey = new[] { pkId };

            // Create some test data
            var now = DateTime.Now;
            dataTable.Rows.Add(1, string.Join(",", new[] { TermDNN, TermDotNetNuke }), 1, now, 1, now, 0);
            dataTable.Rows.Add(2, string.Join(",", new[] { TermLaptop, TermNotebook }), 1, now, 1, now, 0);
            dataTable.Rows.Add(3, string.Join(",", new[] { TermJump, TermLeap, TermHop }), 1, now, 1, now, 0);

            this._dataProvider.Setup(x => x.GetAllSynonymsGroups(0, It.IsAny<string>())).Returns(dataTable.CreateDataReader());
        }

        private IDataReader GetPortalSettingsCallBack(int portalId, string culture)
        {
            var table = new DataTable("PortalSettings");

            var cols = new string[]
                           {
                            "SettingName", "SettingValue", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode",
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
            return this.GetPortalCallBack(PortalId0, CultureEnUs);
        }

        private IDataReader GetPortalCallBack(int portalId, string culture)
        {
            DataTable table = new DataTable("Portal");

            var cols = new string[]
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

            var homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null, "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0", null, homePage.ToString(), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }
    }
}
