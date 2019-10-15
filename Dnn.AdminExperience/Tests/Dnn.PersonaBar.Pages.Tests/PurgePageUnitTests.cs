using System;
using NUnit.Framework;
using Moq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using Dnn.PersonaBar.Library.Helper;

namespace Dnn.PersonaBar.Pages.Tests
{
    [TestFixture]
    public class PurgePageUnitTests
    {
        Mock<ITabController> _tabControllerMock;
        Mock<IRecyclebinController> _recyclebinControllerMock;
        Mock<IContentVerifier> _contentVerifierMock;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            _tabControllerMock = new Mock<ITabController>();
            _recyclebinControllerMock = new Mock<IRecyclebinController>();
            _contentVerifierMock = new Mock<IContentVerifier>();
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

            _tabControllerMock.Setup(t => t.GetTab(tabId, testPortalId)).Returns(tab);
            _contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(testPortalId, portalSettings, It.IsAny<Boolean>())).Returns(true);

            IConsoleCommand purgeCommand = new PurgePage(_tabControllerMock.Object, _recyclebinControllerMock.Object, _contentVerifierMock.Object);

            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();

            // Assert
            Assert.IsFalse(result.IsError);
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

            _tabControllerMock.Setup(t => t.GetTab(tabId, portalId)).Returns(tab);
            _contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(portalId, portalSettings, It.IsAny<Boolean>())).Returns(false);

            IConsoleCommand purgeCommand = new PurgePage(_tabControllerMock.Object, _recyclebinControllerMock.Object, _contentVerifierMock.Object);

            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Call_PurgePage_PageDoesNotExist_ShouldReturnErrorResponse()
        {
            // Arrange
            int tabId = 919;
            PortalSettings portalSettings = new PortalSettings();

            IConsoleCommand purgeCommand = new PurgePage(_tabControllerMock.Object, _recyclebinControllerMock.Object, _contentVerifierMock.Object);

            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            _tabControllerMock.Verify(t => t.GetTab(tabId, portalSettings.PortalId));
        }
    }
}
