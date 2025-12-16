// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Recyclebin.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Recyclebin.Components.Dto;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    public class RecyclebinController : ServiceLocator<IRecyclebinController, RecyclebinController>,
        IRecyclebinController
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string PageDateTimeFormat = "yyyy-MM-dd hh:mm tt";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RecyclebinController));
        private readonly ITabController tabController;
        private readonly ITabVersionSettings tabVersionSettings;
        private readonly ITabChangeSettings tabChangeSettings;
        private readonly ITabWorkflowSettings tabWorkflowSettings;
        private readonly IModuleController moduleController;

        public RecyclebinController()
        {
            this.tabController = TabController.Instance;
            this.tabVersionSettings = TabVersionSettings.Instance;
            this.tabWorkflowSettings = TabWorkflowSettings.Instance;
            this.moduleController = ModuleController.Instance;
            this.tabChangeSettings = TabChangeSettings.Instance;
        }

        private static PortalSettings PortalSettings => PortalSettings.Current;

        /// <inheritdoc/>
        public string LocalizeString(string key)
        {
            return Localization.GetString(key, Constants.LocalResourcesFile);
        }

        /// <inheritdoc/>
        public void DeleteTabs(IEnumerable<PageItem> tabs, StringBuilder errors, bool deleteDescendants = false)
        {
            if (tabs != null)
            {
                foreach (var tab in tabs.OrderByDescending(t => t.Level))
                {
                    var tabInfo = this.tabController.GetTab(tab.Id, PortalSettings.PortalId);
                    if (tabInfo == null)
                    {
                        errors.AppendFormat(CultureInfo.InvariantCulture, this.LocalizeString("PageNotFound"), tab.Id);
                    }
                    else
                    {
                        this.HardDeleteTab(tabInfo, deleteDescendants, errors);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void DeleteTabs(IEnumerable<TabInfo> tabs, StringBuilder errors, bool deleteDescendants = false)
        {
            if (tabs != null)
            {
                foreach (var tab in tabs.OrderByDescending(t => t.Level))
                {
                    var tabInfo = this.tabController.GetTab(tab.TabID, PortalSettings.PortalId);
                    if (tabInfo == null)
                    {
                        errors.AppendFormat(CultureInfo.InvariantCulture, this.LocalizeString("PageNotFound"), tab.TabID);
                    }
                    else
                    {
                        this.HardDeleteTab(tabInfo, deleteDescendants, errors);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void DeleteModules(IEnumerable<ModuleItem> modules, StringBuilder errors)
        {
            if (modules != null)
            {
                foreach (var mod in modules)
                {
                    var moduleInfo = ModuleController.Instance.GetModule(mod.Id, mod.TabID, true);
                    if (moduleInfo == null)
                    {
                        errors.AppendFormat(CultureInfo.InvariantCulture, this.LocalizeString("ModuleNotFound"), mod.Id, mod.TabID);
                    }
                    else
                    {
                        this.HardDeleteModule(moduleInfo, errors);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void DeleteModules(IEnumerable<ModuleInfo> modules, StringBuilder errors)
        {
            modules?.ForEach(mod => this.HardDeleteModule(mod, errors));
        }

        /// <inheritdoc/>
        public bool RestoreTab(TabInfo tab, out string resultmessage)
        {
            var changeControlStateForTab = this.tabChangeSettings.GetChangeControlState(tab.PortalID, tab.TabID);
            if (changeControlStateForTab.IsChangeControlEnabledForTab)
            {
                this.tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, false);
                this.tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
            }

            var success = true;
            resultmessage = null;

            // if parent of the page is deleted, then can't restore - parent should be restored first
            var deletedTabs = this.GetDeletedTabs(out _);
            if (!Null.IsNull(tab.ParentId) && deletedTabs.Any(t => t.TabID == tab.ParentId))
            {
                resultmessage = string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Service_RestoreTabError"), tab.TabName);
                success = false;
            }
            else
            {
                this.tabController.RestoreTab(tab, PortalSettings);

                // restore modules in this tab
                var tabDeletedModules = this.GetDeletedModules(out _).Where(m => m.TabID == tab.TabID);

                foreach (var m in tabDeletedModules)
                {
                    success = this.RestoreModule(m.ModuleID, m.TabID, out resultmessage);
                }

                if (changeControlStateForTab.IsChangeControlEnabledForTab)
                {
                    this.tabVersionSettings.SetEnabledVersioningForTab(
                        tab.TabID,
                        changeControlStateForTab.IsVersioningEnabledForTab);
                    this.tabWorkflowSettings.SetWorkflowEnabled(
                        tab.PortalID,
                        tab.TabID,
                        changeControlStateForTab.IsWorkflowEnabledForTab);
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public bool RestoreModule(int moduleId, int tabId, out string errorMessage)
        {
            errorMessage = null;

            // restore module
            var module = ModulesController.Instance.GetModule(PortalSettings, moduleId, tabId, out _);

            if (module != null)
            {
                var deletedTab = this.GetDeletedTabs(out _).Where(t => t.TabID == module.TabID).SingleOrDefault();
                if (deletedTab is not null)
                {
                    var title = !string.IsNullOrEmpty(module.ModuleTitle)
                        ? module.ModuleTitle
                        : module.DesktopModule.FriendlyName;
                    errorMessage = string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString("Service_RestoreModuleError"),
                        title,
                        deletedTab.TabName);
                    return false;
                }

                if (module.IsDeleted)
                {
                    this.moduleController.RestoreModule(module);
                }
                else
                {
                    errorMessage = string.Format(CultureInfo.CurrentCulture, this.LocalizeString("ModuleNotSoftDeleted"), moduleId);
                    return false;
                }
            }
            else
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, this.LocalizeString("ModuleNotFound"), moduleId, tabId);
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public List<TabInfo> GetDeletedTabs(out int totalRecords, int pageIndex = -1, int pageSize = -1, string sortType = "", string sortDirection = "")
        {
            var adminTabId = PortalSettings.AdminTabId;
            var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, adminTabId, true, true, true, true);
            var deletedtabs =
                tabs.Where(t => t.ParentId != adminTabId && t.IsDeleted && TabPermissionController.CanDeletePage(t));
            if (sortType == "tab")
            {
                deletedtabs = sortDirection == "asc" ? deletedtabs = deletedtabs.OrderBy(tab => tab.TabName) : deletedtabs = deletedtabs.OrderByDescending(tab => tab.TabName);
            }

            if (sortType == "date")
            {
                deletedtabs = sortDirection == "asc" ? deletedtabs = deletedtabs.OrderBy(tab => tab.LastModifiedOnDate) : deletedtabs = deletedtabs.OrderByDescending(tab => tab.LastModifiedOnDate);
            }

            totalRecords = deletedtabs.Count();
            return pageIndex == -1 || pageSize == -1 ? deletedtabs.ToList() : deletedtabs.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        /// <inheritdoc/>
        public List<ModuleInfo> GetDeletedModules(out int totalRecords, int pageIndex = -1, int pageSize = -1, string sortType = "", string sortDirection = "")
        {
            var deletedModules = this.moduleController.GetModules(PortalSettings.PortalId)
                .Cast<ModuleInfo>()
                .Where(module => module.IsDeleted && (
                    TabPermissionController.CanAddContentToPage(TabController.Instance.GetTab(module.TabID, module.PortalID)) ||
                    ModulePermissionController.CanDeleteModule(module)));
            if (sortType == "title")
            {
                deletedModules = sortDirection == "asc" ? deletedModules = deletedModules.OrderBy(module => module.ModuleTitle) : deletedModules = deletedModules.OrderByDescending(module => module.ModuleTitle);
            }

            if (sortType == "tab")
            {
                deletedModules = sortDirection == "asc" ? deletedModules = deletedModules.OrderBy(module => module.ParentTab.TabName) : deletedModules = deletedModules.OrderByDescending(module => module.ParentTab.TabName);
            }

            if (sortType == "date")
            {
                deletedModules = sortDirection == "asc" ? deletedModules = deletedModules.OrderBy(module => module.LastModifiedOnDate) : deletedModules = deletedModules.OrderByDescending(module => module.LastModifiedOnDate);
            }

            totalRecords = deletedModules.Count();
            return pageIndex == -1 || pageSize == -1 ? deletedModules.ToList() : deletedModules.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        /// <inheritdoc/>
        public string GetTabStatus(TabInfo tab)
        {
            if (tab.DisableLink)
            {
                return "Disabled";
            }

            return tab.IsVisible ? "Visible" : "Hidden";
        }

        /// <inheritdoc/>
        public List<UserInfo> GetDeletedUsers(out int totalRecords, int pageIndex = -1, int pageSize = -1, string sortType = "", string sortDirection = "")
        {
            var deletedusers = UserController.GetDeletedUsers(PortalSettings.PortalId).Cast<UserInfo>().Where(this.CanManageUser);
            if (sortType == "username")
            {
                deletedusers = sortDirection == "asc" ? deletedusers = deletedusers.OrderBy(user => user.Username) : deletedusers = deletedusers.OrderByDescending(user => user.Username);
            }

            if (sortType == "displayname")
            {
                deletedusers = sortDirection == "asc" ? deletedusers = deletedusers.OrderBy(user => user.DisplayName) : deletedusers = deletedusers.OrderByDescending(user => user.DisplayName);
            }

            if (sortType == "date")
            {
                deletedusers = sortDirection == "asc" ? deletedusers = deletedusers.OrderBy(user => user.LastModifiedOnDate) : deletedusers = deletedusers.OrderByDescending(user => user.LastModifiedOnDate);
            }

            totalRecords = deletedusers.Count();
            return pageIndex == -1 || pageSize == -1 ? deletedusers.ToList() : deletedusers.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        /// <inheritdoc/>
        public void DeleteUsers(IEnumerable<UserInfo> users)
        {
            var userInfos = users as IList<UserInfo> ?? users.ToList();
            if (users == null || !userInfos.Any())
            {
                return;
            }

            foreach (
                var user in
                    userInfos.Select(u => UserController.GetUserById(u.PortalID, u.UserID))
                        .Where(user => user != null)
                        .Where(user => user.IsDeleted))
            {
                if (this.CanManageUser(user))
                {
                    UserController.RemoveUser(user);
                }
            }
        }

        /// <inheritdoc/>
        public void DeleteUsers(IEnumerable<UserItem> users)
        {
            var userInfos = users.Select(x => new UserInfo { PortalID = x.PortalId, UserID = x.Id });
            this.DeleteUsers(userInfos);
        }

        /// <inheritdoc/>
        public bool RestoreUser(UserInfo user, out string errorMessage)
        {
            errorMessage = null;
            var deletedUsers = UserController.GetDeletedUsers(user.PortalID).Cast<UserInfo>().Where(this.CanManageUser).ToList();
            if (deletedUsers.Any(u => u.UserID == user.UserID))
            {
                UserController.RestoreUser(ref user);
                return true;
            }

            errorMessage = this.LocalizeString("Service_RestoreUserError");
            return false;
        }

        /// <inheritdoc/>
        protected override Func<IRecyclebinController> GetFactory()
        {
            return () => new RecyclebinController();
        }

        private void HardDeleteTab(TabInfo tab, bool deleteDescendants, StringBuilder errors)
        {
            if (TabPermissionController.CanDeletePage(tab) && tab.IsDeleted)
            {
                if (tab.HasChildren && !deleteDescendants)
                {
                    errors.Append(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Service_RemoveTabWithChildError"), tab.TabName));
                    return;
                }

                // get tab modules before deleting page
                var tabModules = this.moduleController.GetTabModules(tab.TabID);

                // hard delete the tab
                this.tabController.DeleteTab(tab.TabID, tab.PortalID, deleteDescendants);

                // delete modules that do not have other instances
                foreach (var kvp in tabModules)
                {
                    // check if all modules instances have been deleted
                    var delModule = this.moduleController.GetModule(kvp.Value.ModuleID, Null.NullInteger, false);
                    if (delModule == null || delModule.TabID == Null.NullInteger)
                    {
                        try
                        {
                            this.moduleController.DeleteModule(kvp.Value.ModuleID);
                        }
                        catch (Exception exc)
                        {
                            Logger.Error(exc);
                        }
                    }
                }
            }
            else
            {
                errors.AppendFormat(
                    CultureInfo.InvariantCulture,
                    !tab.IsDeleted ? this.LocalizeString("TabNotSoftDeleted") : this.LocalizeString("CanNotDeleteTab"),
                    tab.TabID);
            }
        }

        private void HardDeleteModule(ModuleInfo module, StringBuilder errors)
        {
            try
            {
                if (ModulePermissionController.CanDeleteModule(module) && module.IsDeleted)
                {
                    this.moduleController.DeleteTabModule(module.TabID, module.ModuleID, false);
                }
                else
                {
                    errors.AppendFormat(
                        CultureInfo.InvariantCulture,
                        !module.IsDeleted ? this.LocalizeString("ModuleNotSoftDeleted") : this.LocalizeString("CanNotDeleteModule"),
                        module.ModuleID);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            // hard-delete Tab Module Instance
        }

        /// <summary>Checks if the current user has enough rights to manage the provided user or not.</summary>
        /// <param name="user">The user to check.</param>
        /// <returns><see langword="true"/> if the current user can manage the given <paramref name="user"/>, otherwise <see langword="false"/>.</returns>
        private bool CanManageUser(UserInfo user)
        {
            if (PortalSettings.UserInfo.IsSuperUser ||
                (PortalSettings.UserInfo.IsInRole(PortalSettings.AdministratorRoleName) && !user.IsSuperUser) ||
                (!PortalSettings.UserInfo.IsInRole(PortalSettings.AdministratorRoleName) && !user.IsSuperUser &&
                 !user.IsInRole(PortalSettings.AdministratorRoleName)))
            {
                return true;
            }

            return false;
        }
    }
}
