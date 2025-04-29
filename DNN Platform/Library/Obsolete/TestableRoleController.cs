// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles.Internal;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;

[EditorBrowsable(EditorBrowsableState.Never)]
[DnnDeprecated(7, 3, 0, "Please use RoleController instead", RemovalVersion = 10)]
public partial class TestableRoleController : ServiceLocator<IRoleController, TestableRoleController>, IRoleController
{
    /// <inheritdoc/>
    public int AddRole(RoleInfo role)
    {
        return RoleController.Instance.AddRole(role);
    }

    /// <inheritdoc/>
    public int AddRole(RoleInfo role, bool addToExistUsers)
    {
        return RoleController.Instance.AddRole(role, addToExistUsers);
    }

    /// <inheritdoc/>
    public void DeleteRole(RoleInfo role)
    {
        RoleController.Instance.DeleteRole(role);
    }

    /// <inheritdoc/>
    public RoleInfo GetRole(int portalId, Func<RoleInfo, bool> predicate)
    {
        return RoleController.Instance.GetRole(portalId, predicate);
    }

    /// <inheritdoc/>
    public IList<RoleInfo> GetRoles(int portalId)
    {
        return RoleController.Instance.GetRoles(portalId);
    }

    /// <inheritdoc/>
    public IList<RoleInfo> GetRolesBasicSearch(int portalID, int pageSize, string filterBy)
    {
        return RoleController.Instance.GetRolesBasicSearch(portalID, pageSize, filterBy);
    }

    /// <inheritdoc/>
    public IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate)
    {
        return RoleController.Instance.GetRoles(portalId, predicate);
    }

    /// <inheritdoc/>
    public IDictionary<string, string> GetRoleSettings(int roleId)
    {
        return RoleController.Instance.GetRoleSettings(roleId);
    }

    /// <inheritdoc/>
    public void UpdateRole(RoleInfo role)
    {
        RoleController.Instance.UpdateRole(role);
    }

    /// <inheritdoc/>
    public void UpdateRole(RoleInfo role, bool addToExistUsers)
    {
        RoleController.Instance.UpdateRole(role, addToExistUsers);
    }

    /// <inheritdoc/>
    public void UpdateRoleSettings(RoleInfo role, bool clearCache)
    {
        RoleController.Instance.UpdateRoleSettings(role, clearCache);
    }

    /// <inheritdoc/>
    public void ClearRoleCache(int portalId)
    {
        RoleController.Instance.ClearRoleCache(portalId);
    }

    /// <inheritdoc/>
    protected override Func<IRoleController> GetFactory()
    {
        return () => new TestableRoleController();
    }
}
