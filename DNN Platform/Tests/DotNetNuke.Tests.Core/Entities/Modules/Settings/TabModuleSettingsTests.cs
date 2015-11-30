// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Settings;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    [TestFixture]
    public class TabModuleSettingsTests : BaseSettingsTests
    {
        private Mock<IModuleController> _mockModuleController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Setup Mock
            _mockModuleController = MockRepository.Create<IModuleController>();
            ModuleController.SetTestableInstance(_mockModuleController.Object);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            ModuleController.ClearInstance();
        }

        public class MyTabModuleSettings
        {
            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public string StringProperty { get; set; } = "";

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public int IntegerProperty { get; set; }

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public double DoubleProperty { get; set; }

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public bool BooleanProperty { get; set; }

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public DateTime DateTimeProperty { get; set; } = DateTime.Now;

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public TimeSpan TimeSpanProperty { get; set; } = TimeSpan.Zero;

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public TestingEnum EnumProperty { get; set; } = TestingEnum.Value1;
        }

        public class MyTabModuleSettingsRepository : SettingsRepository<MyTabModuleSettings> { }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue,
           bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyTabModuleSettings
            {
                StringProperty = stringValue,
                IntegerProperty = integerValue,
                DoubleProperty = doubleValue,
                BooleanProperty = booleanValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
                EnumProperty = enumValue,
            };

            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "StringProperty", stringValue));
            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "IntegerProperty", integerValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture)));
            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u")));
            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("G")));
            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "EnumProperty", enumValue.ToString()));

            var settingsRepository = new MyTabModuleSettingsRepository();

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
            var settings = new MyTabModuleSettings();

            MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyTabModuleSettingsRepository();

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

            MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyTabModuleSettings());
            var settingsRepository = new MyTabModuleSettingsRepository();

            //Act
            settingsRepository.GetSettings(moduleInfo);

            //Assert
            MockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection(string stringValue, int integerValue, double doubleValue,
           bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var tabModuleSettings = new Hashtable
                                    {
                                        { SettingNamePrefix + "StringProperty", stringValue },
                                        { SettingNamePrefix + "IntegerProperty", integerValue.ToString() },
                                        { SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture) },
                                        { SettingNamePrefix + "BooleanProperty", booleanValue.ToString() },
                                        { SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u") },
                                        { SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("G") },
                                        { SettingNamePrefix + "EnumProperty", enumValue.ToString() },
                                    };

            MockCache.Setup(c => c.GetItem("DNN_" + TabModuleSettingsCacheKey(moduleInfo))).Returns(new Dictionary<int, Hashtable> { { moduleInfo.TabModuleID, tabModuleSettings } });
            MockCache.Setup(c => c.Insert("DNN_" + CacheKey(moduleInfo), It.IsAny<object>()));
            MockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            MockCache.SetupSequence(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(null).Returns(null).Returns(new MyTabModuleSettings());

            var settingsRepository = new MyTabModuleSettingsRepository();

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
