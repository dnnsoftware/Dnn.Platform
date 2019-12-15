// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
    }
}
