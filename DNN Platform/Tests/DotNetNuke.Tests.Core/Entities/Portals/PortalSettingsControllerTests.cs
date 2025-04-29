// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Portals;

using System;
using System.Collections.Generic;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.UI.Skins;

using Microsoft.Extensions.DependencyInjection;

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

    private Mock<IHostController> mockHostController;

    [SetUp]
    public void SetUp()
    {
        MockComponentProvider.ResetContainer();

        var serviceCollection = new ServiceCollection();
        var mockApplicationInfo = new Mock<IApplicationStatusInfo>();
        mockApplicationInfo.Setup(info => info.ApplicationMapPath).Returns("path/to/application");

        this.mockHostController = new Mock<IHostController>();
        this.mockHostController.As<IHostSettingsService>();

        serviceCollection.AddTransient<IApplicationStatusInfo>(container => mockApplicationInfo.Object);
        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient<IHostSettingsService>(container => (IHostSettingsService)this.mockHostController.Object);
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        PortalController.ClearInstance();
        TabController.ClearInstance();
        Globals.DependencyProvider = null;

    }

    [Test]
    [TestCaseSource(typeof(PortalSettingsControllerTestFactory), nameof(PortalSettingsControllerTestFactory.LoadPortalSettings_Loads_Default_Value))]

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

        this.mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
            .Returns((string s) => hostSettings[s]);
        this.mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string s1, string s2) => hostSettings[s1]);
        this.mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string s, bool b) => bool.Parse(hostSettings[s]));
        this.mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
            .Returns((string s, int i) => int.Parse(hostSettings[s]));
        this.mockHostController.Setup(c => c.GetInteger(It.IsAny<string>()))
            .Returns((string s) => int.Parse(hostSettings[s]));

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
            Assert.That(actualValue.ToString().ToLowerInvariant(), Is.EqualTo(defaultValue));
        }
        else
        {
            Assert.That(actualValue.ToString(), Is.EqualTo(defaultValue));
        }
    }

    [Test]
    [TestCaseSource(typeof(PortalSettingsControllerTestFactory), nameof(PortalSettingsControllerTestFactory.LoadPortalSettings_Loads_Setting_Value))]

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

        this.mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
            .Returns((string s) => hostSettings[s]);
        this.mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string s1, string s2) => hostSettings[s1]);
        this.mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string s, bool b) => bool.Parse(hostSettings[s]));
        this.mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
            .Returns((string s, int i) => int.Parse(hostSettings[s]));

        // Act
        controller.LoadPortalSettings(settings);

        // Assert
        var property = settings.GetType().GetProperty(propertyName);
        var actualValue = property.GetValue(settings, null);
        if (actualValue is bool)
        {
            Assert.That(actualValue.ToString().ToLowerInvariant(), Is.EqualTo(propertyValue));
        }
        else
        {
            Assert.That(actualValue.ToString(), Is.EqualTo(propertyValue));
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

        this.mockHostController.Setup(c => c.GetString(It.IsAny<string>()))
            .Returns((string s) => hostSettings[s]);
        this.mockHostController.Setup(c => c.GetString(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string s1, string s2) => hostSettings[s1]);
        this.mockHostController.Setup(c => c.GetBoolean(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string s, bool b) => bool.Parse(hostSettings[s]));
        this.mockHostController.Setup(c => c.GetInteger(It.IsAny<string>(), It.IsAny<int>()))
            .Returns((string s, int i) => int.Parse(hostSettings[s]));

        // Act
        controller.LoadPortalSettings(settings);

        // Assert
        Assert.That(settings.TimeZone, Is.EqualTo(TimeZoneInfo.Local));
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

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(settings.AdminTabId, Is.EqualTo(portal.AdminTabId));
            Assert.That(settings.AdministratorId, Is.EqualTo(portal.AdministratorId));
            Assert.That(settings.AdministratorRoleId, Is.EqualTo(portal.AdministratorRoleId));
            Assert.That(settings.AdministratorRoleName, Is.EqualTo(portal.AdministratorRoleName));
            Assert.That(settings.BackgroundFile, Is.EqualTo(portal.BackgroundFile));
            Assert.That(settings.BannerAdvertising, Is.EqualTo(portal.BannerAdvertising));
            Assert.That(settings.CultureCode, Is.EqualTo(portal.CultureCode));
            Assert.That(settings.Currency, Is.EqualTo(portal.Currency));
            Assert.That(settings.ErrorPage404, Is.EqualTo(portal.Custom404TabId));
            Assert.That(settings.ErrorPage500, Is.EqualTo(portal.Custom500TabId));
            Assert.That(settings.TermsTabId, Is.EqualTo(portal.TermsTabId));
            Assert.That(settings.PrivacyTabId, Is.EqualTo(portal.PrivacyTabId));
            Assert.That(settings.DefaultLanguage, Is.EqualTo(portal.DefaultLanguage));
            Assert.That(settings.Description, Is.EqualTo(portal.Description));
            Assert.That(settings.Email, Is.EqualTo(portal.Email));
            Assert.That(settings.ExpiryDate, Is.EqualTo(portal.ExpiryDate));
            Assert.That(settings.FooterText, Is.EqualTo(portal.FooterText));
            Assert.That(settings.GUID, Is.EqualTo(portal.GUID));
            Assert.That(settings.HomeDirectory, Is.EqualTo(Globals.ApplicationPath + "/" + portal.HomeDirectory + "/"));
            Assert.That(settings.HomeDirectoryMapPath, Is.EqualTo(portal.HomeDirectoryMapPath));
            Assert.That(settings.HomeSystemDirectory, Is.EqualTo(Globals.ApplicationPath + "/" + portal.HomeSystemDirectory + "/"));
            Assert.That(settings.HomeSystemDirectoryMapPath, Is.EqualTo(portal.HomeSystemDirectoryMapPath));
            Assert.That(settings.HomeTabId, Is.EqualTo(portal.HomeTabId));
            Assert.That(settings.HostFee, Is.EqualTo(portal.HostFee));
            Assert.That(settings.HostSpace, Is.EqualTo(portal.HostSpace));
            Assert.That(settings.KeyWords, Is.EqualTo(portal.KeyWords));
            Assert.That(settings.LoginTabId, Is.EqualTo(portal.LoginTabId));
            Assert.That(settings.LogoFile, Is.EqualTo(portal.LogoFile));
            Assert.That(settings.PageQuota, Is.EqualTo(portal.PageQuota));
            Assert.That(settings.Pages, Is.EqualTo(portal.Pages));
            Assert.That(settings.PortalName, Is.EqualTo(portal.PortalName));
            Assert.That(settings.RegisterTabId, Is.EqualTo(portal.RegisterTabId));
            Assert.That(settings.RegisteredRoleId, Is.EqualTo(portal.RegisteredRoleId));
            Assert.That(settings.RegisteredRoleName, Is.EqualTo(portal.RegisteredRoleName));
            Assert.That(settings.SearchTabId, Is.EqualTo(portal.SearchTabId));
            Assert.That(settings.SplashTabId, Is.EqualTo(portal.SplashTabId));
            Assert.That(settings.SuperTabId, Is.EqualTo(portal.SuperTabId));
            Assert.That(settings.UserQuota, Is.EqualTo(portal.UserQuota));
            Assert.That(settings.UserRegistration, Is.EqualTo(portal.UserRegistration));
            Assert.That(settings.UserTabId, Is.EqualTo(portal.UserTabId));
            Assert.That(settings.Users, Is.EqualTo(portal.Users));
        });
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
        Assert.That(tab.TabID, Is.EqualTo(validTab.TabID));
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
        Assert.That(tab.TabID, Is.EqualTo(validTab.TabID));
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
        Assert.That(tab.TabID, Is.EqualTo(SplashTabId));
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
        Assert.That(tab.TabID, Is.EqualTo(HomeTabId));
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
        Assert.That(tab.TabID, Is.EqualTo(SplashTabId));
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

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(tab.StartDate, Is.EqualTo(DateTime.MinValue));
            Assert.That(tab.EndDate, Is.EqualTo(DateTime.MaxValue));
        });
    }

    [Test]

    public void ConfigureTab_Uses_PortalSettings_DefaultSkin_If_SkinSrc_Not_Set()
    {
        // Arrange
        var controller = new PortalSettingsController();
        var settings = new PortalSettings { PortalId = ValidPortalId, DefaultPortalSkin = DefaultSkin, DefaultPortalContainer = DefaultContainer, CultureCode = Null.NullString };
        var validTab = new TabInfo { TabID = ValidTabId, PortalID = ValidPortalId };
        settings.ActiveTab = validTab;

        this.mockHostController.Setup(c => c.GetString("DefaultPortalSkin")).Returns(DefaultSkin);
        this.mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");

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
        Assert.That(settings.ActiveTab.SkinSrc, Is.EqualTo(DefaultSkin));
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

        this.mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");

        // Act
        controller.ConfigureActiveTab(settings);

        // Assert
        Assert.That(settings.ActiveTab.SkinSrc, Is.EqualTo(TabSkin));
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

        this.mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");

        // Act
        controller.ConfigureActiveTab(settings);

        // Assert
        Assert.That(settings.ActiveTab.SkinSrc, Is.EqualTo(SkinController.FormatSkinSrc(GlobalTabSkin, settings)));
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

        this.mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");

        // Act
        controller.ConfigureActiveTab(settings);

        // Assert
        Assert.That(settings.ActiveTab.ContainerSrc, Is.EqualTo(DefaultContainer));
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
        Assert.That(settings.ActiveTab.ContainerSrc, Is.EqualTo(TabContainer));
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
        Assert.That(settings.ActiveTab.ContainerSrc, Is.EqualTo(SkinController.FormatSkinSrc(GlobalTabContainer, settings)));
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

        this.mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");

        // Act
        controller.ConfigureActiveTab(settings);

        // Assert
        Assert.That(settings.ActiveTab.BreadCrumbs, Is.Not.Null);
        Assert.That(settings.ActiveTab.BreadCrumbs, Has.Count.EqualTo(1));
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

        this.mockHostController.Setup(c => c.GetString("DefaultPortalContainer")).Returns("DefaultPortalContainer");

        // Act
        controller.ConfigureActiveTab(settings);

        // Assert
        var actualParent = settings.ActiveTab.BreadCrumbs[0] as TabInfo;
        var actualTab = settings.ActiveTab.BreadCrumbs[1] as TabInfo;
        Assert.Multiple(() =>
        {
            Assert.That(settings.ActiveTab.BreadCrumbs, Has.Count.EqualTo(2));
            Assert.That(actualTab.TabID, Is.EqualTo(ValidTabId));
            Assert.That(actualParent.TabID, Is.EqualTo(ParentTabId));
        });
    }
}
