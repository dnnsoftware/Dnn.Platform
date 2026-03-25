// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Display the group setup view.</summary>
    public partial class Setup : GroupsModuleBase
    {
        private readonly RoleProvider roleProvider;
        private readonly IEventLogger eventLogger;
        private readonly IUserController userController;
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="Setup"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public Setup()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Setup"/> class.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Setup(RoleProvider roleProvider, IEventLogger eventLogger, IUserController userController, IPermissionDefinitionService permissionDefinitionService)
            : this(roleProvider, eventLogger, userController, permissionDefinitionService, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Setup"/> class.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="hostSettings">The host settings.</param>
        public Setup(RoleProvider roleProvider, IEventLogger eventLogger, IUserController userController, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        {
            this.roleProvider = roleProvider ?? this.DependencyProvider.GetRequiredService<RoleProvider>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.userController = userController ?? this.DependencyProvider.GetRequiredService<IUserController>();
            this.permissionDefinitionService = permissionDefinitionService ?? this.DependencyProvider.GetRequiredService<IPermissionDefinitionService>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
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
            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupLoadView, nameof(GroupMode.List));
            ModuleController.Instance.UpdateTabModuleSetting(this.TabModuleId, Constants.GroupViewPage, tab.TabID.ToString(CultureInfo.InvariantCulture));

            // Default Social Groups
            var defaultGroup = RoleController.GetRoleGroupByName(this.roleProvider, this.PortalId, Constants.DefaultGroupName);
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
                groupId = RoleController.AddRoleGroup(this.roleProvider, this.eventLogger, this.userController, this.PortalSettings, groupInfo);
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

        private static int GetDesktopModuleId(IHostSettings hostSettings, int portalId, string moduleName)
        {
            var info = DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, moduleName, portalId);
            return info?.DesktopModuleID ?? -1;
        }

        private static IPermissionInfo AddModulePermission(ModuleInfo objModule, IPermissionDefinitionInfo permission, int roleId, int userId, bool allowAccess)
        {
            IPermissionInfo objModulePermission = new ModulePermissionInfo
            {
                ModuleID = objModule.ModuleID,
                PermissionKey = permission.PermissionKey,
                AllowAccess = allowAccess,
            };

            objModulePermission.PermissionId = permission.PermissionId;
            objModulePermission.RoleId = roleId;
            objModulePermission.UserId = userId;

            // add the permission to the collection
            objModule.ModulePermissions ??= [];
            if (!objModule.ModulePermissions.Contains(objModulePermission))
            {
                objModule.ModulePermissions.Add((ModulePermissionInfo)objModulePermission);
            }

            return objModulePermission;
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
                    foreach (IPermissionInfo t in tab.TabPermissions)
                    {
                        IPermissionInfo tNew = new TabPermissionInfo
                        {
                            AllowAccess = t.AllowAccess,
                            DisplayName = t.DisplayName,
                            PermissionCode = t.PermissionCode,
                            PermissionKey = t.PermissionKey,
                            PermissionName = t.PermissionName,
                            RoleName = t.RoleName,
                            TabID = -1,
                            TabPermissionID = -1,
                            Username = t.Username,
                        };

                        tNew.UserId = t.UserId;
                        tNew.ModuleDefId = t.ModuleDefId;
                        tNew.PermissionId = t.PermissionId;
                        tNew.RoleId = t.RoleId;

                        newTab.TabPermissions.Add((TabPermissionInfo)tNew);
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
                int desktopModuleId = GetDesktopModuleId(this.hostSettings, portalId, moduleName);
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
                        ModuleController.Instance.UpdateTabModuleSetting(mi.TabModuleID, Constants.GroupLoadView, nameof(GroupMode.View));
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

        private int AddNewModule(TabInfo tab, string title, int desktopModuleId, string paneName, int permissionType, string align)
        {
            foreach (var objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId).Values)
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
                var moduleViewPermission = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW").FirstOrDefault();

                // get the permissions from the page
                foreach (IPermissionInfo objTabPermission in tab.TabPermissions)
                {
                    if (objTabPermission.PermissionKey == "VIEW" && permissionType == 0)
                    {
                        // Don't need to explicitly add View permissions if "Same As Page"
                        continue;
                    }

                    // loop through the system module permissions
                    foreach (var objSystemModulePermission in this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", objTabPermission.PermissionKey))
                    {
                        // create the module permission
                        if (objSystemModulePermission.PermissionKey == "VIEW" && permissionType == 1 && objTabPermission.PermissionKey != "EDIT")
                        {
                            // Only Page Editors get View permissions if "Page Editors Only"
                            continue;
                        }

                        var objModulePermission = AddModulePermission(
                            objModule,
                            objSystemModulePermission,
                            objTabPermission.RoleId,
                            objTabPermission.UserId,
                            objTabPermission.AllowAccess);

                        // ensure that every EDIT permission which allows access also provides VIEW permission
                        if (objModulePermission.PermissionKey == "EDIT" & objModulePermission.AllowAccess)
                        {
                            AddModulePermission(
                                objModule,
                                moduleViewPermission,
                                objModulePermission.RoleId,
                                objModulePermission.UserId,
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
    }
}
