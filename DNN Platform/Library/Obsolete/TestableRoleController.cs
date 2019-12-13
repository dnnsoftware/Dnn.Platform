﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;

namespace DotNetNuke.Security.Roles.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use RoleController instead. Scheduled removal in v10.0.0.")]
    public class TestableRoleController : ServiceLocator<IRoleController, TestableRoleController>, IRoleController
    {
        protected override Func<IRoleController> GetFactory()
        {
            return () => new TestableRoleController();
        }

        public int AddRole(RoleInfo role)
        {
            return RoleController.Instance.AddRole(role);
        }

        public int AddRole(RoleInfo role, bool addToExistUsers)
        {
            return RoleController.Instance.AddRole(role, addToExistUsers);
        }

        public void DeleteRole(RoleInfo role)
        {
            RoleController.Instance.DeleteRole(role);
        }

        public RoleInfo GetRole(int portalId, Func<RoleInfo, bool> predicate)
        {
            return RoleController.Instance.GetRole(portalId, predicate);
        }

        public IList<RoleInfo> GetRoles(int portalId)
        {
            return RoleController.Instance.GetRoles(portalId);
        }

        public IList<RoleInfo> GetRolesBasicSearch(int portalID, int pageSize, string filterBy)
        {
            return RoleController.Instance.GetRolesBasicSearch(portalID, pageSize, filterBy);
        }

        public IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate)
        {
            return RoleController.Instance.GetRoles(portalId, predicate);
        }

        public IDictionary<string, string> GetRoleSettings(int roleId)
        {
            return RoleController.Instance.GetRoleSettings(roleId);
        }

        public void UpdateRole(RoleInfo role)
        {
            RoleController.Instance.UpdateRole(role);
        }

        public void UpdateRole(RoleInfo role, bool addToExistUsers)
        {
            RoleController.Instance.UpdateRole(role, addToExistUsers);
        }

        public void UpdateRoleSettings(RoleInfo role, bool clearCache)
        {
            RoleController.Instance.UpdateRoleSettings(role, clearCache);
        }

        public void ClearRoleCache(int portalId)
        {
            RoleController.Instance.ClearRoleCache(portalId);
        }
    }
}
