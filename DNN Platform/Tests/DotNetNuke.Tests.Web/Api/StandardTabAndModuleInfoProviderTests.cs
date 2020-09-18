// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Net.Http;
    using System.Web;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.Api;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class StandardTabAndModuleInfoProviderTests
    {
        private const int ValidPortalId = 0;
        private const int ValidTabModuleId = 999;
        private const int ValidModuleId = 456;
        private const int ValidTabId = 46;

        private const string MonikerSettingName = "Moniker";
        private const string MonikerSettingValue = "TestMoniker";

        private Mock<IModuleController> _mockModuleController;
        private IModuleController _moduleController;
        private Mock<ITabController> _mockTabController;
        private Mock<ITabModulesController> _mockTabModuleController;
        private Mock<DataProvider> _mockDataProvider;

        private ITabController _tabController;
        private ITabModulesController _tabModuleController;
        private TabInfo _tabInfo;
        private ModuleInfo _moduleInfo;

        [SetUp]
        public void Setup()
        {
            MockComponentProvider.CreateDataCacheProvider();
            this._mockDataProvider = MockComponentProvider.CreateDataProvider();
            this._mockDataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
            this._mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);

            this.RegisterMock(ModuleController.SetTestableInstance, out this._mockModuleController, out this._moduleController);
            this.RegisterMock(TabController.SetTestableInstance, out this._mockTabController, out this._tabController);
            this.RegisterMock(TabModulesController.SetTestableInstance, out this._mockTabModuleController, out this._tabModuleController);

            this._tabInfo = new TabInfo { TabID = ValidTabId };
            this._moduleInfo = new ModuleInfo
            {
                TabModuleID = ValidTabModuleId,
                TabID = ValidTabId,
                ModuleID = ValidModuleId,
                PortalID = ValidPortalId,
            };

            this._mockTabController.Setup(x => x.GetTab(ValidTabId, ValidPortalId)).Returns(this._tabInfo);
            this._mockModuleController.Setup(x => x.GetModule(ValidModuleId, ValidTabId, false)).Returns(this._moduleInfo);
            this._mockModuleController.Setup(x => x.GetTabModule(ValidTabModuleId)).Returns(this._moduleInfo);
            this._mockTabModuleController.Setup(x => x.GetTabModuleIdsBySetting(MonikerSettingName, MonikerSettingValue)).Returns(
                new List<int> { ValidTabModuleId });
            this._mockTabModuleController.Setup(x => x.GetTabModuleSettingsByName(MonikerSettingName)).Returns(
                new Dictionary<int, string> { { ValidTabModuleId, MonikerSettingValue } });
        }

        [TearDown]
        public void TearDown()
        {
            ModuleController.ClearInstance();
            TabController.ClearInstance();
        }

        [Test]
        public void ValidTabAndModuleIdLoadsActiveModule()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.Headers.Add("moduleid", ValidModuleId.ToString(CultureInfo.InvariantCulture));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            // Assert
            Assert.IsTrue(result);
            Assert.AreSame(this._moduleInfo, returnedModuleInfo);
        }

        [Test]
        public void ExistingMonikerValueInHeaderShouldFindTheCorrectModuleInfo()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("X-DNN-MONIKER", MonikerSettingValue);

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            // Assert
            Assert.IsTrue(result);
            Assert.AreSame(this._moduleInfo, returnedModuleInfo);
        }

        [Test]
        public void ExistingMonikerValueInQueryStringShouldFindTheCorrectModuleInfo()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(
                "http://mysite.lvh.me/API/internalservices/controlbar/ToggleUserMode?moniker=" + HttpUtility.UrlEncode(MonikerSettingValue));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            // Assert
            Assert.IsTrue(result);
            Assert.AreSame(this._moduleInfo, returnedModuleInfo);
        }

        [Test]
        public void NonExistingMonikerValueShouldFailToReturnResult()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("X-DNN-MONIKER", "This moniker does not exist");

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(returnedModuleInfo);
        }

        [Test]
        public void MissingAllHeadersShouldFailToReturnResult()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(returnedModuleInfo);
        }

        [Test]
        public void OmittedTabIdWillNotLoadModule()
        {
            // Arrange
            // no tabid
            var request = new HttpRequestMessage();
            request.Headers.Add("moduleid", ValidModuleId.ToString(CultureInfo.InvariantCulture));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            // Assert
            this._mockTabController.Verify(x => x.GetTab(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            this._mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
            Assert.IsNull(returnedModuleInfo);
            Assert.IsFalse(result);
        }

        [Test]
        public void OmittedModuleIdWillNotLoadModule()
        {
            // Arrange
            // no moduleid
            var request = new HttpRequestMessage();
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            // Assert
            this._mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
            Assert.IsNull(returnedModuleInfo);
            Assert.IsFalse(result);
        }

        [Test]
        public void TabIdInHeaderTakesPriority()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", "tabid", ValidTabId + 1));

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);

            // Assert
            Assert.AreEqual(ValidTabId, tabId);
            Assert.IsTrue(result);
        }

        [Test]
        public void ModuleIdInHeaderTakesPriority()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("moduleid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", "moduleid", ValidTabId + 1));

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            // Assert
            Assert.AreEqual(ValidTabId, moduleId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("tabid")]
        [TestCase("TABID")]
        [TestCase("tAbiD")]
        public void TabIdInHeaderAllowsTabIdToBeFound(string headerName)
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add(headerName, ValidTabId.ToString(CultureInfo.InvariantCulture));

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);

            // Assert
            Assert.AreEqual(ValidTabId, tabId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("moduleid")]
        [TestCase("MODULEID")]
        [TestCase("modULeid")]
        public void ModuleIdInHeaderAllowsModuleIdToBeFound(string headerName)
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add(headerName, ValidModuleId.ToString(CultureInfo.InvariantCulture));

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            // Assert
            Assert.AreEqual(ValidModuleId, moduleId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("x-dnn-moniker")]
        [TestCase("X-Dnn-Moniker")]
        [TestCase("X-DNN-MONIKER")]
        public void MonikerInHeaderAllowsModuleToBeFound(string headerName)
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add(headerName, MonikerSettingValue);

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(ValidModuleId, moduleId);
        }

        [Test]
        [TestCase("tabid")]
        [TestCase("TABID")]
        [TestCase("tAbiD")]
        public void TabIdInQueryStringAllowsTabIdToBeFound(string paramName)
        {
            // Arrange
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidTabId)),
            };

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);

            // Assert
            Assert.AreEqual(ValidTabId, tabId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("moduleid")]
        [TestCase("MODULEID")]
        [TestCase("modULeid")]
        public void ModuleIdInQueryStringAllowsModuleIdToBeFound(string paramName)
        {
            // Arrange
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidModuleId)),
            };

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            // Assert
            Assert.AreEqual(ValidModuleId, moduleId);
            Assert.IsTrue(result);
        }

        [Test]
        public void NoTabIdInRequestReturnsNoTabId()
        {
            // Arrange

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(new HttpRequestMessage(), out tabId);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(-1, tabId);
        }

        [Test]
        public void NoModuleIdInRequestReturnsNoModuleId()
        {
            // Arrange

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(new HttpRequestMessage(), out moduleId);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(-1, moduleId);
        }

        private static IDataReader GetPortalsCallBack(string culture)
        {
            return GetPortalCallBack(0, culture);
        }

        private static IDataReader GetPortalCallBack(int portalId, string culture)
        {
            var table = new DataTable("Portal");

            var cols = new[]
            {
                "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate",
                "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
                "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId",
                "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor",
                "ProcessorUserId",
                "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset",
                "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
                "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId",
                "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate",
                "CultureCode",
            };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright (c) 2018 DNN Corp.", null,
                "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website",
                "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020",
                null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0", null,
                homePage.ToString(), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11",
                "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }

        private void RegisterMock<T>(Action<T> register, out Mock<T> mock, out T instance)
            where T : class
        {
            mock = new Mock<T>();
            instance = mock.Object;
            register(instance);
        }
    }
}
