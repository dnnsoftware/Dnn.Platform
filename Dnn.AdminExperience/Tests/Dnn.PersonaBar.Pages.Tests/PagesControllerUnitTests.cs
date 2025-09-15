// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Services.Dto;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PagesControllerUnitTests
    {
        private Mock<IBusinessControllerProvider> businessControllerProviderMock;
        private Mock<ITabController> tabControllerMock;
        private Mock<IModuleController> moduleControllerMock;
        private Mock<IPageUrlsController> pageUrlsControllerMock;
        private Mock<ITemplateController> templateControllerMock;
        private Mock<IDefaultPortalThemeController> defaultPortalThemeControllerMock;
        private Mock<ICloneModuleExecutionContext> cloneModuleExecutionContextMock;
        private Mock<IUrlRewriterUtilsWrapper> urlRewriterUtilsWrapperMock;
        private Mock<IFriendlyUrlWrapper> friendlyUrlWrapperMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private PagesControllerImpl pagesController;
        private Mock<IPortalController> portalControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeEachTest()
        {
            this.businessControllerProviderMock = new Mock<IBusinessControllerProvider>();
            this.tabControllerMock = new Mock<ITabController>();
            this.moduleControllerMock = new Mock<IModuleController>();
            this.pageUrlsControllerMock = new Mock<IPageUrlsController>();
            this.templateControllerMock = new Mock<ITemplateController>();
            this.defaultPortalThemeControllerMock = new Mock<IDefaultPortalThemeController>();
            this.cloneModuleExecutionContextMock = new Mock<ICloneModuleExecutionContext>();
            this.urlRewriterUtilsWrapperMock = new Mock<IUrlRewriterUtilsWrapper>();
            this.friendlyUrlWrapperMock = new Mock<IFriendlyUrlWrapper>();
            this.contentVerifierMock = new Mock<IContentVerifier>();
            this.portalControllerMock = new Mock<IPortalController>();
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.businessControllerProviderMock.Object);
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.moduleControllerMock.Object);
                    services.AddSingleton(this.pageUrlsControllerMock.Object);
                    services.AddSingleton(this.templateControllerMock.Object);
                    services.AddSingleton(this.defaultPortalThemeControllerMock.Object);
                    services.AddSingleton(this.cloneModuleExecutionContextMock.Object);
                    services.AddSingleton(this.urlRewriterUtilsWrapperMock.Object);
                    services.AddSingleton(this.friendlyUrlWrapperMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                    services.AddSingleton(this.portalControllerMock.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
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

            this.urlRewriterUtilsWrapperMock.Setup(d => d.GetExtendOptionsForURLs(It.IsAny<int>())).Returns(friendlyOptions);
            this.friendlyUrlWrapperMock.Setup(d => d.CleanNameForUrl(It.IsAny<string>(), friendlyOptions, out modified)).Returns(expected);
            this.friendlyUrlWrapperMock.Setup(d => d.ValidateUrl(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PortalSettings>(), out modified));

            this.InitializePageController();

            PageSettings pageSettings = new PageSettings();
            pageSettings.Url = inputUrl;

            string inValidField = string.Empty;
            string errorMessage = string.Empty;

            // Act
            bool result = this.pagesController.ValidatePageUrlSettings(portalSettings, pageSettings, tab, ref inValidField, ref errorMessage);

            // Assert
            Assert.That(result, Is.True);
            this.urlRewriterUtilsWrapperMock.VerifyAll();
            this.friendlyUrlWrapperMock.Verify(d => d.CleanNameForUrl(expected, friendlyOptions, out modified), Times.Once());
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
            this.tabControllerMock.Setup(t => t.GetTab(It.IsAny<int>(), It.IsAny<int>())).Returns(tab);
            this.portalControllerMock.Setup(p => p.GetCurrentPortalSettings()).Returns(portalSettings);
            this.contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), false)).Returns(false);

            this.InitializePageController();

            // Act
            TestDelegate pageSettingsCall = () => this.pagesController.GetPageSettings(tabId, null);

            // Assert
            Assert.Throws<PageNotFoundException>(pageSettingsCall);
            this.portalControllerMock.Verify(p => p.GetCurrentPortalSettings(), Times.Exactly(2));
            this.contentVerifierMock.Verify(c => c.IsContentExistsForRequestedPortal(portalId, portalSettings, false));
        }

        private void InitializePageController()
        {
            this.pagesController = new PagesControllerImpl(
                this.businessControllerProviderMock.Object,
                this.tabControllerMock.Object,
                this.moduleControllerMock.Object,
                this.pageUrlsControllerMock.Object,
                this.templateControllerMock.Object,
                this.defaultPortalThemeControllerMock.Object,
                this.cloneModuleExecutionContextMock.Object,
                this.urlRewriterUtilsWrapperMock.Object,
                this.friendlyUrlWrapperMock.Object,
                this.contentVerifierMock.Object,
                this.portalControllerMock.Object);
        }
    }
}
