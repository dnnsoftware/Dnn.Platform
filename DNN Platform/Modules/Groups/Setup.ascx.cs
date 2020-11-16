// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins;

    public partial class Setup : GroupsModuleBase
    {
        public void btGo_Click(object sender, EventArgs e)
        {
            // Setup Child Page - Main View/Activity
            TabInfo tab = this.CreatePage(this.PortalSettings.ActiveTab, this.PortalId, this.TabId, "Group Activity", false);

            // Add Module to Child Page
            int groupViewModuleId = this.AddModule(tab, this.PortalId, "Social Groups", "ContentPane");
            int journalModuleId = this.AddModule(tab, this.PortalId, "Journal", "ContentPane");
            int consoleId = this.AddModule(tab, this.PortalId, "Console", "RightPane");

            ModuleInfo groupConsoleModule = ModuleController.Instance.GetModule(consoleId, tab.TabID, false);
            TabInfo memberTab = this.CreatePage(this.PortalSettings.ActiveTab, this.PortalId, tab.TabID, "Members", true);
            ModuleController.Instance.CopyModule(groupConsoleModule, memberTab, "RightPane", true);

            ModuleInfo groupViewModule = ModuleController.Instance.GetModule(groupViewModuleId, tab.TabID, false);
            ModuleController.Instance.CopyModule(groupViewModule, memberTab, "ContentPane", true);
            this.AddModule(memberTab, this.PortalId, "DotNetNuke.Modules.MemberDirectory", "ContentPane");

            // List Settings
            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupLoadView, GroupMode.List.ToString());
            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupViewPage, tab.TabID.ToString(CultureInfo.InvariantCulture));

            // Default Social Groups
            var defaultGroup = RoleController.GetRoleGroupByName(this.PortalId, Constants.DefaultGroupName);
            var groupId = -2;
            if (defaultGroup != null)
            {
                groupId = defaultGroup.RoleGroupID;
            }
            else
            {
                var groupInfo = new RoleGroupInfo();
                groupInfo.PortalID = this.PortalId;
                groupInfo.RoleGroupName = Constants.DefaultGroupName;
                groupInfo.Description = Constants.DefaultGroupName;
                groupId = RoleController.AddRoleGroup(groupInfo);
            }

            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.DefaultRoleGroupSetting, groupId.ToString());

            this.Response.Redirect(this.Request.RawUrl);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.btnGo.Visible = this.Request.IsAuthenticated;
            this.btnGo.Enabled = this.Request.IsAuthenticated;
            this.btnGo.Click += this.btGo_Click;
        }

        private TabInfo CreatePage(TabInfo tab, int portalId, int parentTabId, string tabName, bool includeInMenu)
        {
            var newTab = TabController.Instance.GetTabsByPortal(portalId).WithTabNameAndParentId(tabName, parentTabId);
            if (newTab == null)
            {
                int id = -1;
                newTab = new TabInfo();

                if (tab != null)
                {
                    foreach (TabPermissionInfo t in tab.TabPermissions)
                    {
                        var tNew = new TabPermissionInfo
                        {
                            AllowAccess = t.AllowAccess,
                            DisplayName = t.DisplayName,
                            ModuleDefID = t.ModuleDefID,
                            PermissionCode = t.PermissionCode,
                            PermissionID = t.PermissionID,
                            PermissionKey = t.PermissionKey,
                            PermissionName = t.PermissionName,
                            RoleID = t.RoleID,
                            RoleName = t.RoleName,
                            TabID = -1,
                            TabPermissionID = -1,
                            UserID = t.UserID,
                            Username = t.Username,
                        };
                        newTab.TabPermissions.Add(tNew);
                    }
                }

                newTab.ParentId = parentTabId;
                newTab.PortalID = portalId;
                newTab.TabName = tabName;
                newTab.Title = tabName;
                newTab.IsVisible = includeInMenu;
                newTab.SkinSrc = this.GetSkin();

                id = TabController.Instance.AddTab(newTab);
                newTab = TabController.Instance.GetTab(id, portalId, true);
            }

            return newTab;
        }

        private string GetSkin()
        {
            // attempt to find and load a  skin from the assigned skinned source
            var skinSource = this.PortalSettings.DefaultPortalSkin;

            var tab = TabController.Instance.GetTab(this.TabId, this.PortalId, false);

            if (!string.IsNullOrEmpty(tab.SkinSrc))
            {
                skinSource = tab.SkinSrc;
            }
            else
            {
                skinSource = SkinController.FormatSkinPath(skinSource) + "groups.ascx";
                var physicalSkinFile = SkinController.FormatSkinSrc(skinSource, this.PortalSettings);

                if (!File.Exists(HttpContext.Current.Server.MapPath(physicalSkinFile)))
                {
                    skinSource = string.Empty; // this will load the default skin
                }
            }

            return skinSource;
        }

        private int AddModule(TabInfo tab, int portalId, string moduleName, string pane)
        {
            var module = ModuleController.Instance.GetTabModules(tab.TabID).Values.SingleOrDefault(m => m.DesktopModule.ModuleName == moduleName);
            int id = -1;
            if (module == null)
            {
                int desktopModuleId = this.GetDesktopModuleId(portalId, moduleName);
                int moduleId = -1;
                if (desktopModuleId > -1)
                {
                    if (moduleId <= 0)
                    {
                        moduleId = this.AddNewModule(tab, string.Empty, desktopModuleId, pane, 0, string.Empty);
                    }

                    id = moduleId;
                    ModuleInfo mi = ModuleController.Instance.GetModule(moduleId, tab.TabID, false);
                    if (moduleName == "Social Groups")
                    {
                        ModuleController.Instance.UpdateTabModuleSetting(mi.TabModuleID, Constants.GroupLoadView, GroupMode.View.ToString());
                        ModuleController.Instance.UpdateTabModuleSetting(mi.TabModuleID, Constants.GroupListPage, tab.TabID.ToString(CultureInfo.InvariantCulture));
                    }

                    if (moduleName == "Console")
                    {
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "AllowSizeChange", "False");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "AllowViewChange", "False");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "IncludeParent", "True");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "Mode", "Group");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "DefaultSize", "IconNone");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "ParentTabID", tab.TabID.ToString(CultureInfo.InvariantCulture));
                    }

                    if (moduleName == "DotNetNuke.Modules.MemberDirectory")
                    {
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "FilterBy", "Group");
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "FilterPropertyValue", string.Empty);
                        ModuleController.Instance.UpdateModuleSetting(mi.ModuleID, "FilterValue", "-1");
                        ModuleController.Instance.UpdateTabModuleSetting(mi.TabModuleID, "DisplaySearch", "False");
                    }
                }
            }
            else
            {
                id = module.ModuleID;
            }

            return id;
        }

        private int GetDesktopModuleId(int portalId, string moduleName)
        {
            DesktopModuleInfo info = DesktopModuleController.GetDesktopModuleByModuleName(moduleName, portalId);
            return info == null ? -1 : info.DesktopModuleID;
        }

        private int AddNewModule(TabInfo tab, string title, int desktopModuleId, string paneName, int permissionType, string align)
        {
            TabPermissionCollection objTabPermissions = tab.TabPermissions;
            var objPermissionController = new PermissionController();
            int j;

            foreach (ModuleDefinitionInfo objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
            {
                var objModule = new ModuleInfo();
                objModule.Initialize(tab.PortalID);

                objModule.PortalID = tab.PortalID;
                objModule.TabID = tab.TabID;
                if (string.IsNullOrEmpty(title))
                {
                    objModule.ModuleTitle = objModuleDefinition.FriendlyName;
                }
                else
                {
                    objModule.ModuleTitle = title;
                }

                objModule.PaneName = paneName;
                objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                objModule.CacheTime = 0;
                objModule.InheritViewPermissions = true;
                objModule.DisplayTitle = false;

                // get the default module view permissions
                ArrayList arrSystemModuleViewPermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");

                // get the permissions from the page
                foreach (TabPermissionInfo objTabPermission in objTabPermissions)
                {
                    if (objTabPermission.PermissionKey == "VIEW" && permissionType == 0)
                    {
                        // Don't need to explicitly add View permisisons if "Same As Page"
                        continue;
                    }

                    // get the system module permissions for the permissionkey
                    ArrayList arrSystemModulePermissions = objPermissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", objTabPermission.PermissionKey);

                    // loop through the system module permissions
                    for (j = 0; j <= arrSystemModulePermissions.Count - 1; j++)
                    {
                        // create the module permission
                        PermissionInfo objSystemModulePermission = default(PermissionInfo);
                        objSystemModulePermission = (PermissionInfo)arrSystemModulePermissions[j];
                        if (objSystemModulePermission.PermissionKey == "VIEW" && permissionType == 1 && objTabPermission.PermissionKey != "EDIT")
                        {
                            // Only Page Editors get View permissions if "Page Editors Only"
                            continue;
                        }

                        ModulePermissionInfo objModulePermission = this.AddModulePermission(
                            objModule,
                            objSystemModulePermission,
                            objTabPermission.RoleID,
                            objTabPermission.UserID,
                            objTabPermission.AllowAccess);

                        // ensure that every EDIT permission which allows access also provides VIEW permission
                        if (objModulePermission.PermissionKey == "EDIT" & objModulePermission.AllowAccess)
                        {
                            ModulePermissionInfo objModuleViewperm = this.AddModulePermission(
                                objModule,
                                (PermissionInfo)arrSystemModuleViewPermissions[0],
                                objModulePermission.RoleID,
                                objModulePermission.UserID,
                                true);
                        }
                    }
                }

                objModule.AllTabs = false;
                objModule.Alignment = align;

                return ModuleController.Instance.AddModule(objModule);
            }

            return -1;
        }

        private ModulePermissionInfo AddModulePermission(ModuleInfo objModule, PermissionInfo permission, int roleId, int userId, bool allowAccess)
        {
            var objModulePermission = new ModulePermissionInfo();
            objModulePermission.ModuleID = objModule.ModuleID;
            objModulePermission.PermissionID = permission.PermissionID;
            objModulePermission.RoleID = roleId;
            objModulePermission.UserID = userId;
            objModulePermission.PermissionKey = permission.PermissionKey;
            objModulePermission.AllowAccess = allowAccess;

            // add the permission to the collection
            if (objModule.ModulePermissions == null)
            {
                objModule.ModulePermissions = new ModulePermissionCollection();
            }

            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add(objModulePermission);
            }

            return objModulePermission;
        }
    }
}
