// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public enum DotNetNukeErrorCode
    {
        /// <summary>Not set.</summary>
        NotSet = 0,

        /// <summary>The page already exists.</summary>
        PageExists = 1,

        /// <summary>THe page name is missing.</summary>
        PageNameRequired = 2,

        /// <summary>The page name is invalid.</summary>
        PageNameInvalid = 3,

        /// <summary>Failed deserializing the page's modules.</summary>
        DeserializePanesFailed = 4,

        /// <summary>The page's parent would also be a descendant of the page.</summary>
        PageCircularReference = 5,

        /// <summary>The parent page is invalid.</summary>
        ParentTabInvalid = 6,

        /// <summary>The user does not have permission to edit the page.</summary>
        PageEditorPermissionError = 7,

        /// <summary>Cannot move a tab before or after the host tab.</summary>
        HostBeforeAfterError = 8,

        /// <summary>The page's URL would conflict with a portal alias.</summary>
        DuplicateWithAlias = 9,
    }

    public enum TabRelativeLocation
    {
        /// <summary>Location not set.</summary>
        NOTSET = 0,

        /// <summary>This tab is before the other tab.</summary>
        BEFORE = 1,

        /// <summary>THis tab is after the other tab.</summary>
        AFTER = 2,

        /// <summary>This tab is a child of the other tab.</summary>
        CHILD = 3,
    }

    /// <summary>Manages the old ribbon bar.</summary>
    public partial class RibbonBarManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RibbonBarManager));

        public static TabInfo InitTabInfoObject()
        {
            return InitTabInfoObject(null, TabRelativeLocation.AFTER);
        }

        public static TabInfo InitTabInfoObject(TabInfo relativeToTab)
        {
            return InitTabInfoObject(relativeToTab, TabRelativeLocation.AFTER);
        }

        public static TabInfo InitTabInfoObject(TabInfo relativeToTab, TabRelativeLocation location)
        {
            if (relativeToTab == null)
            {
                if ((PortalSettings.Current != null) && (PortalSettings.Current.ActiveTab != null))
                {
                    relativeToTab = PortalSettings.Current.ActiveTab;
                }
            }

            var newTab = new TabInfo
            {
                TabID = Null.NullInteger,
                TabName = string.Empty,
                Title = string.Empty,
                IsVisible = false,
                DisableLink = false,
                IsDeleted = false,
                IsSecure = false,
                PermanentRedirect = false,
            };

            TabInfo parentTab = GetParentTab(relativeToTab, location);

            if (parentTab != null)
            {
                newTab.PortalID = parentTab.PortalID;
                newTab.ParentId = parentTab.TabID;
                newTab.Level = parentTab.Level + 1;
                switch (PortalSettings.Current.SSLSetup)
                {
                    case Abstractions.Security.SiteSslSetup.Off:
                        newTab.IsSecure = false;
                        break;
                    case Abstractions.Security.SiteSslSetup.Advanced:
                        newTab.IsSecure = parentTab.IsSecure;
                        break;
                    default:
                        newTab.IsSecure = true;
                        break;
                }
            }
            else
            {
                newTab.PortalID = PortalSettings.Current.PortalId;
                newTab.ParentId = Null.NullInteger;
                newTab.Level = 0;
            }

            // Inherit permissions from parent
            newTab.TabPermissions.Clear();
            if (newTab.PortalID != Null.NullInteger && (parentTab != null))
            {
                newTab.TabPermissions.AddRange(parentTab.TabPermissions);
            }
            else if (newTab.PortalID != Null.NullInteger)
            {
                // Give admin full permission
                ArrayList permissions = PermissionController.GetPermissionsByTab();

                foreach (PermissionInfo permission in permissions)
                {
                    TabPermissionInfo newTabPermission = new TabPermissionInfo();
                    newTabPermission.PermissionID = permission.PermissionID;
                    newTabPermission.PermissionKey = permission.PermissionKey;
                    newTabPermission.PermissionName = permission.PermissionName;
                    newTabPermission.AllowAccess = true;
                    newTabPermission.RoleID = PortalSettings.Current.AdministratorRoleId;
                    newTab.TabPermissions.Add(newTabPermission);
                }
            }

            return newTab;
        }

        public static TabInfo GetParentTab(TabInfo relativeToTab, TabRelativeLocation location)
        {
            if (relativeToTab == null)
            {
                return null;
            }

            TabInfo parentTab = null;
            if (location == TabRelativeLocation.CHILD)
            {
                parentTab = relativeToTab;
            }
            else if ((relativeToTab != null) && relativeToTab.ParentId != Null.NullInteger)
            {
                parentTab = TabController.Instance.GetTab(relativeToTab.ParentId, relativeToTab.PortalID, false);
            }

            return parentTab;
        }

        public static IList<TabInfo> GetPagesList()
        {
            IList<TabInfo> portalTabs = null;
            UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
            if ((userInfo != null) && userInfo.UserID != Null.NullInteger)
            {
                if (userInfo.IsSuperUser && PortalSettings.Current.ActiveTab.IsSuperTab)
                {
                    portalTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger).AsList();
                }
                else
                {
                    portalTabs = TabController.GetPortalTabs(PortalSettings.Current.PortalId, Null.NullInteger, false, Null.NullString, true, false, true, false, true);
                }
            }

            if (portalTabs == null)
            {
                portalTabs = new List<TabInfo>();
            }

            return portalTabs;
        }

        public static bool IsHostConsolePage()
        {
            return PortalSettings.Current.ActiveTab.IsSuperTab && PortalSettings.Current.ActiveTab.TabPath == "//Host";
        }

        public static bool IsHostConsolePage(TabInfo tab)
        {
            return tab.IsSuperTab && tab.TabPath == "//Host";
        }

        public static bool CanMovePage()
        {
            // Cannot move the host console page
            if (IsHostConsolePage())
            {
                return false;
            }

            // Page Editors - Can only move children they have 'Manage' permission to, they cannot move the top level page
            if (!PortalSecurity.IsInRole("Administrators"))
            {
                int parentTabID = PortalSettings.Current.ActiveTab.ParentId;
                if (parentTabID == Null.NullInteger)
                {
                    return false;
                }

                TabInfo parentTab = TabController.Instance.GetTab(parentTabID, PortalSettings.Current.ActiveTab.PortalID, false);
                string permissionList = "MANAGE";
                if (!TabPermissionController.HasTabPermission(parentTab.TabPermissions, permissionList))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc cref="SaveTabInfoObject(DotNetNuke.Abstractions.Modules.IBusinessControllerProvider,DotNetNuke.Entities.Tabs.TabInfo,DotNetNuke.Entities.Tabs.TabInfo,DotNetNuke.Web.UI.TabRelativeLocation,string)"/>
        [DnnDeprecated(10, 0, 0, "Please use overload with IServiceProvider")]
        public static partial int SaveTabInfoObject(
            TabInfo tab,
            TabInfo relativeToTab,
            TabRelativeLocation location,
            string templateFileId)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            return SaveTabInfoObject(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), tab, relativeToTab, location, templateFileId);
        }

        public static int SaveTabInfoObject(IBusinessControllerProvider businessControllerProvider, TabInfo tab, TabInfo relativeToTab, TabRelativeLocation location, string templateFileId)
        {
            // Validation:
            // Tab name is required
            // Tab name is invalid
            string invalidType;
            if (!TabController.IsValidTabName(tab.TabName, out invalidType))
            {
                switch (invalidType)
                {
                    case "EmptyTabName":
                        throw new DotNetNukeException("Page name is required.", DotNetNukeErrorCode.PageNameRequired);
                    case "InvalidTabName":
                        throw new DotNetNukeException("Page name is invalid.", DotNetNukeErrorCode.PageNameInvalid);
                }
            }
            else if (Validate_IsCircularReference(tab.PortalID, tab.TabID))
            {
                throw new DotNetNukeException("Cannot move page to that location.", DotNetNukeErrorCode.PageCircularReference);
            }

            bool usingDefaultLanguage = (tab.CultureCode == PortalSettings.Current.DefaultLanguage) || tab.CultureCode == null;

            if (PortalSettings.Current.ContentLocalizationEnabled)
            {
                if (!usingDefaultLanguage)
                {
                    TabInfo defaultLanguageSelectedTab = tab.DefaultLanguageTab;

                    if (defaultLanguageSelectedTab == null)
                    {
                        // get the siblings from the selectedtab and iterate through until you find a sibbling with a corresponding defaultlanguagetab
                        // if none are found get a list of all the tabs from the default language and then select the last one
                        var selectedTabSibblings = TabController.Instance.GetTabsByPortal(tab.PortalID).WithCulture(tab.CultureCode, true).AsList();
                        foreach (TabInfo sibling in selectedTabSibblings)
                        {
                            TabInfo siblingDefaultTab = sibling.DefaultLanguageTab;
                            if (siblingDefaultTab != null)
                            {
                                defaultLanguageSelectedTab = siblingDefaultTab;
                                break;
                            }
                        }

                        // still haven't found it
                        if (defaultLanguageSelectedTab == null)
                        {
                            var defaultLanguageTabs = TabController.Instance.GetTabsByPortal(tab.PortalID).WithCulture(PortalSettings.Current.DefaultLanguage, true).AsList();
                            defaultLanguageSelectedTab = defaultLanguageTabs[defaultLanguageTabs.Count];

                            // get the last tab
                        }
                    }

                    relativeToTab = defaultLanguageSelectedTab;
                }
            }

            if (location != TabRelativeLocation.NOTSET)
            {
                // Check Host tab - don't allow adding before or after
                if (IsHostConsolePage(relativeToTab) && (location == TabRelativeLocation.AFTER || location == TabRelativeLocation.BEFORE))
                {
                    throw new DotNetNukeException("You cannot add or move pages before or after the Host tab.", DotNetNukeErrorCode.HostBeforeAfterError);
                }

                TabInfo parentTab = GetParentTab(relativeToTab, location);
                string permissionList = "ADD,COPY,EDIT,MANAGE";

                // Check permissions for Page Editors when moving or inserting
                if (!PortalSecurity.IsInRole("Administrators"))
                {
                    if ((parentTab == null) || !TabPermissionController.HasTabPermission(parentTab.TabPermissions, permissionList))
                    {
                        throw new DotNetNukeException(
                            "You do not have permissions to add or move pages to this location. You can only add or move pages as children of pages you can edit.",
                            DotNetNukeErrorCode.PageEditorPermissionError);
                    }
                }

                if (parentTab != null)
                {
                    tab.ParentId = parentTab.TabID;
                    tab.Level = parentTab.Level + 1;
                }
                else
                {
                    tab.ParentId = Null.NullInteger;
                    tab.Level = 0;
                }
            }

            if (tab.TabID > Null.NullInteger && tab.TabID == tab.ParentId)
            {
                throw new DotNetNukeException("Parent page is invalid.", DotNetNukeErrorCode.ParentTabInvalid);
            }

            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);

            // check whether have conflict between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(PortalSettings.Current.PortalId, tab.TabPath))
            {
                throw new DotNetNukeException("The page path is duplicate with a site alias", DotNetNukeErrorCode.DuplicateWithAlias);
            }

            try
            {
                if (tab.TabID < 0)
                {
                    if (tab.TabPermissions.Count == 0 && tab.PortalID != Null.NullInteger)
                    {
                        // Give admin full permission
                        ArrayList permissions = PermissionController.GetPermissionsByTab();

                        foreach (PermissionInfo permission in permissions)
                        {
                            TabPermissionInfo newTabPermission = new TabPermissionInfo();
                            newTabPermission.PermissionID = permission.PermissionID;
                            newTabPermission.PermissionKey = permission.PermissionKey;
                            newTabPermission.PermissionName = permission.PermissionName;
                            newTabPermission.AllowAccess = true;
                            newTabPermission.RoleID = PortalSettings.Current.AdministratorRoleId;
                            tab.TabPermissions.Add(newTabPermission);
                        }
                    }

                    PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                    if (portalSettings.ContentLocalizationEnabled)
                    {
                        Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(tab.PortalID);
                        tab.CultureCode = defaultLocale.Code;
                    }
                    else
                    {
                        tab.CultureCode = Null.NullString;
                    }

                    if (location == TabRelativeLocation.AFTER && (relativeToTab != null))
                    {
                        tab.TabID = TabController.Instance.AddTabAfter(tab, relativeToTab.TabID);
                    }
                    else if (location == TabRelativeLocation.BEFORE && (relativeToTab != null))
                    {
                        tab.TabID = TabController.Instance.AddTabBefore(tab, relativeToTab.TabID);
                    }
                    else
                    {
                        tab.TabID = TabController.Instance.AddTab(tab);
                    }

                    if (portalSettings.ContentLocalizationEnabled)
                    {
                        TabController.Instance.CreateLocalizedCopies(tab);
                    }

                    TabController.Instance.UpdateTabSetting(tab.TabID, "CacheProvider", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "CacheDuration", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "CacheIncludeExclude", "0");
                    TabController.Instance.UpdateTabSetting(tab.TabID, "IncludeVaryBy", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "ExcludeVaryBy", string.Empty);
                    TabController.Instance.UpdateTabSetting(tab.TabID, "MaxVaryByCount", string.Empty);
                }
                else
                {
                    TabController.Instance.UpdateTab(tab);

                    if (location == TabRelativeLocation.AFTER && (relativeToTab != null))
                    {
                        TabController.Instance.MoveTabAfter(tab, relativeToTab.TabID);
                    }
                    else if (location == TabRelativeLocation.BEFORE && (relativeToTab != null))
                    {
                        TabController.Instance.MoveTabBefore(tab, relativeToTab.TabID);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                if (ex.Message.StartsWith("Page Exists", StringComparison.OrdinalIgnoreCase))
                {
                    throw new DotNetNukeException(ex.Message, DotNetNukeErrorCode.PageExists);
                }
            }

            // create the page from a template
            if (!string.IsNullOrEmpty(templateFileId))
            {
                XmlDocument xmlDoc = new XmlDocument { XmlResolver = null, };
                try
                {
                    var templateFile = FileManager.Instance.GetFile(Convert.ToInt32(templateFileId, CultureInfo.InvariantCulture));
                    xmlDoc.Load(FileManager.Instance.GetFileContent(templateFile));
                    TabController.DeserializePanes(businessControllerProvider, xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());

                    // save tab permissions
                    DeserializeTabPermissions(xmlDoc.SelectNodes("//portal/tabs/tab/tabpermissions/permission"), tab);
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                    throw new DotNetNukeException("Unable to process page template.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
                }
            }

            return tab.TabID;
        }

        public static bool Validate_IsCircularReference(int portalID, int tabID)
        {
            if (tabID != -1)
            {
                TabInfo objtab = TabController.Instance.GetTab(tabID, portalID, false);

                if (objtab == null)
                {
                    return false;
                }

                if (objtab.Level == 0)
                {
                    return false;
                }

                if (tabID == objtab.ParentId)
                {
                    return true;
                }

                return Validate_IsCircularReference(portalID, objtab.ParentId);
            }

            return false;
        }

        public static void DeserializeTabPermissions(XmlNodeList nodeTabPermissions, TabInfo tab)
        {
            var permissionController = new PermissionController();
            foreach (XmlNode xmlTabPermission in nodeTabPermissions)
            {
                var permissionKey = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "permissionkey");
                var permissionCode = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "permissioncode");
                var roleName = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "rolename");
                var allowAccess = XmlUtils.GetNodeValueBoolean(xmlTabPermission, "allowaccess");
                var permissions = permissionController.GetPermissionByCodeAndKey(permissionCode, permissionKey);
                var permissionId = permissions.Cast<PermissionInfo>().Last().PermissionID;

                var roleId = int.MinValue;
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleId = Convert.ToInt32(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleId = Convert.ToInt32(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);
                        break;
                    default:
                        var portal = PortalController.Instance.GetPortal(tab.PortalID);
                        var role = RoleController.Instance.GetRole(portal.PortalID, r => r.RoleName == roleName);
                        if (role != null)
                        {
                            roleId = role.RoleID;
                        }

                        break;
                }

                if (roleId != int.MinValue &&
                        !tab.TabPermissions.Cast<TabPermissionInfo>().Any(p =>
                                                                            p.RoleID == roleId
                                                                            && p.PermissionID == permissionId))
                {
                    var tabPermission = new TabPermissionInfo
                    {
                        TabID = tab.TabID,
                        PermissionID = permissionId,
                        RoleID = roleId,
                        AllowAccess = allowAccess,
                    };

                    tab.TabPermissions.Add(tabPermission);
                }
            }

            TabController.Instance.UpdateTab(tab);
        }
    }
}
