// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;

    public class UserSecurityController : ServiceLocator<IUserSecurityController, UserSecurityController>, IUserSecurityController
    {
        public bool IsHostAdminUser(int portalId)
        {
            return this.IsHostAdminUser(portalId, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        public bool IsHostAdminUser(int portalId, int userId)
        {
            if (userId == Null.NullInteger)
            {
                return false;
            }

            var user = UserController.Instance.GetUserById(portalId, userId);
            return user.IsSuperUser || (portalId > Null.NullInteger && user.IsInRole(PortalController.Instance.GetPortal(portalId).AdministratorRoleName));
        }

        public bool HasFolderPermission(IFolderInfo folder, string permissionKey)
        {
            return UserController.Instance.GetCurrentUserInfo().IsSuperUser ||
                   FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
        }

        protected override System.Func<IUserSecurityController> GetFactory()
        {
            return () => new UserSecurityController();
        }
    }
}
