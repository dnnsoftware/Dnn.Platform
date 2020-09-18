// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class RecyclebinController : PersonaBarApiController
    {
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinPagesView)]
        public HttpResponseMessage GetDeletedPageList(int pageIndex = -1, int pageSize = -1)
        {
            var totalRecords = 0;
            var tabs = Components.RecyclebinController.Instance.GetDeletedTabs(out totalRecords, pageIndex, pageSize);
            var deletedtabs = from t in tabs
                              select this.ConvertToPageItem(t, tabs);
            var response = new
            {
                Success = true,
                Results = deletedtabs,
                TotalResults = totalRecords
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinModulesView)]
        public HttpResponseMessage GetDeletedModuleList(int pageIndex = -1, int pageSize = -1)
        {
            var totalRecords = 0;
            var mods = Components.RecyclebinController.Instance.GetDeletedModules(out totalRecords, pageIndex, pageSize);
            var deletedmodules = from t in mods select this.ConvertToModuleItem(t);
            var response = new
            {
                Success = true,
                Results = deletedmodules,
                TotalResults = totalRecords
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinUsersView)]
        public HttpResponseMessage GetDeletedUserList(int pageIndex = -1, int pageSize = -1)
        {
            var totalRecords = 0;
            var users = Components.RecyclebinController.Instance.GetDeletedUsers(out totalRecords, pageIndex, pageSize);
            var deletedusers = from t in users select this.ConvertToUserItem(t);
            var response = new
            {
                Success = true,
                Results = deletedusers,
                TotalResults = totalRecords
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinPagesView + "&" + Components.Constants.RecycleBinPagesEdit)]
        public HttpResponseMessage RemovePage(List<PageItem> pages)
        {
            var errors = new StringBuilder();

            Components.RecyclebinController.Instance.DeleteTabs(pages, errors);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message =
                        string.Format(
                            Components.RecyclebinController.Instance.LocalizeString("Service_RemoveTabError"),
                            errors)
                });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinModulesView + "&" + Components.Constants.RecycleBinModulesEdit)]
        public HttpResponseMessage RemoveModule(List<ModuleItem> modules)
        {
            var errors = new StringBuilder();

            Components.RecyclebinController.Instance.DeleteModules(modules, errors);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message =
                        string.Format(
                            Components.RecyclebinController.Instance.LocalizeString("Service_RemoveTabModuleError"),
                            errors)
                });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinUsersView + "&" + Components.Constants.RecycleBinUsersEdit)]
        public HttpResponseMessage RemoveUser(List<UserItem> users)
        {
            var errors = new StringBuilder();

            Components.RecyclebinController.Instance.DeleteUsers(users);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message =
                        string.Format(
                            Components.RecyclebinController.Instance.LocalizeString("Service_RemoveUserError"), errors)
                });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinPagesView + "&" + Components.Constants.RecycleBinPagesEdit)]
        public HttpResponseMessage RestorePage(List<PageItem> pages)
        {
            var errors = new StringBuilder();
            if (pages != null && pages.Any())
            {
                foreach (
                    var tab in pages.Select(page => TabController.Instance.GetTab(page.Id, this.PortalSettings.PortalId)))
                {
                    if (tab == null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                    string resultmessage;
                    Components.RecyclebinController.Instance.RestoreTab(tab, out resultmessage);
                    errors.Append(resultmessage);
                }
            }
            return errors.Length > 0
                ? this.Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Status = 1,
                        Message =
                            string.Format(
                                Components.RecyclebinController.Instance.LocalizeString("Service_RestoreTabModuleError"),
                                errors)
                    })
                : this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinModulesView + "&" + Components.Constants.RecycleBinModulesEdit)]
        public HttpResponseMessage RestoreModule(List<ModuleItem> modules)
        {
            //modules dic stores module.Key=moduleId, module.Value=pageId;
            var result = true;
            var errors = new StringBuilder();
            if (modules != null && modules.Any())
            {
                foreach (var module in modules)
                {
                    var tab = TabController.Instance.GetTab(module.TabID, this.PortalSettings.PortalId);
                    if (tab == null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                    string resultmessage;
                    result = Components.RecyclebinController.Instance.RestoreModule(module.Id, module.TabID,
                        out resultmessage);
                    errors.Append(resultmessage);

                }
            }
            if (!result)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, Message = errors });
            }
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RecycleBinUsersView + "&" + Components.Constants.RecycleBinUsersEdit)]
        public HttpResponseMessage RestoreUser(List<UserItem> users)
        {
            var errors = new StringBuilder();
            if (users != null && users.Any())
            {
                foreach (
                    var user in users.Select(u => UserController.Instance.GetUserById(this.PortalSettings.PortalId, u.Id)))
                {
                    if (user == null)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                    string resultmessage;
                    Components.RecyclebinController.Instance.RestoreUser(user, out resultmessage);
                    errors.Append(resultmessage);
                }
            }
            return errors.Length > 0
                ? this.Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Status = 1,
                        Message =
                            string.Format(
                                Components.RecyclebinController.Instance.LocalizeString("Service_RestoreTabModuleError"),
                                errors)
                    })
                : this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName,
            Permission =
                Components.Constants.RecycleBinPagesView + "&" + Components.Constants.RecycleBinModulesView + "&" +
                Components.Constants.RecycleBinUsersView + "&" + Components.Constants.RecycleBinPagesEdit + "&" +
                Components.Constants.RecycleBinModulesEdit + "&" + Components.Constants.RecycleBinUsersEdit)]
        public HttpResponseMessage EmptyRecycleBin()
        {
            var totalRecords = 0;
            var deletedTabs = Components.RecyclebinController.Instance.GetDeletedTabs(out totalRecords);
            var deletedModules = Components.RecyclebinController.Instance.GetDeletedModules(out totalRecords);
            var deletedUsers = Components.RecyclebinController.Instance.GetDeletedUsers(out totalRecords);
            var errors = new StringBuilder();

            Components.RecyclebinController.Instance.DeleteModules(deletedModules, errors);

            Components.RecyclebinController.Instance.DeleteTabs(deletedTabs, errors, true);

            Components.RecyclebinController.Instance.DeleteUsers(deletedUsers);

            if (errors.Length > 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Status = 1,
                    Message =
                        string.Format(
                            Components.RecyclebinController.Instance.LocalizeString("Service_EmptyRecycleBinError"),
                            errors)
                });
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        private PageItem ConvertToPageItem(TabInfo tab, IEnumerable<TabInfo> portalTabs)
        {
            return new PageItem
            {
                Id = tab.TabID,
                Name = tab.LocalizedTabName,
                Url = tab.FullUrl,
                ChildrenCount = portalTabs != null ? portalTabs.Count(ct => ct.ParentId == tab.TabID) : 0,
                PublishDate = tab.CreatedOnDate.ToString("MM/dd/yyyy"),
                Status = Components.RecyclebinController.Instance.GetTabStatus(tab),
                ParentId = tab.ParentId,
                Level = tab.Level,
                IsSpecial = TabController.IsSpecialTab(tab.TabID, this.PortalSettings),
                TabPath = tab.TabPath.Replace("//", "/"),
                LastModifiedOnDate =
                    tab.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                FriendlyLastModifiedOnDate =
                    tab.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(tab.CultureCode ?? "en-US")),
                UseDefaultSkin = this.UseDefaultSkin(tab)
            };
        }

        private bool UseDefaultSkin(TabInfo tab)
        {
            return !string.IsNullOrEmpty(tab.SkinSrc) &&
                   tab.SkinSrc.Equals(this.PortalSettings.DefaultPortalSkin, StringComparison.OrdinalIgnoreCase);
        }

        private ModuleItem ConvertToModuleItem(ModuleInfo mod)
        {
            var tab = TabController.Instance.GetTab(mod.TabID, this.PortalSettings.PortalId);
            return new ModuleItem
            {
                Id = mod.ModuleID,
                Title = mod.ModuleTitle,
                TabModuleId = mod.TabModuleID,
                PortalId = mod.PortalID,
                TabName = tab.TabName,
                TabID = tab.TabID,
                TabDeleted = tab.IsDeleted,
                LastModifiedOnDate =
                    mod.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(mod.CultureCode ?? "en-US")),
                FriendlyLastModifiedOnDate =
                    mod.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(mod.CultureCode ?? "en-US"))
            };
        }

        private UserItem ConvertToUserItem(UserInfo user)
        {
            return new UserItem
            {
                Id = user.UserID,
                Username = user.Username,
                PortalId = user.PortalID,
                DisplayName = user.DisplayName,
                Email = user.Email,
                LastModifiedOnDate =
                    user.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(user.Profile.PreferredLocale ?? "en-US")),
                FriendlyLastModifiedOnDate =
                    user.LastModifiedOnDate.ToString("MM/dd/yyyy h:mm:ss tt",
                        CultureInfo.CreateSpecificCulture(user.Profile.PreferredLocale ?? "en-US"))
            };
        }
    }
}
