// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PurgePageUnitTests
    {
        private Mock<ITabController> tabControllerMock;
        private Mock<IRecyclebinController> recycleBinControllerMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this.tabControllerMock = new Mock<ITabController>();
            this.recycleBinControllerMock = new Mock<IRecyclebinController>();
            this.contentVerifierMock = new Mock<IContentVerifier>();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.recycleBinControllerMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        public void Call_PurgePage_WithValidCommand_ShouldReturnSuccessResponse()
        {
            // Arrange
            int tabId = 91;
            int testPortalId = 1;
            TabInfo tab = new TabInfo();
            tab.TabID = tabId;
            tab.PortalID = testPortalId;
            PortalSettings portalSettings = new PortalSettings();
            portalSettings.PortalId = testPortalId;

            this.tabControllerMock.Setup(t => t.GetTab(tabId, testPortalId)).Returns(tab);
            this.contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(testPortalId, portalSettings, It.IsAny<bool>())).Returns(true);

            IConsoleCommand purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();

            // Assert
            Assert.That(result.IsError, Is.False);
        }

        [Test]
        public void Call_PurgePage_WithValidCommandAndPageContentNotAllowed_ShouldReturnErrorResponse()
        {
            // Arrange
            int tabId = 91;
            int portalId = 1;
            TabInfo tab = new TabInfo();
            tab.TabID = tabId;
            tab.PortalID = portalId;
            PortalSettings portalSettings = new PortalSettings();
            portalSettings.PortalId = portalId;

            this.tabControllerMock.Setup(t => t.GetTab(tabId, portalId)).Returns(tab);
            this.contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(portalId, portalSettings, It.IsAny<bool>())).Returns(false);

            IConsoleCommand purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();

            // Assert
            Assert.That(result.IsError, Is.True);
        }

        [Test]
        public void Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()
        {
            // Arrange
            int tabId = 919;
            PortalSettings portalSettings = new PortalSettings();

            IConsoleCommand purgeCommand = new PurgePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();

            // Assert
            Assert.That(result.IsError, Is.True);
            this.tabControllerMock.Verify(t => t.GetTab(tabId, portalSettings.PortalId));
        }
    }
}
