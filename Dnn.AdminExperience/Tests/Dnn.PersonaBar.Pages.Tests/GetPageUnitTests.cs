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
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GetPageUnitTests
    {
        private readonly int tabId = 21;
        private readonly int testPortalId = 1;

        private Mock<ITabController> tabControllerMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private Mock<ISecurityService> securityServiceMock;
        private PortalSettings portalSettings;
        private IConsoleCommand getCommand;
        private TabInfo tab;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            this.tab = new TabInfo { TabID = this.tabId, PortalID = this.testPortalId, };

            this.portalSettings = new PortalSettings { PortalId = this.testPortalId, };

            this.tabControllerMock = new Mock<ITabController>();
            this.securityServiceMock = new Mock<ISecurityService>();
            this.contentVerifierMock = new Mock<IContentVerifier>();

            this.tabControllerMock.SetReturnsDefault(this.tab);
            this.securityServiceMock.SetReturnsDefault(true);
            this.contentVerifierMock.SetReturnsDefault(true);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.securityServiceMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        public void Run_GetPageWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.False);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Records, Is.EqualTo(1));
                Assert.That(result is ConsoleErrorResultModel, Is.False);
            });
        }

        [Test]
        public void Run_GetPageWithValidCommandForNonExistingTab_ShouldErrorResponse()
        {
            // Arrange
            this.tab = null;

            this.tabControllerMock.Setup(t => t.GetTab(this.tabId, this.testPortalId)).Returns(this.tab);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            });
        }

        [Test]
        public void Run_GetPageWithValidCommandForRequestedPortalNotAllowed_ShouldErrorResponse()
        {
            // Arrange
            this.contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(this.testPortalId, this.portalSettings, false)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            });
        }

        [Test]
        public void Run_GetPageWithValidCommandForPortalNotAllowed_ShouldErrorResponse()
        {
            // Arrange
            this.securityServiceMock.Setup(s => s.CanManagePage(this.tabId)).Returns(false);

            this.SetupCommand();

            // Act
            var result = this.getCommand.Run();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result.IsError, Is.True);
                Assert.That(result is ConsoleErrorResultModel, Is.True);
            });
        }

        private void SetupCommand()
        {
            this.getCommand = new GetPage(this.tabControllerMock.Object, this.securityServiceMock.Object, this.contentVerifierMock.Object);

            var args = new[] { "get-page", this.tabId.ToString() };
            this.getCommand.Initialize(args, this.portalSettings, null, this.tabId);
        }
    }
}
