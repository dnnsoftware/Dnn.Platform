// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Prompt.Commands;
    using Dnn.PersonaBar.Pages.Components.Security;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GetPageUnitTests
    {
        private Mock<ITabController> _tabControllerMock;
        private Mock<IContentVerifier> _contentVerifierMock;
        private Mock<ISecurityService> _securityServiceMock;
        private PortalSettings _portalSettings;
        private IConsoleCommand _getCommand;
        private TabInfo _tab;

        private int _tabId = 21;
        private int _testPortalId = 1;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this._tab = new TabInfo();
            this._tab.TabID = this._tabId;
            this._tab.PortalID = this._testPortalId;

            this._portalSettings = new PortalSettings();
            this._portalSettings.PortalId = this._testPortalId;

            this._tabControllerMock = new Mock<ITabController>();
            this._securityServiceMock = new Mock<ISecurityService>();
            this._contentVerifierMock = new Mock<IContentVerifier>();

            this._tabControllerMock.SetReturnsDefault(this._tab);
            this._securityServiceMock.SetReturnsDefault(true);
            this._contentVerifierMock.SetReturnsDefault(true);
        }

        [Test]
        public void Run_GetPageWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            this._tabControllerMock.Setup(t => t.GetTab(this._tabId, this._testPortalId)).Returns(this._tab);

            this.SetupCommand();

            // Act
            var result = this._getCommand.Run();

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Records);
            Assert.IsFalse(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()
        {
            // Arrange
            this._tab = null;

            this._tabControllerMock.Setup(t => t.GetTab(this._tabId, this._testPortalId)).Returns(this._tab);

            this.SetupCommand();

            // Act
            var result = this._getCommand.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()
        {
            // Arrange
            this._contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(this._testPortalId, this._portalSettings, false)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this._getCommand.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()
        {
            // Arrange
            this._securityServiceMock.Setup(s => s.CanManagePage(this._tabId)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this._getCommand.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        private void SetupCommand()
        {
            this._getCommand = new GetPage(this._tabControllerMock.Object, this._securityServiceMock.Object, this._contentVerifierMock.Object);

            var args = new[] { "get-page", this._tabId.ToString() };
            this._getCommand.Initialize(args, this._portalSettings, null, this._tabId);
        }
    }
}
