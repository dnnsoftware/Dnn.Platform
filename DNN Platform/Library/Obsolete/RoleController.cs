#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Web.Services.Description;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Security.Roles
{
    /// <summary>
    /// The RoleController class provides Business Layer methods for Roles
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class RoleController 
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.0. This function has been replaced by AddUserRole with additional params. Scheduled removal in v10.0.0.")]
        public static void AddUserRole(UserInfo user, RoleInfo role, PortalSettings portalSettings, DateTime effectiveDate, DateTime expiryDate, int userId, bool notifyUser)
        {
            AddUserRole(user, role, portalSettings, RoleStatus.Approved, effectiveDate, expiryDate, notifyUser, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by overload with extra parameters. Scheduled removal in v10.0.0.")]
        public void AddUserRole(int portalId, int userId, int roleId, DateTime expiryDate)
        {
            AddUserRole(portalId, userId, roleId, RoleStatus.Approved, false, Null.NullDate, expiryDate);
        }

        public void AddUserRole(int portalId, int userId, int roleId, DateTime effectiveDate, DateTime expiryDate)
        {
            AddUserRole(portalId, userId, roleId, RoleStatus.Approved, false, effectiveDate, expiryDate);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by DeleteRole(role). Scheduled removal in v10.0.0.")]
        public void DeleteRole(int roleId, int portalId)
        {
            RoleInfo role = GetRole(portalId, r => r.RoleID == roleId);
            if (role != null)
            {
                DeleteRole(role);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by GetRoles(PortalId, predicate). Scheduled removal in v10.0.0.")]
        public ArrayList GetPortalRoles(int portalId)
        {
            return new ArrayList(Instance.GetRoles(portalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This method has been replacd by GetRoleById. Scheduled removal in v10.0.0.")]
        public RoleInfo GetRole(int roleId, int portalId)
        {
            return GetRoleById(portalId, roleId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by GetRoles(PortalId, predicate). Scheduled removal in v10.0.0.")]
        public ArrayList GetRoles()
        {
            return new ArrayList(Instance.GetRoles(Null.NullInteger, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
        }

        public ArrayList GetRolesByGroup(int portalId, int roleGroupId)
        {
            return new ArrayList(Instance.GetRoles(portalId, r => r.RoleGroupID == roleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This method has been replaced by RoleController.Instance.GetUsersByRole(portalId, roleName). Scheduled removal in v10.0.0.")]
        public ArrayList GetUsersByRoleName(int portalId, string roleName)
        {
            return new ArrayList(Instance.GetUsersByRole(portalId, roleName).ToList());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by RoleController.Instance.UpdateRole(role). Scheduled removal in v10.0.0.")]
        public void UpdateRole(RoleInfo role)
        {
            Instance.UpdateRole(role);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by overload with extra parameters. Scheduled removal in v10.0.0.")]
        public void UpdateUserRole(int portalId, int userId, int roleId)
        {
            UpdateUserRole(portalId, userId, roleId, RoleStatus.Approved, false, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by overload with extra parameters. Scheduled removal in v10.0.0.")]
        public void UpdateUserRole(int portalId, int userId, int roleId, bool cancel)
        {
            UpdateUserRole(portalId, userId, roleId, RoleStatus.Approved, false, cancel);
        }


     }
}
