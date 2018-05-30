using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Services.Dto;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Pages.Components.Exceptions;

namespace Dnn.PersonaBar.Pages.Tests
{
    [TestFixture]
    public class PagesControllerUnitTests
    {
        Mock<ITabController> _tabControllerMock;
        Mock<IModuleController> _moduleControllerMock;
        Mock<IPageUrlsController> _pageUrlsControllerMock;
        Mock<ITemplateController> _templateControllerMock;
        Mock<IDefaultPortalThemeController> _defaultPortalThemeControllerMock;
        Mock<ICloneModuleExecutionContext> _cloneModuleExecutionContextMock;
        Mock<IUrlRewriterUtilsWrapper> _urlRewriterUtilsWrapperMock;
        Mock<IFriendlyUrlWrapper> _friendlyUrlWrapperMock;
        Mock<IContentVerifier> _contentVerifierMock;
        PagesControllerImpl _pagesController;
        Mock<IPortalController> _portalControllerMock;

        TabInfo _tab = new TabInfo();
        PortalSettings _portalSettings = new PortalSettings();

        [SetUp]
        public void RunBeforeEachTest()
        {
            _tabControllerMock = new Mock<ITabController>();
            _moduleControllerMock = new Mock<IModuleController>();
            _pageUrlsControllerMock = new Mock<IPageUrlsController>();
            _templateControllerMock = new Mock<ITemplateController>();
            _defaultPortalThemeControllerMock = new Mock<IDefaultPortalThemeController>();
            _cloneModuleExecutionContextMock = new Mock<ICloneModuleExecutionContext>();
            _urlRewriterUtilsWrapperMock = new Mock<IUrlRewriterUtilsWrapper>();
            _friendlyUrlWrapperMock = new Mock<IFriendlyUrlWrapper>();
            _contentVerifierMock = new Mock<IContentVerifier>();
            _portalControllerMock = new Mock<IPortalController>();
        }

        [TestCase("http://www.websitename.com/home/", "/home")]
        [TestCase("/news/", "/news")]
        [TestCase("contactus/", "contactus")]
        [TestCase("blogs", "blogs")]
        public void ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(string inputUrl, string expected)
        {
            // Arrange
            var friendlyOptions = new FriendlyUrlOptions();
            var modified = false;

            _urlRewriterUtilsWrapperMock.Setup(d => d.GetExtendOptionsForURLs(It.IsAny<int>())).Returns(friendlyOptions);
            _friendlyUrlWrapperMock.Setup(d => d.CleanNameForUrl(It.IsAny<string>(), friendlyOptions, out modified)).Returns(expected);
            _friendlyUrlWrapperMock.Setup(d => d.ValidateUrl(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PortalSettings>(), out modified));

            InitializePageController();

            PageSettings pageSettings = new PageSettings();
            pageSettings.Url = inputUrl;

            string inValidField = string.Empty;
            string errorMessage = string.Empty;

            // Act
            bool result = _pagesController.ValidatePageUrlSettings(_portalSettings, pageSettings, _tab, ref inValidField, ref errorMessage);

            // Assert
            Assert.IsTrue(result);
            _urlRewriterUtilsWrapperMock.VerifyAll();
            _friendlyUrlWrapperMock.Verify(d => d.CleanNameForUrl(expected, friendlyOptions, out modified), Times.Once());
        }
                
        [Test]
        public void GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull()
        {
            int tabId = 0;
            int portalId = 0;
            _tab.PortalID = portalId;

            // Arrange
            _tabControllerMock.Setup(t => t.GetTab(It.IsAny<int>(), It.IsAny<int>())).Returns(_tab);
            _portalControllerMock.Setup(p => p.GetCurrentPortalSettings()).Returns(_portalSettings);
            _contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), false)).Returns(false);

            InitializePageController();

            // Act
            TestDelegate pageSettingsCall = () =>  _pagesController.GetPageSettings(tabId, null);

            // Assert
            Assert.Throws<PageNotFoundException>(pageSettingsCall);
            _portalControllerMock.Verify(p => p.GetCurrentPortalSettings(), Times.Exactly(2));
            _contentVerifierMock.Verify(c => c.IsContentExistsForRequestedPortal(portalId, _portalSettings, false));
        }

        private void InitializePageController()
        {
            _pagesController = new PagesControllerImpl(
                _tabControllerMock.Object,
                _moduleControllerMock.Object,
                _pageUrlsControllerMock.Object,
                _templateControllerMock.Object,
                _defaultPortalThemeControllerMock.Object,
                _cloneModuleExecutionContextMock.Object,
                _urlRewriterUtilsWrapperMock.Object,
                _friendlyUrlWrapperMock.Object,
                _contentVerifierMock.Object,
                _portalControllerMock.Object
                );
        }
    }
}
