// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Settings;
using Moq;
using NUnit.Framework;
using System.Globalization;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    [TestFixture]
    public class ModuleSettingsTests : BaseSettingsTests
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

        public class ModulesSettings
        {
            [ModuleSetting(Prefix = SettingNamePrefix)]
            public string StringProperty { get; set; } = "";

            [ModuleSetting(Prefix = SettingNamePrefix)]
            public int IntegerProperty { get; set; }

            [ModuleSetting(Prefix = SettingNamePrefix)]
            public double DoubleProperty { get; set; }
            
            [ModuleSetting(Prefix = SettingNamePrefix)]
            public bool BooleanProperty { get; set; }

            [ModuleSetting(Prefix = SettingNamePrefix)]
            public DateTime DateTimeProperty { get; set; } = DateTime.Now;

            [ModuleSetting(Prefix = SettingNamePrefix)]
            public TimeSpan TimeSpanProperty { get; set; } = TimeSpan.Zero;

            [ModuleSetting(Prefix = SettingNamePrefix)]
            public TestingEnum EnumProperty { get; set; } = TestingEnum.Value1;
        }

        public class ModulesSettingsRepository : SettingsRepository<ModulesSettings> { }
        
        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue, 
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new ModulesSettings
            {
                StringProperty = stringValue,
                IntegerProperty = integerValue,
                DoubleProperty = doubleValue,
                BooleanProperty = booleanValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
                EnumProperty = enumValue,
            };

            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "StringProperty", stringValue));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "IntegerProperty", integerValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture)));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u")));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c")));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "EnumProperty", enumValue.ToString()));

            var settingsRepository = new ModulesSettingsRepository();

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
            var settings = new ModulesSettings();

            MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new ModulesSettingsRepository();

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

            MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new ModulesSettings());
            var settingsRepository = new ModulesSettingsRepository();

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
            var moduleSettings = new Hashtable
                                 {
                                     { SettingNamePrefix + "StringProperty", stringValue },
                                     { SettingNamePrefix + "IntegerProperty", integerValue.ToString() },
                                     { SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture) },
                                     { SettingNamePrefix + "BooleanProperty", booleanValue.ToString() },
                                     { SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u") },
                                     { SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c") },
                                     { SettingNamePrefix + "EnumProperty", enumValue.ToString() },
                                 };

            MockCache.Setup(c => c.GetItem("DNN_" + ModuleSettingsCacheKey(moduleInfo))).Returns(new Dictionary<int,Hashtable>{ { moduleInfo.ModuleID, moduleSettings }});
            MockCache.Setup(c => c.Insert("DNN_" + CacheKey(moduleInfo), It.IsAny<object>()));
            MockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            MockCache.SetupSequence(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(null).Returns(null).Returns(new ModulesSettings());

            var settingsRepository = new ModulesSettingsRepository();

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
