#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Internal;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Services.FileSystem.Internal
{
    public class UserSecurityController : ServiceLocator<IUserSecurityController, UserSecurityController>, IUserSecurityController
    {
        public bool IsHostAdminUser(int portalId)
        {
            return IsHostAdminUser(portalId, UserController.GetCurrentUserInfo().UserID);
        }

        public bool IsHostAdminUser(int portalId, int userId)
        {
            if (userId == Null.NullInteger)
            {
                return false;
            }
            var user = TestableUserController.Instance.GetUserById(portalId, userId);
            return user.IsSuperUser || portalId > Null.NullInteger && user.IsInRole(PortalControllerWrapper.Instance.GetPortal(portalId).AdministratorRoleName);
        }

        public bool HasFolderPermission(IFolderInfo folder, string permissionKey)
        {
            return UserController.GetCurrentUserInfo().IsSuperUser ||
                   FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
        }

        protected override System.Func<IUserSecurityController> GetFactory()
        {
            return () => new UserSecurityController();
        }
    }
}
