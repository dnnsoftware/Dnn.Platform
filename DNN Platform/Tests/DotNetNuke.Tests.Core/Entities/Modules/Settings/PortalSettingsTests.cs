// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Settings;
using DotNetNuke.Entities.Portals;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    [TestFixture]
    public class PortalSettingsTests : BaseSettingsTests
    {
        private Mock<IPortalController> _mockPortalController;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Setup Mock
            _mockPortalController = MockRepository.Create<IPortalController>();
            PortalController.SetTestableInstance(_mockPortalController.Object);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            PortalController.ClearInstance();
        }

        public class MyPortalSettings
        {
            [PortalSetting(Prefix = SettingNamePrefix)]
            public string StringProperty { get; set; } = "";

            [PortalSetting(Prefix = SettingNamePrefix)]
            public int IntegerProperty { get; set; }

            [PortalSetting(Prefix = SettingNamePrefix)]
            public double DoubleProperty { get; set; }

            [PortalSetting(Prefix = SettingNamePrefix)]
            public bool BooleanProperty { get; set; }

            [PortalSetting(Prefix = SettingNamePrefix)]
            public DateTime DateTimeProperty { get; set; } = DateTime.Now;

            [PortalSetting(Prefix = SettingNamePrefix)]
            public TimeSpan TimeSpanProperty { get; set; } = TimeSpan.Zero;

            [PortalSetting(Prefix = SettingNamePrefix)]
            public TestingEnum EnumProperty { get; set; } = TestingEnum.Value1;
        }

        public class MyPortalSettingsRepository : SettingsRepository<MyPortalSettings> { }
        
        [Test]
        [TestCaseSource("SettingsCases")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue,
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyPortalSettings
            {
                StringProperty = stringValue,
                IntegerProperty = integerValue,
                DoubleProperty = doubleValue,
                BooleanProperty = booleanValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
                EnumProperty = enumValue
            };

            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "StringProperty", stringValue, true, Null.NullString));
            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "IntegerProperty", integerValue.ToString(), true, Null.NullString));
            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture), 
                true, Null.NullString));
            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString(), true, Null.NullString));
            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u"), true, Null.NullString));
            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("G"), true, Null.NullString));
            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "EnumProperty", enumValue.ToString(), true, Null.NullString));

            var settingsRepository = new MyPortalSettingsRepository();

            //Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            //Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyPortalSettings();

            MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyPortalSettingsRepository();

            //Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            //Assert
            MockRepository.VerifyAll();
        }

        [Test]
        public void GetSettings_CallsGetCachedObject()
        {
            //Arrange
            var moduleInfo = GetModuleInfo;

            MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyPortalSettings());
            var settingsRepository = new MyPortalSettingsRepository();

            //Act
            settingsRepository.GetSettings(moduleInfo);

            //Assert
            MockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource("SettingsCases")]
        public void GetSettings_GetsValuesFrom_PortalSettings(string stringValue, int integerValue, double doubleValue,
           bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var portalSettings = new Dictionary<string, string>();
            portalSettings.Add(SettingNamePrefix + "StringProperty", stringValue);
            portalSettings.Add(SettingNamePrefix + "IntegerProperty", integerValue.ToString());
            portalSettings.Add(SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture));
            portalSettings.Add(SettingNamePrefix + "BooleanProperty", booleanValue.ToString());
            portalSettings.Add(SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u"));
            portalSettings.Add(SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("G"));
            portalSettings.Add(SettingNamePrefix + "EnumProperty", enumValue.ToString());

            _mockPortalController.Setup(pc => pc.GetPortalSettings(moduleInfo.PortalID)).Returns(portalSettings);
            MockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            MockCache.SetupSequence(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(null).Returns(null).Returns(new MyPortalSettings());

            var settingsRepository = new MyPortalSettingsRepository();

            //Act
            var settings = settingsRepository.GetSettings(moduleInfo);

            //Assert
            Assert.AreEqual(stringValue, settings.StringProperty, "The retrieved string property value is not equal to the stored one");
            Assert.AreEqual(integerValue, settings.IntegerProperty, "The retrieved integer property value is not equal to the stored one");
            Assert.AreEqual(doubleValue, settings.DoubleProperty, "The retrieved double property value is not equal to the stored one");
            Assert.AreEqual(booleanValue, settings.BooleanProperty, "The retrieved boolean property value is not equal to the stored one");
            Assert.AreEqual(datetimeValue, settings.DateTimeProperty, "The retrieved datetime property value is not equal to the stored one");
            Assert.AreEqual(timeSpanValue, settings.TimeSpanProperty, "The retrieved timespan property value is not equal to the stored one");
            Assert.AreEqual(enumValue, settings.EnumProperty, "The retrieved enum property value is not equal to the stored one");
            MockRepository.VerifyAll();
        }
    }
}
