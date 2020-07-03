// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security;
    using System.Web;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Roles.Services.DTO;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class RolesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RolesController));

        [HttpGet]
        public HttpResponseMessage GetRoles(int groupId, string keyword, int startIndex, int pageSize)
        {
            try
            {
                int total;
                var roles = Components.RolesController.Instance.GetRoles(this.PortalSettings, groupId, keyword, out total, startIndex, pageSize).Select(RoleDto.FromRoleInfo);
                var loadMore = total > startIndex + pageSize;
                var rsvpLink = Globals.AddHTTP(Globals.GetDomainName(HttpContext.Current.Request)) + "/" + Globals.glbDefaultPage + "?portalid=" + this.PortalId;
                return this.Request.CreateResponse(HttpStatusCode.OK, new { roles, loadMore, rsvpLink });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage SaveRole(RoleDto roleDto, [FromUri] bool assignExistUsers)
        {
            try
            {
                this.Validate(roleDto);
                KeyValuePair<HttpStatusCode, string> message;
                return Components.RolesController.Instance.SaveRole(this.PortalSettings, roleDto, assignExistUsers, out message)
                    ? this.Request.CreateResponse(HttpStatusCode.OK, this.GetRole(roleDto.Id))
                    : this.Request.CreateErrorResponse(message.Key, message.Value);
            }
            catch (ArgumentException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage DeleteRole(RoleDto roleDto)
        {
            KeyValuePair<HttpStatusCode, string> message;
            var roleName = Components.RolesController.Instance.DeleteRole(this.PortalSettings, roleDto.Id, out message);
            return !string.IsNullOrEmpty(roleName) ? this.Request.CreateResponse(HttpStatusCode.OK, new { roleId = roleDto.Id })
                : this.Request.CreateErrorResponse(message.Key, message.Value);
        }

        [HttpGet]
        public HttpResponseMessage GetRoleGroups(bool reload = false)
        {
            try
            {
                if (reload)
                {
                    DataCache.RemoveCache(string.Format(DataCache.RoleGroupsCacheKey, this.PortalId));
                }
                var groups = RoleController.GetRoleGroups(this.PortalId)
                    .Cast<RoleGroupInfo>()
                    .Select(RoleGroupDto.FromRoleGroupInfo);

                return this.Request.CreateResponse(HttpStatusCode.OK, groups);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage SaveRoleGroup(RoleGroupDto roleGroupDto)
        {
            try
            {
                this.Validate(roleGroupDto);

                var roleGroup = roleGroupDto.ToRoleGroupInfo();
                roleGroup.PortalID = this.PortalId;

                if (roleGroup.RoleGroupID < Null.NullInteger)
                {
                    try
                    {
                        RoleController.AddRoleGroup(roleGroup);
                    }
                    catch
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            Localization.GetString("DuplicateRoleGroup", Components.Constants.LocalResourcesFile));
                    }
                }
                else
                {
                    try
                    {
                        RoleController.UpdateRoleGroup(roleGroup);
                    }
                    catch
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            Localization.GetString("DuplicateRoleGroup", Components.Constants.LocalResourcesFile));
                    }
                }

                roleGroup = RoleController.GetRoleGroups(this.PortalId).Cast<RoleGroupInfo>()
                    .FirstOrDefault(r => r.RoleGroupName == roleGroupDto.Name?.Trim());

                return this.Request.CreateResponse(HttpStatusCode.OK, RoleGroupDto.FromRoleGroupInfo(roleGroup));
            }
            catch (ArgumentException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage DeleteRoleGroup(RoleGroupDto roleGroupDto)
        {
            var roleGroup = RoleController.GetRoleGroup(this.PortalId, roleGroupDto.Id);
            if (roleGroup == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    Localization.GetString("RoleGroupNotFound", Components.Constants.LocalResourcesFile));
            }

            RoleController.DeleteRoleGroup(roleGroup);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { groupId = roleGroupDto.Id });
        }

        [HttpGet]
        public HttpResponseMessage GetSuggestUsers(string keyword, int roleId, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new List<UserRoleDto>());
                }

                var displayMatch = keyword + "%";
                var totalRecords = 0;
                var totalRecords2 = 0;
                var isAdmin = this.IsAdmin();

                var matchedUsers = UserController.GetUsersByDisplayName(this.PortalId, displayMatch, 0, count,
                    ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(this.PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
                var finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => isAdmin || !x.Roles.Contains(this.PortalSettings.AdministratorRoleName))
                    .Select(u => new UserRoleDto()
                    {
                        UserId = u.UserID,
                        DisplayName = $"{u.DisplayName} ({u.Username})"
                    });

                return this.Request.CreateResponse(HttpStatusCode.OK,
                    finalUsers.ToList().GroupBy(x => x.UserId).Select(group => group.First()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        public HttpResponseMessage GetRoleUsers(string keyword, int roleId, int pageIndex, int pageSize)
        {
            try
            {
                var role = RoleController.Instance.GetRoleById(this.PortalId, roleId);
                if (role == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, Localization.GetString("RoleNotFound", Components.Constants.LocalResourcesFile));
                }

                if (role.RoleID == this.PortalSettings.AdministratorRoleId && !this.IsAdmin())
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
                }

                var users = RoleController.Instance.GetUserRoles(this.PortalId, Null.NullString, role.RoleName);
                if (!string.IsNullOrEmpty(keyword))
                {
                    users =
                        users.Where(u => u.FullName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                }

                var totalRecords = users.Count;
                var startIndex = pageIndex * pageSize;
                var portal = PortalController.Instance.GetPortal(this.PortalId);
                var pagedData = users.Skip(startIndex).Take(pageSize).Select(u => new UserRoleDto()
                {
                    UserId = u.UserID,
                    RoleId = u.RoleID,
                    DisplayName = u.FullName,
                    StartTime = u.EffectiveDate,
                    ExpiresTime = u.ExpiryDate,
                    AllowExpired = this.AllowExpired(u.UserID, u.RoleID),
                    AllowDelete = RoleController.CanRemoveUserFromRole(portal, u.UserID, u.RoleID)
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, new { users = pagedData, totalRecords });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage AddUserToRole(UserRoleDto userRoleDto, bool notifyUser, bool isOwner)
        {
            try
            {
                this.Validate(userRoleDto);

                if (!this.AllowExpired(userRoleDto.UserId, userRoleDto.RoleId))
                {
                    userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
                }
                HttpResponseMessage response;
                var user = this.GetUser(userRoleDto.UserId, out response);
                if (user == null)
                    return response;

                var role = RoleController.Instance.GetRoleById(this.PortalId, userRoleDto.RoleId);
                if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
                    isOwner = false;
                if (role.Status != RoleStatus.Approved)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Localization.GetString("CannotAssginUserToUnApprovedRole",
                            Components.Constants.LocalResourcesFile));
                }

                RoleController.AddUserRole(user, role, this.PortalSettings, RoleStatus.Approved, userRoleDto.StartTime,
                    userRoleDto.ExpiresTime, notifyUser, isOwner);

                var addedUser = RoleController.Instance.GetUserRole(this.PortalId, userRoleDto.UserId, userRoleDto.RoleId);
                var portal = PortalController.Instance.GetPortal(this.PortalId);

                return this.Request.CreateResponse(HttpStatusCode.OK,
                    new UserRoleDto
                    {
                        UserId = addedUser.UserID,
                        RoleId = addedUser.RoleID,
                        DisplayName = addedUser.FullName,
                        StartTime = addedUser.EffectiveDate,
                        ExpiresTime = addedUser.ExpiryDate,
                        AllowExpired = this.AllowExpired(addedUser.UserID, addedUser.RoleID),
                        AllowDelete = RoleController.CanRemoveUserFromRole(portal, addedUser.UserID, addedUser.RoleID)
                    });
            }
            catch (ArgumentException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage RemoveUserFromRole(UserRoleDto userRoleDto)
        {
            try
            {
                this.Validate(userRoleDto);
                HttpResponseMessage response;
                var user = this.GetUser(userRoleDto.UserId, out response);
                if (user == null)
                    return response;

                RoleController.Instance.UpdateUserRole(this.PortalId, userRoleDto.UserId, userRoleDto.RoleId,
                    RoleStatus.Approved, false, true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { userRoleDto.UserId, userRoleDto.RoleId });
            }
            catch (ArgumentException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private void Validate(RoleDto role)
        {
            Requires.NotNullOrEmpty("Name", role.Name);

            if (!this.IsAdmin() && role.Id == this.PortalSettings.AdministratorRoleId)
            {
                throw new SecurityException(Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
            }
        }

        private void Validate(RoleGroupDto role)
        {
            Requires.NotNullOrHasNoWhiteSpace("Name", role.Name);
        }

        private void Validate(UserRoleDto userRoleDto)
        {
            Requires.NotNegative("UserId", userRoleDto.UserId);
            Requires.NotNegative("RoleId", userRoleDto.RoleId);

            if (!this.IsAdmin() && userRoleDto.RoleId == this.PortalSettings.AdministratorRoleId)
            {
                throw new SecurityException(Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
            }
        }

        private bool AllowExpired(int userId, int roleId)
        {
            return userId != this.PortalSettings.AdministratorId || roleId != this.PortalSettings.AdministratorRoleId;
        }

        private RoleDto GetRole(int roleId)
        {
            return RoleDto.FromRoleInfo(RoleController.Instance.GetRoleById(this.PortalId, roleId));
        }

        private bool IsAdmin()
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(this.PortalSettings.AdministratorRoleName);
        }

        private bool IsAdmin(UserInfo user)
        {
            return user.IsSuperUser || user.IsInRole(this.PortalSettings.AdministratorRoleName);
        }

        private UserInfo GetUser(int userId, out HttpResponseMessage response)
        {
            response = null;
            var user = UserController.Instance.GetUserById(this.PortalId, userId);
            if (user == null)
            {
                response = this.Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    Localization.GetString("UserNotFound", Components.Constants.LocalResourcesFile));
                return null;
            }
            if (!this.IsAdmin(user)) return user;

            if ((user.IsSuperUser && !this.UserInfo.IsSuperUser) || !this.IsAdmin())
            {
                response = this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
                return null;
            }
            if (user.IsSuperUser)
                user = UserController.Instance.GetUserById(Null.NullInteger, userId);
            return user;
        }
    }
}
