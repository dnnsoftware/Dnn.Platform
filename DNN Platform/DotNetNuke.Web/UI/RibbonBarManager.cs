// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
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

    /// <summary>An error code for a <see cref="DotNetNukeException"/>.</summary>
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

    /// <summary>The relative location of a tab.</summary>
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

        /// <summary>Initializes tab info user to add a new tab/page.</summary>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial TabInfo InitTabInfoObject()
            => InitTabInfoObject(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>());

        /// <summary>Initializes tab info user to add a new tab/page.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        public static TabInfo InitTabInfoObject(IPermissionDefinitionService permissionDefinitionService)
            => InitTabInfoObject(permissionDefinitionService, null, TabRelativeLocation.AFTER);

        /// <summary>Initializes tab info user to add a new tab/page after the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial TabInfo InitTabInfoObject(TabInfo relativeToTab)
            => InitTabInfoObject(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), relativeToTab);

        /// <summary>Initializes tab info user to add a new tab/page after the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        public static TabInfo InitTabInfoObject(IPermissionDefinitionService permissionDefinitionService, TabInfo relativeToTab)
            => InitTabInfoObject(permissionDefinitionService, relativeToTab, TabRelativeLocation.AFTER);

        /// <summary>Initializes tab info user to add a new tab/page, relative to the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial TabInfo InitTabInfoObject(TabInfo relativeToTab, TabRelativeLocation location)
            => InitTabInfoObject(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), relativeToTab, location);

        /// <summary>Initializes tab info user to add a new tab/page, relative to the given <paramref name="relativeToTab"/>.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <returns>The new <see cref="TabInfo"/> instance.</returns>
        public static TabInfo InitTabInfoObject(IPermissionDefinitionService permissionDefinitionService, TabInfo relativeToTab, TabRelativeLocation location)
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
                foreach (var permission in permissionDefinitionService.GetDefinitionsByTab())
                {
                    IPermissionInfo newTabPermission = new TabPermissionInfo();
                    newTabPermission.PermissionId = permission.PermissionId;
                    newTabPermission.PermissionKey = permission.PermissionKey;
                    newTabPermission.PermissionName = permission.PermissionName;
                    newTabPermission.AllowAccess = true;
                    newTabPermission.RoleId = PortalSettings.Current.AdministratorRoleId;
                    newTab.TabPermissions.Add((TabPermissionInfo)newTabPermission);
                }
            }

            return newTab;
        }

        /// <summary>Gets the parent of the new page/tab.</summary>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <returns>The <see cref="TabInfo"/> or <see langword="null"/> if there is no parent.</returns>
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

        /// <summary>Gets the list of pages the current user can edit.</summary>
        /// <returns>A list of <see cref="TabInfo"/> instances.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IList<TabInfo> GetPagesList()
            => GetPagesList(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>());

        /// <summary>Gets the list of pages the current user can edit.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <returns>A list of <see cref="TabInfo"/> instances.</returns>
        public static IList<TabInfo> GetPagesList(IHostSettings hostSettings, IApplicationStatusInfo appStatus)
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
                    portalTabs = TabController.GetPortalTabs(hostSettings, appStatus, PortalSettings.Current.PortalId, Null.NullInteger, false, Null.NullString, true, false, true, false, true);
                }
            }

            if (portalTabs == null)
            {
                portalTabs = new List<TabInfo>();
            }

            return portalTabs;
        }

        /// <summary>Gets a value indicating whether the current page is the host dashboard page.</summary>
        /// <returns><see langword="true"/> if the current page is the host console, otherwise <see langword="false"/>.</returns>
        public static bool IsHostConsolePage()
        {
            return PortalSettings.Current.ActiveTab.IsSuperTab && PortalSettings.Current.ActiveTab.TabPath == "//Host";
        }

        /// <summary>Gets a value indicating whether the given <paramref name="tab"/> is the host dashboard page.</summary>
        /// <param name="tab">The page.</param>
        /// <returns><see langword="true"/> if the given <paramref name="tab"/> is the host console, otherwise <see langword="false"/>.</returns>
        public static bool IsHostConsolePage(TabInfo tab)
        {
            return tab.IsSuperTab && tab.TabPath == "//Host";
        }

        /// <summary>Gets a value indicating whether the current page can be moved.</summary>
        /// <returns><see langword="true"/> if the current page can be moved, otherwise <see langword="false"/>.</returns>
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

        /// <summary>Creates a new page/tab.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="tab">The page to create.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <param name="templateFileId">The file ID of the page template (or <see langword="null"/> or <see cref="string.Empty"/> to not use a page template).</param>
        /// <returns>The new tab's ID.</returns>
        /// <exception cref="DotNetNukeException">If the page is invalid.</exception>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial int SaveTabInfoObject(IBusinessControllerProvider businessControllerProvider, TabInfo tab, TabInfo relativeToTab, TabRelativeLocation location, string templateFileId)
            => SaveTabInfoObject(businessControllerProvider, Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), tab, relativeToTab, location, templateFileId);

        /// <summary>Creates a new page/tab.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="tab">The page to create.</param>
        /// <param name="relativeToTab">The page/tab to which the new page/tab should be relative.</param>
        /// <param name="location">The new page/tab's location relative to <paramref name="relativeToTab"/>.</param>
        /// <param name="templateFileId">The file ID of the page template (or <see langword="null"/> or <see cref="string.Empty"/> to not use a page template).</param>
        /// <returns>The new tab's ID.</returns>
        /// <exception cref="DotNetNukeException">If the page is invalid.</exception>
        public static int SaveTabInfoObject(IBusinessControllerProvider businessControllerProvider, IPermissionDefinitionService permissionDefinitionService, TabInfo tab, TabInfo relativeToTab, TabRelativeLocation location, string templateFileId)
        {
            // Validation:
            // Tab name is required
            // Tab name is invalid
            if (!TabController.IsValidTabName(tab.TabName, out var invalidType))
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
                        // get the siblings from the selected tab and iterate through until you find a sibling with a corresponding default language tab
                        // if none are found get a list of all the tabs from the default language and then select the last one
                        var selectedTabSiblings = TabController.Instance.GetTabsByPortal(tab.PortalID).WithCulture(tab.CultureCode, true).AsList();
                        foreach (TabInfo sibling in selectedTabSiblings)
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
                if (IsHostConsolePage(relativeToTab) && location is TabRelativeLocation.AFTER or TabRelativeLocation.BEFORE)
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
                        foreach (IPermissionDefinitionInfo permission in permissionDefinitionService.GetDefinitionsByTab())
                        {
                            IPermissionInfo newTabPermission = new TabPermissionInfo();
                            newTabPermission.PermissionId = permission.PermissionId;
                            newTabPermission.PermissionKey = permission.PermissionKey;
                            newTabPermission.PermissionName = permission.PermissionName;
                            newTabPermission.AllowAccess = true;
                            newTabPermission.RoleId = PortalSettings.Current.AdministratorRoleId;
                            tab.TabPermissions.Add((TabPermissionInfo)newTabPermission);
                        }
                    }

                    var portalSettings = PortalController.Instance.GetCurrentSettings();
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
                    using (var fileContent = FileManager.Instance.GetFileContent(templateFile))
                    using (var xmlReader = XmlReader.Create(fileContent, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        xmlDoc.Load(xmlReader);
                    }

                    TabController.DeserializePanes(businessControllerProvider, permissionDefinitionService, xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());

                    // save tab permissions
                    DeserializeTabPermissions(permissionDefinitionService, xmlDoc.SelectNodes("//portal/tabs/tab/tabpermissions/permission"), tab);
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                    throw new DotNetNukeException("Unable to process page template.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
                }
            }

            return tab.TabID;
        }

        /// <summary>Validates whether the page's parent contains a circular reference back to this page/tab.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="tabID">The page/tab ID.</param>
        /// <returns><see langword="true"/> if there is a circular reference, otherwise <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
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

        /// <summary>Updates a <paramref name="tab"/> with permission information.</summary>
        /// <param name="nodeTabPermissions">A list of tab permission elements.</param>
        /// <param name="tab">The tab/page to update.</param>
        [DnnDeprecated(10, 2, 2, "Please use overload with IPermissionDefinitionService")]
        public static partial void DeserializeTabPermissions(XmlNodeList nodeTabPermissions, TabInfo tab)
            => DeserializeTabPermissions(Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), nodeTabPermissions, tab);

        /// <summary>Updates a <paramref name="tab"/> with permission information.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="nodeTabPermissions">A list of tab permission elements.</param>
        /// <param name="tab">The tab/page to update.</param>
        public static void DeserializeTabPermissions(IPermissionDefinitionService permissionDefinitionService, XmlNodeList nodeTabPermissions, TabInfo tab)
        {
            foreach (XmlNode xmlTabPermission in nodeTabPermissions)
            {
                var permissionKey = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "permissionkey");
                var permissionCode = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "permissioncode");
                var roleName = XmlUtils.GetNodeValue(xmlTabPermission.CreateNavigator(), "rolename");
                var allowAccess = XmlUtils.GetNodeValueBoolean(xmlTabPermission, "allowaccess");
                var permissions = permissionDefinitionService.GetDefinitionsByCodeAndKey(permissionCode, permissionKey);
                var permissionId = permissions.Last().PermissionId;

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
                        IPortalInfo portal = PortalController.Instance.GetPortal(tab.PortalID);
                        var role = RoleController.Instance.GetRole(portal.PortalId, r => r.RoleName == roleName);
                        if (role != null)
                        {
                            roleId = role.RoleID;
                        }

                        break;
                }

                if (roleId != int.MinValue && !tab.TabPermissions.Cast<IPermissionInfo>().Any(p => p.RoleId == roleId && p.PermissionId == permissionId))
                {
                    var tabPermission = new TabPermissionInfo
                    {
                        TabID = tab.TabID,
                        AllowAccess = allowAccess,
                    };
                    ((IPermissionInfo)tabPermission).PermissionId = permissionId;
                    ((IPermissionInfo)tabPermission).RoleId = roleId;

                    tab.TabPermissions.Add(tabPermission);
                }
            }

            TabController.Instance.UpdateTab(tab);
        }
    }
}
