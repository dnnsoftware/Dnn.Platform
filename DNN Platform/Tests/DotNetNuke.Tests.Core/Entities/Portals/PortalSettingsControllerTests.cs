// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Portals
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.UI.Skins;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PortalSettingsControllerTests
    {
        private const int HostPortalId = -1;
        private const int HostTabId = 24;
        private const int ValidPortalId = 0;
        private const int ValidTabId = 42;
        private const int ParentTabId = 55;
        private const int SplashTabId = 41;
        private const int HomeTabId = 40;
        private const int InValidTabId = -1;

        private const string DefaultSkin = "DefaultSkin";
        private const string TabSkin = "TabSkin";
        private const string GlobalTabSkin = "[g]TabSkin";

        private const string DefaultContainer = "DefaultContainer";
        private const string TabContainer = "TabContainer";
        private const string GlobalTabContainer = "[g]TabContainer";

        [SetUp]
        public void SetUp()
        {
            MockComponentProvider.ResetContainer();
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
            TabController.ClearInstance();
        }

        [Test]
        [TestCaseSource(typeof(PortalSettingsControllerTestFactory), "LoadPortalSettings_Loads_Default_Value")]
        public void LoadPortalSettings_Loads_Default_Value(Dictionary<string, string> testFields)
        {
            // Arrange
            var propertyName = testFields["PropertyName"];
            var settingName = testFields["SettingName"];
            var isHostDefault = bool.Parse(testFields["IsHostDefault"]);
            var defaultValue = testFields["DefaultValue"];
            var controller = new PortalSettingsController();
            var settings = new PortalSettings() { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var hostSettings = PortalSettingsControllerTestFactory.GetHostSettings();

            var mockPortalController = new Mock<IPortalController>();
            mockPortalController
                .Setup(c => c.GetPortalSettings(It.IsAny<int>()))
                .Returns(new Dictionary<string, string>());
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
                            .Returns((string s) => hostSettings[s]);
            mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns((string s1, string s2) => hostSettings[s1]);
            mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
                            .Returns((string s, bool b) => bool.Parse(hostSettings[s]));
            mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
                            .Returns((string s, int i) => int.Parse(hostSettings[s]));
            mockHostController.Setup(c => c.GetInteger(It.IsAny<string>()))
                            .Returns((string s) => int.Parse(hostSettings[s]));
            HostController.RegisterInstance(mockHostController.Object);

            if (isHostDefault)
            {
                defaultValue = hostSettings[settingName];
            }

            // Act
            controller.LoadPortalSettings(settings);

            // Assert
            var property = settings.GetType().GetProperty(propertyName);
            var actualValue = property.GetValue(settings, null);
            if (actualValue is bool)
            {
                Assert.AreEqual(defaultValue, actualValue.ToString().ToLowerInvariant());
            }
            else
            {
                Assert.AreEqual(defaultValue, actualValue.ToString());
            }
        }

        [Test]
        [TestCaseSource(typeof(PortalSettingsControllerTestFactory), "LoadPortalSettings_Loads_Setting_Value")]
        public void LoadPortalSettings_Loads_Setting_Value(Dictionary<string, string> testFields)
        {
            // Arrange
            var propertyName = testFields["PropertyName"];
            var settingName = testFields["SettingName"];
            var settingValue = testFields["SettingValue"];
            var propertyValue = testFields.ContainsKey("PropertyValue") ? testFields["PropertyValue"] : settingValue;
            var controller = new PortalSettingsController();
            var settings = new PortalSettings() { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var hostSettings = PortalSettingsControllerTestFactory.GetHostSettings();

            var mockPortalController = new Mock<IPortalController>();
            mockPortalController
                .Setup(c => c.GetPortalSettings(It.IsAny<int>()))
                .Returns(new Dictionary<string, string> { { settingName, settingValue } });
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
                            .Returns((string s) => hostSettings[s]);
            mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns((string s1, string s2) => hostSettings[s1]);
            mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
                            .Returns((string s, bool b) => bool.Parse(hostSettings[s]));
            mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
                            .Returns((string s, int i) => int.Parse(hostSettings[s]));
            HostController.RegisterInstance(mockHostController.Object);

            // Act
            controller.LoadPortalSettings(settings);

            // Assert
            var property = settings.GetType().GetProperty(propertyName);
            var actualValue = property.GetValue(settings, null);
            if (actualValue is bool)
            {
                Assert.AreEqual(propertyValue, actualValue.ToString().ToLowerInvariant());
            }
            else
            {
                Assert.AreEqual(propertyValue, actualValue.ToString());
            }
        }

        [Test]
        public void LoadPortalSettings_Sets_TimeZone_Property_To_Local_TimeZone()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings() { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var hostSettings = PortalSettingsControllerTestFactory.GetHostSettings();

            var mockPortalController = new Mock<IPortalController>();
            mockPortalController
                .Setup(c => c.GetPortalSettings(It.IsAny<int>()))
                .Returns(new Dictionary<string, string>());
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
                            .Returns((string s) => hostSettings[s]);
            mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
                            .Returns((string s1, string s2) => hostSettings[s1]);
            mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
                            .Returns((string s, bool b) => bool.Parse(hostSettings[s]));
            mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
                            .Returns((string s, int i) => int.Parse(hostSettings[s]));
            HostController.RegisterInstance(mockHostController.Object);

            // Act
            controller.LoadPortalSettings(settings);

            // Assert
            Assert.AreEqual(TimeZoneInfo.Local, settings.TimeZone);
        }

        [Test]
        public void LoadPortal_Loads_Portal_Property_Values()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var portal = new PortalInfo()
            {
                Users = 2,
                Pages = 5,
                DefaultLanguage = Localization.SystemLocale,
                HomeDirectory = "Portals/0",
            };
            var settings = new PortalSettings() { PortalId = ValidPortalId, CultureCode = Null.NullString };

            // Act
            controller.LoadPortal(portal, settings);

            // Assert
            Assert.AreEqual(portal.AdminTabId, settings.AdminTabId);
            Assert.AreEqual(portal.AdministratorId, settings.AdministratorId);
            Assert.AreEqual(portal.AdministratorRoleId, settings.AdministratorRoleId);
            Assert.AreEqual(portal.AdministratorRoleName, settings.AdministratorRoleName);
            Assert.AreEqual(portal.BackgroundFile, settings.BackgroundFile);
            Assert.AreEqual(portal.BannerAdvertising, settings.BannerAdvertising);
            Assert.AreEqual(portal.CultureCode, settings.CultureCode);
            Assert.AreEqual(portal.Currency, settings.Currency);
            Assert.AreEqual(portal.Custom404TabId, settings.ErrorPage404);
            Assert.AreEqual(portal.Custom500TabId, settings.ErrorPage500);
            Assert.AreEqual(portal.TermsTabId, settings.TermsTabId);
            Assert.AreEqual(portal.PrivacyTabId, settings.PrivacyTabId);
            Assert.AreEqual(portal.DefaultLanguage, settings.DefaultLanguage);
            Assert.AreEqual(portal.Description, settings.Description);
            Assert.AreEqual(portal.Email, settings.Email);
            Assert.AreEqual(portal.ExpiryDate, settings.ExpiryDate);
            Assert.AreEqual(portal.FooterText, settings.FooterText);
            Assert.AreEqual(portal.GUID, settings.GUID);
            Assert.AreEqual(Globals.ApplicationPath + "/" + portal.HomeDirectory + "/", settings.HomeDirectory);
            Assert.AreEqual(portal.HomeDirectoryMapPath, settings.HomeDirectoryMapPath);
            Assert.AreEqual(Globals.ApplicationPath + "/" + portal.HomeSystemDirectory + "/", settings.HomeSystemDirectory);
            Assert.AreEqual(portal.HomeSystemDirectoryMapPath, settings.HomeSystemDirectoryMapPath);
            Assert.AreEqual(portal.HomeTabId, settings.HomeTabId);
            Assert.AreEqual(portal.HostFee, settings.HostFee);
            Assert.AreEqual(portal.HostSpace, settings.HostSpace);
            Assert.AreEqual(portal.KeyWords, settings.KeyWords);
            Assert.AreEqual(portal.LoginTabId, settings.LoginTabId);
            Assert.AreEqual(portal.LogoFile, settings.LogoFile);
            Assert.AreEqual(portal.PageQuota, settings.PageQuota);
            Assert.AreEqual(portal.Pages, settings.Pages);
            Assert.AreEqual(portal.PortalName, settings.PortalName);
            Assert.AreEqual(portal.RegisterTabId, settings.RegisterTabId);
            Assert.AreEqual(portal.RegisteredRoleId, settings.RegisteredRoleId);
            Assert.AreEqual(portal.RegisteredRoleName, settings.RegisteredRoleName);
            Assert.AreEqual(portal.SearchTabId, settings.SearchTabId);
            Assert.AreEqual(portal.SplashTabId, settings.SplashTabId);
            Assert.AreEqual(portal.SuperTabId, settings.SuperTabId);
            Assert.AreEqual(portal.UserQuota, settings.UserQuota);
            Assert.AreEqual(portal.UserRegistration, settings.UserRegistration);
            Assert.AreEqual(portal.UserTabId, settings.UserTabId);
            Assert.AreEqual(portal.Users, settings.Users);
        }

        [Test]
        public void GetActiveTab_Gets_Correct_Tab_If_Valid_Portal_TabId()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId };

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            var tab = controller.GetActiveTab(ValidTabId, settings);

            // Assert
            Assert.AreEqual(validTab.TabID, tab.TabID);
        }

        [Test]
        public void GetActiveTab_Gets_Correct_Tab_If_Valid_Host_TabId()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = HostTabId, PortalID = HostPortalId };

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            var tab = controller.GetActiveTab(HostTabId, settings);

            // Assert
            Assert.AreEqual(validTab.TabID, tab.TabID);
        }

        [Test]
        public void GetActiveTab_Gets_Splash_Tab_If_InValid_TabId_And_SplashTab_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, SplashTabId = SplashTabId, CultureCode = Null.NullString };
            var splashTabId = new TabInfo { TabID = SplashTabId, PortalID = ValidPortalId };

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(ValidPortalId)).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { splashTabId }));
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            var tab = controller.GetActiveTab(InValidTabId, settings);

            // Assert
            Assert.AreEqual(SplashTabId, tab.TabID);
        }

        [Test]
        public void GetActiveTab_Gets_Home_Tab_If_InValid_TabId_And_Home_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, HomeTabId = HomeTabId, CultureCode = Null.NullString };
            var homeTabId = new TabInfo { TabID = HomeTabId, PortalID = ValidPortalId };

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(ValidPortalId)).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { homeTabId }));
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            var tab = controller.GetActiveTab(InValidTabId, settings);

            // Assert
            Assert.AreEqual(HomeTabId, tab.TabID);
        }

        [Test]
        public void GetActiveTab_Gets_Splash_Tab_If_InValid_TabId_And_Both_HomeTab_And_SplashTab_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, SplashTabId = SplashTabId, HomeTabId = HomeTabId, CultureCode = Null.NullString };
            var splashTabId = new TabInfo { TabID = SplashTabId, PortalID = ValidPortalId };
            var homeTabId = new TabInfo { TabID = HomeTabId, PortalID = ValidPortalId };

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(ValidPortalId)).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { splashTabId, homeTabId }));
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            var tab = controller.GetActiveTab(InValidTabId, settings);

            // Assert
            Assert.AreEqual(SplashTabId, tab.TabID);
        }

        [Test]
        public void GetActiveTab_Sets_StartDate_And_EndDate_Of_Tab_If_Not_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId };

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            var tab = controller.GetActiveTab(ValidTabId, settings);

            // Assert
            Assert.AreEqual(DateTime.MinValue, tab.StartDate);
            Assert.AreEqual(DateTime.MaxValue, tab.EndDate);
        }

        [Test]
        public void ConfigureTab_Uses_PortalSettings_DefaultSkin_If_SkinSrc_Not_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, DefaultPortalSkin = DefaultSkin, DefaultPortalContainer = DefaultContainer, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId };
            settings.ActiveTab = validTab;

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("DefaultPortalSkin")).Returns(DefaultSkin);
            mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");
            HostController.RegisterInstance(mockHostController.Object);

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            Assert.AreEqual(DefaultSkin, settings.ActiveTab.SkinSrc);
        }

        [Test]
        public void ConfigureTab_Uses_Tab_SkinSrc_If_SkinSrc_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, DefaultPortalSkin = DefaultSkin, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId, SkinSrc = TabSkin };
            settings.ActiveTab = validTab;

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");
            HostController.RegisterInstance(mockHostController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            Assert.AreEqual(TabSkin, settings.ActiveTab.SkinSrc);
        }

        [Test]
        public void ConfigureTab_Formats_Tab_SkinSrc_If_Neccessary()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, DefaultPortalSkin = DefaultSkin, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId, SkinSrc = GlobalTabSkin };
            settings.ActiveTab = validTab;

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");
            HostController.RegisterInstance(mockHostController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            Assert.AreEqual(SkinController.FormatSkinSrc(GlobalTabSkin, settings), settings.ActiveTab.SkinSrc);
        }

        [Test]
        public void ConfigureTab_Uses_PortalSettings_DefaultContainer_If_ContainerSrc_Not_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, DefaultPortalContainer = DefaultContainer, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId };
            settings.ActiveTab = validTab;
            settings.ActiveTab.SkinSrc = TabSkin;

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");
            HostController.RegisterInstance(mockHostController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            Assert.AreEqual(DefaultContainer, settings.ActiveTab.ContainerSrc);
        }

        [Test]
        public void ConfigureTab_Uses_Tab_ContainerSrc_If_ContainerSrc_Set()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, DefaultPortalContainer = DefaultContainer, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId, ContainerSrc = TabContainer };
            settings.ActiveTab = validTab;
            settings.ActiveTab.SkinSrc = TabSkin;

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            Assert.AreEqual(TabContainer, settings.ActiveTab.ContainerSrc);
        }

        [Test]
        public void ConfigureTab_Formats_Tab_ContainerSrc_If_Neccessary()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, DefaultPortalContainer = DefaultContainer, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId, ContainerSrc = GlobalTabContainer };
            settings.ActiveTab = validTab;
            settings.ActiveTab.SkinSrc = TabSkin;

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            Assert.AreEqual(SkinController.FormatSkinSrc(GlobalTabContainer, settings), settings.ActiveTab.ContainerSrc);
        }

        [Test]
        public void ConfigureTab_Builds_Breadcrumbs_For_Tab()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId, SkinSrc = GlobalTabSkin };
            settings.ActiveTab = validTab;

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");
            HostController.RegisterInstance(mockHostController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            Assert.NotNull(settings.ActiveTab.BreadCrumbs);
            Assert.AreEqual(1, settings.ActiveTab.BreadCrumbs.Count);
        }

        [Test]
        public void ConfigureTab_Builds_Breadcrumbs_For_Tab_And_Parent()
        {
            // Arrange
            var controller = new PortalSettingsController();
            var settings = new PortalSettings { PortalId = ValidPortalId, CultureCode = Null.NullString };
            var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId, ParentId = ParentTabId };
            var parentTab = new TabInfo { TabID = ParentTabId, PortalID = ValidPortalId };
            settings.ActiveTab = validTab;
            settings.ActiveTab.SkinSrc = TabSkin;

            var mockLocaleController = new Mock<ILocaleController>();
            mockLocaleController.Setup(c => c.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>());
            LocaleController.RegisterInstance(mockLocaleController.Object);

            var mockTabController = new Mock<ITabController>();
            mockTabController.Setup(c => c.GetTabsByPortal(ValidPortalId)).Returns(new TabCollection(new List<TabInfo> { validTab, parentTab }));
            mockTabController.Setup(c => c.GetTabsByPortal(HostPortalId)).Returns(new TabCollection());
            TabController.SetTestableInstance(mockTabController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");
            HostController.RegisterInstance(mockHostController.Object);

            // Act
            controller.ConfigureActiveTab(settings);

            // Assert
            var actualParent = settings.ActiveTab.BreadCrumbs[0] as TabInfo;
            var actualTab = settings.ActiveTab.BreadCrumbs[1] as TabInfo;
            Assert.AreEqual(2, settings.ActiveTab.BreadCrumbs.Count);
            Assert.AreEqual(ValidTabId, actualTab.TabID);
            Assert.AreEqual(ParentTabId, actualParent.TabID);
        }
    }
}
