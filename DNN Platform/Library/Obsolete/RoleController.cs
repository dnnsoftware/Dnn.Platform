// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles;

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Internal.SourceGenerators;

/// <summary>The RoleController class provides Business Layer methods for Roles.</summary>
public partial class RoleController
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 0, 0, "This function has been replaced by AddUserRole with additional params", RemovalVersion = 10)]
    public static partial void AddUserRole(UserInfo user, RoleInfo role, PortalSettings portalSettings, DateTime effectiveDate, DateTime expiryDate, int userId, bool notifyUser)
    {
        AddUserRole(user, role, portalSettings, RoleStatus.Approved, effectiveDate, expiryDate, notifyUser, false);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This function has been replaced by overload with extra parameters", RemovalVersion = 10)]
    public partial void AddUserRole(int portalId, int userId, int roleId, DateTime expiryDate)
    {
        this.AddUserRole(portalId, userId, roleId, RoleStatus.Approved, false, Null.NullDate, expiryDate);
    }

    public void AddUserRole(int portalId, int userId, int roleId, DateTime effectiveDate, DateTime expiryDate)
    {
        this.AddUserRole(portalId, userId, roleId, RoleStatus.Approved, false, effectiveDate, expiryDate);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This function has been replaced by DeleteRole(role)", RemovalVersion = 10)]
    public partial void DeleteRole(int roleId, int portalId)
    {
        RoleInfo role = this.GetRole(portalId, r => r.RoleID == roleId);
        if (role != null)
        {
            this.DeleteRole(role);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This function has been replaced by GetRoles(PortalId, predicate)", RemovalVersion = 10)]
    public partial ArrayList GetPortalRoles(int portalId)
    {
        return new ArrayList(Instance.GetRoles(portalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This method has been replaced by GetRoleById", RemovalVersion = 10)]
    public partial RoleInfo GetRole(int roleId, int portalId)
    {
        return this.GetRoleById(portalId, roleId);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This function has been replaced by GetRoles(PortalId, predicate)", RemovalVersion = 10)]
    public partial ArrayList GetRoles()
    {
        return new ArrayList(Instance.GetRoles(Null.NullInteger, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
    }

    public ArrayList GetRolesByGroup(int portalId, int roleGroupId)
    {
        return new ArrayList(Instance.GetRoles(portalId, r => r.RoleGroupID == roleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved).ToArray());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This method has been replaced by RoleController.Instance.GetUsersByRole(portalId, roleName)", RemovalVersion = 10)]
    public partial ArrayList GetUsersByRoleName(int portalId, string roleName)
    {
        return new ArrayList(Instance.GetUsersByRole(portalId, roleName).ToList());
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This function has been replaced by RoleController.Instance.UpdateRole(role)", RemovalVersion = 10)]
    public partial void UpdateRole(RoleInfo role)
    {
        Instance.UpdateRole(role);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This function has been replaced by overload with extra parameters", RemovalVersion = 10)]
    public partial void UpdateUserRole(int portalId, int userId, int roleId)
    {
        this.UpdateUserRole(portalId, userId, roleId, RoleStatus.Approved, false, false);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "This function has been replaced by overload with extra parameters", RemovalVersion = 10)]
    public partial void UpdateUserRole(int portalId, int userId, int roleId, bool cancel)
    {
        this.UpdateUserRole(portalId, userId, roleId, RoleStatus.Approved, false, cancel);
    }
}
