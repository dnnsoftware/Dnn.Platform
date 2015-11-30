// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Settings;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    [TestFixture]
    public class PortalSettingsTests
    {
        private const string SettingNamePrefix = "UnitTestSetting_";
        private const int ModuleId = 1234;
        private const int TabModuleId = 653;
        private const int TabId = 344597;
        private const int PortalId = 246;

        private MockRepository _mockRepository;
        private Mock<IPortalController> _mockPortalController;
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
            _mockPortalController = _mockRepository.Create<IPortalController>();
            PortalController.SetTestableInstance(_mockPortalController.Object);
            _mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            HostController.RegisterInstance(_mockHostController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
            MockComponentProvider.ResetContainer();
        }

        public enum TestingEnum
        {
            Value1,
            Value2
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
        
        private static readonly object[] SettingsCases =
        {
            new object[] {"AbcdeF#2@kfdfdfds", 9, 1.45, false, new DateTime(2015, 11, 30, 13, 45, 16), TimeSpan.Zero, TestingEnum.Value1},
            new object[] {"Bsskk41233[]#%&", -5, -13456.456, true, DateTime.Today.AddDays(-1), new TimeSpan(1,5,6,7), TestingEnum.Value2 }
        };

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
            _mockRepository.VerifyAll();
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyPortalSettings();

            _mockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyPortalSettingsRepository();

            //Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            //Assert
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetSettings_CallsGetCachedObject()
        {
            //Arrange
            var moduleInfo = GetModuleInfo;

            _mockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyPortalSettings());
            var settingsRepository = new MyPortalSettingsRepository();

            //Act
            settingsRepository.GetSettings(moduleInfo);

            //Assert
            _mockRepository.VerifyAll();
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
            _mockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            _mockCache.SetupSequence(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(null).Returns(null).Returns(new MyPortalSettings());

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
            _mockRepository.VerifyAll();
        }

        private static ModuleInfo GetModuleInfo => new ModuleInfo { ModuleID = ModuleId, TabModuleID = TabModuleId, TabID = TabId , PortalID = PortalId };

        private static string CacheKey(ModuleInfo moduleInfo) => $"SettingsModule{moduleInfo.TabModuleID}";
    }
}
