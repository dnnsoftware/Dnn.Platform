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

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class StandardTabAndModuleInfoProviderTests
    {
        private Mock<IModuleController> _mockModuleController;
        private IModuleController _moduleController;
        private Mock<ITabController> _mockTabController;
        private Mock<ITabModulesController> _mockTabModuleController;
        private Mock<DataProvider> _mockDataProvider;

        private ITabController _tabController;
        private ITabModulesController _tabModuleController;
        private TabInfo _tabInfo;
        private ModuleInfo _moduleInfo;

        private const int ValidPortalId = 0;
        private const int ValidTabModuleId = 999;
        private const int ValidModuleId = 456;
        private const int ValidTabId = 46;

        private const string MonikerSettingName = "Moniker";
        private const string MonikerSettingValue = "TestMoniker";

        [SetUp]
        public void Setup()
        {
            MockComponentProvider.CreateDataCacheProvider();
            _mockDataProvider = MockComponentProvider.CreateDataProvider();
            _mockDataProvider.Setup(d => d.GetProviderPath()).Returns("");
            _mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);

            RegisterMock(ModuleController.SetTestableInstance, out _mockModuleController, out _moduleController);
            RegisterMock(TabController.SetTestableInstance, out _mockTabController, out _tabController);
            RegisterMock(TabModulesController.SetTestableInstance, out _mockTabModuleController, out _tabModuleController);

            _tabInfo = new TabInfo { TabID = ValidTabId };
            _moduleInfo = new ModuleInfo
            {
                TabModuleID  = ValidTabModuleId, TabID = ValidTabId, ModuleID = ValidModuleId, PortalID = ValidPortalId
            };

            _mockTabController.Setup(x => x.GetTab(ValidTabId, ValidPortalId)).Returns(_tabInfo);
            _mockModuleController.Setup(x => x.GetModule(ValidModuleId, ValidTabId, false)).Returns(_moduleInfo);
            _mockModuleController.Setup(x => x.GetTabModule(ValidTabModuleId)).Returns(_moduleInfo);
            _mockTabModuleController.Setup(x => x.GetTabModuleIdsBySetting(MonikerSettingName, MonikerSettingValue)).Returns(
                new List<int> { ValidTabModuleId });
            _mockTabModuleController.Setup(x => x.GetTabModuleSettingsByName(MonikerSettingName)).Returns(
                new Dictionary<int, string> { { ValidTabModuleId, MonikerSettingValue } });
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
                "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "SuperTabId",
                "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate",
                "CultureCode"
            };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright (c) 2018 DNN Corp.", null,
                "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website",
                "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020",
                null, null, null, "0", "admin@change.me", "en-US", "-8", "58", "Portals/0", null,
                homePage.ToString(), null, null, "57", "56", "-1", "-1", "7", "-1", "2011-08-25 07:34:11",
                "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }

        [TearDown]
        public void TearDown()
        {
            ModuleController.ClearInstance();
            TabController.ClearInstance();
        }

        private void RegisterMock<T>(Action<T> register, out Mock<T> mock, out T instance) where T : class
        {
            mock = new Mock<T>();
            instance = mock.Object;
            register(instance);
        }

        [Test]
        public void ValidTabAndModuleIdLoadsActiveModule()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.Headers.Add("moduleid", ValidModuleId.ToString(CultureInfo.InvariantCulture));
            
            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            Assert.IsTrue(result);
            Assert.AreSame(_moduleInfo, returnedModuleInfo);
        }

        [Test]
        public void ExistingMonikerValueInHeaderShouldFindTheCorrectModuleInfo()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("X-DNN-MONIKER", MonikerSettingValue);

            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            Assert.IsTrue(result);
            Assert.AreSame(_moduleInfo, returnedModuleInfo);
        }

        [Test]
        public void ExistingMonikerValueInQueryStringShouldFindTheCorrectModuleInfo()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(
                "http://mysite.lvh.me/API/internalservices/controlbar/ToggleUserMode?moniker=" + HttpUtility.UrlEncode(MonikerSettingValue));

            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            Assert.IsTrue(result);
            Assert.AreSame(_moduleInfo, returnedModuleInfo);
        }

        [Test]
        public void NonExistingMonikerValueShouldFailToReturnResult()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("X-DNN-MONIKER", "This moniker does not exist");

            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            Assert.IsFalse(result);
            Assert.IsNull(returnedModuleInfo);
        }

        [Test]
        public void MissingAllHeadersShouldFailToReturnResult()
        {
            //Arrange
            var request = new HttpRequestMessage();

            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            Assert.IsFalse(result);
            Assert.IsNull(returnedModuleInfo);
        }

        [Test]
        public void OmittedTabIdWillNotLoadModule()
        {
            //Arrange
            //no tabid
            var request = new HttpRequestMessage();
            request.Headers.Add("moduleid", ValidModuleId.ToString(CultureInfo.InvariantCulture));
            
            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            _mockTabController.Verify(x => x.GetTab(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            _mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
            Assert.IsNull(returnedModuleInfo);
            Assert.IsFalse(result);
        }

        [Test]
        public void OmittedModuleIdWillNotLoadModule()
        {
            //Arrange
            //no moduleid
            var request = new HttpRequestMessage();
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));

            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            _mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
            Assert.IsNull(returnedModuleInfo);
            Assert.IsFalse(result);
        }

        [Test]
        public void TabIdInHeaderTakesPriority()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", "tabid", ValidTabId + 1));
            
            //Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);

            //Assert
            Assert.AreEqual(ValidTabId, tabId);
            Assert.IsTrue(result);
        }

        [Test]
        public void ModuleIdInHeaderTakesPriority()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("moduleid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", "moduleid", ValidTabId + 1));

            //Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            //Assert
            Assert.AreEqual(ValidTabId, moduleId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("tabid")]
        [TestCase("TABID")]
        [TestCase("tAbiD")]
        public void TabIdInHeaderAllowsTabIdToBeFound(string headerName)
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add(headerName, ValidTabId.ToString(CultureInfo.InvariantCulture));

            //Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);

            //Assert
            Assert.AreEqual(ValidTabId, tabId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("moduleid")]
        [TestCase("MODULEID")]
        [TestCase("modULeid")]
        public void ModuleIdInHeaderAllowsModuleIdToBeFound(string headerName)
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add(headerName, ValidModuleId.ToString(CultureInfo.InvariantCulture));

            //Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            //Assert
            Assert.AreEqual(ValidModuleId, moduleId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("x-dnn-moniker")]
        [TestCase("X-Dnn-Moniker")]
        [TestCase("X-DNN-MONIKER")]
        public void MonikerInHeaderAllowsModuleToBeFound(string headerName)
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add(headerName, MonikerSettingValue);

            //Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(ValidModuleId, moduleId);
        }

        [Test]
        [TestCase("tabid")]
        [TestCase("TABID")]
        [TestCase("tAbiD")]
        public void TabIdInQueryStringAllowsTabIdToBeFound(string paramName)
        {
            //Arrange
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidTabId))
            };

            //Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);

            //Assert
            Assert.AreEqual(ValidTabId, tabId);
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("moduleid")]
        [TestCase("MODULEID")]
        [TestCase("modULeid")]
        public void ModuleIdInQueryStringAllowsModuleIdToBeFound(string paramName)
        {
            //Arrange
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidModuleId))
            };

            //Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);

            //Assert
            Assert.AreEqual(ValidModuleId, moduleId);
            Assert.IsTrue(result);
        }

        [Test]
        public void NoTabIdInRequestReturnsNoTabId()
        {
            //Arrange

            //Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(new HttpRequestMessage(), out tabId);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(-1, tabId);
        }

        [Test]
        public void NoModuleIdInRequestReturnsNoModuleId()
        {
            //Arrange

            //Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(new HttpRequestMessage(), out moduleId);

            //Assert
            Assert.IsFalse(result);
            Assert.AreEqual(-1, moduleId);
        }
    }
}