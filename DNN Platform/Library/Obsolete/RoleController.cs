#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by AddRole(role)")]
        public int AddRole(RoleInfo role, bool synchronizationMode)
        {
            role.SecurityMode = SecurityMode.SecurityRole;
            role.Status = RoleStatus.Approved;
            return RoleController.Instance.AddRole(role);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.0. This function has been replaced by AddUserRole with additional params")]
        public static void AddUserRole(UserInfo user, RoleInfo role, PortalSettings portalSettings, DateTime effectiveDate, DateTime expiryDate, int userId, bool notifyUser)
        {
            AddUserRole(user, role, portalSettings, RoleStatus.Approved, effectiveDate, expiryDate, notifyUser, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by overload with extra parameters")]
        public void AddUserRole(int portalId, int userId, int roleId, DateTime expiryDate)
        {
            AddUserRole(portalId, userId, roleId, RoleStatus.Approved, false, Null.NullDate, expiryDate);
        }

        public void AddUserRole(int portalId, int userId, int roleId, DateTime effectiveDate, DateTime expiryDate)
        {
            AddUserRole(portalId, userId, roleId, RoleStatus.Approved, false, effectiveDate, expiryDate);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by DeleteRole(role)")]
        public void DeleteRole(int roleId, int portalId)
        {
            RoleInfo role = GetRole(portalId, r => r.RoleID == roleId);
            if (role != null)
            {
                DeleteRole(role);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public static bool DeleteUserRole(int userId, RoleInfo role, PortalSettings portalSettings, bool notifyUser)
        {
            UserInfo objUser = UserController.GetUserById(portalSettings.PortalId, userId);
            return DeleteUserRole(objUser, role, portalSettings, notifyUser);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public static bool DeleteUserRole(int roleId, UserInfo user, PortalSettings portalSettings, bool notifyUser)
        {
            RoleInfo role = RoleController.Instance.GetRole(portalSettings.PortalId, r => r.RoleID == roleId);
            return DeleteUserRole(user, role, portalSettings, notifyUser);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public bool DeleteUserRole(int portalId, int userId, int roleId)
        {
            return DeleteUserRoleInternal(portalId, userId, roleId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by GetRoles(PortalId, predicate)")]
        public ArrayList GetPortalRoles(int portalId, bool synchronizeRoles)
        {
            return new ArrayList(Instance.GetRoles(portalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by GetRoles(PortalId, predicate)")]
        public ArrayList GetPortalRoles(int portalId)
        {
            return new ArrayList(Instance.GetRoles(portalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by GetRolesByUser")]
        public string[] GetPortalRolesByUser(int UserId, int PortalId)
        {
            return GetRolesByUser(UserId, PortalId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This method has been replacd by GetRoleById")]
        public RoleInfo GetRole(int roleId, int portalId)
        {
            return GetRoleById(portalId, roleId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public string[] GetRoleNames(int portalId)
        {
            string[] roles = { };
            var roleList = RoleController.Instance.GetRoles(portalId, role => role.SecurityMode != SecurityMode.SocialGroup && role.Status == RoleStatus.Approved);
            var strRoles = roleList.Aggregate("", (current, role) => current + (role.RoleName + "|"));
            if (strRoles.IndexOf("|", StringComparison.Ordinal) > 0)
            {
                roles = strRoles.Substring(0, strRoles.Length - 1).Split('|');
            }
            return roles;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by GetRoles(PortalId, predicate)")]
        public ArrayList GetRoles()
        {
            return new ArrayList(Instance.GetRoles(Null.NullInteger, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
        }

        public ArrayList GetRolesByGroup(int portalId, int roleGroupId)
        {
            return new ArrayList(Instance.GetRoles(portalId, r => r.RoleGroupID == roleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public string[] GetRolesByUser(int UserId, int PortalId)
        {
            if(UserId == -1)
            {
                return new string[0];
            }
            return UserController.GetUserById(PortalId, UserId).Roles;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by GetUserRoles")]
        public ArrayList GetServices(int PortalId)
        {
            return GetServices(PortalId, -1);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by GetUserRoles")]
        public ArrayList GetServices(int PortalId, int UserId)
        {
            return provider.GetUserRoles(PortalId, UserId, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public ArrayList GetUserRoles(int PortalId)
        {
            return GetUserRoles(PortalId, -1);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2. Replaced by overload that returns IList")]
        public ArrayList GetUserRoles(int PortalId, int UserId)
        {
            return provider.GetUserRoles(PortalId, UserId, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2. Replaced by overload that returns IList")]
        public ArrayList GetUserRoles(int PortalId, int UserId, bool includePrivate)
        {
            return provider.GetUserRoles(PortalId, UserId, includePrivate);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2. Replaced by overload of GetUserRoles that returns IList")]
        public ArrayList GetUserRolesByUsername(int PortalID, string Username, string Rolename)
        {
            return provider.GetUserRoles(PortalID, Username, Rolename);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public ArrayList GetUserRolesByRoleName(int portalId, string roleName)
        {
            return provider.GetUserRoles(portalId, null, roleName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This method has been replaced by RoleController.Instance.GetUsersByRole(portalId, roleName)")]
        public ArrayList GetUsersByRoleName(int portalId, string roleName)
        {
            return new ArrayList(Instance.GetUsersByRole(portalId, roleName).ToList());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by GetUserRolesByRole")]
        public ArrayList GetUsersInRole(int PortalID, string RoleName)
        {
            return provider.GetUserRolesByRoleName(PortalID, RoleName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public static void SerializeRoles(XmlWriter writer, int portalId)
        {
            //Serialize Global Roles
            writer.WriteStartElement("roles");
            foreach (RoleInfo role in RoleController.Instance.GetRoles(portalId, r => r.RoleGroupID == Null.NullInteger))
            {
                CBO.SerializeObject(role, writer);
            }
            writer.WriteEndElement();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by RoleController.Instance.UpdateRole(role)")]
        public void UpdateRole(RoleInfo role)
        {
            Instance.UpdateRole(role);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by UpdateUserRole")]
        public void UpdateService(int PortalId, int UserId, int RoleId)
        {
            UpdateUserRole(PortalId, UserId, RoleId, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by UpdateUserRole")]
        public void UpdateService(int PortalId, int UserId, int RoleId, bool Cancel)
        {
            UpdateUserRole(PortalId, UserId, RoleId, Cancel);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by overload with extra parameters")]
        public void UpdateUserRole(int portalId, int userId, int roleId)
        {
            UpdateUserRole(portalId, userId, roleId, RoleStatus.Approved, false, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. This function has been replaced by overload with extra parameters")]
        public void UpdateUserRole(int portalId, int userId, int roleId, bool cancel)
        {
            UpdateUserRole(portalId, userId, roleId, RoleStatus.Approved, false, cancel);
        }


     }
}
