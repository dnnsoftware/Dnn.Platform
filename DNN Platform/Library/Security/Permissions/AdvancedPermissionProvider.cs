// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    /// <inheritdoc/>
    public class AdvancedPermissionProvider : PermissionProvider
    {
        /// <summary>
        /// Content Editors Advanced Role Name.
        /// </summary>
        public const string ContentEditors = "Content Editors";

        /// <summary>
        /// Content Managers Advanced Role Name.
        /// </summary>
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
        private readonly List<string> advancedRoles = new List<string>() { ContentEditors, ContentManagers };

        // Dependencies
        private readonly IModuleController moduleController = ModuleController.Instance;
        private readonly IPermissionDefinitionService permissionDefinitions = new PermissionController();
        private readonly IRoleController roleController = RoleController.Instance;
        private readonly ITabController tabController = TabController.Instance;

        /// <inheritdoc/>
        public override bool IsPortalEditor()
            => this.advancedRoles.Any(PortalSecurity.IsInRole)
            || base.IsPortalEditor();

        /// <inheritdoc/>
        public override bool CanAddFolder(FolderInfo folder)
            => this.CanDoOnFolder(folder, AddFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanCopyFolder(FolderInfo folder)
            => this.CanDoOnFolder(folder, CopyFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanDeleteFolder(FolderInfo folder)
            => this.CanDoOnFolder(folder, DeleteFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanManageFolder(FolderInfo folder)
            => this.CanDoOnFolder(folder, ManageFolderPermissionKey);

        /// <inheritdoc/>
        public override IEnumerable<RoleInfo> ImplicitRolesForFolders(int portalId)
        {
            var list = base.ImplicitRolesForFolders(portalId).ToList();
            list.AddRange(this.GetOrCreateAdvancedRoles(portalId));
            return list;
        }

        /// <inheritdoc/>
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
            => this.CanDoOnModule(module, ContentModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanDeleteModule(ModuleInfo module)
            => this.CanDoOnModule(module, DeleteModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanExportModule(ModuleInfo module)
            => this.CanDoOnModule(module, ExportModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanImportModule(ModuleInfo module)
            => this.CanDoOnModule(module, ImportModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanManageModule(ModuleInfo module)
            => this.CanDoOnModule(module, ManageModulePermissionKey);

        /// <inheritdoc/>
        public override bool CanViewModule(ModuleInfo module)
            => this.ForAdvancedRoles(module)
            || base.CanViewModule(module);

        /// <inheritdoc/>
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
            || this.CanDoOnPage(tab, ContentPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanAdminPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, AdminPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanAddPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, AddPagePermissionKey)
            || (tab.TabID == Null.NullInteger && this.CanAddTopLevel(tab.PortalID))
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanCopyPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, CopyPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanDeletePage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, DeletePagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanExportPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, ExportPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanImportPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, ImportPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanManagePage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, ManagePagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanNavigateToPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, NavigatePagePermissionKey)
            || this.CanDoOnPage(tab, ViewPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanViewPage(TabInfo tab)
            => this.ForAdvancedRoles(tab)
            || this.CanDoOnPage(tab, ViewPagePermissionKey)
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
            => folderPermissions.FirstOrDefault()?.FolderID ?? Null.NullInteger;

        private IEnumerable<RoleInfo> GetOrCreateAdvancedRoles(int portalId)
            => portalId >= 0
            ? this.advancedRoles.Select(roleName => this.GetOrCreateAdvancedRole(portalId, roleName)).ToList()
            : new List<RoleInfo>();

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

        private bool CanDoOnFolder(FolderInfo folder, string exactKey)
        {
            return folder != null
                && this.HasPermission(folder.FolderPermissions.ToString(exactKey), folder.FolderPermissions.ToString(AdminFolderPermissionKey));
        }

        private bool CanDoOnModule(ModuleInfo module, string exactKey)
        {
            return module != null
                && this.HasPermission(module.ModulePermissions.ToString(exactKey), module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        private bool CanDoOnPage(TabInfo tab, string exactKey)
        {
            return tab != null
                && this.HasPermission(tab.TabPermissions.ToString(exactKey), tab.TabPermissions.ToString(AdminPagePermissionKey));
        }

        private bool HasPermission(string rolesOfExactKey, string editRoles)
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
                .Select(set => new FolderPermissionInfo(set.PermissionInfo)
                {
                    AllowAccess = true,
                    FolderID = folderId,
                    FolderPath = folderPath,
                    RoleID = set.RoleInfo.RoleID,
                    RoleName = set.RoleInfo.RoleName,
                    UserID = Null.NullInteger,
                }).ToList();

        private List<ModulePermissionInfo> GenerateVirtualModulePermissions(List<PermissionInfoBase> permissions, int portalId, int tabId, int moduleId)
            => this.GenerateVirtualPermissions(permissions, portalId, this.permissionDefinitions.GetDefinitionsByModule(moduleId, tabId)) // get list of all possible permissions for this module type e.g. CONTENT, DELETE
                .Select(set => new ModulePermissionInfo(set.PermissionInfo)
                {
                    AllowAccess = true,
                    ModuleID = moduleId,
                    RoleID = set.RoleInfo.RoleID,
                    RoleName = set.RoleInfo.RoleName,
                    UserID = Null.NullInteger,
                }).ToList();

        private List<TabPermissionInfo> GenerateVirtualTabPermissions(List<PermissionInfoBase> permissions, int portalId, int tabId)
            => this.GenerateVirtualPermissions(permissions, portalId, this.permissionDefinitions.GetDefinitionsByTab()) // get list of all possible permissions for this tab type e.g. ADD, CONTENT
                .Select(set => new TabPermissionInfo(set.PermissionInfo)
                {
                    AllowAccess = true,
                    RoleID = set.RoleInfo.RoleID,
                    RoleName = set.RoleInfo.RoleName,
                    TabID = tabId,
                    UserID = Null.NullInteger,
                }).ToList();

        private List<(PermissionInfo PermissionInfo, RoleInfo RoleInfo)> GenerateVirtualPermissions(
            List<PermissionInfoBase> permissions,
            int portalId,
            IEnumerable<IPermissionDefinitionInfo> possiblePermissions)
            => this.GetOrCreateAdvancedRoles(portalId)
                .SelectMany(role => possiblePermissions
                    .Where(ep => !permissions.Any(p => p.PermissionKey == ep.PermissionKey && p.RoleID == role.RoleID))
                    .Select(permissionInfo => ((PermissionInfo)permissionInfo, role))).ToList();
    }
}
