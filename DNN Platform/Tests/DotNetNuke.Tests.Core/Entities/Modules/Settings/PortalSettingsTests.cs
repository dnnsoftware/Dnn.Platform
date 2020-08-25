// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Entities.Portals;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PortalSettingsTests : BaseSettingsTests
    {
        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyPortalSettings();

            this.MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyPortalSettingsRepository();

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

            this.MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyPortalSettings());
            var settingsRepository = new MyPortalSettingsRepository();

            // Act
            settingsRepository.GetSettings(moduleInfo);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void GetSettings_GetsValuesFrom_PortalSettings_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void GetSettings_GetsValuesFrom_PortalSettings_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void GetSettings_GetsValuesFrom_PortalSettings_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void GetSettings_GetsValuesFrom_PortalSettings_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void GetSettings_GetsValuesFrom_PortalSettings_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        private void SaveSettings_CallsUpdatePortalSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue,
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyPortalSettings
            {
                StringProperty = stringValue,
                IntegerProperty = integerValue,
                DoubleProperty = doubleValue,
                BooleanProperty = booleanValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
                EnumProperty = enumValue,
                ComplexProperty = complexValue,
            };

            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "StringProperty", stringValue, true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "IntegerProperty", integerValue.ToString(), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString(), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("o", CultureInfo.InvariantCulture), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c", CultureInfo.InvariantCulture), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "EnumProperty", enumValue.ToString(), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "ComplexProperty", $"{complexValue.X} | {complexValue.Y}", true, Null.NullString, false));

            var settingsRepository = new MyPortalSettingsRepository();

            // Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            // Assert
            this.MockRepository.VerifyAll();
        }

        private void GetSettings_GetsValuesFrom_PortalSettings(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var portalSettings = new Dictionary<string, string>
                                 {
                                     { SettingNamePrefix + "StringProperty", stringValue },
                                     { SettingNamePrefix + "IntegerProperty", integerValue.ToString() },
                                     { SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture) },
                                     { SettingNamePrefix + "BooleanProperty", booleanValue.ToString() },
                                     { SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("o", CultureInfo.InvariantCulture) },
                                     { SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c", CultureInfo.InvariantCulture) },
                                     { SettingNamePrefix + "EnumProperty", enumValue.ToString() },
                                     { SettingNamePrefix + "ComplexProperty", $"{complexValue.X} | {complexValue.Y}" },
                                 };

            this.MockPortalSettings(moduleInfo, portalSettings);

            var settingsRepository = new MyPortalSettingsRepository();

            // Act
            var settings = settingsRepository.GetSettings(moduleInfo);

            // Assert
            Assert.AreEqual(stringValue, settings.StringProperty, "The retrieved string property value is not equal to the stored one");
            Assert.AreEqual(integerValue, settings.IntegerProperty, "The retrieved integer property value is not equal to the stored one");
            Assert.AreEqual(doubleValue, settings.DoubleProperty, "The retrieved double property value is not equal to the stored one");
            Assert.AreEqual(booleanValue, settings.BooleanProperty, "The retrieved boolean property value is not equal to the stored one");
            Assert.AreEqual(datetimeValue, settings.DateTimeProperty, "The retrieved datetime property value is not equal to the stored one");
            Assert.AreEqual(timeSpanValue, settings.TimeSpanProperty, "The retrieved timespan property value is not equal to the stored one");
            Assert.AreEqual(enumValue, settings.EnumProperty, "The retrieved enum property value is not equal to the stored one");
            Assert.AreEqual(complexValue, settings.ComplexProperty, "The retrieved complex property value is not equal to the stored one");
            this.MockRepository.VerifyAll();
        }

        public class MyPortalSettings
        {
            [PortalSetting(Prefix = SettingNamePrefix)]
            public string StringProperty { get; set; } = string.Empty;

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

            [PortalSetting(Prefix = SettingNamePrefix, Serializer = "DotNetNuke.Tests.Core.Entities.Modules.Settings.ComplexTypeSerializer,DotNetNuke.Tests.Core")]
            public ComplexType ComplexProperty { get; set; } = new ComplexType(20, 25);
        }

        public class MyPortalSettingsRepository : SettingsRepository<MyPortalSettings>
        {}
    }
}
