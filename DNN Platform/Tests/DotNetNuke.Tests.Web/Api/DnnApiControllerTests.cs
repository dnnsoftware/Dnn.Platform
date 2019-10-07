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