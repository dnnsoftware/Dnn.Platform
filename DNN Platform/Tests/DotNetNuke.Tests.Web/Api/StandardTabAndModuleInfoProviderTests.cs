using System;
using System.Globalization;
using System.Net.Http;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.Internal;
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
        private ITabController _tabController;

        private const int ValidPortalId = 0;
        private const int ValidModuleId = 456;
        private const int ValidTabId = 46;

        [SetUp]
        public void Setup()
        {
            RegisterMock(TestableModuleController.SetTestableInstance, out _mockModuleController, out _moduleController);
            RegisterMock(TestableTabController.SetTestableInstance, out _mockTabController, out _tabController);
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
            
            _mockTabController.Setup(x => x.GetTab(ValidTabId, ValidPortalId)).Returns(new TabInfo());
            var moduleInfo = new ModuleInfo();
            _mockModuleController.Setup(x => x.GetModule(ValidModuleId, ValidTabId)).Returns(moduleInfo);

            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            Assert.IsTrue(result);
            Assert.AreSame(moduleInfo, returnedModuleInfo);
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
            _mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
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
            
            _mockTabController.Setup(x => x.GetTab(ValidTabId, ValidPortalId)).Returns(new TabInfo());

            //Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);

            //Assert
            _mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
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
        [TestCase("tabid")]
        [TestCase("TABID")]
        [TestCase("tAbiD")]
        public void TabIdInQueryStringAllowsTabIdToBeFound(string paramName)
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidTabId));

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
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidModuleId));

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