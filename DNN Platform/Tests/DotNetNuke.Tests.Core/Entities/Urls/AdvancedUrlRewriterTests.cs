namespace DotNetNuke.Tests.Core.Entities.Urls;

using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Reflection;
using System.Web;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Tests.Instance.Utilities.HttpSimulator;
using DotNetNuke.Tests.Utilities.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AdvancedUrlRewriterTests
{
    private const int GenericPortalId = 1;
    private const int GenericTabId = 1;
    private const string GenericHost = "dnn";

    [Test]

    public void CheckForRedirects_WithUmlautUrls_DontRedirectInfinitely()
    {
        // Arrange
        const string CheckForRedirectsMethodName = "CheckForRedirects";
        const string HostSettingsTableName = "HostSettings";
        const string PortalSettingsTableName = "PortalSettings";
        const string LanguagesTableName = "Languages";
        const string TabsTableName = "Tabs";
        const string TabUrlsTableName = "TabUrls";
        const string PortalsTableName = "Portals";
        const string TabSettingsTableName = "TabSettings";
        const string PortalAliasTableName = "PortalAlias";
        const string ExtensionUrlProviderTableName = "ExtensionUrlProvider";
        const string HttpScheme = "http";
        const string HttpsScheme = "https";
        const string UriUrl = HttpScheme + "://" + GenericHost + "/über-uns/ctl/module/moduleid/466?returnurl=/über-uns";
        const string FullUrl = HttpScheme + "://" + GenericHost + "/%C3%BCber-uns/ctl/module/moduleid/466?returnurl=/%C3%BCber-uns";
        const string RewritePath = "Default.aspx?TabId=1&ctl=module&moduleid=466&returnurl=/%C3%BCber-uns&language=en-US";
        const string SampleHttpsUrl = HttpsScheme + "://google.com";
        const string PortalSettingsControllerRegistrationName = "PortalSettingsController";
        const string DNNPlatformText = "DNN Platform";
        const string WebsiteText = "Website";
        const string ApplicationPath = "/";
        const string UrlRewriteItemName = "UrlRewrite:OriginalUrl";
        ComponentFactory.Container = null;
        PortalController.ClearInstance();
        Host.PerformanceSetting = Globals.PerformanceSettings.ModerateCaching;
        var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
        var path = HttpUtility.UrlDecode(Path.GetFullPath(uri.AbsolutePath));
        var websiteRootPath = path.Substring(0, path.IndexOf(DNNPlatformText, StringComparison.Ordinal));
        var physicalAppPath = Path.Combine(websiteRootPath, WebsiteText);
        var simulator = new HttpSimulator(ApplicationPath, physicalAppPath);
        simulator.SimulateRequest(new Uri(SampleHttpsUrl));
        HttpContext.Current.Items.Add(UrlRewriteItemName, FullUrl);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(container => Mock.Of<IHostSettingsService>());
        serviceCollection.AddTransient(container => Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddTransient(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient(container => Mock.Of<IPortalAliasService>());
        MockComponentProvider.CreateNew<CachingProvider>();
        MockComponentProvider.CreateNew<DataProvider>();
        MockComponentProvider.CreateNew<LoggingProvider>();
        MockComponentProvider.CreateNew<PortalSettingsController>(PortalSettingsControllerRegistrationName);
        var dataProvider = MockComponentProvider.CreateDataProvider();
        var hostSettingsTable = new DataTable(HostSettingsTableName);
        dataProvider
            .Setup(s => s.GetHostSettings())
            .Returns(() => hostSettingsTable.CreateDataReader());
        var portalSettingsTable = new DataTable(PortalSettingsTableName);
        dataProvider
            .Setup(s => s.GetPortalSettings(
                It.IsAny<int>(),
                It.IsAny<string>()))
            .Returns(() => portalSettingsTable.CreateDataReader());
        var languagesTable = new DataTable(LanguagesTableName);
        dataProvider
            .Setup(s => s.GetLanguagesByPortal(It.IsAny<int>()))
            .Returns(() => languagesTable.CreateDataReader());
        var tabsTable = new DataTable(TabsTableName);
        FillTabsTable(tabsTable);
        dataProvider
            .Setup(s => s.GetTabs(It.IsAny<int>()))
            .Returns(() => tabsTable.CreateDataReader());
        dataProvider
            .Setup(s => s.GetTabCustomAliases(It.IsAny<int>()))
            .Returns(() => tabsTable.CreateDataReader());
        var tabUrlsTable = new DataTable(TabUrlsTableName);
        dataProvider
            .Setup(s => s.GetTabPaths(
                It.IsAny<int>(),
                It.IsAny<string>()))
            .Returns(() => tabUrlsTable.CreateDataReader());
        dataProvider
            .Setup(s => s.GetTabUrls(It.IsAny<int>()))
            .Returns(() => tabUrlsTable.CreateDataReader());
        var tabSettingsTable = new DataTable(TabSettingsTableName);
        dataProvider
            .Setup(s => s.GetTabSettings(It.IsAny<int>()))
            .Returns(() => tabSettingsTable.CreateDataReader());
        var portalsTable = new DataTable(PortalsTableName);
        FillPortalsTable(portalsTable);
        dataProvider
            .Setup(s => s.GetPortals(It.IsAny<string>()))
            .Returns(() => portalsTable.CreateDataReader());
        var extensionUrlProviderTable = new DataTable(ExtensionUrlProviderTableName);
        dataProvider
            .Setup(s => s.GetExtensionUrlProviders(It.IsAny<int>()))
            .Returns(() => extensionUrlProviderTable.CreateDataReader());
        var portalAliasTable = new DataTable(PortalAliasTableName);
        FillPortalAliasTable(portalAliasTable);
        dataProvider
            .Setup(s => s.GetPortalAliases())
            .Returns(() => portalAliasTable.CreateDataReader());
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
        var urlRewriter = new AdvancedUrlRewriter();
        var checkForRedirectsMethod = typeof(AdvancedUrlRewriter)
            .GetMethod(
                CheckForRedirectsMethodName,
                BindingFlags.Static | BindingFlags.NonPublic);
        var requestUri = new Uri(UriUrl);
        var queryStringCollection = new NameValueCollection();
        var friendlyUrlSettings = new FriendlyUrlSettings(GenericPortalId);
        var urlAction = new UrlAction(
            HttpScheme,
            string.Empty,
            string.Empty)
        {
            TabId = GenericTabId,
            Action = ActionType.CheckFor301,
            PortalId = GenericPortalId,
            IsSSLOffloaded = true,
            PortalAlias = new PortalAliasInfo
            {
                HTTPAlias = GenericHost,
            },
            DoRewrite = true,
            RewritePath = RewritePath,
        };
        urlAction.SetRedirectAllowed(string.Empty, friendlyUrlSettings);
        var requestType = string.Empty;
        var portalHomeTabId = GenericTabId;

        // Act
        var isRedirected = checkForRedirectsMethod.Invoke(
            urlRewriter,
            new object[]
            {
                requestUri,
                FullUrl,
                queryStringCollection,
                urlAction,
                requestType,
                friendlyUrlSettings,
                portalHomeTabId,
            });

        // Assert
        Assert.That(isRedirected, Is.EqualTo(false));
    }

    private void FillTabsTable(DataTable tabsTable)
    {
        var tabColumns = new string[]
        {
            "TabID",
            "UniqueId",
            "VersionGuid",
            "DefaultLanguageGuid",
            "LocalizedVersionGuid",
            "TabOrder",
            "PortalID",
            "TabName",
            "IsVisible",
            "ParentId",
            "Level",
            "IconFile",
            "IconFileLarge",
            "DisableLink",
            "Title",
            "Description",
            "KeyWords",
            "IsDeleted",
            "SkinSrc",
            "ContainerSrc",
            "TabPath",
            "StartDate",
            "EndDate",
            "Url",
            "HasChildren",
            "RefreshInterval",
            "PageHeadText",
            "IsSecure",
            "PermanentRedirect",
            "SiteMapPriority",
            "ContentItemID",
            "Content",
            "ContentTypeID",
            "ModuleID",
            "ContentKey",
            "Indexed",
            "CultureCode",
            "CreatedByUserID",
            "CreatedOnDate",
            "LastModifiedByUserID",
            "LastModifiedOnDate",
            "StateID",
            "HasBeenPublished",
            "IsSystem",
        };
        foreach (var column in tabColumns)
        {
            tabsTable.Columns.Add(column);
        }

        tabsTable.Rows.Add(
            GenericTabId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            "3",
            GenericPortalId,
            "Über-uns",
            true,
            null,
            "0",
            null,
            null,
            false,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            "[G]Skins/DarkKnight/Home-Mega-Menu.ascx",
            "[G]Containers/DarkKnight/SubTitle_Grey.ascx",
            "//Über-uns",
            null,
            null,
            string.Empty,
            false,
            null,
            null,
            true,
            false,
            "0.5",
            "89",
            "Über-uns",
            "1",
            "-1",
            null,
            false,
            null,
            "-1",
            DateTime.Now,
            "-1",
            DateTime.Now,
            "0",
            true,
            false);

    }

    private void FillPortalsTable(DataTable portalsTable)
    {
        var portalColumns = new[]
        {
            "PortalID",
            "PortalGroupID",
            "PortalName",
            "LogoFile",
            "FooterText",
            "ExpiryDate",
            "UserRegistration",
            "BannerAdvertising",
            "AdministratorId",
            "Currency",
            "HostFee",
            "HostSpace",
            "PageQuota",
            "UserQuota",
            "AdministratorRoleId",
            "RegisteredRoleId",
            "Description",
            "KeyWords",
            "BackgroundFile",
            "GUID",
            "PaymentProcessor",
            "ProcessorUserId",
            "ProcessorPassword",
            "SiteLogHistory",
            "Email",
            "DefaultLanguage",
            "TimezoneOffset",
            "AdminTabId",
            "HomeDirectory",
            "SplashTabId",
            "HomeTabId",
            "LoginTabId",
            "RegisterTabId",
            "UserTabId",
            "SearchTabId",
            "Custom404TabId",
            "Custom500TabId",
            "TermsTabId",
            "PrivacyTabId",
            "SuperTabId",
            "CreatedByUserID",
            "CreatedOnDate",
            "LastModifiedByUserID",
            "LastModifiedOnDate",
            "CultureCode",
        };
        foreach (var column in portalColumns)
        {
            portalsTable.Columns.Add(column);
        }

        portalsTable.Rows.Add(
            GenericPortalId,
            null,
            "My Website",
            "Logo.png",
            "Copyright (c) 2018 DNN Corp.",
            null,
            "2",
            "0",
            "2",
            "USD",
            "0",
            "0",
            "0",
            "0",
            "0",
            "1",
            "My Website",
            "DotNetNuke, DNN, Content, Management, CMS",
            null,
            "1057AC7A-3C08-4849-A3A6-3D2AB4662020",
            null,
            null,
            null,
            "0",
            "admin@changeme.invalid",
            "en-US",
            "-8",
            "1",
            "Portals/0",
            null,
            "1",
            null,
            null,
            "57",
            "56",
            "-1",
            "-1",
            null,
            null,
            "7",
            "-1",
            "2011-08-25 07:34:11",
            "-1",
            "2011-08-25 07:34:29",
            "en-US");
    }

    private void FillPortalAliasTable(DataTable portalAliasTable)
    {
        var portalAliasColumns = new[]
        {
            "PortalAliasId",
            "PortalID",
            "HTTPAlias",
            "CreatedByUserID",
            "CreatedOnDate",
            "LastModifiedByUserID",
            "LastModifiedOnDate",
            "BrowserType",
            "Skin",
            "CultureCode",
            "IsPrimary",
        };
        foreach (var column in portalAliasColumns)
        {
            portalAliasTable.Columns.Add(column);
        }

        portalAliasTable.Rows.Add(
            1,
            GenericPortalId,
            GenericHost,
            1,
            "2011-08-25 07:34:11",
            1,
            "2011-08-25 07:34:11",
            "Normal",
            null,
            "en-US",
            true);
    }
}
