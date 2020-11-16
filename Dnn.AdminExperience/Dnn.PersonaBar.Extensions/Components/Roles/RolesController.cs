// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Roles.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Roles.Services.DTO;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;

    public class RolesController : ServiceLocator<IRolesController, RolesController>, IRolesController
    {
        /// <summary>
        /// Gets a paginated list of Roles matching given search criteria.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="groupId"></param>
        /// <param name="keyword"></param>
        /// <param name="total"></param>
        /// <param name="startIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<RoleInfo> GetRoles(PortalSettings portalSettings, int groupId, string keyword, out int total, int startIndex, int pageSize)
        {
            var isAdmin = this.IsAdmin(portalSettings);
            var roles = (groupId < Null.NullInteger
                ? RoleController.Instance.GetRoles(portalSettings.PortalId)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == groupId))
                .Where(r => isAdmin || r.RoleID != portalSettings.AdministratorRoleId);
            if (!string.IsNullOrEmpty(keyword))
            {
                roles = roles.Where(r => r.RoleName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) > Null.NullInteger);
            }

            var roleInfos = roles as IList<RoleInfo> ?? roles.ToList();
            total = roleInfos.Count;
            return roleInfos.Skip(startIndex).Take(pageSize);
        }

        /// <summary>
        /// Gets a list (not paginated) of Roles given a comma separated list of Roles' names.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="groupId"></param>
        /// <param name="rolesFilter"></param>
        /// <returns>List of found Roles.</returns>
        public IList<RoleInfo> GetRolesByNames(PortalSettings portalSettings, int groupId, IList<string> rolesFilter)
        {
            var isAdmin = this.IsAdmin(portalSettings);

            List<RoleInfo> foundRoles = null;
            if (rolesFilter.Count() > 0)
            {
                var allRoles = (groupId < Null.NullInteger
                ? RoleController.Instance.GetRoles(portalSettings.PortalId)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == groupId));

                foundRoles = allRoles.Where(r =>
                {
                    bool adminCheck = isAdmin || r.RoleID != portalSettings.AdministratorRoleId;
                    return adminCheck && rolesFilter.Contains(r.RoleName);
                }).ToList();

            }

            return foundRoles;
        }

        public RoleInfo GetRole(PortalSettings portalSettings, int roleId)
        {
            var isAdmin = this.IsAdmin(portalSettings);
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
            if (!isAdmin && role.RoleID == portalSettings.AdministratorRoleId)
                return null;
            return role;
        }

        public bool SaveRole(PortalSettings portalSettings, RoleDto roleDto, bool assignExistUsers, out KeyValuePair<HttpStatusCode, string> message)
        {
            message = new KeyValuePair<HttpStatusCode, string>();
            if (!this.IsAdmin(portalSettings) && roleDto.Id == portalSettings.AdministratorRoleId)
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return false;
            }
            var role = roleDto.ToRoleInfo();
            role.PortalID = portalSettings.PortalId;
            var rolename = role.RoleName.ToUpperInvariant();

            if (roleDto.Id == Null.NullInteger)
            {
                if (RoleController.Instance.GetRole(portalSettings.PortalId, r => rolename.Equals(r.RoleName, StringComparison.OrdinalIgnoreCase)) == null)
                {
                    RoleController.Instance.AddRole(role, assignExistUsers);
                    roleDto.Id = role.RoleID;
                }
                else
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("DuplicateRole", Constants.LocalResourcesFile));
                    return false;
                }
            }
            else
            {
                var existingRole = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleDto.Id);
                if (existingRole == null)
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("RoleNotFound", Constants.LocalResourcesFile));
                    return false;
                }

                if (existingRole.IsSystemRole)
                {
                    if (role.Description != existingRole.Description)//In System roles only description can be updated.
                    {
                        existingRole.Description = role.Description;
                        RoleController.Instance.UpdateRole(existingRole, assignExistUsers);
                    }
                }
                else if (RoleController.Instance.GetRole(portalSettings.PortalId, r => rolename.Equals(r.RoleName, StringComparison.OrdinalIgnoreCase) && r.RoleID != roleDto.Id) == null)
                {
                    existingRole.RoleName = role.RoleName;
                    existingRole.Description = role.Description;
                    existingRole.RoleGroupID = role.RoleGroupID;
                    existingRole.SecurityMode = role.SecurityMode;
                    existingRole.Status = role.Status;
                    existingRole.IsPublic = role.IsPublic;
                    existingRole.AutoAssignment = role.AutoAssignment;
                    existingRole.RSVPCode = role.RSVPCode;
                    RoleController.Instance.UpdateRole(existingRole, assignExistUsers);
                }
                else
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("DuplicateRole", Constants.LocalResourcesFile));
                    return false;
                }
            }
            return true;
        }

        public string DeleteRole(PortalSettings portalSettings, int roleId, out KeyValuePair<HttpStatusCode, string> message)
        {
            message = new KeyValuePair<HttpStatusCode, string>();
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
            if (role == null)
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound,
                    Localization.GetString("RoleNotFound", Constants.LocalResourcesFile));
                return string.Empty;
            }
            if (role.IsSystemRole)
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest,
                   Localization.GetString("SecurityRoleDeleteNotAllowed", Constants.LocalResourcesFile));
                return string.Empty;
            }

            if (role.RoleID == portalSettings.AdministratorRoleId && !this.IsAdmin(portalSettings))
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest,
                    Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return string.Empty;
            }

            RoleController.Instance.DeleteRole(role);
            DataCache.RemoveCache("GetRoles");
            return role.RoleName;
        }

        protected override Func<IRolesController> GetFactory()
        {
            return () => new RolesController();
        }

        private bool IsAdmin(PortalSettings portalSettings)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }
    }
}
