// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc/>
    public class AdvancedPermissionProvider : PermissionProvider
    {
        /// <summary>Content Editors Advanced Role Name.</summary>
        public const string ContentEditors = "Content Editors";

        /// <summary>Content Managers Advanced Role Name.</summary>
        public const string ContentManagers = "Content Managers";

        // Constants representing various permission keys for folders, modules, and tabs. These keys are used to check specific permissions.

        // Note: Permissions keys that are used in base but not here are commented

        // Folder Permission Keys
        private const string AdminFolderPermissionKey = "WRITE";
        private const string AddFolderPermissionKey = "ADD"; // "WRITE";

        // private const string BrowseFolderPermissionKey = "BROWSE";
        private const string CopyFolderPermissionKey = "COPY"; // "WRITE";
        private const string DeleteFolderPermissionKey = "DELETE"; // "WRITE";
        private const string ManageFolderPermissionKey = "MANAGE"; // "WRITE";

        // private const string ViewFolderPermissionKey = "READ";

        // Module Permission Keys
        private const string AdminModulePermissionKey = "EDIT";
        private const string ContentModulePermissionKey = "CONTENT"; // "EDIT";
        private const string DeleteModulePermissionKey = "DELETE"; // "EDIT";
        private const string ExportModulePermissionKey = "EXPORT"; // "EDIT";
        private const string ImportModulePermissionKey = "IMPORT"; // "EDIT";
        private const string ManageModulePermissionKey = "MANAGE"; // "EDIT";

        // private const string ViewModulePermissionKey = "VIEW";

        // Page Permission Keys
        private const string AddPagePermissionKey = "ADD"; // "EDIT";
        private const string AdminPagePermissionKey = "EDIT";
        private const string ContentPagePermissionKey = "CONTENT"; // "EDIT";
        private const string CopyPagePermissionKey = "COPY"; // "EDIT";
        private const string DeletePagePermissionKey = "DELETE"; // "EDIT";
        private const string ExportPagePermissionKey = "EXPORT"; // "EDIT";
        private const string ImportPagePermissionKey = "IMPORT"; // "EDIT";
        private const string ManagePagePermissionKey = "MANAGE"; // "EDIT";
        private const string NavigatePagePermissionKey = "NAVIGATE"; // "VIEW";
        private const string ViewPagePermissionKey = "VIEW";

        // A list of advanced roles such as "Content Editors" and "Content Managers" that are checked and created if necessary.
        private readonly List<string> advancedRoles = [ContentEditors, ContentManagers];

        // Dependencies
        private readonly IPermissionDefinitionService permissionDefinitions;
        private readonly IModuleController moduleController;
        private readonly IRoleController roleController;
        private readonly ITabController tabController;

        /// <summary>Initializes a new instance of the <see cref="AdvancedPermissionProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public AdvancedPermissionProvider()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AdvancedPermissionProvider"/> class.</summary>
        /// <param name="permissionDefinitions">The permission definition service.</param>
        /// <param name="moduleController">The module controller.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="tabController">The tab controller.</param>
        public AdvancedPermissionProvider(IPermissionDefinitionService permissionDefinitions, IModuleController moduleController, IRoleController roleController, ITabController tabController)
        {
            this.permissionDefinitions = permissionDefinitions ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
            this.moduleController = moduleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IModuleController>();
            this.roleController = roleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        /// <inheritdoc/>
        public override bool IsPortalEditor()
            => this.advancedRoles.Any(PortalSecurity.IsInRole)
            || base.IsPortalEditor();

        /// <inheritdoc/>
        public override bool CanAddFolder(FolderInfo folder)
            => CanDoOnFolder(folder, AddFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanCopyFolder(FolderInfo folder)
            => CanDoOnFolder(folder, CopyFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanDeleteFolder(FolderInfo folder)
            => CanDoOnFolder(folder, DeleteFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanManageFolder(FolderInfo folder)
            => CanDoOnFolder(folder, ManageFolderPermissionKey);

        /// <inheritdoc/>
        public override IEnumerable<RoleInfo> ImplicitRolesForFolders(int portalId)
        {
            var list = base.ImplicitRolesForFolders(portalId).ToList();
            list.AddRange(this.GetOrCreateAdvancedRoles(portalId));
            return list;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int portalId, string folder)
        {
            var basePermissions = base.GetFolderPermissionsCollectionByFolder(portalId, folder);

            // Create a new permission collection and copy base permissions
            var permissions = new FolderPermissionCollection();
            permissions.AddRange(basePermissions);

            var folderPermissions = permissions.Cast<FolderPermissionInfo>().ToList();
            var folderId = GetFolderId(folderPermissions);
            var folderPath = GetFolderPath(folderPermissions);

            var virtualPermissions = this.GenerateVirtualFolderPermissions(folderPermissions.ToList<PermissionInfoBase>(), portalId, folderId, folderPath);
            permissions.AddRange(virtualPermissions);

            return permissions;
        }

        /// <inheritdoc/>
        public override bool HasFolderPermission(FolderPermissionCollection objFolderPermissions, string permissionKey)
            => base.HasFolderPermission(objFolderPermissions, permissionKey)
            || base.HasFolderPermission(objFolderPermissions, AdminFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanEditModuleContent(ModuleInfo module)
            => CanDoOnModule(module, ContentModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanDeleteModule(ModuleInfo module)
            => CanDoOnModule(module, DeleteModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanExportModule(ModuleInfo module)
            => CanDoOnModule(module, ExportModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanImportModule(ModuleInfo module)
            => CanDoOnModule(module, ImportModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanManageModule(ModuleInfo module)
            => CanDoOnModule(module, ManageModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanViewModule(ModuleInfo module)
            => this.ForAdvancedRoles(module)
            || base.CanViewModule(module);

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override ModulePermissionCollection GetModulePermissions(int moduleId, int tabId)
        {
            var module = this.moduleController.GetModule(moduleId, tabId, false);
            if (module == null)
            {
                return new ModulePermissionCollection();
            }

            var tab = this.tabController.GetTab(tabId, module.PortalID);
            var basePermissions = base.GetModulePermissions(moduleId, tabId);

            // Create a new permission collection and copy base permissions
            var permissions = new ModulePermissionCollection();
            permissions.AddRange(basePermissions);

            if (this.tabController.IsHostOrAdminPage(tab))
            {
                return permissions;
            }

            var modulePermissions = permissions.Cast<PermissionInfoBase>().ToList();

            var virtualPermissions = this.GenerateVirtualModulePermissions(modulePermissions, module.PortalID, tabId, moduleId);
            virtualPermissions.ForEach(v => permissions.Add(v));

            return permissions;
        }

        /// <inheritdoc/>
        public override bool HasModuleAccess(SecurityAccessLevel accessLevel, string permissionKey, ModuleInfo moduleConfiguration)
        {
            var hasModuleAccess = base.HasModuleAccess(accessLevel, permissionKey, moduleConfiguration);
            return accessLevel switch
            {
                SecurityAccessLevel.Admin or SecurityAccessLevel.Host => hasModuleAccess,
                _ => this.ForAdvancedRoles(moduleConfiguration) | hasModuleAccess,
            };
        }

        /// <inheritdoc/>
        public override bool CanAddContentToPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, ContentPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanAdminPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, AdminPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanAddPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, AddPagePermissionKey)
            || (tab.TabID == Null.NullInteger && this.CanAddTopLevel(tab.PortalID))
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanCopyPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, CopyPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanDeletePage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, DeletePagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanExportPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, ExportPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanImportPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, ImportPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanManagePage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, ManagePagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanNavigateToPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, NavigatePagePermissionKey)
            || CanDoOnPage(tab, ViewPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanViewPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || CanDoOnPage(tab, ViewPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override IEnumerable<RoleInfo> ImplicitRolesForPages(int portalId)
        {
            var roles = base.ImplicitRolesForPages(portalId).ToList();
            roles.AddRange(this.GetOrCreateAdvancedRoles(portalId));
            return roles;
        }

        /// <inheritdoc/>
        public override TabPermissionCollection GetTabPermissions(int tabId, int portalId)
        {
            var tab = this.tabController.GetTab(tabId, portalId);
            if (tab == null)
            {
                return new TabPermissionCollection();
            }

            var basePermissions = base.GetTabPermissions(tabId, portalId);

            // Create a new permission collection and copy base permissions
            var permissions = new TabPermissionCollection();
            permissions.AddRange(basePermissions);

            if (this.tabController.IsHostOrAdminPage(tab))
            {
                return permissions;
            }

            var tabPermissions = permissions.Cast<PermissionInfoBase>().ToList();

            var virtualPermissions = this.GenerateVirtualTabPermissions(tabPermissions, tab.PortalID, tab.TabID);
            permissions.AddRange(virtualPermissions);

            return permissions;
        }

        /// <inheritdoc/>
        public override bool HasTabPermission(TabPermissionCollection tabPermissions, string permissionKey)
        {
            if (tabPermissions.Count == 0)
            {
                return this.IsPortalEditor()
                    || base.HasTabPermission(tabPermissions, permissionKey);
            }

            var tab = this.tabController.GetTab(tabPermissions[0].TabID, Null.NullInteger);
            if (tab == null)
            {
                return this.IsPortalEditor()
                    || base.HasTabPermission(tabPermissions, permissionKey);
            }

            return this.ForAdvancedRoles(tab)
                || base.HasTabPermission(tabPermissions, permissionKey);
        }

        /// <inheritdoc/>
        public override bool HasDesktopModulePermission(DesktopModulePermissionCollection desktopModulePermissions, string permissionKey)
            => this.IsPortalEditor()
            || base.HasDesktopModulePermission(desktopModulePermissions, permissionKey);

        private static string GetFolderPath(List<FolderPermissionInfo> folderPermissions)
            => folderPermissions.FirstOrDefault()?.FolderPath ?? Null.NullString;

        private static int GetFolderId(List<FolderPermissionInfo> folderPermissions)
            => folderPermissions.Cast<IFolderPermissionInfo>().Select(p => (int?)p.FolderId).FirstOrDefault() ?? Null.NullInteger;

        private static bool HasPermission(string rolesOfExactKey, string editRoles)
        {
            // first check for explicit Deny permissions
            if (PortalSecurity.IsDenied(rolesOfExactKey))
            {
                return false;
            }

            // last check for Allowed permissions
            return PortalSecurity.IsInRoles(rolesOfExactKey)
                   || PortalSecurity.IsInRoles(editRoles);
        }

        private static bool CanDoOnFolder(FolderInfo folder, string exactKey)
        {
            return folder != null
                   && HasPermission(folder.FolderPermissions.ToString(exactKey), folder.FolderPermissions.ToString(AdminFolderPermissionKey));
        }

        private static bool CanDoOnModule(ModuleInfo module, string exactKey)
        {
            return module != null
                   && HasPermission(module.ModulePermissions.ToString(exactKey), module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        private static bool CanDoOnPage(TabInfo tab, string exactKey)
        {
            return tab != null
                   && HasPermission(tab.TabPermissions.ToString(exactKey), tab.TabPermissions.ToString(AdminPagePermissionKey));
        }

        private List<RoleInfo> GetOrCreateAdvancedRoles(int portalId)
            => portalId >= 0
            ? this.advancedRoles.Select(roleName => this.GetOrCreateAdvancedRole(portalId, roleName)).ToList()
            : [];

        private RoleInfo GetOrCreateAdvancedRole(int portalId, string roleName)
        {
            var role = this.roleController.GetRoleByName(portalId, roleName);
            if (role != null)
            {
                return role;
            }

            // add missing content role when missing
            // similar to CreateRole in PortalTemplateImporter
            role = new RoleInfo
            {
                PortalID = portalId,
                RoleName = roleName,
                RoleGroupID = Null.NullInteger,
                Description = $"Advanced Role {roleName}",
                ServiceFee = 0,
                BillingPeriod = 0,
                BillingFrequency = "M",
                TrialFee = 0,
                TrialPeriod = 0,
                TrialFrequency = "N",
                IsPublic = false,
                AutoAssignment = false,
                SecurityMode = SecurityMode.SecurityRole,
                Status = RoleStatus.Approved,
                IsSystemRole = true,
            };

            this.roleController.AddRole(role);
            return role;
        }

        private bool ForAdvancedRoles(ModuleInfo module)
            => this.ForAdvancedRoles(module.ParentTab);

        private bool ForAdvancedRoles(TabInfo tab)
        {
            if (!this.IsPortalEditor())
            {
                return false;
            }

            return !this.tabController.IsHostOrAdminPage(tab);
        }

        private List<FolderPermissionInfo> GenerateVirtualFolderPermissions(List<PermissionInfoBase> permissions, int portalId, int folderId, string folderPath)
            => this.GenerateVirtualPermissions(permissions, portalId, this.permissionDefinitions.GetDefinitionsByFolder()) // get list of all possible permissions for this folder type e.g.ADD, COPY
                .Select(set =>
                {
                    var info = new FolderPermissionInfo(set.PermissionInfo)
                    {
                        AllowAccess = true,
                        FolderPath = folderPath,
                        RoleName = set.RoleInfo.RoleName,
                    };
                    ((IFolderPermissionInfo)info).FolderId = folderId;
                    ((IFolderPermissionInfo)info).RoleId = set.RoleInfo.RoleID;
                    ((IFolderPermissionInfo)info).UserId = Null.NullInteger;
                    return info;
                }).ToList();

        private List<ModulePermissionInfo> GenerateVirtualModulePermissions(List<PermissionInfoBase> permissions, int portalId, int tabId, int moduleId)
            => this.GenerateVirtualPermissions(permissions, portalId, this.permissionDefinitions.GetDefinitionsByModule(moduleId, tabId)) // get list of all possible permissions for this module type e.g. CONTENT, DELETE
                .Select(set =>
                {
                    var info = new ModulePermissionInfo(set.PermissionInfo)
                    {
                        AllowAccess = true,
                        ModuleID = moduleId,
                        RoleName = set.RoleInfo.RoleName,
                    };
                    ((IPermissionInfo)info).UserId = Null.NullInteger;
                    ((IPermissionInfo)info).RoleId = set.RoleInfo.RoleID;
                    return info;
                }).ToList();

        private List<TabPermissionInfo> GenerateVirtualTabPermissions(List<PermissionInfoBase> permissions, int portalId, int tabId)
            => this.GenerateVirtualPermissions(permissions, portalId, this.permissionDefinitions.GetDefinitionsByTab()) // get list of all possible permissions for this tab type e.g. ADD, CONTENT
                .Select(set =>
                {
                    var info = new TabPermissionInfo(set.PermissionInfo)
                    {
                        AllowAccess = true,
                        RoleName = set.RoleInfo.RoleName,
                        TabID = tabId,
                    };
                    ((IPermissionInfo)info).RoleId = set.RoleInfo.RoleID;
                    ((IPermissionInfo)info).UserId = Null.NullInteger;
                    return info;
                }).ToList();

        private List<(PermissionInfo PermissionInfo, RoleInfo RoleInfo)> GenerateVirtualPermissions(
            List<PermissionInfoBase> permissions,
            int portalId,
            IEnumerable<IPermissionDefinitionInfo> possiblePermissions)
            => this.GetOrCreateAdvancedRoles(portalId)
                .SelectMany(role => possiblePermissions
                    .Where(ep => !permissions.Any((IPermissionInfo p) => p.PermissionKey == ep.PermissionKey && p.RoleId == role.RoleID))
                    .Select(permissionInfo => ((PermissionInfo)permissionInfo, role))).ToList();
    }
}
