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

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles.Internal;

#endregion

namespace DotNetNuke.Security.Roles
{
    public abstract class RoleProvider
    {
		#region Shared/Static Methods

        //return the provider
        public static RoleProvider Instance()
        {
            return ComponentFactory.GetComponent<RoleProvider>();
        }
		
		#endregion

        #region Role Methods

        public virtual bool CreateRole(RoleInfo role)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteRole(RoleInfo role)
        {
			throw new NotImplementedException();
        }

        public abstract ArrayList GetRoles(int portalId);

        public abstract IList<RoleInfo> GetRolesBasicSearch(int portalID, int pageSize, string filterBy);

        public virtual IDictionary<string, string> GetRoleSettings(int roleId)
        {
           return new Dictionary<string, string>(); 
        }

        public abstract void UpdateRole(RoleInfo role);

        public virtual void UpdateRoleSettings(RoleInfo role)
        {
            
        }

        #endregion

        #region RoleGroup Methods

        //Role Groups
        public abstract int CreateRoleGroup(RoleGroupInfo roleGroup);

        public abstract void DeleteRoleGroup(RoleGroupInfo roleGroup);

        public abstract RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId);

        public abstract ArrayList GetRoleGroups(int portalId);

        public abstract void UpdateRoleGroup(RoleGroupInfo roleGroup);

        #endregion

        #region UserRole Methods

        public abstract bool AddUserToRole(int portalId, UserInfo user, UserRoleInfo userRole);

        public abstract UserRoleInfo GetUserRole(int PortalId, int UserId, int RoleId);

        public abstract ArrayList GetUserRoles(int portalId, string Username, string Rolename);

        public abstract ArrayList GetUsersByRoleName(int portalId, string roleName);

        public abstract void RemoveUserFromRole(int portalId, UserInfo user, UserRoleInfo userRole);

        public abstract void UpdateUserRole(UserRoleInfo userRole);

        #endregion

        public virtual RoleGroupInfo GetRoleGroupByName(int PortalID, string RoleGroupName)
        {
            return null;
        }

        public virtual IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate)
        {
			throw new NotImplementedException();
        }

        #region Obsolete Methods

        [Obsolete("Deprecated in DotNetNuke 6.2. Replaced by overload which takes a single RoleInfo object")]
        public virtual bool CreateRole(int portalId, ref RoleInfo role)
        {
            return CreateRole(role);
        }

        [Obsolete("Deprecated in DotNetNuke 6.2. Replaced by overload which takes a single RoleInfo object")]
        public virtual void DeleteRole(int portalId, ref RoleInfo role)
        {
            DeleteRole(role);
        }

        [Obsolete("Deprecated in DotNetNuke 6.2. Roles are cached in the business layer")]
        public virtual RoleInfo GetRole(int portalId, int roleId)
        {
            return GetRoles(portalId).Cast<RoleInfo>().SingleOrDefault(r => r.RoleID == roleId);
        }

        [Obsolete("Deprecated in DotNetNuke 6.2. Roles are cached in the business layer")]
        public virtual RoleInfo GetRole(int portalId, string roleName)
        {
            return GetRoles(portalId).Cast<RoleInfo>().SingleOrDefault(r => r.RoleName == roleName);
        }

        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public virtual string[] GetRoleNames(int portalId)
        {
            string[] roles = { };
            var roleList = TestableRoleController.Instance.GetRoles(portalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved);
            var strRoles = roleList.Aggregate("", (current, role) => current + (role.RoleName + "|"));
            if (strRoles.IndexOf("|", StringComparison.Ordinal) > 0)
            {
                roles = strRoles.Substring(0, strRoles.Length - 1).Split('|');
            }
            return roles;
        }

        [Obsolete("Deprecated in DotNetNuke 6.2.")]
        public virtual string[] GetRoleNames(int portalId, int userId)
        {
            return UserController.GetUserById(portalId, userId).Roles;
        }

        [Obsolete("Deprecated in DotNetNuke 6.2. Roles are cached in the business layer")]
        public virtual ArrayList GetRolesByGroup(int portalId, int roleGroupId)
        {
            return new ArrayList(GetRoles(portalId).Cast<RoleInfo>().Where(r => r.RoleGroupID == roleGroupId).ToArray());
        }

        [Obsolete("Deprecated in DotNetNuke 6.2. Replaced by overload that returns IList")]
        public virtual ArrayList GetUserRoles(int portalId, int userId, bool includePrivate)
        {
            UserInfo user;
            if(userId != -1)
            {
                var userController = new UserController();
                user = userController.GetUser(portalId, userId);
            }
            else
            {
                user = new UserInfo() {UserID = -1, PortalID = portalId};
            }

            return new ArrayList(GetUserRoles(user, includePrivate).ToArray());
        }

        [Obsolete("Deprecated in DotNetNuke 6.2. Replaced by GetUserRoles overload that returns IList")]
        public virtual ArrayList GetUserRolesByRoleName(int portalId, string roleName)
        {
            return GetUserRoles(portalId, null, roleName);
        }

        #endregion
    }
}