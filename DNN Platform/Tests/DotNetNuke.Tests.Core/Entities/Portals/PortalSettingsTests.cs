#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    [TestFixture]
    public class PortalSettingsTests
    {
        private const int Valid_PortalId = 0;
        private const int Valid_TabId = 42;
        private const int InValid_PortalId = -1;
        private const int Invalid_TabId = -1;

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
            var settings = new PortalSettings(Valid_TabId, Valid_PortalId);

            //Assert
            Assert.AreEqual(Valid_PortalId, settings.PortalId);
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_PortalAlias()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = new PortalAliasInfo()
                                    {
                                        PortalID = Valid_PortalId,
                                    };

            //Act
            var settings = new PortalSettings(Valid_TabId, portalAlias);

            //Assert
            Assert.AreEqual(Valid_PortalId, settings.PortalId);
        }

        [Test]
        public void Constructor_Sets_PortalAlias_When_Passed_PortalAlias()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = new PortalAliasInfo()
                                    {
                                        PortalID = Valid_PortalId,
                                    };

            //Act
            var settings = new PortalSettings(Valid_TabId, portalAlias);

            //Assert
            Assert.AreEqual(portalAlias, settings.PortalAlias);
        }

        [Test]
        public void Constructor_Sets_PortalId_When_Passed_Portal()
        {
            //Arrange
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portal = new PortalInfo()
            {
                PortalID = Valid_PortalId,
            };


            //Act
            var settings = new PortalSettings(Valid_TabId, portal);

            //Assert
            Assert.AreEqual(Valid_PortalId, settings.PortalId);
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalId()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            //Act
            var settings = new PortalSettings(Valid_TabId, Valid_PortalId);

            //Assert
            mockPortalController.Verify(c => c.GetPortal(Valid_PortalId));
        }

        [Test]
        public void Constructor_Calls_PortalController_GetPortal_When_Passed_PortalAlias()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(mockPortalController.Object);
            var mockPortalSettingsController = MockComponentProvider.CreateNew<IPortalSettingsController>("PortalSettingsController");

            var portalAlias = new PortalAliasInfo()
                                    {
                                        PortalID = Valid_PortalId,
                                    };


            //Act
            var settings = new PortalSettings(Valid_TabId, portalAlias);

            //Assert
            mockPortalController.Verify(c => c.GetPortal(Valid_PortalId));
        }
    }
}
