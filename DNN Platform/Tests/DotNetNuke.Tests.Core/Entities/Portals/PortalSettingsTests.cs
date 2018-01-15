#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    [TestFixture]
    public class PortalSettingsTests
    {
        private const int ValidPortalId = 0;
        private const int ValidTabId = 42;
        private const int InValidPortalId = -1;
        private const int InValidTabId = -1;

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void Constructor_Creates_Registration_Property()
        {
            //Arrange

            //Act
            var settings = new PortalSettings();

            //Assert
            Assert.IsNotNull(settings.Registration);
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalId()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            //Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            //Assert
            Assert.AreEqual(ValidPortalId, settings.PortalId);
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalAlias()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            Assert.AreEqual(ValidPortalId, settings.PortalId);
        }

        [Test]
        public void Constructor_Sets_PortalAlias_When_Passed_PortalAlias()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            Assert.AreEqual(portalAlias, settings.PortalAlias);
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_Portal()
        {
            //Arrange
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portal = CreatePortal(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portal);

            //Assert
            Assert.AreEqual(ValidPortalId, settings.PortalId);
        }

        [Test]
        public void Constructor_Does_Not_Set_PortalId_When_Passed_Null_Portal()
        {
            //Arrange
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portal = CreatePortal(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

            //Assert
            Assert.AreEqual(InValidPortalId, settings.PortalId);
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            //Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            //Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            mockPortalController.Verify(c => c.GetPortal(ValidPortalId));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Portal()
        {
            //Arrange
             var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portal = CreatePortal(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portal);

            //Assert
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortal_When_Passed_Valid_PortalAlias()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings));
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Portal_Is_Null()
        {
            //Arrange
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            //Act
            var settings = new PortalSettings(ValidTabId, (PortalInfo) null);

            //Assert
            mockPortalSettingsController.Verify(c => c.LoadPortal(It.IsAny<PortalInfo>(), settings), Times.Never);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_LoadPortal_When_Passed_InValid_PortalAlias()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(InValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            mockPortalSettingsController.Verify(c => c.LoadPortal(portal, settings), Times.Never);
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalId()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            //Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            //Assert
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_Portal()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portal = CreatePortal(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portal);

            //Assert
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_LoadPortalSettings_When_Passed_PortalAlias()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            mockPortalSettingsController.Verify(c => c.LoadPortalSettings(settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Portal()
        {
            //Arrange
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portal = CreatePortal(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portal);

            //Assert
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
        }

        [Test]
        public void Constructor_Calls_PortalSettingsController_GetActiveTab_When_Passed_Valid_PortalAlias()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId, settings));
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Portal_Is_Null()
        {
            //Arrange
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            //Act
            var settings = new PortalSettings(ValidTabId, (PortalInfo)null);

            //Assert
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId,settings), Times.Never);
        }

        [Test]
        public void Constructor_Does_Not_Call_PortalSettingsController_GetActiveTab_When_Passed_InValid_PortalAlias()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = CreatePortalAlias(InValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            mockPortalSettingsController.Verify(c => c.GetActiveTab(ValidTabId,settings), Times.Never);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalId()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            //Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            //Assert
            Assert.NotNull(settings.ActiveTab);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_To_Null_If_InValid_TabId_And_PortalId()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(InValidTabId, It.IsAny<PortalSettings>()))
                .Returns((TabInfo)null);

            //Act
            var settings = new PortalSettings(ValidTabId, ValidPortalId);

            //Assert
            Assert.Null(settings.ActiveTab);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_Portal()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            //Act
            var settings = new PortalSettings(ValidTabId, portal);

            //Assert
            Assert.NotNull(settings.ActiveTab);
        }

        [Test]
        public void Constructor_Sets_ActiveTab_Property_If_Valid_TabId_And_PortalAlias()
        {
            //Arrange
            var portal = CreatePortal(ValidPortalId);
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(c => c.GetPortal(ValidPortalId)).Returns(portal);
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");
            mockPortalSettingsController
                .Setup(c => c.GetActiveTab(ValidTabId, It.IsAny<PortalSettings>()))
                .Returns(new TabInfo());

            var portalAlias = CreatePortalAlias(ValidPortalId);

            //Act
            var settings = new PortalSettings(ValidTabId, portalAlias);

            //Assert
            Assert.NotNull(settings.ActiveTab);
        }

        private PortalInfo CreatePortal(int portalId)
        {
            var portal = new PortalInfo()
            {
                PortalID = portalId,
            };

            return portal;
        }

        private PortalAliasInfo CreatePortalAlias(int portalId)
        {
            var portalAlias = new PortalAliasInfo()
            {
                PortalID = portalId,
            };

            return portalAlias;
        }
    }
}
