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

namespace Dnn.PersonaBar.Pages.Tests
{
    [TestFixture]
    public class PagesControllerUnitTests
    {
        Mock<ITabController> _tabControllerMockMock;
        Mock<IModuleController> _moduleControllerMock;
        Mock<IPageUrlsController> _pageUrlsControllerMock;
        Mock<ITemplateController> _templateControllerMock;
        Mock<IDefaultPortalThemeController> _defaultPortalThemeControllerMock;
        Mock<ICloneModuleExecutionContext> _cloneModuleExecutionContextMock;
        Mock<IUrlRewriterUtilsWrapper> _urlRewriterUtilsWrapperMock;
        Mock<IFriendlyUrlWrapper> _friendlyUrlWrapperMock;
        Mock<IContentVerifier> _contentVerifierMock;

        PagesControllerImpl _pagesController;

        [SetUp]
        public void RunBeforeEachTest()
        {
            _tabControllerMockMock = new Mock<ITabController>();
            _moduleControllerMock = new Mock<IModuleController>();
            _pageUrlsControllerMock = new Mock<IPageUrlsController>();
            _templateControllerMock = new Mock<ITemplateController>();
            _defaultPortalThemeControllerMock = new Mock<IDefaultPortalThemeController>();
            _cloneModuleExecutionContextMock = new Mock<ICloneModuleExecutionContext>();
            _urlRewriterUtilsWrapperMock = new Mock<IUrlRewriterUtilsWrapper>();
            _friendlyUrlWrapperMock = new Mock<IFriendlyUrlWrapper>();
            _contentVerifierMock = new Mock<IContentVerifier>();
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

            _pagesController = new PagesControllerImpl(
                _tabControllerMockMock.Object,
                _moduleControllerMock.Object,
                _pageUrlsControllerMock.Object,
                _templateControllerMock.Object,
                _defaultPortalThemeControllerMock.Object,
                _cloneModuleExecutionContextMock.Object,
                _urlRewriterUtilsWrapperMock.Object,
                _friendlyUrlWrapperMock.Object,
                _contentVerifierMock.Object
                );

            PortalSettings portalSettings = new PortalSettings();
            PageSettings pageSettings = new PageSettings();
            pageSettings.Url = inputUrl;
            TabInfo tabInfo = new TabInfo();
            string inValidField = string.Empty;
            string errorMessage = string.Empty;

            // Act
            bool result = _pagesController.ValidatePageUrlSettings(portalSettings, pageSettings, tabInfo, ref inValidField, ref errorMessage);

            // Assert
            Assert.IsTrue(result);
            _urlRewriterUtilsWrapperMock.VerifyAll();
            _friendlyUrlWrapperMock.Verify(d => d.CleanNameForUrl(expected, friendlyOptions, out modified), Times.Once());
        }
    }
}
