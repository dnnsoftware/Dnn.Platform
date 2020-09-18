// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client;

    public class PortalSettingsController : IPortalSettingsController
    {
        public static IPortalSettingsController Instance()
        {
            var controller = ComponentFactory.GetComponent<IPortalSettingsController>("PortalSettingsController");
            if (controller == null)
            {
                ComponentFactory.RegisterComponent<IPortalSettingsController, PortalSettingsController>("PortalSettingsController");
            }

            return ComponentFactory.GetComponent<IPortalSettingsController>("PortalSettingsController");
        }

        public virtual void ConfigureActiveTab(PortalSettings portalSettings)
        {
            var activeTab = portalSettings.ActiveTab;

            if (activeTab == null || activeTab.TabID == Null.NullInteger)
            {
                return;
            }

            this.UpdateSkinSettings(activeTab, portalSettings);

            activeTab.BreadCrumbs = new ArrayList(this.GetBreadcrumbs(activeTab.TabID, portalSettings.PortalId));
        }

        public virtual TabInfo GetActiveTab(int tabId, PortalSettings portalSettings)
        {
            var portalId = portalSettings.PortalId;
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);

            // Check portal
            var activeTab = GetTab(tabId, portalTabs)
                ?? GetTab(tabId, hostTabs) // check host
                ?? GetSpecialTab(portalId, portalSettings.SplashTabId) // check splash tab
                ?? GetSpecialTab(portalId, portalSettings.HomeTabId); // check home tab

            if (activeTab == null)
            {
                var tab = (from TabInfo t in portalTabs.AsList() where !t.IsDeleted && t.IsVisible && t.HasAVisibleVersion select t).FirstOrDefault();

                if (tab != null)
                {
                    activeTab = tab.Clone();
                }
            }

            if (activeTab != null)
            {
                if (Null.IsNull(activeTab.StartDate))
                {
                    activeTab.StartDate = DateTime.MinValue;
                }

                if (Null.IsNull(activeTab.EndDate))
                {
                    activeTab.EndDate = DateTime.MaxValue;
                }
            }

            return activeTab;
        }

        public virtual PortalSettings.PortalAliasMapping GetPortalAliasMappingMode(int portalId)
        {
            var aliasMapping = PortalSettings.PortalAliasMapping.None;
            string setting;
            if (PortalController.Instance.GetPortalSettings(portalId).TryGetValue("PortalAliasMapping", out setting))
            {
                switch (setting.ToUpperInvariant())
                {
                    case "CANONICALURL":
                        aliasMapping = PortalSettings.PortalAliasMapping.CanonicalUrl;
                        break;
                    case "REDIRECT":
                        aliasMapping = PortalSettings.PortalAliasMapping.Redirect;
                        break;
                    default:
                        aliasMapping = PortalSettings.PortalAliasMapping.None;
                        break;
                }
            }

            return aliasMapping;
        }

        public virtual IList<ModuleInfo> GetTabModules(PortalSettings portalSettings)
        {
            return portalSettings.ActiveTab.Modules.Cast<ModuleInfo>().Select(m => m).ToList();
        }

        public virtual void LoadPortal(PortalInfo portal, PortalSettings portalSettings)
        {
            portalSettings.PortalName = portal.PortalName;
            portalSettings.LogoFile = portal.LogoFile;
            portalSettings.FooterText = portal.FooterText;
            portalSettings.ExpiryDate = portal.ExpiryDate;
            portalSettings.UserRegistration = portal.UserRegistration;
            portalSettings.BannerAdvertising = portal.BannerAdvertising;
            portalSettings.Currency = portal.Currency;
            portalSettings.AdministratorId = portal.AdministratorId;
            portalSettings.Email = portal.Email;
            portalSettings.HostFee = portal.HostFee;
            portalSettings.HostSpace = Null.IsNull(portal.HostSpace) ? 0 : portal.HostSpace;
            portalSettings.PageQuota = portal.PageQuota;
            portalSettings.UserQuota = portal.UserQuota;
            portalSettings.AdministratorRoleId = portal.AdministratorRoleId;
            portalSettings.AdministratorRoleName = portal.AdministratorRoleName;
            portalSettings.RegisteredRoleId = portal.RegisteredRoleId;
            portalSettings.RegisteredRoleName = portal.RegisteredRoleName;
            portalSettings.Description = portal.Description;
            portalSettings.KeyWords = portal.KeyWords;
            portalSettings.BackgroundFile = portal.BackgroundFile;
            portalSettings.GUID = portal.GUID;
            portalSettings.AdminTabId = portal.AdminTabId;
            portalSettings.SuperTabId = portal.SuperTabId;
            portalSettings.SplashTabId = portal.SplashTabId;
            portalSettings.HomeTabId = portal.HomeTabId;
            portalSettings.LoginTabId = portal.LoginTabId;
            portalSettings.RegisterTabId = portal.RegisterTabId;
            portalSettings.UserTabId = portal.UserTabId;
            portalSettings.SearchTabId = portal.SearchTabId;
            portalSettings.ErrorPage404 = portal.Custom404TabId;
            portalSettings.ErrorPage500 = portal.Custom500TabId;
            portalSettings.TermsTabId = portal.TermsTabId;
            portalSettings.PrivacyTabId = portal.PrivacyTabId;
            portalSettings.DefaultLanguage = Null.IsNull(portal.DefaultLanguage) ? Localization.SystemLocale : portal.DefaultLanguage;
            portalSettings.HomeDirectory = Globals.ApplicationPath + "/" + portal.HomeDirectory + "/";
            portalSettings.HomeDirectoryMapPath = portal.HomeDirectoryMapPath;
            portalSettings.HomeSystemDirectory = Globals.ApplicationPath + "/" + portal.HomeSystemDirectory + "/";
            portalSettings.HomeSystemDirectoryMapPath = portal.HomeSystemDirectoryMapPath;
            portalSettings.Pages = portal.Pages;
            portalSettings.Users = portal.Users;
            portalSettings.CultureCode = portal.CultureCode;
        }

        public virtual void LoadPortalSettings(PortalSettings portalSettings)
        {
            var settings = PortalController.Instance.GetPortalSettings(portalSettings.PortalId);
            portalSettings.Registration = new RegistrationSettings(settings);

            var clientResourcesSettings = new ClientResourceSettings();
            bool overridingDefaultSettings = clientResourcesSettings.IsOverridingDefaultSettingsEnabled();

            int crmVersion;
            if (overridingDefaultSettings)
            {
                int? globalVersion = new ClientResourceSettings().GetVersion();
                crmVersion = globalVersion ?? default(int);
            }
            else
            {
                crmVersion = settings.GetValueOrDefault("CrmVersion", HostController.Instance.GetInteger("CrmVersion"));
            }

            portalSettings.AllowUserUICulture = settings.GetValueOrDefault("AllowUserUICulture", false);
            portalSettings.CdfVersion = crmVersion;
            portalSettings.ContentLocalizationEnabled = settings.GetValueOrDefault("ContentLocalizationEnabled", false);
            portalSettings.DefaultAdminContainer = settings.GetValueOrDefault("DefaultAdminContainer", Host.Host.DefaultAdminContainer);
            portalSettings.DefaultAdminSkin = settings.GetValueOrDefault("DefaultAdminSkin", Host.Host.DefaultAdminSkin);
            portalSettings.DefaultIconLocation = settings.GetValueOrDefault("DefaultIconLocation", "icons/sigma");
            portalSettings.DefaultModuleId = settings.GetValueOrDefault("defaultmoduleid", Null.NullInteger);
            portalSettings.DefaultModuleActionMenu = settings.GetValueOrDefault("DefaultModuleActionMenu", "~/admin/Menus/ModuleActions/ModuleActions.ascx");
            portalSettings.DefaultPortalContainer = settings.GetValueOrDefault("DefaultPortalContainer", Host.Host.DefaultPortalContainer);
            portalSettings.DefaultPortalSkin = settings.GetValueOrDefault("DefaultPortalSkin", Host.Host.DefaultPortalSkin);
            portalSettings.DefaultTabId = settings.GetValueOrDefault("defaulttabid", Null.NullInteger);
            portalSettings.EnableBrowserLanguage = settings.GetValueOrDefault("EnableBrowserLanguage", Host.Host.EnableBrowserLanguage);
            portalSettings.EnableCompositeFiles = settings.GetValueOrDefault("EnableCompositeFiles", false);
            portalSettings.EnablePopUps = settings.GetValueOrDefault("EnablePopUps", true);
            portalSettings.HideLoginControl = settings.GetValueOrDefault("HideLoginControl", false);
            portalSettings.EnableSkinWidgets = settings.GetValueOrDefault("EnableSkinWidgets", true);
            portalSettings.ShowCookieConsent = settings.GetValueOrDefault("ShowCookieConsent", false);
            portalSettings.CookieMoreLink = settings.GetValueOrDefault("CookieMoreLink", Null.NullString);
            portalSettings.EnableUrlLanguage = settings.GetValueOrDefault("EnableUrlLanguage", Host.Host.EnableUrlLanguage);
            portalSettings.HideFoldersEnabled = settings.GetValueOrDefault("HideFoldersEnabled", true);
            portalSettings.InlineEditorEnabled = settings.GetValueOrDefault("InlineEditorEnabled", true);
            portalSettings.SearchIncludeCommon = settings.GetValueOrDefault("SearchIncludeCommon", Host.Host.SearchIncludeCommon);
            portalSettings.SearchIncludeNumeric = settings.GetValueOrDefault("SearchIncludeNumeric", Host.Host.SearchIncludeNumeric);
            portalSettings.SearchIncludedTagInfoFilter = settings.GetValueOrDefault("SearchIncludedTagInfoFilter", Host.Host.SearchIncludedTagInfoFilter);
            portalSettings.SearchMaxWordlLength = settings.GetValueOrDefault("MaxSearchWordLength", Host.Host.SearchMaxWordlLength);
            portalSettings.SearchMinWordlLength = settings.GetValueOrDefault("MinSearchWordLength", Host.Host.SearchMinWordlLength);
            portalSettings.SSLEnabled = settings.GetValueOrDefault("SSLEnabled", false);
            portalSettings.SSLEnforced = settings.GetValueOrDefault("SSLEnforced", false);
            portalSettings.SSLURL = settings.GetValueOrDefault("SSLURL", Null.NullString);
            portalSettings.STDURL = settings.GetValueOrDefault("STDURL", Null.NullString);
            portalSettings.EnableRegisterNotification = settings.GetValueOrDefault("EnableRegisterNotification", true);
            portalSettings.DefaultAuthProvider = settings.GetValueOrDefault("DefaultAuthProvider", "DNN");
            portalSettings.SMTPConnectionLimit = settings.GetValueOrDefault("SMTPConnectionLimit", 2);
            portalSettings.SMTPMaxIdleTime = settings.GetValueOrDefault("SMTPMaxIdleTime", 100000);

            portalSettings.ControlPanelSecurity = PortalSettings.ControlPanelPermission.ModuleEditor;
            string setting = settings.GetValueOrDefault("ControlPanelSecurity", string.Empty);
            if (setting.Equals("TAB", StringComparison.InvariantCultureIgnoreCase))
            {
                portalSettings.ControlPanelSecurity = PortalSettings.ControlPanelPermission.TabEditor;
            }

            portalSettings.DefaultControlPanelMode = PortalSettings.Mode.View;
            setting = settings.GetValueOrDefault("ControlPanelMode", string.Empty);
            if (setting.Equals("EDIT", StringComparison.InvariantCultureIgnoreCase))
            {
                portalSettings.DefaultControlPanelMode = PortalSettings.Mode.Edit;
            }

            setting = settings.GetValueOrDefault("ControlPanelVisibility", string.Empty);
            portalSettings.DefaultControlPanelVisibility = !setting.Equals("MIN", StringComparison.InvariantCultureIgnoreCase);

            setting = settings.GetValueOrDefault("TimeZone", string.Empty);
            if (!string.IsNullOrEmpty(setting))
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(setting);
                if (timeZone != null)
                {
                    portalSettings.TimeZone = timeZone;
                }
            }

            setting = settings.GetValueOrDefault("DataConsentActive", "False");
            portalSettings.DataConsentActive = bool.Parse(setting);
            setting = settings.GetValueOrDefault("DataConsentTermsLastChange", string.Empty);
            if (!string.IsNullOrEmpty(setting))
            {
                portalSettings.DataConsentTermsLastChange = DateTime.Parse(setting, System.Globalization.CultureInfo.InvariantCulture);
            }

            setting = settings.GetValueOrDefault("DataConsentConsentRedirect", "-1");
            portalSettings.DataConsentConsentRedirect = int.Parse(setting);
            setting = settings.GetValueOrDefault("DataConsentUserDeleteAction", "0");
            portalSettings.DataConsentUserDeleteAction = (PortalSettings.UserDeleteAction)int.Parse(setting);
            setting = settings.GetValueOrDefault("DataConsentDelay", "1");
            portalSettings.DataConsentDelay = int.Parse(setting);
            setting = settings.GetValueOrDefault("DataConsentDelayMeasurement", "d");
            portalSettings.DataConsentDelayMeasurement = setting;
            setting = settings.GetValueOrDefault("AllowedExtensionsWhitelist", HostController.Instance.GetString("DefaultEndUserExtensionWhitelist"));
            portalSettings.AllowedExtensionsWhitelist = new FileExtensionWhitelist(setting);
        }

        protected List<TabInfo> GetBreadcrumbs(int tabId, int portalId)
        {
            var breadCrumbs = new List<TabInfo>();
            GetBreadCrumbs(breadCrumbs, tabId, portalId);
            return breadCrumbs;
        }

        protected virtual void UpdateSkinSettings(TabInfo activeTab, PortalSettings portalSettings)
        {
            if (Globals.IsAdminSkin())
            {
                // DNN-6170 ensure skin value is culture specific
                activeTab.SkinSrc = string.IsNullOrEmpty(PortalController.GetPortalSetting("DefaultAdminSkin", portalSettings.PortalId,
                    Host.Host.DefaultAdminSkin, portalSettings.CultureCode)) ? portalSettings.DefaultAdminSkin : PortalController.GetPortalSetting("DefaultAdminSkin", portalSettings.PortalId,
                    Host.Host.DefaultAdminSkin, portalSettings.CultureCode);
            }
            else if (string.IsNullOrEmpty(activeTab.SkinSrc))
            {
                // DNN-6170 ensure skin value is culture specific
                activeTab.SkinSrc = string.IsNullOrEmpty(PortalController.GetPortalSetting("DefaultPortalSkin", portalSettings.PortalId,
                    Host.Host.DefaultPortalSkin, portalSettings.CultureCode)) ? portalSettings.DefaultPortalSkin : PortalController.GetPortalSetting("DefaultPortalSkin", portalSettings.PortalId,
                    Host.Host.DefaultPortalSkin, portalSettings.CultureCode);
            }

            activeTab.SkinSrc = SkinController.FormatSkinSrc(activeTab.SkinSrc, portalSettings);
            activeTab.SkinPath = SkinController.FormatSkinPath(activeTab.SkinSrc);

            if (Globals.IsAdminSkin())
            {
                activeTab.ContainerSrc = string.IsNullOrEmpty(PortalController.GetPortalSetting("DefaultAdminContainer", portalSettings.PortalId,
                    Host.Host.DefaultAdminContainer, portalSettings.CultureCode)) ? portalSettings.DefaultAdminContainer : PortalController.GetPortalSetting("DefaultAdminContainer", portalSettings.PortalId,
                    Host.Host.DefaultAdminContainer, portalSettings.CultureCode);
            }
            else if (string.IsNullOrEmpty(activeTab.ContainerSrc))
            {
                activeTab.ContainerSrc = string.IsNullOrEmpty(PortalController.GetPortalSetting("DefaultPortalContainer", portalSettings.PortalId,
                    Host.Host.DefaultPortalContainer, portalSettings.CultureCode)) ? portalSettings.DefaultPortalContainer : PortalController.GetPortalSetting("DefaultPortalContainer", portalSettings.PortalId,
                    Host.Host.DefaultPortalContainer, portalSettings.CultureCode);
            }

            activeTab.ContainerSrc = SkinController.FormatSkinSrc(activeTab.ContainerSrc, portalSettings);
            activeTab.ContainerPath = SkinController.FormatSkinPath(activeTab.ContainerSrc);
        }

        private static void GetBreadCrumbs(IList<TabInfo> breadCrumbs, int tabId, int portalId)
        {
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);
            while (true)
            {
                TabInfo tab;
                if (portalTabs.TryGetValue(tabId, out tab) || hostTabs.TryGetValue(tabId, out tab))
                {
                    // add tab to breadcrumb collection
                    breadCrumbs.Insert(0, tab.Clone());

                    // get the tab parent
                    if (!Null.IsNull(tab.ParentId) && tabId != tab.ParentId)
                    {
                        tabId = tab.ParentId;
                        continue;
                    }
                }

                break;
            }
        }

        private static TabInfo GetSpecialTab(int portalId, int tabId)
        {
            TabInfo activeTab = null;

            if (tabId > 0)
            {
                var tab = TabController.Instance.GetTab(tabId, portalId, false);
                if (tab != null)
                {
                    activeTab = tab.Clone();
                }
            }

            return activeTab;
        }

        // This method is called few times wiht each request; it would be
        // better to have it cache the "activeTab" for a short period.
        private static TabInfo GetTab(int tabId, TabCollection tabs)
        {
            TabInfo tab;
            var activeTab = tabId != Null.NullInteger && tabs.TryGetValue(tabId, out tab) && !tab.IsDeleted
                ? tab.Clone()
                : null;
            return activeTab;
        }
    }
}
