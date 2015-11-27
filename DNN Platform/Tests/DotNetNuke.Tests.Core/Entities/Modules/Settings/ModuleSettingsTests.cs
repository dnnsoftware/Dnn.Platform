// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Settings;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    [TestFixture]
    public class ModuleSettingsTests
    {
        private const string SettingNamePrefix = "UnitTestSetting_";
        private const int ModuleId = 1234;

        private MockRepository _mockRepository;
        private Mock<IModuleController> _mockModuleController;
        private Mock<CachingProvider> _mockCache;

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
        }

        [TearDown]
        public void TearDown()
        {
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
        }

        public class ModulesSettingsRepository : SettingsRepository<ModulesSettings> { }

        private static readonly object[] SaveSettingsCases =
        {
            new object[] {"AbcdeF#2@kfdfdfds", 9, 1.45, false, DateTime.Now, TimeSpan.Zero},
            new object[] {"Bsskk41233[]#%&", -5, -13456.456, true, DateTime.Today.AddDays(-1), new TimeSpan(1,5,6,7) }
        };

        [Test]
        [TestCaseSource("SaveSettingsCases")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_ForString(string stringValue, int integerValue, double doubleValue, 
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue)
        {
            //Arrange
            var moduleInfo = new ModuleInfo {ModuleID = ModuleId };
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
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString()));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("u")));
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString()));
            
            var settingsRepository = new ModulesSettingsRepository();

            //Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            //Assert
            _mockRepository.VerifyAll();
        }
    }
}
