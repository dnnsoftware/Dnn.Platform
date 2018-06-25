#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library;
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
using System.Net;
using Dnn.PersonaBar.Library.Controllers;

namespace Dnn.PersonaBar.Recyclebin.Components
{
    public class RecyclebinController : ServiceLocator<IRecyclebinController, RecyclebinController>,
        IRecyclebinController
    {
        public static string PageDateTimeFormat = "yyyy-MM-dd hh:mm tt";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RecyclebinController));

        #region Fields

        private readonly ITabController _tabController;
        private readonly ITabVersionSettings _tabVersionSettings;
        private readonly ITabChangeSettings _tabChangeSettings;
        private readonly ITabWorkflowSettings _tabWorkflowSettings;
        private readonly IModuleController _moduleController;

        #endregion

        public RecyclebinController()
        {
            _tabController = TabController.Instance;
            _tabVersionSettings = TabVersionSettings.Instance;
            _tabWorkflowSettings = TabWorkflowSettings.Instance;
            _moduleController = ModuleController.Instance;
            _tabChangeSettings = TabChangeSettings.Instance;
        }

        #region Properties

        private static PortalSettings PortalSettings => PortalSettings.Current;

        #endregion

        #region ServiceLocator

        protected override Func<IRecyclebinController> GetFactory()
        {
            return () => new RecyclebinController();
        }

        #endregion

        #region Public Methods

        public string LocalizeString(string key)
        {
            return Localization.GetString(key, Constants.LocalResourcesFile);
        }

        public void DeleteTabs(IEnumerable<PageItem> tabs, StringBuilder errors, bool deleteDescendants = false)
        {
            if (tabs != null)
            {
                foreach (var tab in tabs.OrderByDescending(t => t.Level))
                {
                    var tabInfo = _tabController.GetTab(tab.Id, PortalSettings.PortalId);
                    if (tabInfo == null)
                    {
                        errors.AppendFormat(LocalizeString("PageNotFound"), tab.Id);
                    }
                    else
                    {
                        HardDeleteTab(tabInfo, deleteDescendants, errors);
                    }
                }
            }
        }

        public void DeleteTabs(IEnumerable<TabInfo> tabs, StringBuilder errors, bool deleteDescendants = false)
        {
            if (tabs != null)
            {
                foreach (var tab in tabs.OrderByDescending(t => t.Level))
                {
                    var tabInfo = _tabController.GetTab(tab.TabID, PortalSettings.PortalId);
                    if (tabInfo == null)
                    {
                        errors.AppendFormat(LocalizeString("PageNotFound"), tab.TabID);
                    }
                    else
                    {
                        HardDeleteTab(tabInfo, deleteDescendants, errors);
                    }
                }
            }
        }

        public void DeleteModules(IEnumerable<ModuleItem> modules, StringBuilder errors)
        {
            if (modules != null)
            {
                foreach (var mod in modules)
                {
                    var moduleInfo = ModuleController.Instance.GetModule(mod.Id, mod.TabID, true);
                    if (moduleInfo == null)
                    {
                        errors.AppendFormat(LocalizeString("ModuleNotFound"), mod.Id, mod.TabID);
                    }
                    else
                    {
                        HardDeleteModule(moduleInfo, errors);
                    }
                }
            }
        }

        public void DeleteModules(IEnumerable<ModuleInfo> modules, StringBuilder errors)
        {
            modules?.ForEach(mod => HardDeleteModule(mod, errors));
        }

        private void HardDeleteTab(TabInfo tab, bool deleteDescendants, StringBuilder errors)
        {
            if (TabPermissionController.CanDeletePage(tab) && tab.IsDeleted)
            {
                if (tab.HasChildren && !deleteDescendants)
                {
                    errors.Append(string.Format(LocalizeString("Service_RemoveTabWithChildError"), tab.TabName));
                    return;
                }
                //get tab modules before deleting page
                var tabModules = _moduleController.GetTabModules(tab.TabID);

                //hard delete the tab
                _tabController.DeleteTab(tab.TabID, tab.PortalID, deleteDescendants);

                //delete modules that do not have other instances
                foreach (var kvp in tabModules)
                {
                    //check if all modules instances have been deleted
                    var delModule = _moduleController.GetModule(kvp.Value.ModuleID, Null.NullInteger, false);
                    if (delModule == null || delModule.TabID == Null.NullInteger)
                    {
                        try
                        {
                            _moduleController.DeleteModule(kvp.Value.ModuleID);
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
                errors.AppendFormat(!tab.IsDeleted ? LocalizeString("TabNotSoftDeleted") : LocalizeString("CanNotDeleteTab"), tab.TabID);
            }
        }

        private void HardDeleteModule(ModuleInfo module, StringBuilder errors)
        {
            try
            {
                if (ModulePermissionController.CanDeleteModule(module) && module.IsDeleted)
                {
                    _moduleController.DeleteTabModule(module.TabID, module.ModuleID, false);
                }
                else
                {
                    errors.AppendFormat(!module.IsDeleted ? LocalizeString("ModuleNotSoftDeleted") : LocalizeString("CanNotDeleteModule"), module.ModuleID);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            //hard-delete Tab Module Instance
        }

        public bool RestoreTab(TabInfo tab, out string resultmessage)
        {
            var changeControlStateForTab = _tabChangeSettings.GetChangeControlState(tab.PortalID, tab.TabID);
            if (changeControlStateForTab.IsChangeControlEnabledForTab)
            {
                _tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, false);
                _tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
            }

            var success = true;
            resultmessage = null;

            //if parent of the page is deleted, then can't restore - parent should be restored first
            var totalRecords = 0;
            var deletedTabs = GetDeletedTabs(out totalRecords);
            if (!Null.IsNull(tab.ParentId) && deletedTabs.Any(t => t.TabID == tab.ParentId))
            {
                resultmessage = string.Format(LocalizeString("Service_RestoreTabError"), tab.TabName);
                success = false;
            }
            else
            {
                _tabController.RestoreTab(tab, PortalSettings);

                //restore modules in this tab
                var tabdeletedModules = GetDeletedModules(out totalRecords).Where(m => m.TabID == tab.TabID);

                foreach (var m in tabdeletedModules)
                {
                    success = RestoreModule(m.ModuleID, m.TabID, out resultmessage);
                }

                if (changeControlStateForTab.IsChangeControlEnabledForTab)
                {
                    _tabVersionSettings.SetEnabledVersioningForTab(tab.TabID,
                        changeControlStateForTab.IsVersioningEnabledForTab);
                    _tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID,
                        changeControlStateForTab.IsWorkflowEnabledForTab);
                }
            }
            return success;
        }

        public bool RestoreModule(int moduleId, int tabId, out string errorMessage)
        {
            errorMessage = null;
            // restore module

            KeyValuePair<HttpStatusCode, string> message;
            var module = ModulesController.Instance.GetModule(PortalSettings, moduleId, tabId, out message);

            if (module != null)
            {
                var totalRecords = 0;
                var deletedTabs = GetDeletedTabs(out totalRecords).Where(t => t.TabID == module.TabID);
                if (deletedTabs.Any())
                {
                    var title = !string.IsNullOrEmpty(module.ModuleTitle)
                        ? module.ModuleTitle
                        : module.DesktopModule.FriendlyName;
                    errorMessage = string.Format(LocalizeString("Service_RestoreModuleError"), title,
                        deletedTabs.SingleOrDefault().TabName);
                    return false;
                }
                if (module.IsDeleted)
                    _moduleController.RestoreModule(module);
                else
                {
                    errorMessage = string.Format(LocalizeString("ModuleNotSoftDeleted"), moduleId);
                    return false;
                }
            }
            else
            {
                errorMessage = string.Format(LocalizeString("ModuleNotFound"), moduleId, tabId);
                return false;
            }
            return true;
        }

        public List<TabInfo> GetDeletedTabs(out int totalRecords, int pageIndex = -1, int pageSize = -1)
        {
            var adminTabId = PortalSettings.AdminTabId;
            var tabs = TabController.GetPortalTabs(PortalSettings.PortalId, adminTabId, true, true, true, true);
            var deletedtabs =
                tabs.Where(t => t.ParentId != adminTabId && t.IsDeleted && TabPermissionController.CanDeletePage(t));
            totalRecords = deletedtabs.Count();
            return pageIndex == -1 || pageSize == -1 ? deletedtabs.ToList() : deletedtabs.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public List<ModuleInfo> GetDeletedModules(out int totalRecords, int pageIndex = -1, int pageSize = -1)
        {
            var deletedModules = _moduleController.GetModules(PortalSettings.PortalId)
                .Cast<ModuleInfo>()
                .Where(module => module.IsDeleted && (
                    TabPermissionController.CanAddContentToPage(TabController.Instance.GetTab(module.TabID, module.PortalID)) ||
                    ModulePermissionController.CanDeleteModule(module))
                );
            totalRecords = deletedModules.Count();
            return pageIndex == -1 || pageSize == -1 ? deletedModules.ToList() : deletedModules.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public string GetTabStatus(TabInfo tab)
        {
            if (tab.DisableLink)
            {
                return "Disabled";
            }

            return tab.IsVisible ? "Visible" : "Hidden";
        }

        public List<UserInfo> GetDeletedUsers(out int totalRecords, int pageIndex = -1, int pageSize = -1)
        {
            var deletedusers = UserController.GetDeletedUsers(PortalSettings.PortalId).Cast<UserInfo>().Where(CanManageUser);
            totalRecords = deletedusers.Count();
            return pageIndex == -1 || pageSize == -1 ? deletedusers.ToList() : deletedusers.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public void DeleteUsers(IEnumerable<UserInfo> users)
        {
            var userInfos = users as IList<UserInfo> ?? users.ToList();
            if (users == null || !userInfos.Any()) return;
            foreach (
                var user in
                    userInfos.Select(u => UserController.GetUserById(u.PortalID, u.UserID))
                        .Where(user => user != null)
                        .Where(user => user.IsDeleted))
            {
                if (CanManageUser(user))
                    UserController.RemoveUser(user);
            }
        }

        public void DeleteUsers(IEnumerable<UserItem> users)
        {
            var userInfos = users.Select(x => new UserInfo { PortalID = x.PortalId, UserID = x.Id });
            DeleteUsers(userInfos);
        }

        public bool RestoreUser(UserInfo user, out string errorMessage)
        {
            errorMessage = null;
            var deletedusers = UserController.GetDeletedUsers(user.PortalID).Cast<UserInfo>().Where(CanManageUser).ToList();
            if ((user != null) && deletedusers.Any(u => u.UserID == user.UserID))
            {
                UserController.RestoreUser(ref user);
                return true;
            }
            errorMessage = string.Format(LocalizeString("Service_RestoreUserError"));
            return false;
        }

        /// <summary>
        /// Checks if the current user has enough rights to manage the provided user or not
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
        #endregion        
    }
}