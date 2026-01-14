// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RestorePageUnitTests
    {
        private readonly int tabId = 91;
        private readonly int testPortalId = 1;

        private TabInfo tab;
        private PortalSettings portalSettings;
        private IConsoleCommand restorePage;

        private Mock<ITabController> tabControllerMock;
        private Mock<IRecyclebinController> recycleBinControllerMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this.tab = new TabInfo { TabID = this.tabId, PortalID = this.testPortalId, };
            this.portalSettings = new PortalSettings { PortalId = this.testPortalId, };

            this.tabControllerMock = new Mock<ITabController>();
            this.recycleBinControllerMock = new Mock<IRecyclebinController>();
            this.contentVerifierMock = new Mock<IContentVerifier>();

            this.tabControllerMock.SetReturnsDefault(this.tab);
            this.contentVerifierMock.SetReturnsDefault(true);
            string message;
            this.recycleBinControllerMock.Setup(r => r.RestoreTab(this.tab, out message));

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
        public void Run_RestorePage_WithValidCommand_ShouldReturnSuccessResponse()
        {
            // Arrange
            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.False);
                Assert.That(result.Records, Is.EqualTo(1));
                Assert.That(result is ConsoleErrorResultModel, Is.False);
            });
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse()
        {
            // Arrange
            this.tab = null;

            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            });
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForRequestedPortalNotAllowed_ShouldReturnErrorResponse()
        {
            // Arrange
            this.contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(this.testPortalId, this.portalSettings, false)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            });
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse()
        {
            // Arrange
            var message = "Tab not found";
            this.recycleBinControllerMock.Setup(r => r.RestoreTab(this.tab, out message));

            this.SetupCommand();

            // Act
            var result = this.restorePage.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            });
        }

        private void SetupCommand()
        {
            this.restorePage = new RestorePage(this.tabControllerMock.Object, this.recycleBinControllerMock.Object, this.contentVerifierMock.Object);

            var args = new[] { "restore-page", this.tabId.ToString() };
            this.restorePage.Initialize(args, this.portalSettings, null, 0);
        }
    }
}
