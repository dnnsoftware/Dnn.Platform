// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Security
{
    using System;
    using System.Security.Permissions;

    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using Newtonsoft.Json.Linq;

    public class SecurityService : ISecurityService
    {
        private readonly ITabController _tabController;

        public SecurityService()
        {
            this._tabController = TabController.Instance;
        }

        public static ISecurityService Instance
        {
            get
            {
                var controller = ComponentFactory.GetComponent<ISecurityService>("PagesSecurityService");
                if (controller == null)
                {
                    ComponentFactory.RegisterComponent<ISecurityService, SecurityService>("PagesSecurityService");
                }

                return ComponentFactory.GetComponent<ISecurityService>("PagesSecurityService");
            }
        }

        public virtual bool IsPageAdminUser()
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }

        public virtual bool IsVisible(MenuItem menuItem)
        {
            return this.IsPageAdminUser() || this.CanViewPageList(menuItem.MenuId);
        }

        public virtual JObject GetCurrentPagePermissions()
        {
            var permissions = new JObject
            {
                {"addContentToPage", TabPermissionController.CanAddContentToPage()},
                {"addPage", TabPermissionController.CanAddPage()},
                {"adminPage", TabPermissionController.CanAdminPage()},
                {"copyPage", TabPermissionController.CanCopyPage()},
                {"deletePage", TabPermissionController.CanDeletePage()},
                {"exportPage", TabPermissionController.CanExportPage()},
                {"importPage", TabPermissionController.CanImportPage()},
                {"managePage", TabPermissionController.CanManagePage()}
            };

            return permissions;
        }

        public virtual JObject GetPagePermissions(TabInfo tab)
        {
            var permissions = new JObject
            {
                {"addContentToPage", TabPermissionController.CanAddContentToPage(tab)},
                {"addPage", TabPermissionController.CanAddPage(tab)},
                {"adminPage", TabPermissionController.CanAdminPage(tab)},
                {"copyPage", TabPermissionController.CanCopyPage(tab)},
                {"deletePage", TabPermissionController.CanDeletePage(tab)},
                {"exportPage", TabPermissionController.CanExportPage(tab)},
                {"importPage", TabPermissionController.CanImportPage(tab)},
                {"managePage", TabPermissionController.CanManagePage(tab)}
            };

            return permissions;
        }

        public virtual bool CanAdminPage(int tabId)
        {
            return this.IsPageAdminUser() || TabPermissionController.CanAdminPage(this.GetTabById(tabId));
        }

        public virtual bool CanManagePage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanManagePage(this.GetTabById(tabId));
        }

        public virtual bool CanDeletePage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanDeletePage(this.GetTabById(tabId));
        }

        public virtual bool CanAddPage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanAddPage(this.GetTabById(tabId));
        }

        public virtual bool CanCopyPage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanCopyPage(this.GetTabById(tabId));
        }

        public virtual bool CanExportPage(int tabId)
        {
            return this.CanAdminPage(tabId) || TabPermissionController.CanExportPage(this.GetTabById(tabId));
        }

        public virtual bool CanViewPageList(int menuId)
        {
            var permissions = MenuPermissionController.GetMenuPermissions(PortalSettings.Current.PortalId, menuId);
            return MenuPermissionController.HasMenuPermission(new MenuPermissionCollection(permissions), "VIEW_PAGE_LIST") || this.IsPageAdmin();
        }

        public virtual bool CanSavePageDetails(PageSettings pageSettings)
        {
            var tabId = pageSettings.TabId;
            var pageType = pageSettings.PageType;
            var parentId = pageSettings.ParentId ?? 0;
            var creatingPage = tabId <= 0;
            var updatingPage = tabId > 0;
            var creatingTemplate = tabId <= 0 && pageSettings.TemplateTabId > 0 && pageType == "template";
            var duplicatingPage = tabId <= 0 && pageSettings.TemplateTabId > 0;
            var updatingParentPage = false;
            if (updatingPage)
            {
                var tab = TabController.Instance.GetTab(tabId, PortalSettings.Current.PortalId);
                if (tab != null && tab.ParentId != parentId)
                {
                    updatingParentPage = true;
                }
            }

            return (
                this.IsPageAdminUser() ||
                creatingPage && this.CanAddPage(parentId) && !creatingTemplate ||
                creatingTemplate && this.CanExportPage(pageSettings.TemplateTabId) ||
                updatingPage && this.CanManagePage(tabId) && !updatingParentPage ||
                updatingParentPage && this.CanManagePage(tabId) && this.CanAddPage(parentId) ||
                duplicatingPage && this.CanCopyPage(pageSettings.TemplateTabId) && this.CanAddPage(parentId)
            );
        }

        public virtual bool IsAdminHostSystemPage()
        {
            return PortalSettings.Current.ActiveTab.IsSystem || PortalSettings.Current.ActiveTab.IsSuperTab ||
                PortalSettings.Current.ActiveTab.ParentId == PortalSettings.Current.AdminTabId || PortalSettings.Current.ActiveTab.TabID == PortalSettings.Current.AdminTabId;
        }

        private bool IsPageAdmin()
        {
            return //TabPermissionController.CanAddContentToPage() ||
                TabPermissionController.CanAddPage()
                || TabPermissionController.CanAdminPage()
                || TabPermissionController.CanCopyPage()
                || TabPermissionController.CanDeletePage()
                || TabPermissionController.CanExportPage()
                || TabPermissionController.CanImportPage()
                || TabPermissionController.CanManagePage();
        }

        private TabInfo GetTabById(int pageId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return pageId <= 0 ? new TabInfo() : this._tabController.GetTab(pageId, portalSettings.PortalId, false);
        }
    }
}
