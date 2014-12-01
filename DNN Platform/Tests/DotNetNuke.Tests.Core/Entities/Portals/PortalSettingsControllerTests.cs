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

using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    [TestFixture]
    public class PortalSettingsControllerTests
    {
        private const int ValidPortalId = 0;
        private const int ValidTabId = 42;

        [SetUp]
        public void SetUp()
        {
            MockComponentProvider.ResetContainer();
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
        }

        [Test]
        [TestCaseSource(typeof(PortalSettingsControllerTestFactory), "LoadPortalSettings_Loads_Default_Value")]
        public void LoadPortalSettings_Loads_Default_Value(Dictionary<string, string> testFields)
        {
            //Arrange
            var propertyName = testFields["PropertyName"];
            var settingName = testFields["SettingName"];
            var isHostDefault = Boolean.Parse(testFields["IsHostDefault"]);
            var defaultValue = testFields["DefaultValue"];
            var controller = new PortalSettingsController();
            var settings = new PortalSettings() { PortalId = ValidPortalId};
            var hostSettings = PortalSettingsControllerTestFactory.GetHostSettings();

            var mockPortalController = new Mock<IPortalController>();
            mockPortalController
                .Setup(c => c.GetPortalSettings(It.IsAny<int>()))
                .Returns(new Dictionary<string, string>());
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
                            .Returns((string s) => hostSettings[s]);
            mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns((string s1, string s2) => hostSettings[s1]);
            mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
                            .Returns((string s, bool b) => Boolean.Parse(hostSettings[s]));
            mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
                            .Returns((string s, int i) => Int32.Parse(hostSettings[s]));
            HostController.RegisterInstance(mockHostController.Object);

            if (isHostDefault)
            {
                defaultValue = hostSettings[settingName];
            }

            //Act
            controller.LoadPortalSettings(settings);


            //Assert
            var property = settings.GetType().GetProperty(propertyName);
            var actualValue = property.GetValue(settings, null);
            if (actualValue is bool)
            {
                Assert.AreEqual(defaultValue, actualValue.ToString().ToLower());
            }
            else
            {
                Assert.AreEqual(defaultValue, actualValue.ToString());
            }
        }

        [Test]
        [TestCaseSource(typeof(PortalSettingsControllerTestFactory), "LoadPortalSettings_Loads_Setting_Value")]
        public void LoadPortalSettings_Loads_Setting_Value(Dictionary<string, string> testFields)
        {
            //Arrange
            var propertyName = testFields["PropertyName"];
            var settingName = testFields["SettingName"];
            var settingValue = testFields["SettingValue"];
            var propertyValue = (testFields.ContainsKey("PropertyValue")) ? testFields["PropertyValue"] : settingValue;
            var controller = new PortalSettingsController();
            var settings = new PortalSettings() { PortalId = ValidPortalId };
            var hostSettings = PortalSettingsControllerTestFactory.GetHostSettings();

            var mockPortalController = new Mock<IPortalController>();
            mockPortalController
                .Setup(c => c.GetPortalSettings(It.IsAny<int>()))
                .Returns(new Dictionary<string, string> { { settingName, settingValue } });
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
                            .Returns((string s) => hostSettings[s]);
            mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns((string s1, string s2) => hostSettings[s1]);
            mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
                            .Returns((string s, bool b) => Boolean.Parse(hostSettings[s]));
            mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
                            .Returns((string s, int i) => Int32.Parse(hostSettings[s]));
            HostController.RegisterInstance(mockHostController.Object);

            //Act
            controller.LoadPortalSettings(settings);

            //Assert
            var property = settings.GetType().GetProperty(propertyName);
            var actualValue = property.GetValue(settings, null);
            if (actualValue is bool)
            {
                Assert.AreEqual(propertyValue, actualValue.ToString().ToLower());
            }
            else
            {
                Assert.AreEqual(propertyValue, actualValue.ToString());
            }
        }
    }
}
