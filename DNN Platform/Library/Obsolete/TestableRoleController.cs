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
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;

namespace DotNetNuke.Security.Roles.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use RoleController instead")]
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
