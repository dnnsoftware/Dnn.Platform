// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Dnn.PersonaBar.Library.DTO.Tabs;
    using Dnn.PersonaBar.Library.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class TabsController
    {
        public string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/Tabs.resx");

        private string IconHome => Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Home.png");

        private string IconPortal => Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Portal.png");

        private string AdminOnlyIcon => Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserAdmin.png");

        private string RegisteredUsersIcon => Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_User.png");

        private string SecuredIcon => Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_UserSecure.png");

        private string AllUsersIcon => Globals.ResolveUrl("~/DesktopModules/Admin/Tabs/images/Icon_Everyone.png");

        private PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        public TabDto GetPortalTabs(UserInfo userInfo, int portalId, string cultureCode, bool isMultiLanguage, bool excludeAdminTabs = true,
            string roles = "", bool disabledNotSelectable = false, int sortOrder = 0,
            int selectedTabId = -1, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeleted = false, bool includeDeletedChildren = true)
        {
            var portalInfo = PortalController.Instance.GetPortal(portalId);

            var rootNode = new TabDto
            {
                Name = portalInfo.PortalName,
                ImageUrl = this.IconPortal,
                TabId = Null.NullInteger.ToString(CultureInfo.InvariantCulture),
                ChildTabs = new List<TabDto>(),
                HasChildren = true,
            };
            var tabs = new List<TabInfo>();

            cultureCode = string.IsNullOrEmpty(cultureCode) ? portalInfo.CultureCode : cultureCode;
            if (portalId > -1)
            {
                tabs =
                    TabController.GetPortalTabs(
                        isMultiLanguage
                            ? TabController.GetTabsBySortOrder(portalId, portalInfo.DefaultLanguage, true)
                            : TabController.GetTabsBySortOrder(portalId, cultureCode, true), Null.NullInteger, false,
                        "<" + Localization.GetString("None_Specified") + ">", true, includeDeleted, true, false, false, includeDeletedChildren)
                        .Where(t => (!t.DisableLink || includeDisabled) && !t.IsSystem)
                        .ToList();

                if (userInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(
                        TabController.Instance.GetTabsByPortal(-1)
                            .AsList()
                            .Where(t => !t.IsDeleted && !t.DisableLink && !t.IsSystem)
                            .ToList());
                }
            }
            else
            {
                if (userInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).AsList().Where(t => !t.IsDeleted && !t.DisableLink && !t.IsSystem).ToList();
                }
            }

            tabs = excludeAdminTabs
                ? tabs.Where(tab => tab.Level == 0 && tab.TabID != portalInfo.AdminTabId).ToList()
                : tabs.Where(tab => tab.Level == 0).ToList();

            if (!string.IsNullOrEmpty(validateTab))
            {
                tabs = this.ValidateModuleInTab(tabs, validateTab).ToList();
            }

            var filterTabs = this.FilterTabsByRole(tabs, roles, disabledNotSelectable);
            rootNode.HasChildren = tabs.Count > 0;
            rootNode.Selectable = SecurityService.Instance.IsPagesAdminUser();
            foreach (var tab in tabs)
            {
                string tooltip;
                var nodeIcon = this.GetNodeIcon(tab, out tooltip);
                var node = new TabDto
                {
                    Name = tab.LocalizedTabName, // $"{tab.TabName} {GetNodeStatusIcon(tab)}",
                    TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                    ImageUrl = nodeIcon,
                    Tooltip = tooltip,
                    ParentTabId = tab.ParentId,
                    HasChildren = tab.HasChildren,
                    Selectable = filterTabs.Contains(tab.TabID) && TabPermissionController.CanAddPage(tab),
                    ChildTabs = new List<TabDto>(),
                };
                rootNode.ChildTabs.Add(node);
            }

            rootNode.ChildTabs = ApplySort(rootNode.ChildTabs, sortOrder).ToList();

            return selectedTabId > -1
                ? this.MarkSelectedTab(rootNode, selectedTabId, portalInfo, cultureCode, isMultiLanguage, validateTab)
                : rootNode;
        }

        public TabDto SearchPortalTabs(UserInfo userInfo, string searchText, int portalId, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeleted = false)
        {
            var rootNode = new TabDto
            {
                Name = this.PortalSettings.PortalName,
                ImageUrl = this.IconPortal,
                TabId = Null.NullInteger.ToString(CultureInfo.InvariantCulture),
                ChildTabs = new List<TabDto>(),
                HasChildren = true,
            };
            Func<TabInfo, bool> searchFunc;
            if (string.IsNullOrEmpty(searchText))
            {
                searchFunc = page => true;
            }
            else
            {
                searchFunc =
                    page => page.LocalizedTabName.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) > -1;
            }

            var tabs = new List<TabInfo>();
            if (portalId > -1)
            {
                tabs =
                    TabController.Instance.GetTabsByPortal(portalId)
                        .Where(
                            tab =>
                                (includeDisabled || !tab.Value.DisableLink) &&
                                (includeDeleted || !tab.Value.IsDeleted) &&
                                (tab.Value.TabType == TabType.Normal) &&
                                searchFunc(tab.Value) &&
                                !tab.Value.IsSystem)
                        .Select(tab => tab.Value).ToList();

                if (userInfo.IsSuperUser && includeHostPages)
                {
                    tabs.AddRange(TabController.Instance.GetTabsByPortal(-1).Where(tab => !tab.Value.DisableLink && searchFunc(tab.Value) && !tab.Value.IsSystem)
                    .OrderBy(tab => tab.Value.TabOrder)
                    .Select(tab => tab.Value)
                    .ToList());
                }
            }
            else
            {
                if (userInfo.IsSuperUser)
                {
                    tabs = TabController.Instance.GetTabsByPortal(-1).Where(tab => !tab.Value.DisableLink && searchFunc(tab.Value) && !tab.Value.IsSystem)
                    .OrderBy(tab => tab.Value.TabOrder)
                    .Select(tab => tab.Value)
                    .ToList();
                }
            }

            var filterTabs = this.FilterTabsByRole(tabs, roles, disabledNotSelectable);
            rootNode.HasChildren = tabs.Any();
            rootNode.Selectable = SecurityService.Instance.IsPagesAdminUser();
            foreach (var tab in tabs)
            {
                string tooltip;
                var nodeIcon = this.GetNodeIcon(tab, out tooltip);
                var node = new TabDto
                {
                    Name = tab.LocalizedTabName,
                    TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                    ImageUrl = nodeIcon,
                    ParentTabId = tab.ParentId,
                    HasChildren = false,
                    Selectable = filterTabs.Contains(tab.TabID) && TabPermissionController.CanAddPage(tab),
                };
                rootNode.ChildTabs.Add(node);
            }

            rootNode.ChildTabs = ApplySort(rootNode.ChildTabs, sortOrder).ToList();
            if (!string.IsNullOrEmpty(validateTab))
            {
                rootNode.ChildTabs = this.ValidateModuleInTab(rootNode.ChildTabs, validateTab).ToList();
            }

            return rootNode;
        }

        public TabDto GetTabByCulture(int tabId, int portalId, string cultureCode)
        {
            cultureCode = string.IsNullOrEmpty(cultureCode) ? PortalController.Instance.GetPortal(portalId).CultureCode : cultureCode;

            var locale = LocaleController.Instance.GetLocale(cultureCode);
            return this.GetTabByCulture(tabId, portalId, locale);
        }

        public IEnumerable<TabDto> GetTabsDescendants(int portalId, int parentId, string cultureCode, bool isMultiLanguage, string roles = "", bool disabledNotSelectable = false, int sortOrder = 0, string validateTab = "", bool includeHostPages = false, bool includeDisabled = false, bool includeDeletedChildren = true)
        {
            var descendants = new List<TabDto>();
            cultureCode = string.IsNullOrEmpty(cultureCode) ? PortalController.Instance.GetPortal(portalId).CultureCode : cultureCode;

            var tabs =
                this.GetExportableTabs(TabController.Instance.GetTabsByPortal(portalId)
                    .WithCulture(cultureCode, true))
                    .WithParentId(parentId).ToList();

            if (!string.IsNullOrEmpty(validateTab))
            {
                tabs = this.ValidateModuleInTab(tabs, validateTab).ToList();
            }

            var filterTabs = this.FilterTabsByRole(tabs, roles, disabledNotSelectable);
            foreach (var tab in tabs.Where(
                x => x.ParentId == parentId &&
                (
                    includeDeletedChildren ||
                    !x.IsDeleted)))
            {
                string tooltip;
                var nodeIcon = this.GetNodeIcon(tab, out tooltip);
                var node = new TabDto
                {
                    Name = tab.TabName, // $"{tab.TabName} {GetNodeStatusIcon(tab)}",
                    TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                    ImageUrl = nodeIcon,
                    Tooltip = tooltip,
                    ParentTabId = tab.ParentId,
                    HasChildren = tab.HasChildren,
                    Selectable = filterTabs.Contains(tab.TabID) && TabPermissionController.CanAddPage(tab),
                };
                descendants.Add(node);
            }

            return ApplySort(descendants, sortOrder);
        }

        private static bool IsSecuredTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return
                perms.Cast<TabPermissionInfo>()
                    .All(perm => perm.RoleName != Globals.glbRoleAllUsersName || !perm.AllowAccess);
        }

        private static IEnumerable<TabDto> ApplySort(IEnumerable<TabDto> items, int sortOrder)
        {
            switch (sortOrder)
            {
                case 1: // sort by a-z
                    return items.OrderBy(item => item.Name).ToList();
                case 2: // sort by z-a
                    return items.OrderByDescending(item => item.Name).ToList();
                default: // no sort
                    return items;
            }
        }

        private IEnumerable<TabDto> ValidateModuleInTab(IEnumerable<TabDto> tabs, string validateTab)
        {
            return tabs.Where(
                tab =>
                    (Convert.ToInt32(tab.TabId) > 0 &&
                     Globals.ValidateModuleInTab(Convert.ToInt32(tab.TabId), validateTab)) ||
                    Convert.ToInt32(tab.TabId) == Null.NullInteger);
        }

        private IEnumerable<TabInfo> ValidateModuleInTab(IEnumerable<TabInfo> tabs, string validateTab)
        {
            return tabs.Where(tab => (tab.TabID > 0 && Globals.ValidateModuleInTab(tab.TabID, validateTab)) || tab.TabID == Null.NullInteger);
        }

        private List<int> FilterTabsByRole(IList<TabInfo> tabs, string roles, bool disabledNotSelectable)
        {
            var filterTabs = new List<int>();
            if (!string.IsNullOrEmpty(roles))
            {
                var roleList = roles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

                filterTabs.AddRange(
                    tabs.Where(
                        t =>
                            t.TabPermissions.Cast<TabPermissionInfo>()
                                .Any(
                                    p =>
                                        roleList.Contains(p.RoleID) && p.UserID == Null.NullInteger &&
                                        p.PermissionKey == "VIEW" && p.AllowAccess)).ToList()
                        .Where(t => !disabledNotSelectable || !t.DisableLink)
                        .Select(t => t.TabID));
            }
            else
            {
                filterTabs.AddRange(tabs.Where(t => !disabledNotSelectable || !t.DisableLink).Select(t => t.TabID));
            }

            return filterTabs;
        }

        private TabDto MarkSelectedTab(TabDto rootNode, int selectedTabId, PortalInfo portalInfo, string cultureCode,
            bool isMultiLanguage, string validateTab)
        {
            var tempTabs = new List<int>();
            cultureCode = string.IsNullOrEmpty(cultureCode) ? portalInfo.CultureCode : cultureCode;
            var locale = LocaleController.Instance.GetLocale(cultureCode);
            var selectedTab = this.GetTabByCulture(selectedTabId, portalInfo.PortalID, locale);
            if (selectedTab != null)
            {
                tempTabs.Add(Convert.ToInt32(selectedTab.TabId));
                if (selectedTab.ParentTabId > Null.NullInteger)
                {
                    var parentTab = selectedTab;
                    do
                    {
                        parentTab = this.GetTabByCulture(parentTab.ParentTabId, portalInfo.PortalID, locale);
                        if (parentTab != null)
                        {
                            tempTabs.Add(Convert.ToInt32(parentTab.TabId));
                        }
                    }
                    while (parentTab != null && parentTab.ParentTabId > Null.NullInteger);
                }
            }

            tempTabs.Reverse();
            rootNode.ChildTabs = this.GetDescendantsForTabs(tempTabs, rootNode.ChildTabs, selectedTabId, portalInfo.PortalID,
                cultureCode, isMultiLanguage).ToList();
            if (!string.IsNullOrEmpty(validateTab))
            {
                rootNode.ChildTabs = this.ValidateModuleInTab(rootNode.ChildTabs, validateTab).ToList();
            }

            return rootNode;
        }

        private IEnumerable<TabDto> GetDescendantsForTabs(IEnumerable<int> tabIds, IEnumerable<TabDto> tabs,
            int selectedTabId,
            int portalId, string cultureCode, bool isMultiLanguage)
        {
            var enumerable = tabIds as int[] ?? tabIds.ToArray();
            if (tabs == null || tabIds == null || !enumerable.Any())
            {
                return tabs;
            }

            var tabDtos = tabs as List<TabDto> ?? tabs.ToList();
            var tabId = enumerable.First();
            if (selectedTabId != tabId)
            {
                if (!tabDtos.Exists(x => Convert.ToInt32(x.TabId) == tabId))
                {
                    return this.GetDescendantsForTabs(enumerable.Except(new List<int> { tabId }), tabDtos, selectedTabId,
                        portalId, cultureCode, isMultiLanguage);
                }

                tabDtos.First(x => Convert.ToInt32(x.TabId) == tabId).ChildTabs =
                    this.GetTabsDescendants(portalId, tabId, cultureCode,
                        isMultiLanguage).ToList();
                tabDtos.First(x => Convert.ToInt32(x.TabId) == tabId).IsOpen = true;
                tabDtos.First(x => Convert.ToInt32(x.TabId) == tabId).ChildTabs =
                    this.GetDescendantsForTabs(
                        enumerable.Except(new List<int> { tabId }),
                        tabDtos.First(x => Convert.ToInt32(x.TabId) == tabId).ChildTabs, selectedTabId,
                        portalId, cultureCode, isMultiLanguage).ToList();
            }
            else
            {
                tabDtos.First(x => Convert.ToInt32(x.TabId) == tabId).CheckedState =
                    NodeCheckedState.Checked;
            }

            return tabDtos;
        }

        private TabDto GetTabByCulture(int tabId, int portalId, Locale locale)
        {
            var tab = TabController.Instance.GetTabByCulture(tabId, portalId, locale);
            string tooltip;
            var nodeIcon = this.GetNodeIcon(tab, out tooltip);
            return new TabDto
            {
                Name = tab.TabName, // $"{tab.TabName} {GetNodeStatusIcon(tab)}",
                TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                ImageUrl = nodeIcon,
                Tooltip = tooltip,
                ParentTabId = tab.ParentId,
                HasChildren = tab.HasChildren,
                ChildTabs = new List<TabDto>(),
                Selectable = TabPermissionController.CanAddPage(tab),
            };
        }

        private TabCollection GetExportableTabs(TabCollection tabs)
        {
            var exportableTabs = tabs.Where(kvp => !kvp.Value.IsSystem).Select(kvp => kvp.Value);
            return new TabCollection(exportableTabs);
        }

        private string GetNodeIcon(TabInfo tab, out string toolTip)
        {
            if (this.PortalSettings.HomeTabId == tab.TabID)
            {
                toolTip = Localization.GetString("lblHome", this.LocalResourcesFile);
                return this.IconHome;
            }

            if (IsSecuredTab(tab))
            {
                if (this.IsAdminTab(tab))
                {
                    toolTip = Localization.GetString("lblAdminOnly", this.LocalResourcesFile);
                    return this.AdminOnlyIcon;
                }

                if (this.IsRegisteredUserTab(tab))
                {
                    toolTip = Localization.GetString("lblRegistered", this.LocalResourcesFile);
                    return this.RegisteredUsersIcon;
                }

                toolTip = Localization.GetString("lblSecure", this.LocalResourcesFile);
                return this.SecuredIcon;
            }

            toolTip = Localization.GetString("lblEveryone", this.LocalResourcesFile);
            return this.AllUsersIcon;
        }

        private bool IsAdminTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return
                perms.Cast<TabPermissionInfo>()
                    .All(perm => perm.RoleName == this.PortalSettings.AdministratorRoleName || !perm.AllowAccess);
        }

        private bool IsRegisteredUserTab(TabInfo tab)
        {
            var perms = tab.TabPermissions;
            return
                perms.Cast<TabPermissionInfo>()
                    .Any(perm => perm.RoleName == this.PortalSettings.RegisteredRoleName && perm.AllowAccess);
        }
    }
}
