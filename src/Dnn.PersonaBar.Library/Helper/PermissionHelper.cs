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
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.DTO;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Library.Helper
{
    #region Permission Extension Class

    public static class PermissionHelper
    {
        public static void AddUserPermission(this DTO.Permissions dto, PermissionInfoBase permissionInfo)
        {
            var userPermission = dto.UserPermissions.FirstOrDefault(p => p.UserId == permissionInfo.UserID);
            if (userPermission == null)
            {
                userPermission = new UserPermission
                                    {
                                        UserId = permissionInfo.UserID,
                                        DisplayName = permissionInfo.DisplayName
                                    };
                dto.UserPermissions.Add(userPermission);
            }

            if (userPermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionID))
            {
                userPermission.Permissions.Add(new Permission
                                                    {
                                                        PermissionId = permissionInfo.PermissionID,
                                                        PermissionName = permissionInfo.PermissionName,
                                                        AllowAccess = permissionInfo.AllowAccess
                                                    });
            }
        }

        public static void AddRolePermission(this DTO.Permissions dto, PermissionInfoBase permissionInfo)
        {
            var rolePermission = dto.RolePermissions.FirstOrDefault(p => p.RoleId == permissionInfo.RoleID);
            if (rolePermission == null)
            {
                rolePermission = new RolePermission
                                    {
                                        RoleId = permissionInfo.RoleID,
                                        RoleName = permissionInfo.RoleName
                                    };
                dto.RolePermissions.Add(rolePermission);
            }

            if (rolePermission.Permissions.All(p => p.PermissionId != permissionInfo.PermissionID))
            {
                rolePermission.Permissions.Add(new Permission
                                                    {
                                                        PermissionId = permissionInfo.PermissionID,
                                                        PermissionName = permissionInfo.PermissionName,
                                                        AllowAccess = permissionInfo.AllowAccess
                                                    });
            }
        }

        public static void EnsureDefaultRoles(this DTO.Permissions dto)
        {
            //Administrators Role always has implicit permissions, then it should be always in
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.AdministratorRoleId), true, true);
            
            //Show also default roles
            dto.EnsureRole(RoleController.Instance.GetRoleById(PortalSettings.Current.PortalId, PortalSettings.Current.RegisteredRoleId), false, true);
            dto.EnsureRole(new RoleInfo { RoleID = Int32.Parse(Globals.glbRoleAllUsers), RoleName = Globals.glbRoleAllUsersName }, false, true);
        }

        public static void EnsureRole(this DTO.Permissions dto, RoleInfo role)
        {
            dto.EnsureRole(role, false);
        }

        public static void EnsureRole(this DTO.Permissions dto, RoleInfo role, bool locked)
        {
            dto.EnsureRole(role, locked, false);
        }

        public static void EnsureRole(this DTO.Permissions dto, RoleInfo role, bool locked, bool isDefault)
        {
            if (dto.RolePermissions.All(r => r.RoleId != role.RoleID))
            {
                dto.RolePermissions.Add(new RolePermission
                                            {
                                                RoleId = role.RoleID,
                                                RoleName = role.RoleName,
                                                Locked = locked,
                                                IsDefault = isDefault
                                            });
            }
        }

        public static bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        public static bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "VIEW");
        }

        public static object GetRoles(int portalId)
        {
            var data = new { Groups = new List<object>(), Roles = new List<object>() };

            //retreive role groups info
            data.Groups.Add(new { GroupId = -2, Name = "AllRoles" });
            data.Groups.Add(new { GroupId = -1, Name = "GlobalRoles", Selected = true });

            foreach (RoleGroupInfo group in RoleController.GetRoleGroups(portalId))
            {
                data.Groups.Add(new { GroupId = group.RoleGroupID, Name = group.RoleGroupName });
            }

            //retreive roles info
            data.Roles.Add(new { RoleID = Int32.Parse(Globals.glbRoleUnauthUser), GroupId = -1, RoleName = Globals.glbRoleUnauthUserName });
            data.Roles.Add(new { RoleID = Int32.Parse(Globals.glbRoleAllUsers), GroupId = -1, RoleName = Globals.glbRoleAllUsersName });
            foreach (RoleInfo role in RoleController.Instance.GetRoles(portalId).OrderBy(r => r.RoleName))
            {
                data.Roles.Add(new { GroupId = role.RoleGroupID, RoleId = role.RoleID, Name = role.RoleName });
            }

            return data;
        }
    }

    #endregion

}