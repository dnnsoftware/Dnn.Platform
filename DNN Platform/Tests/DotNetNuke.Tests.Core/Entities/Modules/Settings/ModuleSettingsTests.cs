// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections;
    using System.Globalization;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    [TestFixture]
    public class ModuleSettingsTests : BaseSettingsTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton<ISerializationManager, SerializationManager>();
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void SaveSettings_CallsUpdateModuleSetting_WithRightParameters_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.SaveSettings_CallsUpdateModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new ModulesSettings();

            this.MockModuleSettings(moduleInfo, new Hashtable());
            this.MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new ModulesSettingsRepository();

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

            this.MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new ModulesSettings());
            var settingsRepository = new ModulesSettingsRepository();

            // Act
            settingsRepository.GetSettings(moduleInfo);

            // Assert
            this.MockRepository.VerifyAll();
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection(string stringValue, int integerValue, double doubleValue,
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var moduleSettings = new Hashtable
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

            this.MockModuleSettings(moduleInfo, moduleSettings);

            var settingsRepository = new ModulesSettingsRepository();

            // Act
            var settings = settingsRepository.GetSettings(moduleInfo);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(settings.StringProperty, Is.EqualTo(stringValue), "The retrieved string property value is not equal to the stored one");
                Assert.That(settings.IntegerProperty, Is.EqualTo(integerValue), "The retrieved integer property value is not equal to the stored one");
                Assert.That(settings.DoubleProperty, Is.EqualTo(doubleValue), "The retrieved double property value is not equal to the stored one");
                Assert.That(settings.BooleanProperty, Is.EqualTo(booleanValue), "The retrieved boolean property value is not equal to the stored one");
                Assert.That(settings.DateTimeProperty, Is.EqualTo(datetimeValue), "The retrieved datetime property value is not equal to the stored one");
                Assert.That(settings.TimeSpanProperty, Is.EqualTo(timeSpanValue), "The retrieved timespan property value is not equal to the stored one");
                Assert.That(settings.EnumProperty, Is.EqualTo(enumValue), "The retrieved enum property value is not equal to the stored one");
                Assert.That(settings.ComplexProperty, Is.EqualTo(complexValue), "The retrieved complex property value is not equal to the stored one");
            });
            this.MockRepository.VerifyAll();
        }

        private void SaveSettings_CallsUpdateModuleSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue,
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            // Arrange
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
                ComplexProperty = complexValue,
            };

            this.MockModuleSettings(moduleInfo, new Hashtable());
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "StringProperty", stringValue));
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "IntegerProperty", integerValue.ToString()));
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture)));
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString()));
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("o", CultureInfo.InvariantCulture)));
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c", CultureInfo.InvariantCulture)));
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "EnumProperty", enumValue.ToString()));
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, SettingNamePrefix + "ComplexProperty", $"{complexValue.X} | {complexValue.Y}"));

            var settingsRepository = new ModulesSettingsRepository();

            // Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            // Assert
            this.MockRepository.VerifyAll();
        }

        public class ModulesSettings
        {
            [ModuleSetting(Prefix = SettingNamePrefix)]
            public string StringProperty { get; set; } = string.Empty;

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

            [ModuleSetting(Prefix = SettingNamePrefix, Serializer = "DotNetNuke.Tests.Core.Entities.Modules.Settings.ComplexTypeSerializer,DotNetNuke.Tests.Core")]
            public ComplexType ComplexProperty { get; set; } = new ComplexType(20, 25);
        }

        public class ModulesSettingsRepository : SettingsRepository<ModulesSettings>
        {
        }
    }
}
