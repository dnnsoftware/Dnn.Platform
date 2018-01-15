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

namespace Dnn.PersonaBar.Roles.Services
{
    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class RolesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RolesController));

        #region Role API

        [HttpGet]
        public HttpResponseMessage GetRoles(int groupId, string keyword, int startIndex, int pageSize)
        {
            try
            {
                int total;
                var roles = Components.RolesController.Instance.GetRoles(PortalSettings, groupId, keyword, out total, startIndex, pageSize).Select(RoleDto.FromRoleInfo);
                var loadMore = total > startIndex + pageSize;
                var rsvpLink = Globals.AddHTTP(Globals.GetDomainName(HttpContext.Current.Request)) + "/" + Globals.glbDefaultPage + "?portalid=" + PortalId;
                return Request.CreateResponse(HttpStatusCode.OK, new { roles, loadMore, rsvpLink });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage SaveRole(RoleDto roleDto, [FromUri] bool assignExistUsers)
        {
            try
            {
                Validate(roleDto);
                KeyValuePair<HttpStatusCode, string> message;
                return Components.RolesController.Instance.SaveRole(PortalSettings, roleDto, assignExistUsers, out message)
                    ? Request.CreateResponse(HttpStatusCode.OK, GetRole(roleDto.Id))
                    : Request.CreateErrorResponse(message.Key, message.Value);
            }
            catch (ArgumentException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage DeleteRole(RoleDto roleDto)
        {
            KeyValuePair<HttpStatusCode, string> message;
            var roleName = Components.RolesController.Instance.DeleteRole(PortalSettings, roleDto.Id, out message);
            return !string.IsNullOrEmpty(roleName) ? Request.CreateResponse(HttpStatusCode.OK, new { roleId = roleDto.Id })
                : Request.CreateErrorResponse(message.Key, message.Value);
        }

        #endregion

        #region Role Group API

        [HttpGet]
        public HttpResponseMessage GetRoleGroups(bool reload = false)
        {
            try
            {
                if (reload)
                {
                    DataCache.RemoveCache(string.Format(DataCache.RoleGroupsCacheKey, PortalId));
                }
                var groups = RoleController.GetRoleGroups(PortalId)
                    .Cast<RoleGroupInfo>()
                    .Select(RoleGroupDto.FromRoleGroupInfo);

                return Request.CreateResponse(HttpStatusCode.OK, groups);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage SaveRoleGroup(RoleGroupDto roleGroupDto)
        {
            try
            {
                Validate(roleGroupDto);

                var roleGroup = roleGroupDto.ToRoleGroupInfo();
                roleGroup.PortalID = PortalId;

                if (roleGroup.RoleGroupID < Null.NullInteger)
                {
                    try
                    {
                        RoleController.AddRoleGroup(roleGroup);
                    }
                    catch
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
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
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                            Localization.GetString("DuplicateRoleGroup", Components.Constants.LocalResourcesFile));
                    }
                }

                roleGroup = RoleController.GetRoleGroups(PortalId).Cast<RoleGroupInfo>()
                    .FirstOrDefault(r => r.RoleGroupName == roleGroupDto.Name?.Trim());

                return Request.CreateResponse(HttpStatusCode.OK, RoleGroupDto.FromRoleGroupInfo(roleGroup));
            }
            catch (ArgumentException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage DeleteRoleGroup(RoleGroupDto roleGroupDto)
        {
            var roleGroup = RoleController.GetRoleGroup(PortalId, roleGroupDto.Id);
            if (roleGroup == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    Localization.GetString("RoleGroupNotFound", Components.Constants.LocalResourcesFile));
            }

            RoleController.DeleteRoleGroup(roleGroup);

            return Request.CreateResponse(HttpStatusCode.OK, new { groupId = roleGroupDto.Id });
        }

        #endregion

        #region Role Users API

        [HttpGet]
        public HttpResponseMessage GetSuggestUsers(string keyword, int roleId, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<UserRoleDto>());
                }

                var displayMatch = keyword + "%";
                var totalRecords = 0;
                var totalRecords2 = 0;
                var isAdmin = IsAdmin();

                var matchedUsers = UserController.GetUsersByDisplayName(PortalId, displayMatch, 0, count,
                    ref totalRecords, false, false);
                matchedUsers.AddRange(UserController.GetUsersByUserName(PortalId, displayMatch, 0, count, ref totalRecords2, false, false));
                var finalUsers = matchedUsers
                    .Cast<UserInfo>()
                    .Where(x => isAdmin || !x.Roles.Contains(PortalSettings.AdministratorRoleName))
                    .Select(u => new UserRoleDto()
                    {
                        UserId = u.UserID,
                        DisplayName = $"{u.DisplayName} ({u.Username})"
                    });

                return Request.CreateResponse(HttpStatusCode.OK,
                    finalUsers.ToList().GroupBy(x => x.UserId).Select(group => group.First()));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        public HttpResponseMessage GetRoleUsers(string keyword, int roleId, int pageIndex, int pageSize)
        {
            try
            {
                var role = RoleController.Instance.GetRoleById(PortalId, roleId);
                if (role == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, Localization.GetString("RoleNotFound", Components.Constants.LocalResourcesFile));
                }

                if (role.RoleID == PortalSettings.AdministratorRoleId && !IsAdmin())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
                }

                var users = RoleController.Instance.GetUserRoles(PortalId, Null.NullString, role.RoleName);
                if (!string.IsNullOrEmpty(keyword))
                {
                    users =
                        users.Where(u => u.FullName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                }

                var totalRecords = users.Count;
                var startIndex = pageIndex * pageSize;
                var portal = PortalController.Instance.GetPortal(PortalId);
                var pagedData = users.Skip(startIndex).Take(pageSize).Select(u => new UserRoleDto()
                {
                    UserId = u.UserID,
                    RoleId = u.RoleID,
                    DisplayName = u.FullName,
                    StartTime = u.EffectiveDate,
                    ExpiresTime = u.ExpiryDate,
                    AllowExpired = AllowExpired(u.UserID, u.RoleID),
                    AllowDelete = RoleController.CanRemoveUserFromRole(portal, u.UserID, u.RoleID)
                });

                return Request.CreateResponse(HttpStatusCode.OK, new { users = pagedData, totalRecords });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage AddUserToRole(UserRoleDto userRoleDto, bool notifyUser, bool isOwner)
        {
            try
            {
                Validate(userRoleDto);

                if (!AllowExpired(userRoleDto.UserId, userRoleDto.RoleId))
                {
                    userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
                }
                HttpResponseMessage response;
                var user = GetUser(userRoleDto.UserId, out response);
                if (user == null)
                    return response;

                var role = RoleController.Instance.GetRoleById(PortalId, userRoleDto.RoleId);
                if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
                    isOwner = false;
                if (role.Status != RoleStatus.Approved)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Localization.GetString("CannotAssginUserToUnApprovedRole",
                            Components.Constants.LocalResourcesFile));
                }

                RoleController.AddUserRole(user, role, PortalSettings, RoleStatus.Approved, userRoleDto.StartTime,
                    userRoleDto.ExpiresTime, notifyUser, isOwner);

                var addedUser = RoleController.Instance.GetUserRole(PortalId, userRoleDto.UserId, userRoleDto.RoleId);
                var portal = PortalController.Instance.GetPortal(PortalId);

                return Request.CreateResponse(HttpStatusCode.OK,
                    new UserRoleDto
                    {
                        UserId = addedUser.UserID,
                        RoleId = addedUser.RoleID,
                        DisplayName = addedUser.FullName,
                        StartTime = addedUser.EffectiveDate,
                        ExpiresTime = addedUser.ExpiryDate,
                        AllowExpired = AllowExpired(addedUser.UserID, addedUser.RoleID),
                        AllowDelete = RoleController.CanRemoveUserFromRole(portal, addedUser.UserID, addedUser.RoleID)
                    });
            }
            catch (ArgumentException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = "Edit")]
        public HttpResponseMessage RemoveUserFromRole(UserRoleDto userRoleDto)
        {
            try
            {
                Validate(userRoleDto);
                HttpResponseMessage response;
                var user = GetUser(userRoleDto.UserId, out response);
                if (user == null)
                    return response;

                RoleController.Instance.UpdateUserRole(PortalId, userRoleDto.UserId, userRoleDto.RoleId,
                    RoleStatus.Approved, false, true);

                return Request.CreateResponse(HttpStatusCode.OK, new { userRoleDto.UserId, userRoleDto.RoleId });
            }
            catch (ArgumentException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (SecurityException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private void Validate(RoleDto role)
        {
            Requires.NotNullOrEmpty("Name", role.Name);

            if (!IsAdmin() && role.Id == PortalSettings.AdministratorRoleId)
            {
                throw new SecurityException(Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
            }
        }

        private void Validate(RoleGroupDto role)
        {
            Requires.NotNullOrEmpty("Name", role.Name);
        }

        private void Validate(UserRoleDto userRoleDto)
        {
            Requires.NotNegative("UserId", userRoleDto.UserId);
            Requires.NotNegative("RoleId", userRoleDto.RoleId);

            if (!IsAdmin() && userRoleDto.RoleId == PortalSettings.AdministratorRoleId)
            {
                throw new SecurityException(Localization.GetString("InvalidRequest", Components.Constants.LocalResourcesFile));
            }
        }

        private bool AllowExpired(int userId, int roleId)
        {
            return userId != PortalSettings.AdministratorId || roleId != PortalSettings.AdministratorRoleId;
        }

        private RoleDto GetRole(int roleId)
        {
            return RoleDto.FromRoleInfo(RoleController.Instance.GetRoleById(PortalId, roleId));
        }

        private bool IsAdmin()
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(PortalSettings.AdministratorRoleName);
        }

        private bool IsAdmin(UserInfo user)
        {
            return user.IsSuperUser || user.IsInRole(PortalSettings.AdministratorRoleName);
        }

        private UserInfo GetUser(int userId, out HttpResponseMessage response)
        {
            response = null;
            var user = UserController.Instance.GetUserById(PortalId, userId);
            if (user == null)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    Localization.GetString("UserNotFound", Components.Constants.LocalResourcesFile));
                return null;
            }
            if (!IsAdmin(user)) return user;

            if ((user.IsSuperUser && !UserInfo.IsSuperUser) || !IsAdmin())
            {
                response = Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
                return null;
            }
            if (user.IsSuperUser)
                user = UserController.Instance.GetUserById(Null.NullInteger, userId);
            return user;
        }

        #endregion
    }
}