// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.FileSystem;

    /// <inheritdoc/>
    public class DetailedPermissionProvider : PermissionProvider
    {
        // Folder Permission Keys
        private const string AdminFolderPermissionKey = "WRITE";
        private const string AddFolderPermissionKey = "ADD"; // "WRITE";
        private const string BrowseFolderPermissionKey = "BROWSE";
        private const string CopyFolderPermissionKey = "COPY"; // "WRITE";
        private const string DeleteFolderPermissionKey = "DELETE"; // "WRITE";
        private const string ManageFolderPermissionKey = "MANAGE"; // "WRITE";
        private const string ViewFolderPermissionKey = "READ";

        // Module Permission Keys
        private const string AdminModulePermissionKey = "EDIT";
        private const string ContentModulePermissionKey = "CONTENT"; // "EDIT";
        private const string DeleteModulePermissionKey = "DELETE"; // "EDIT";
        private const string ExportModulePermissionKey = "EXPORT"; // "EDIT";
        private const string ImportModulePermissionKey = "IMPORT"; // "EDIT";
        private const string ManageModulePermissionKey = "MANAGE"; // "EDIT";
        private const string ViewModulePermissionKey = "VIEW";

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

        /// <inheritdoc/>
        public override bool CanAddFolder(FolderInfo folder)
            => this.HasFolderPermission(folder, AddFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanCopyFolder(FolderInfo folder)
            => this.HasFolderPermission(folder, CopyFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanDeleteFolder(FolderInfo folder)
            => this.HasFolderPermission(folder, DeleteFolderPermissionKey);

        /// <inheritdoc/>
        public override bool CanManageFolder(FolderInfo folder)
            => this.HasFolderPermission(folder, ManageFolderPermissionKey);

        /// <inheritdoc/>
        public override bool HasFolderPermission(FolderPermissionCollection objFolderPermissions, string permissionKey)
            => PortalSecurity.IsInRoles(objFolderPermissions.ToString(permissionKey))
               || PortalSecurity.IsInRoles(objFolderPermissions.ToString(AdminFolderPermissionKey));

        /// <inheritdoc/>
        public override bool CanDeleteModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(DeleteModulePermissionKey))
                || PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        /// <inheritdoc/>
        public override bool CanEditModuleContent(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ContentModulePermissionKey))
                || PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        /// <inheritdoc/>
        public override bool CanExportModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ExportModulePermissionKey))
                || PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        /// <inheritdoc/>
        public override bool CanImportModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ImportModulePermissionKey))
                || PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        /// <inheritdoc/>
        public override bool CanManageModule(ModuleInfo module)
        {
            return PortalSecurity.IsInRoles(module.ModulePermissions.ToString(ManageModulePermissionKey))
                || PortalSecurity.IsInRoles(module.ModulePermissions.ToString(AdminModulePermissionKey));
        }

        /// <inheritdoc/>
        public override bool CanAddContentToPage(TabInfo tab)
            => this.HasPagePermission(tab, ContentPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanAddPage(TabInfo tab)
            => this.HasPagePermission(tab, AddPagePermissionKey)
            || (tab.TabID == Null.NullInteger && this.CanAddTopLevel(tab.PortalID))
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanCopyPage(TabInfo tab)
            => this.HasPagePermission(tab, CopyPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanDeletePage(TabInfo tab)
            => this.HasPagePermission(tab, DeletePagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanExportPage(TabInfo tab)
            => this.HasPagePermission(tab, ExportPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanImportPage(TabInfo tab)
            => this.HasPagePermission(tab, ImportPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanManagePage(TabInfo tab)
            => this.HasPagePermission(tab, ManagePagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        /// <inheritdoc/>
        public override bool CanNavigateToPage(TabInfo tab)
            => this.HasPagePermission(tab, NavigatePagePermissionKey)
            || this.HasPagePermission(tab, ViewPagePermissionKey)
            || this.IsPageAdmin(tab.PortalID);

        private bool HasFolderPermission(FolderInfo folder, string permissionKey)
        {
            if (folder == null)
            {
                return false;
            }

            return (PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(permissionKey))
                    || PortalSecurity.IsInRoles(folder.FolderPermissions.ToString(AdminFolderPermissionKey)))
                   && !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(permissionKey));

            // Deny on Edit permission on folder shouldn't take away any other explicitly Allowed
            // && !PortalSecurity.IsDenied(folder.FolderPermissions.ToString(AdminFolderPermissionKey));
        }

        private bool HasPagePermission(TabInfo tab, string permissionKey)
        {
            return (PortalSecurity.IsInRoles(tab.TabPermissions.ToString(permissionKey))
                    || PortalSecurity.IsInRoles(tab.TabPermissions.ToString(AdminPagePermissionKey)))
                    && !PortalSecurity.IsDenied(tab.TabPermissions.ToString(permissionKey));

            // Deny on Edit permission on page shouldn't take away any other explicitly Allowed
            // &&!PortalSecurity.IsDenied(tab.TabPermissions.ToString(AdminPagePermissionKey));
        }
    }
}
