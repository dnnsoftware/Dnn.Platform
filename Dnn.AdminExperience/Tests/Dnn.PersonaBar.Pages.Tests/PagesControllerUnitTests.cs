// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Tests
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PagesControllerUnitTests
    {
        private Mock<ITabController> _tabControllerMock;
        private Mock<IModuleController> _moduleControllerMock;
        private Mock<IPageUrlsController> _pageUrlsControllerMock;
        private Mock<ITemplateController> _templateControllerMock;
        private Mock<IDefaultPortalThemeController> _defaultPortalThemeControllerMock;
        private Mock<ICloneModuleExecutionContext> _cloneModuleExecutionContextMock;
        private Mock<IUrlRewriterUtilsWrapper> _urlRewriterUtilsWrapperMock;
        private Mock<IFriendlyUrlWrapper> _friendlyUrlWrapperMock;
        private Mock<IContentVerifier> _contentVerifierMock;
        PagesControllerImpl _pagesController;
        Mock<IPortalController> _portalControllerMock;

        [SetUp]
        public void RunBeforeEachTest()
        {
            this._tabControllerMock = new Mock<ITabController>();
            this._moduleControllerMock = new Mock<IModuleController>();
            this._pageUrlsControllerMock = new Mock<IPageUrlsController>();
            this._templateControllerMock = new Mock<ITemplateController>();
            this._defaultPortalThemeControllerMock = new Mock<IDefaultPortalThemeController>();
            this._cloneModuleExecutionContextMock = new Mock<ICloneModuleExecutionContext>();
            this._urlRewriterUtilsWrapperMock = new Mock<IUrlRewriterUtilsWrapper>();
            this._friendlyUrlWrapperMock = new Mock<IFriendlyUrlWrapper>();
            this._contentVerifierMock = new Mock<IContentVerifier>();
            this._portalControllerMock = new Mock<IPortalController>();
        }

        [TestCase("http://www.websitename.com/home/", "/home")]
        [TestCase("/news/", "/news")]
        [TestCase("contactus/", "contactus")]
        [TestCase("blogs", "blogs")]
        public void ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(string inputUrl, string expected)
        {
            // Arrange
            var modified = false;
            var tab = new TabInfo();
            var portalSettings = new PortalSettings();
            var friendlyOptions = new FriendlyUrlOptions();

            this._urlRewriterUtilsWrapperMock.Setup(d => d.GetExtendOptionsForURLs(It.IsAny<int>())).Returns(friendlyOptions);
            this._friendlyUrlWrapperMock.Setup(d => d.CleanNameForUrl(It.IsAny<string>(), friendlyOptions, out modified)).Returns(expected);
            this._friendlyUrlWrapperMock.Setup(d => d.ValidateUrl(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PortalSettings>(), out modified));

            this.InitializePageController();

            PageSettings pageSettings = new PageSettings();
            pageSettings.Url = inputUrl;

            string inValidField = string.Empty;
            string errorMessage = string.Empty;

            // Act
            bool result = this._pagesController.ValidatePageUrlSettings(portalSettings, pageSettings, tab, ref inValidField, ref errorMessage);

            // Assert
            Assert.IsTrue(result);
            this._urlRewriterUtilsWrapperMock.VerifyAll();
            this._friendlyUrlWrapperMock.Verify(d => d.CleanNameForUrl(expected, friendlyOptions, out modified), Times.Once());
        }

        [Test]
        public void GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull()
        {
            var tabId = 0;
            var portalId = 0;

            var tab = new TabInfo();
            tab.PortalID = portalId;

            var portalSettings = new PortalSettings();

            // Arrange
            this._tabControllerMock.Setup(t => t.GetTab(It.IsAny<int>(), It.IsAny<int>())).Returns(tab);
            this._portalControllerMock.Setup(p => p.GetCurrentPortalSettings()).Returns(portalSettings);
            this._contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), false)).Returns(false);

            this.InitializePageController();

            // Act
            TestDelegate pageSettingsCall = () => this._pagesController.GetPageSettings(tabId, null);

            // Assert
            Assert.Throws<PageNotFoundException>(pageSettingsCall);
            this._portalControllerMock.Verify(p => p.GetCurrentPortalSettings(), Times.Exactly(2));
            this._contentVerifierMock.Verify(c => c.IsContentExistsForRequestedPortal(portalId, portalSettings, false));
        }

        private void InitializePageController()
        {
            this._pagesController = new PagesControllerImpl(
                this._tabControllerMock.Object,
                this._moduleControllerMock.Object,
                this._pageUrlsControllerMock.Object,
                this._templateControllerMock.Object,
                this._defaultPortalThemeControllerMock.Object,
                this._cloneModuleExecutionContextMock.Object,
                this._urlRewriterUtilsWrapperMock.Object,
                this._friendlyUrlWrapperMock.Object,
                this._contentVerifierMock.Object,
                this._portalControllerMock.Object);
        }
    }
}
