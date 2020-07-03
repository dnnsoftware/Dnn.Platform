// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Settings;
    using NUnit.Framework;

    [TestFixture]
    public class NullableSettingsTests : BaseSettingsTests
    {
        public readonly object[] NullableCases =
        {
            new object[] { null, null, null, null, },
            new object[] { string.Empty, -1, DateTime.UtcNow, TimeSpan.FromMilliseconds(3215648), },
            new object[] { "lorem ipsum", 456, DateTime.Now, DateTime.Today - DateTime.Now, },
        };

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ar-JO")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_ar_JO(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ca-ES")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_ca_ES(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("zh-CN")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_zh_CN(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("en-US")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_en_US(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("fr-FR")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_fr_FR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("he-IL")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_he_IL(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ru-RU")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_ru_RU(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("tr-TR")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_tr_TR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyNullableSettings();

            this.MockModuleSettings(moduleInfo, new Hashtable());
            this.MockTabModuleSettings(moduleInfo, new Hashtable());
            this.MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyNullableSettingsRepository();

            // Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        public void GetSettings_CallsGetCachedObject()
        {
            // Arrange
            var moduleInfo = GetModuleInfo;

            this.MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyNullableSettings());
            var settingsRepository = new MyNullableSettingsRepository();

            // Act
            settingsRepository.GetSettings(moduleInfo);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ar-JO")]
        public void GetSettings_GetsValues_FromCorrectSettings_ar_JO(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ca-ES")]
        public void GetSettings_GetsValues_FromCorrectSettings_ca_ES(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("zh-CN")]
        public void GetSettings_GetsValues_FromCorrectSettings_zh_CN(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("en-US")]
        public void GetSettings_GetsValues_FromCorrectSettings_en_US(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("fr-FR")]
        public void GetSettings_GetsValues_FromCorrectSettings_fr_FR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("he-IL")]
        public void GetSettings_GetsValues_FromCorrectSettings_he_IL(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ru-RU")]
        public void GetSettings_GetsValues_FromCorrectSettings_ru_RU(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("tr-TR")]
        public void GetSettings_GetsValues_FromCorrectSettings_tr_TR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        private void SaveSettings_CallsUpdateSetting_WithRightParameters(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyNullableSettings
            {
                StringProperty = stringValue,
                IntegerProperty = integerValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
            };

            this.MockModuleSettings(moduleInfo, new Hashtable());
            this.MockTabModuleSettings(moduleInfo, new Hashtable());
            var expectedStringValue = stringValue ?? string.Empty;
            this.MockModuleController.Setup(pc => pc.UpdateModuleSetting(ModuleId, "StringProperty", expectedStringValue));
            var integerString = integerValue?.ToString() ?? string.Empty;
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, "IntegerProperty", integerString, true, Null.NullString, false));
            var dateTimeString = datetimeValue?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, "DateTimeProperty", dateTimeString));
            var timeSpanString = timeSpanValue?.ToString("c", CultureInfo.InvariantCulture) ?? string.Empty;
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, "TimeSpanProperty", timeSpanString));

            var settingsRepository = new MyNullableSettingsRepository();

            // Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            // Assert
            this.MockRepository.VerifyAll();
        }

        private void GetSettings_GetsValues_FromCorrectSettings(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
            // Arrange
            var expectedStringValue = stringValue ?? string.Empty;
            var moduleInfo = GetModuleInfo;
            var portalSettings = new Dictionary<string, string> { ["IntegerProperty"] = integerValue?.ToString() ?? string.Empty, };
            var moduleSettings = new Hashtable { ["DateTimeProperty"] = datetimeValue?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty, ["StringProperty"] = expectedStringValue, };
            var tabModuleSettings = new Hashtable { ["TimeSpanProperty"] = timeSpanValue?.ToString("c", CultureInfo.InvariantCulture) ?? string.Empty, };

            this.MockPortalSettings(moduleInfo, portalSettings);
            this.MockModuleSettings(moduleInfo, moduleSettings);
            this.MockTabModuleSettings(moduleInfo, tabModuleSettings);

            var settingsRepository = new MyNullableSettingsRepository();

            // Act
            var settings = settingsRepository.GetSettings(moduleInfo);

            // Assert
            Assert.AreEqual(expectedStringValue, settings.StringProperty, "The retrieved string property value is not equal to the stored one");
            Assert.AreEqual(integerValue, settings.IntegerProperty, "The retrieved integer property value is not equal to the stored one");
            Assert.AreEqual(datetimeValue, settings.DateTimeProperty, "The retrieved datetime property value is not equal to the stored one");
            Assert.AreEqual(timeSpanValue, settings.TimeSpanProperty, "The retrieved timespan property value is not equal to the stored one");
            this.MockRepository.VerifyAll();
        }

        public class MyNullableSettings
        {
            [ModuleSetting]
            public string StringProperty { get; set; } = "Default Value";

            [PortalSetting]
            public int? IntegerProperty { get; set; } = 500;

            [ModuleSetting]
            public DateTime? DateTimeProperty { get; set; } = DateTime.MaxValue;

            [TabModuleSetting]
            public TimeSpan? TimeSpanProperty { get; set; } = TimeSpan.FromHours(12);
        }

        public class MyNullableSettingsRepository : SettingsRepository<MyNullableSettings>
        {}
    }
}
