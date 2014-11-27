#region Copyright

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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Tabs;
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
            if (PortalController.GetPortalSettingsDictionary(portalId).TryGetValue("PortalAliasMapping", out setting))
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

        protected virtual void UpdateSkinSettings(TabInfo activeTab, PortalSettings portalSettings)
        {
            if (Globals.IsAdminSkin())
            {
                activeTab.SkinSrc = portalSettings.DefaultAdminSkin;
            }
            else if (String.IsNullOrEmpty(activeTab.SkinSrc))
            {
                activeTab.SkinSrc = portalSettings.DefaultPortalSkin;
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
