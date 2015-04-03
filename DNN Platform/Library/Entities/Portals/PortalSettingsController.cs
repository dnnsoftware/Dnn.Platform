﻿#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;

namespace DotNetNuke.Entities.Portals
{
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

            if (activeTab == null || activeTab.TabID == Null.NullInteger) return;

            UpdateSkinSettings(activeTab, portalSettings);

            activeTab.BreadCrumbs = new ArrayList(GetBreadcrumbs(activeTab.TabID, portalSettings.PortalId));
        }

        public virtual TabInfo GetActiveTab(int tabId, PortalSettings portalSettings)
        {
            var portalId = portalSettings.PortalId;
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);

            //Check portal
            TabInfo activeTab = GetTab(tabId, portalTabs);

            if (activeTab == null)
            {
                //check host
                activeTab = GetTab(tabId, hostTabs);
            }

            if (activeTab == null)
            {
                //check splash tab
                activeTab = GetSpecialTab(portalId, portalSettings.SplashTabId);
            }

            if (activeTab == null)
            {
                //check home tab
                activeTab = GetSpecialTab(portalId, portalSettings.HomeTabId);
            }

            if (activeTab == null)
            {
                TabInfo tab = (from TabInfo t in portalTabs.AsList() where !t.IsDeleted && t.IsVisible && t.HasAVisibleVersion select t).FirstOrDefault();

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

        protected List<TabInfo> GetBreadcrumbs(int tabId, int portalId)
        {
            var breadCrumbs = new List<TabInfo>();
            GetBreadCrumbsRecursively(ref breadCrumbs, tabId, portalId);
            return breadCrumbs;            
        }

        private void GetBreadCrumbsRecursively(ref List<TabInfo> breadCrumbs, int tabId, int portalId)
        {
            TabInfo tab;
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);
            bool tabFound = portalTabs.TryGetValue(tabId, out tab);
            if (!tabFound)
            {
                tabFound = hostTabs.TryGetValue(tabId, out tab);
            }
            //if tab was found
            if (tabFound)
            {
                //add tab to breadcrumb collection
                breadCrumbs.Insert(0, tab.Clone());

                //get the tab parent
                if (!Null.IsNull(tab.ParentId) && tabId != tab.ParentId)
                {
                    GetBreadCrumbsRecursively(ref breadCrumbs, tab.ParentId, portalId);
                }
            }
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

        private TabInfo GetSpecialTab(int portalId, int tabId)
        {
            TabInfo activeTab = null;

            if (tabId > 0)
            {
                TabInfo tab = TabController.Instance.GetTab(tabId, portalId, false);
                if (tab != null)
                {
                    activeTab = tab.Clone();
                }
            }

            return activeTab;
        }

        private TabInfo GetTab(int tabId, TabCollection tabs)
        {
            TabInfo activeTab = null;

            if (tabId != Null.NullInteger)
            {
                TabInfo tab;
                if (tabs.TryGetValue(tabId, out tab))
                {
                    if (!tab.IsDeleted)
                    {
                        activeTab = tab.Clone();
                    }
                }
            }
            return activeTab;
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
            portalSettings.SiteLogHistory = portal.SiteLogHistory;
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

            portalSettings.CdfVersion = settings.GetValueOrDefault("CdfVersion", Null.NullInteger);
            portalSettings.ContentLocalizationEnabled = settings.GetValueOrDefault("ContentLocalizationEnabled", false);
            portalSettings.DefaultAdminContainer = settings.GetValueOrDefault("DefaultAdminContainer", Host.Host.DefaultAdminContainer);
            portalSettings.DefaultAdminSkin = settings.GetValueOrDefault("DefaultAdminSkin", Host.Host.DefaultAdminSkin);
            portalSettings.DefaultIconLocation = settings.GetValueOrDefault("DefaultIconLocation", "icons/sigma");
            portalSettings.DefaultModuleId = settings.GetValueOrDefault("defaultmoduleid", Null.NullInteger);
            portalSettings.DefaultPortalContainer = settings.GetValueOrDefault("DefaultPortalContainer", Host.Host.DefaultPortalContainer);
            portalSettings.DefaultPortalSkin = settings.GetValueOrDefault("DefaultPortalSkin", Host.Host.DefaultPortalSkin);
            portalSettings.DefaultTabId = settings.GetValueOrDefault("defaulttabid", Null.NullInteger);
            portalSettings.EnableBrowserLanguage = settings.GetValueOrDefault("EnableBrowserLanguage", Host.Host.EnableBrowserLanguage);
            portalSettings.EnableCompositeFiles = settings.GetValueOrDefault("EnableCompositeFiles", false);
            portalSettings.EnablePopUps = settings.GetValueOrDefault("EnablePopUps", true);
            portalSettings.HideLoginControl = settings.GetValueOrDefault("HideLoginControl", false);
            portalSettings.EnableSkinWidgets = settings.GetValueOrDefault("EnableSkinWidgets", true);
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
            portalSettings.SMTPConnectionLimit = settings.GetValueOrDefault("SMTPConnectionLimit", 1);
            portalSettings.SMTPMaxIdleTime = settings.GetValueOrDefault("SMTPMaxIdleTime", 0);

            portalSettings.ControlPanelSecurity = PortalSettings.ControlPanelPermission.ModuleEditor;
            string setting = settings.GetValueOrDefault("ControlPanelSecurity", "");
            if (setting.ToUpperInvariant() == "TAB")
            {
                portalSettings.ControlPanelSecurity = PortalSettings.ControlPanelPermission.TabEditor;
            }

            portalSettings.DefaultControlPanelMode = PortalSettings.Mode.View;
            setting = settings.GetValueOrDefault("ControlPanelMode", "");
            if (setting.ToUpperInvariant() == "EDIT")
            {
                portalSettings.DefaultControlPanelMode = PortalSettings.Mode.Edit;
            }

            setting = settings.GetValueOrDefault("ControlPanelVisibility", "");
            portalSettings.DefaultControlPanelVisibility = setting.ToUpperInvariant() != "MIN";

            setting = settings.GetValueOrDefault("TimeZone", "");
            if (!string.IsNullOrEmpty(setting))
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(setting);
                if (timeZone != null)
                    portalSettings.TimeZone = timeZone;
            }
        }

        protected virtual void UpdateSkinSettings(TabInfo activeTab, PortalSettings portalSettings)
        {
            if (Globals.IsAdminSkin())
            {
                //DNN-6170 ensure skin value is culture specific
                activeTab.SkinSrc = String.IsNullOrEmpty(PortalController.GetPortalSetting("DefaultAdminSkin", portalSettings.PortalId,
                    Host.Host.DefaultPortalSkin, portalSettings.CultureCode)) ? portalSettings.DefaultAdminSkin : PortalController.GetPortalSetting("DefaultAdminSkin", portalSettings.PortalId,
                    Host.Host.DefaultPortalSkin, portalSettings.CultureCode);
            }
            else if (String.IsNullOrEmpty(activeTab.SkinSrc))
            {
                //DNN-6170 ensure skin value is culture specific
                activeTab.SkinSrc = String.IsNullOrEmpty(PortalController.GetPortalSetting("DefaultPortalSkin", portalSettings.PortalId,
                    Host.Host.DefaultPortalSkin, portalSettings.CultureCode)) ? portalSettings.DefaultPortalSkin : PortalController.GetPortalSetting("DefaultPortalSkin", portalSettings.PortalId,
                    Host.Host.DefaultPortalSkin, portalSettings.CultureCode);
            }
            activeTab.SkinSrc = SkinController.FormatSkinSrc(activeTab.SkinSrc, portalSettings);
            activeTab.SkinPath = SkinController.FormatSkinPath(activeTab.SkinSrc);

            if (Globals.IsAdminSkin())
            {
                activeTab.ContainerSrc = portalSettings.DefaultAdminContainer;
            }
            else if (String.IsNullOrEmpty(activeTab.ContainerSrc))
            {
                activeTab.ContainerSrc = portalSettings.DefaultPortalContainer;
            }

            activeTab.ContainerSrc = SkinController.FormatSkinSrc(activeTab.ContainerSrc, portalSettings);
            activeTab.ContainerPath = SkinController.FormatSkinPath(activeTab.ContainerSrc);            
        }
    }
}
