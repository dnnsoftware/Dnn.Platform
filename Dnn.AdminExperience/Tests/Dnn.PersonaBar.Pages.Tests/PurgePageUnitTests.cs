// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Tests
{
    using System;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PurgePageUnitTests
    {
        private Mock<ITabController> _tabControllerMock;
        private Mock<IRecyclebinController> _recyclebinControllerMock;
        private Mock<IContentVerifier> _contentVerifierMock;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this._tabControllerMock = new Mock<ITabController>();
            this._recyclebinControllerMock = new Mock<IRecyclebinController>();
            this._contentVerifierMock = new Mock<IContentVerifier>();
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

            this._tabControllerMock.Setup(t => t.GetTab(tabId, testPortalId)).Returns(tab);
            this._contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(testPortalId, portalSettings, It.IsAny<bool>())).Returns(true);

            IConsoleCommand purgeCommand = new PurgePage(this._tabControllerMock.Object, this._recyclebinControllerMock.Object, this._contentVerifierMock.Object);

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

            this._tabControllerMock.Setup(t => t.GetTab(tabId, portalId)).Returns(tab);
            this._contentVerifierMock.Setup(p => p.IsContentExistsForRequestedPortal(portalId, portalSettings, It.IsAny<bool>())).Returns(false);

            IConsoleCommand purgeCommand = new PurgePage(this._tabControllerMock.Object, this._recyclebinControllerMock.Object, this._contentVerifierMock.Object);

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

            IConsoleCommand purgeCommand = new PurgePage(this._tabControllerMock.Object, this._recyclebinControllerMock.Object, this._contentVerifierMock.Object);

            var args = new[] { "purge-page", tabId.ToString() };
            purgeCommand.Initialize(args, portalSettings, null, 0);

            // Act
            var result = purgeCommand.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            this._tabControllerMock.Verify(t => t.GetTab(tabId, portalSettings.PortalId));
        }
    }
}
