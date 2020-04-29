using System;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using Dnn.Modules.ResourceManager.Components.Common;

namespace Dnn.Modules.ResourceManager.Components
{
    public class PermissionsManager : ServiceLocator<IPermissionsManager, PermissionsManager>, IPermissionsManager
    {
        #region Private Members

        private readonly IFolderManager _folderManager;
        private readonly IRoleController _roleController;
        private readonly IUserController _userController;

        #endregion

        #region Constructors

        public PermissionsManager()
        {
            _folderManager = FolderManager.Instance;
            _roleController = RoleController.Instance;
            _userController = UserController.Instance;
        }

        #endregion

        public bool HasFolderContentPermission(int folderId, int moduleMode)
        {
            return HasGroupFolderPublicOrMemberPermission(folderId);
        }

        public bool HasGetFileContentPermission(int folderId)
        {
            if (!HasGroupFolderPublicOrMemberPermission(folderId))
            {
                return false;
            }

            var folder = _folderManager.GetFolder(folderId);
            return HasPermission(folder, "READ");
        }

        public bool HasAddFilesPermission(int moduleMode, int folderId)
        {
            if (!HasGroupFolderMemberPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !IsUserFolder(folderId))
            {
                return false;
            }

            var folder = _folderManager.GetFolder(folderId);
            return folder != null && HasPermission(folder, "ADD");
        }

        public bool HasAddFoldersPermission(int moduleMode, int folderId)
        {
            if (!HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User)
            {
                return false;
            }

            var folder = _folderManager.GetFolder(folderId);
            return folder != null && HasPermission(folder, "ADD");
        }

        public bool HasDeletePermission(int moduleMode, int folderId)
        {
            if (!HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !IsUserFolder(folderId))
            {
                return false;
            }

            var folder = _folderManager.GetFolder(folderId);
            return FolderPermissionController.CanDeleteFolder((FolderInfo) folder);
        }

        public bool HasManagePermission(int moduleMode, int folderId)
        {
            if (!HasGroupFolderOwnerPermission(folderId))
            {
                return false;
            }

            if (moduleMode == (int)Constants.ModuleModes.User && !IsUserFolder(folderId))
            {
                return false;
            }

            var folder = _folderManager.GetFolder(folderId);
            return FolderPermissionController.CanManageFolder((FolderInfo) folder);
        }

        #region Private methods

        #region Group Folder Permissions

        private bool HasGroupFolderPublicOrMemberPermission(int folderId)
        {
            var groupId = Utils.GetFolderGroupId(folderId);
            if (groupId < 0)
            {
                return true;
            }

            var portalId = PortalSettings.Current.PortalId;
            var folderGroup = _roleController.GetRoleById(portalId, groupId);

            return folderGroup.IsPublic || UserIsGroupMember(groupId);
        }

        public bool HasGroupFolderMemberPermission(int folderId)
        {
            var groupId = Utils.GetFolderGroupId(folderId);
            if (groupId < 0)
            {
                return true;
            }

            return UserIsGroupMember(groupId);
        }

        private bool HasGroupFolderOwnerPermission(int folderId)
        {
            var groupId = Utils.GetFolderGroupId(folderId);
            if (groupId < 0)
            {
                return true;
            }

            return UserIsGroupOwner(groupId);
        }

        private bool UserIsGroupMember(int groupId)
        {
            return GetUserRoleInfo(groupId) != null;
        }

        private bool UserIsGroupOwner(int groupId)
        {
            var userRole = GetUserRoleInfo(groupId);
            return userRole != null && userRole.IsOwner;
        }

        #endregion

        private bool IsUserFolder(int folderId)
        {
            var user = _userController.GetCurrentUserInfo();
            return _folderManager.GetUserFolder(user).FolderID == folderId;
        }

        private UserRoleInfo GetUserRoleInfo(int groupId)
        {
            var userId = _userController.GetCurrentUserInfo().UserID;
            var portalId = PortalSettings.Current.PortalId;
            return _roleController.GetUserRole(portalId, userId, groupId);
        }

        private static bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermission = PortalSettings.Current.UserInfo.IsSuperUser;

            if (!hasPermission && folder != null)
            {
                hasPermission = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermission;
        }

        #endregion

        #region Service Locator

        protected override Func<IPermissionsManager> GetFactory()
        {
            return () => new PermissionsManager();
        }

        #endregion
    }
}