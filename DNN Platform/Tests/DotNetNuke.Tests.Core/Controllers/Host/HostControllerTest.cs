// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Host
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class HostControllerTest
    {
        private DataTable hostSettingsTable;
        private Mock<CachingProvider> mockCache;
        private Mock<DataProvider> mockData;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            this.hostSettingsTable = new DataTable("HostSettings");

            var nameCol = this.hostSettingsTable.Columns.Add("SettingName");
            this.hostSettingsTable.Columns.Add("SettingValue");
            this.hostSettingsTable.Columns.Add("SettingIsSecure");
            this.hostSettingsTable.PrimaryKey = new[] { nameCol };

            this.hostSettingsTable.Rows.Add("String_1_S", "String_1_S", true);
            this.hostSettingsTable.Rows.Add("String_2_S", "String_1_S", true);
            this.hostSettingsTable.Rows.Add("String_3_U", "Value_3_U", false);
            this.hostSettingsTable.Rows.Add("String_4_U", "Value_4_U", false);
            this.hostSettingsTable.Rows.Add("Int_5_U", "5", false);
            this.hostSettingsTable.Rows.Add("Int_6_S", "6", true);
            this.hostSettingsTable.Rows.Add("Double_7_S", "7", true);
            this.hostSettingsTable.Rows.Add("Double_8_U", "8", false);
            this.hostSettingsTable.Rows.Add("Bool_9_U", false, false);
            this.hostSettingsTable.Rows.Add("Bool_10_S", false, true);

            this.mockData = MockComponentProvider.CreateDataProvider();
            this.mockData.Setup(c => c.GetHostSettings()).Returns(this.hostSettingsTable.CreateDataReader());
            this.mockData.Setup(c => c.GetProviderPath()).Returns(string.Empty);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(this.mockData.Object);
                });

            DataCache.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
            this.hostSettingsTable?.Dispose();
        }

        [Test]
        public void HostController_GetSettings_GetList()
        {
            // Arrange
            var expectedDic = new Dictionary<string, ConfigurationSetting>();

            foreach (DataRow row in this.hostSettingsTable.Rows)
            {
                var conf = new ConfigurationSetting();
                conf.Key = row["SettingName"].ToString();
                conf.Value = row["SettingValue"].ToString();
                bool IsSecure;
                bool.TryParse(row["SettingIsSecure"].ToString(), out IsSecure);
                conf.IsSecure = IsSecure;
                expectedDic.Add(conf.Key, conf);
            }

            // Act
            var settingsDic = HostController.Instance.GetSettings();

            // Assert
            foreach (var currentConfig in settingsDic)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(currentConfig.Value.Key, Is.EqualTo(currentConfig.Key));
                    Assert.That(currentConfig.Value.Value, Is.EqualTo(expectedDic[currentConfig.Key].Value));
                    Assert.That(currentConfig.Value.IsSecure, Is.EqualTo(expectedDic[currentConfig.Key].IsSecure));
                });
            }
        }

        [Test]
        public void HostController_GetSettingsDictionary_GetList()
        {
            // Arrange
            // Convert table to Dictionary<string,string>
            var expectedDic = this.hostSettingsTable.Rows.Cast<DataRow>().ToDictionary(row => row["SettingName"].ToString(), row => row["SettingValue"].ToString());

            // Act
            var settingsDic = HostController.Instance.GetSettingsDictionary();

            // Assert
            Assert.That(settingsDic.Values, Is.EquivalentTo(expectedDic.Values));
        }

        [Test]
        public void HostController_Update_ExistingValue()
        {
            // Arrange
            const string key = "String_1_S";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(key).Read()).Returns(true);

            // Act
            HostController.Instance.Update(key, value);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()), Times.Exactly(1));
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Exactly(1));
        }

        [Test]
        public void HostController_Update_ExistingValue_ResetCache()
        {
            // Arrange
            const string key = "String_1_S";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(key).Read()).Returns(true);

            // Act
            HostController.Instance.Update(key, value);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()), Times.Exactly(1));
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Exactly(1));
        }

        [Test]
        public void HostController_Update_ExistingValue_ResetCache_With_Overload()
        {
            // Arrange
            const string key = "String_1_S";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(key).Read()).Returns(true);

            // Act
            HostController.Instance.Update(key, value, true);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()), Times.Exactly(1));
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Exactly(1));
        }

        [Test]
        public void HostController_Update_ExistingValue_Dont_Reset_Cache()
        {
            // Arrange
            const string key = "String_1_S";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(key).Read()).Returns(true);

            // Act
            HostController.Instance.Update(key, value, false);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()));

            // Clear was not called a second time
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Never);
        }

        [Test]
        public void HostController_Update_Dictionary()
        {
            // Arrange
            var settings = new Dictionary<string, string>
                                                      {
                                                          { "String_1_S", "MyValue" },
                                                      };

            // Act
            HostController.Instance.Update(settings);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting("String_1_S", "MyValue", false, It.IsAny<int>()), Times.Exactly(1));
            this.mockCache.Verify(c => c.Clear("Host", string.Empty), Times.Exactly(1));
        }

        [Test]
        public void HostController_Update_NewValue()
        {
            // Arrange
            const string key = "MyKey";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(It.IsAny<string>()).Read()).Returns(false);

            // Act
            HostController.Instance.Update(key, value);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()));
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Once);
        }

        [Test]
        public void HostController_Update_NewValue_ResetCache_With_Overload()
        {
            // Arrange
            const string key = "MyKey";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(It.IsAny<string>()).Read()).Returns(false);

            // Act
            HostController.Instance.Update(key, value, true);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()));
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Once);
        }

        [Test]
        public void HostController_Update_NewValue_ResetCache()
        {
            // Arrange
            const string key = "MyKey";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(It.IsAny<string>()).Read()).Returns(false);

            // Act
            HostController.Instance.Update(key, value);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()));
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Once);
        }

        [Test]
        public void HostController_Update_NewValue_Dont_Reset_Cache()
        {
            // Arrange
            const string key = "MyKey";
            const string value = "MyValue";
            this.mockData.Setup(c => c.GetHostSetting(It.IsAny<string>()).Read()).Returns(false);

            // Act
            HostController.Instance.Update(key, value, false);

            // Assert
            this.mockData.Verify(c => c.UpdateHostSetting(key, value, false, It.IsAny<int>()));
            this.mockCache.Verify(c => c.Remove("DNN_HostSettings"), Times.Never);
        }

        [Test]
        [TestCase("String_1_S")]
        [TestCase("String_2_S")]
        [TestCase("String_3_U")]
        [TestCase("String_4_U")]
        public void HostController_GetString_If_Key_Exists(string key)
        {
            Assert.That(this.GetValue(key), Is.EqualTo(HostController.Instance.GetString(key)));
            Assert.That(this.GetValue(key), Is.EqualTo(HostController.Instance.GetString(key, "Hello Default")));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetString_InvalidKey_Returns_Null_Value(string key)
        {
            Assert.That(Null.NullString, Is.EqualTo(HostController.Instance.GetString(key)));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetString_InvalidKey_Returns_Default_Value(string key)
        {
            Assert.That(HostController.Instance.GetString(key, "Hello Default"), Is.EqualTo("Hello Default"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void HostController_GetString_NullEmpty(string key)
        {
            Assert.Throws<ArgumentException>(() => HostController.Instance.GetString(key));
        }

        [Test]
        [TestCase("Int_5_U")]
        [TestCase("Int_6_S")]
        public void HostController_GetInteger_If_Key_Exists(string key)
        {
            int s = HostController.Instance.GetInteger(key);
            Assert.That(this.GetValue(key), Is.EqualTo(s.ToString()));
            Assert.That(this.GetValue(key), Is.EqualTo(HostController.Instance.GetInteger(key, 12).ToString()));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetInteger_InvalidKey_Returns_Null_Value(string key)
        {
            Assert.That(Null.NullInteger, Is.EqualTo(HostController.Instance.GetInteger(key)));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetInteger_InvalidKey_Returns_Default_Value(string key)
        {
            Assert.That(HostController.Instance.GetInteger(key, 6969), Is.EqualTo(6969));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void HostController_GetInteger_NullEmpty(string key)
        {
            Assert.Throws<ArgumentException>(() => HostController.Instance.GetInteger(key));
        }

        [Test]
        [TestCase("Bool_9_U")]
        [TestCase("Bool_10_S")]
        public void HostController_GetBoolean_If_Key_Exists(string key)
        {
            Assert.That(this.GetValue(key), Is.EqualTo(HostController.Instance.GetBoolean(key).ToString()));
            Assert.That(this.GetValue(key), Is.EqualTo(HostController.Instance.GetBoolean(key, false).ToString()));
            Assert.That(this.GetValue(key), Is.EqualTo(HostController.Instance.GetBoolean(key, true).ToString()));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetBoolean_InvalidKey_Returns_Null_Value(string key)
        {
            Assert.That(Null.NullBoolean, Is.EqualTo(HostController.Instance.GetBoolean(key)));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetBoolean_InvalidKey_Returns_Default_Value(string key)
        {
            Assert.Multiple(() =>
            {
                Assert.That(HostController.Instance.GetBoolean(key, true), Is.EqualTo(true));
                Assert.That(HostController.Instance.GetBoolean(key, false), Is.EqualTo(false));
            });
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void HostController_GetBoolean_NullEmpty(string key)
        {
            Assert.Throws<ArgumentException>(() => HostController.Instance.GetBoolean(key));
        }

        [Test]
        [TestCase("Double_7_S")]
        [TestCase("Double_8_U")]
        public void HostController_GetDouble_If_Key_Exists(string key)
        {
            double s = HostController.Instance.GetDouble(key);
            Assert.That(this.GetValue(key), Is.EqualTo(s.ToString()));
            Assert.That(this.GetValue(key), Is.EqualTo(HostController.Instance.GetDouble(key, 54.54).ToString()));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetDouble_InvalidKey_Returns_Null_Value(string key)
        {
            Assert.That(Null.NullDouble, Is.EqualTo(HostController.Instance.GetDouble(key)));
        }

        [Test]
        [TestCase("BadKey1")]
        [TestCase("AAAAAAA")]
        public void HostController_GetDouble_InvalidKey_Returns_Default_Value(string key)
        {
            Assert.That(HostController.Instance.GetDouble(key, 21.58), Is.EqualTo(21.58));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void HostController_GetDouble_NullEmpty(string key)
        {
            Assert.Throws<ArgumentException>(() => HostController.Instance.GetDouble(key));
        }

        private string GetValue(string key)
        {
            return this.hostSettingsTable.Rows.Find(key)["SettingValue"].ToString();
        }
    }
}
