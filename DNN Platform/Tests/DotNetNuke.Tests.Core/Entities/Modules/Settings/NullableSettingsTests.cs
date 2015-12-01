// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Settings;
using DotNetNuke.Entities.Portals;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System.Collections;
    using System.Globalization;

    using DotNetNuke.Entities.Modules;

    [TestFixture]
    public class NullableSettingsTests : BaseSettingsTests
    {
        private Mock<IModuleController> _mockModuleController;
        private Mock<IPortalController> _mockPortalController;
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Setup Mocks
            _mockPortalController = MockRepository.Create<IPortalController>();
            PortalController.SetTestableInstance(_mockPortalController.Object);
            _mockModuleController = MockRepository.Create<IModuleController>();
            ModuleController.SetTestableInstance(_mockModuleController.Object);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            PortalController.ClearInstance();
            ModuleController.ClearInstance();
        }

        public class MyNullableSettings
        {
            [PortalSetting]
            public int? IntegerProperty { get; set; }

            [ModuleSetting]
            public DateTime? DateTimeProperty { get; set; }

            [TabModuleSetting]
            public TimeSpan? TimeSpanProperty { get; set; }
        }

        public class MyNullableSettingsRepository : SettingsRepository<MyNullableSettings> { }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ar-JO")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ar_JO(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ca-ES")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ca_ES(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("zh-CN")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_zh_CN(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("en-US")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_en_US(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("fr-FR")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_fr_FR(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("he-IL")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_he_IL(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ru-RU")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ru_RU(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("tr-TR")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_tr_TR(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(integerValue, datetimeValue, timeSpanValue);
        }

        private void SaveSettings_CallsUpdateSetting_WithRightParameters(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyNullableSettings
            {
                IntegerProperty = integerValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
            };

            var integerString = integerValue?.ToString() ?? string.Empty;
            _mockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, "IntegerProperty", integerString, true, Null.NullString));
            var dateTimeString = datetimeValue?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
            _mockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, "DateTimeProperty", dateTimeString));
            var timeSpanString = timeSpanValue?.ToString("c", CultureInfo.InvariantCulture) ?? string.Empty;
            _mockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, "TimeSpanProperty", timeSpanString));

            var settingsRepository = new MyNullableSettingsRepository();

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
            var settings = new MyNullableSettings();

            MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyNullableSettingsRepository();

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

            MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyNullableSettings());
            var settingsRepository = new MyNullableSettingsRepository();

            //Act
            settingsRepository.GetSettings(moduleInfo);

            //Assert
            MockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ar-JO")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ar_JO(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ca-ES")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ca_ES(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("zh-CN")]
        public void GetSettings_GetsValuesFrom_PortalSettings_zh_CN(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("en-US")]
        public void GetSettings_GetsValuesFrom_PortalSettings_en_US(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("fr-FR")]
        public void GetSettings_GetsValuesFrom_PortalSettings_fr_FR(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("he-IL")]
        public void GetSettings_GetsValuesFrom_PortalSettings_he_IL(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ru-RU")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ru_RU(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("tr-TR")]
        public void GetSettings_GetsValuesFrom_PortalSettings_tr_TR(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(integerValue, datetimeValue, timeSpanValue);
        }

        private void GetSettings_GetsValues_FromCorrectSettings(int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            //Arrange
            var moduleInfo = GetModuleInfo;
            var portalSettings = new Dictionary<string, string> { { "IntegerProperty", integerValue?.ToString() ?? string.Empty }, };
            var moduleSettings = new Hashtable { { "DateTimeProperty", datetimeValue?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty }, };
            var tabModuleSettings = new Hashtable { { "TimeSpanProperty", timeSpanValue?.ToString("c", CultureInfo.InvariantCulture) ?? string.Empty }, };

            _mockPortalController.Setup(pc => pc.GetPortalSettings(moduleInfo.PortalID)).Returns(portalSettings);
            MockHostController.Setup(hc => hc.GetString("PerformanceSetting")).Returns("3");
            MockCache.Setup(c => c.GetItem("DNN_" + ModuleSettingsCacheKey(moduleInfo))).Returns(new Dictionary<int, Hashtable> { { moduleInfo.ModuleID, moduleSettings } });
            MockCache.Setup(c => c.GetItem("DNN_" + TabModuleSettingsCacheKey(moduleInfo))).Returns(new Dictionary<int, Hashtable> { { moduleInfo.TabModuleID, tabModuleSettings } });
            MockCache.Setup(c => c.Insert("DNN_" + CacheKey(moduleInfo), It.IsAny<object>()));
            MockCache.SetupSequence(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(null).Returns(null).Returns(new MyNullableSettings());

            var settingsRepository = new MyNullableSettingsRepository();

            //Act
            var settings = settingsRepository.GetSettings(moduleInfo);

            //Assert
            Assert.AreEqual(integerValue, settings.IntegerProperty, "The retrieved integer property value is not equal to the stored one");
            Assert.AreEqual(datetimeValue, settings.DateTimeProperty, "The retrieved datetime property value is not equal to the stored one");
            Assert.AreEqual(timeSpanValue, settings.TimeSpanProperty, "The retrieved timespan property value is not equal to the stored one");
            MockRepository.VerifyAll();
        }

        public readonly object[] NullableCases =
        {
            new object[] { null, null, null, },
            new object[] { -1, DateTime.UtcNow, TimeSpan.FromMilliseconds(3215648), },
        };
    }
}
