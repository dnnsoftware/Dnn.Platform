// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PortalSettingsTests
    {
        private const int ValidPortalId = 0;
        private const int ValidTabId = 42;
        private const int InValidPortalId = -1;
        private const int InValidTabId = -1;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
                services.AddSingleton(Mock.Of<IPortalSettingsController>());
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        public void Constructor_Creates_Registration_Property()
        {
            var settings = new PortalSettings();

            Assert.That(settings.Registration, Is.Not.Null);
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalId()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalAlias()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
        }

        [Test]
        public void Constructor_Sets_PortalAlias_When_Passed_PortalAlias()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            Assert.That(settings.PortalAlias, Is.EqualTo(portalAlias));
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_Portal()
        {
            var portal = CreatePortal(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portal);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(ValidPortalId));
        }

        [Test]
        public void Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()
        {
            var portal = CreatePortal(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

            // Assert
            Assert.That(settings.PortalId, Is.EqualTo(InValidPortalId));
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            // Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portal);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()
        {
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(It.IsAny<PortalInfo>(), settings), Times.Never);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(InValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings), Times.Never);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portal = CreatePortal(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portal);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portal);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()
        {
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings), Times.Never);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var portalAlias = CreatePortalAlias(InValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings), Times.Never);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(InValidTabId, It.IsAny<PortalSettings>()))
                .Returns((TabInfo)null);

            // Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            // Assert
            Assert.That(settings.ActiveTab, Is.Null);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            // Act
            var settings = new PortalSettings(ValidTabId, portal);

            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()
        {
            // Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = Mock.Get(this.serviceProvider.GetService<IPortalSettingsController>());
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            var portalAlias = CreatePortalAlias(ValidPortalId);

            // Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            // Assert
            Assert.That(settings.ActiveTab, Is.Not.Null);
        }

        private static PortalInfo CreatePortal(int portalId) =>
            new() { PortalID = portalId, };

        private static PortalAliasInfo CreatePortalAlias(int portalId) =>
            new() { PortalID = portalId, };
    }
}
