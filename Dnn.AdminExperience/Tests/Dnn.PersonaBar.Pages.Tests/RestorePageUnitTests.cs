// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using Moq;
using NUnit.Framework;

namespace Dnn.PersonaBar.Pages.Tests
{
    [TestFixture]
    public class RestorePageUnitTests
    {
        TabInfo _tab;
        string _message;
        PortalSettings _portalSettings;
        IConsoleCommand _restorePage;

        Mock<ITabController> _tabControllerMock;
        Mock<IRecyclebinController> _recyclebinControllerMock;
        Mock<IContentVerifier> _contentVerifierMock;

        int _tabId = 91;
        int _testPortalId = 1;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this._message = string.Empty;

            this._tab = new TabInfo();
            this._tab.TabID = this._tabId;
            this._tab.PortalID = this._testPortalId;

            this._portalSettings = new PortalSettings();
            this._portalSettings.PortalId = this._testPortalId;

            this._tabControllerMock = new Mock<ITabController>();
            this._recyclebinControllerMock = new Mock<IRecyclebinController>();
            this._contentVerifierMock = new Mock<IContentVerifier>();

            this._tabControllerMock.SetReturnsDefault(this._tab);
            this._contentVerifierMock.SetReturnsDefault(true);
            this._recyclebinControllerMock.Setup(r => r.RestoreTab(this._tab, out this._message));
        }

        [Test]
        public void Run_RestorePage_WithValidCommand_ShouldReturnSuccessResponse()
        {
            // Arrange
            this.SetupCommand();

            // Act
            var result = this._restorePage.Run();

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(1, result.Records);
            Assert.IsFalse(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse()
        {
            // Arrange
            this._tab = null;

            this._tabControllerMock.Setup(t => t.GetTab(this._tabId, this._testPortalId)).Returns(this._tab);

            this.SetupCommand();

            // Act
            var result = this._restorePage.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForRequestedPortalNotAllowed_ShouldReturnErrorResponse()
        {
            // Arrange
            this._contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(this._testPortalId, this._portalSettings, false)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this._restorePage.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse()
        {
            // Arrange
            this._message = "Tab not found";

            this._recyclebinControllerMock.Setup(r => r.RestoreTab(this._tab, out this._message));

            this.SetupCommand();

            // Act
            var result = this._restorePage.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        private void SetupCommand()
        {
            this._restorePage = new RestorePage(this._tabControllerMock.Object, this._recyclebinControllerMock.Object, this._contentVerifierMock.Object);

            var args = new[] { "restore-page", this._tabId.ToString() };
            this._restorePage.Initialize(args, this._portalSettings, null, 0);
        }
    }
}
