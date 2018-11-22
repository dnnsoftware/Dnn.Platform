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

#region Usings

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

#endregion

namespace Dnn.PersonaBar.Roles.Components
{
    public class RolesController : ServiceLocator<IRolesController, RolesController>, IRolesController
    {
        protected override Func<IRolesController> GetFactory()
        {
            return () => new RolesController();
        }

        #region Public Methods
        /// <summary>
        /// Gets a paginated list of Roles matching given search criteria
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
            var isAdmin = IsAdmin(portalSettings);
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
        /// <returns>List of found Roles</returns>
        public IList<RoleInfo> GetRolesByNames(PortalSettings portalSettings, int groupId, IList<string> rolesFilter)
        {
            var isAdmin = IsAdmin(portalSettings);

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
            var isAdmin = IsAdmin(portalSettings);
            var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
            if (!isAdmin && role.RoleID == portalSettings.AdministratorRoleId)
                return null;
            return role;
        }

        public bool SaveRole(PortalSettings portalSettings, RoleDto roleDto, bool assignExistUsers, out KeyValuePair<HttpStatusCode, string> message)
        {
            message = new KeyValuePair<HttpStatusCode, string>();
            if (!IsAdmin(portalSettings) && roleDto.Id == portalSettings.AdministratorRoleId)
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

            if (role.RoleID == portalSettings.AdministratorRoleId && !IsAdmin(portalSettings))
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest,
                    Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return string.Empty;
            }

            RoleController.Instance.DeleteRole(role);
            DataCache.RemoveCache("GetRoles");
            return role.RoleName;
        }

        #endregion

        #region Private Methods
        private bool IsAdmin(PortalSettings portalSettings)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }

        #endregion
    }
}