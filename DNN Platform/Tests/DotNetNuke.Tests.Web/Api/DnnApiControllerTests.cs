﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.Api;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class DnnApiControllerTests
    {
        internal class DnnApiControllerHelper : DnnApiController {}

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
        }

        [Test]
        public void GetsModuleInfoViaTheTabModuleInfoProviders()
        {
            //Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var provider = new Mock<ITabAndModuleInfoProvider>();
            var expectedModule = new ModuleInfo();
            provider.Setup(x => x.TryFindModuleInfo(request, out expectedModule)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            var controller = new DnnApiControllerHelper {Request = request};

            //Act
            var result = controller.ActiveModule;

            //Assert
            Assert.AreEqual(expectedModule, result);
            provider.Verify(x => x.TryFindModuleInfo(request, out expectedModule), Times.Once());
        }

        [Test]
        public void GetsPortalSettingsViaTestablePortalController()
        {
            //Arrange
            var controller = new DnnApiControllerHelper();
            var mockPortalController = new Mock<IPortalController>();
            var expectedPortalSettings = new PortalSettings();
            mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(expectedPortalSettings);
            PortalController.SetTestableInstance(mockPortalController.Object);

            //Act
            var result = controller.PortalSettings;

            //Assert
            mockPortalController.Verify(x => x.GetCurrentPortalSettings(), Times.Once());
            Assert.AreEqual(expectedPortalSettings, result);
        }

        //A test that would be nice to run, but I see not good way to test the source of the 
        //userinfo
//        [Test]
//        public void UserInfoComesFromPortalSettings()
//        {
//            //Arrange
//            var controller = new DnnApiControllerHelper();
//            var mockPortalController = new Mock<IPortalController>();
//            var expectedPortalSettings = new PortalSettings();
              //expectedPortalSettings.UserInfo = ??????
//            mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(expectedPortalSettings);
//            TestablePortalController.SetTestableInstance(mockPortalController.Object);
//
//            //Act
//            var result = controller.PortalSettings;
//
//            //Assert
//            mockPortalController.Verify(x => x.GetCurrentPortalSettings(), Times.Once());
//            Assert.AreEqual(expectedPortalSettings, result);
//        }
    }
}
