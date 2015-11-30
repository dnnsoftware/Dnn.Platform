// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Settings;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;
using System.Globalization;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    [TestFixture]
    public class ModuleSettingsTests
    {
        private const string SettingNamePrefix = "UnitTestSetting_";
        private const int ModuleId = 1234;
        private const int TabModuleId = 653;
        private const int TabId = 344597;

        private MockRepository _mockRepository;
        private Mock<IModuleController> _mockModuleController;
        private Mock<CachingProvider> _mockCache;
        private Mock<IHostController> _mockHostController;

        [TestFixtureSetUp]
        public void TestFixtureSetUpAttribute()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);
            _mockHostController = _mockRepository.Create<IHostController>();
        }

        [SetUp]
        public void SetUp()
        {
            //Mock Repository and component factory
            _mockRepository = new MockRepository(MockBehavior.Default);
            ComponentFactory.Container = new SimpleContainer();

            // Setup Mock
            _mockModuleController = _mockRepository.Create<IModuleController>();
            ModuleController.SetTestableInstance(_mockModuleController.Object);
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            HostController.RegisterInstance(_mockHostController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            ModuleController.ClearInstance();
            MockComponentProvider.ResetContainer();
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
        }

        public class ModulesSettingsRepository : SettingsRepository<ModulesSettings> { }

        private static readonly object[] SettingsCases =
        {
            new object[] {"AbcdeF#2@kfdfdfds", 9, 1.45, false, new DateTime(2015, 11, 30, 13, 45, 16), TimeSpan.Zero},
            new object[] {"Bsskk41233[]#%&", -5, -13456.456, true, DateTime.Today.AddDays(-1), new TimeSpan(1,5,6,7) }
        };

        [Test]
        [TestCaseSource("SettingsCases")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue, 
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue)
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
                TimeSpanProperty = timeSpanValue
            };

            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "StringProperty", stringValue));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "IntegerProperty", integerValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture)));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u")));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("G")));
            
            var settingsRepository = new ModulesSettingsRepository();

            //Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            //Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new ModulesSettings();

            _mockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new ModulesSettingsRepository();

            //Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            //Assert
            _mockRepository.VerifyAll();
        }

        //GetCachedObject

        [Test]
        public void GetSettings_CallsGetCachedObject()
        {
            //Arrange
            var moduleInfo = GetModuleInfo;

            _mockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new ModulesSettings());
            var settingsRepository = new ModulesSettingsRepository();

            //Act
            settingsRepository.GetSettings(moduleInfo);

            //Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource("SettingsCases")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection(string stringValue, int integerValue, double doubleValue,
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var moduleSettings = new Hashtable();
            moduleSettings.Add(SettingNamePrefix + "StringProperty", stringValue);
            moduleSettings.Add(SettingNamePrefix + "IntegerProperty", integerValue.ToString());
            moduleSettings.Add(SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture));
            moduleSettings.Add(SettingNamePrefix + "BooleanProperty", booleanValue.ToString());
            moduleSettings.Add(SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u"));
            moduleSettings.Add(SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("G"));

            _mockCache.Setup(c => c.GetItem("DNN_" + ModuleSettingsCacheKey(moduleInfo))).Returns(new Dictionary<int,Hashtable>{ { moduleInfo.ModuleID, moduleSettings }});
            _mockCache.Setup(c => c.Insert("DNN_" + CacheKey(moduleInfo), It.IsAny<object>()));
            _mockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            _mockCache.SetupSequence(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(null).Returns(null).Returns(new ModulesSettings());

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
            _mockRepository.VerifyAll();
        }

        private static ModuleInfo GetModuleInfo => new ModuleInfo {ModuleID = ModuleId, TabModuleID = TabModuleId, TabID = TabId};

        private static string CacheKey(ModuleInfo moduleInfo) => $"SettingsModule{moduleInfo.TabModuleID}";

        private static string ModuleSettingsCacheKey(ModuleInfo moduleInfo) => $"ModuleSettings{moduleInfo.TabID}";
    }
}
