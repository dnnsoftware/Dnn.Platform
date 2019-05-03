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
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.IO;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Users.Services
{
    [MenuPermission(MenuName = "Dnn.Users")]
    public class UsersController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UsersController));

        #region Users API

        /// <summary>
        /// Create a User.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AddUser)]
        public HttpResponseMessage CreateUser(CreateUserContract contract)
        {
            try
            {
                var settings = new RegisterationDetails
                {
                    PortalSettings = PortalSettings,
                    Email = contract.Email,
                    FirstName = contract.FirstName,
                    LastName = contract.LastName,
                    UserName = contract.UserName,
                    Password = contract.Password,
                    Question = contract.Question,
                    Answer = contract.Answer,
                    Notify = contract.Notify,
                    Authorize = contract.Authorize,
                    RandomPassword = contract.RandomPassword,
                    IgnoreRegistrationMode = true
                };
                var userInfo = RegisterController.Instance.Register(settings);
                return Request.CreateResponse(HttpStatusCode.OK, userInfo != null
                    ? UserBasicDto.FromUserDetails(Components.UsersController.Instance.GetUserDetail(PortalId,
                        userInfo.UserId))
                    : null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Perform a search on Users registered in the Site.
        /// </summary>
        /// <param name="searchText">Search filter text (if any)</param>
        /// <param name="filter">User filter. Send -1 to disable.</param>
        /// <param name="pageIndex">Page index to begin from (0, 1, 2)</param>
        /// <param name="pageSize">Number of records to return per page</param>
        /// <param name="sortColumn">Column to sort on</param>
        /// <param name="sortAscending">Sort ascending or descending</param>
        [HttpGet]
        public HttpResponseMessage GetUsers(string searchText, UserFilters filter, int pageIndex, int pageSize,
            string sortColumn,
            bool sortAscending)
        {
            try
            {
                int totalRecords;
                var getUsersContract = new GetUsersContract
                {
                    SearchText = searchText,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    SortColumn = sortColumn,
                    SortAscending = sortAscending,
                    PortalId = PortalController.GetEffectivePortalId(PortalId),
                    Filter = filter
                };

                var results = Components.UsersController.Instance.GetUsers(getUsersContract, UserInfo.IsSuperUser,
                    out totalRecords);
                var response = new
                {
                    Results = results,
                    TotalResults = totalRecords
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUserFilters()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    Components.UsersController.Instance.GetUserFilters(UserInfo.IsSuperUser));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Get User Detail Info.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetUserDetail(int userId)
        {
            try
            {
                var userDetail = Components.UsersController.Instance.GetUserDetail(PortalId, userId);
                if (userDetail == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                        Localization.GetString("UserNotFound", Components.Constants.LocalResourcesFile));
                }
                if (userDetail.IsSuperUser)
                {
                    if (!UserInfo.IsSuperUser)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                            Localization.GetString("InSufficientPermissions.Text", Components.Constants.LocalResourcesFile));
                    }
                    userDetail = Components.UsersController.Instance.GetUserDetail(Null.NullInteger, userId);
                }

                return Request.CreateResponse(HttpStatusCode.OK, userDetail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManagePassword)]
        public HttpResponseMessage ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = changePasswordDto.UserId;
                var password = changePasswordDto.Password;
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);

                var controller = Components.UsersController.Instance;
                controller.ChangePassword(PortalId, userId, password);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManagePassword)]
        public HttpResponseMessage ForceChangePassword([FromUri] int userId)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);
                HttpResponseMessage httpResponseMessage;
                if (IsCurrentUser(userId, out httpResponseMessage))
                    return httpResponseMessage;

                return Components.UsersController.Instance.ForceChangePassword(user, PortalId, true)
                    ? Request.CreateResponse(HttpStatusCode.OK, new { Success = true })
                    : Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Localization.GetString("OptionUnavailable", Components.Constants.LocalResourcesFile));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManagePassword)]
        public HttpResponseMessage SendPasswordResetLink([FromUri] int userId)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);

                var errorMessage = string.Empty;
                if (MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    errorMessage = Localization.GetString("OptionUnavailable", Components.Constants.LocalResourcesFile);
                }
                else
                {
                    try
                    {
                        //create resettoken
                        UserController.ResetPasswordToken(user, Host.AdminMembershipResetLinkValidity);

                        var canSend = Mail.SendMail(user, MessageType.PasswordReminder, PortalSettings) == string.Empty;
                        if (!canSend)
                        {
                            errorMessage = Localization.GetString("OptionUnavailable", Components.Constants.LocalResourcesFile);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                        }
                    }
                    catch (ArgumentException exc)
                    {
                        Logger.Error(exc);
                        errorMessage = Localization.GetString("InvalidPasswordAnswer", Components.Constants.LocalResourcesFile);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                        errorMessage = Localization.GetString("PasswordResetFailed", Components.Constants.LocalResourcesFile);
                    }
                }

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, errorMessage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AuthorizeUnAuthorizeUser)]
        public HttpResponseMessage UpdateAuthorizeStatus([FromUri] int userId, [FromUri] bool authorized)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);
                HttpResponseMessage httpResponseMessage;
                if (IsCurrentUser(userId, out httpResponseMessage))
                    return httpResponseMessage;
                if (user.Membership.Approved == authorized)//Do nothing if the new status is same as current status.
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });

                Components.UsersController.Instance.UpdateAuthorizeStatus(user, PortalId, authorized);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.DeleteUser)]
        public HttpResponseMessage SoftDeleteUser([FromUri] int userId)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);
                var deleted = !user.IsDeleted && UserController.DeleteUser(ref user, true, false);

                return !deleted
                    ? Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("UserDeleteError", Components.Constants.LocalResourcesFile))
                    : Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.DeleteUser)]
        public HttpResponseMessage HardDeleteUser([FromUri] int userId)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);
                var deleted = user.IsDeleted && UserController.RemoveUser(user);
                return !deleted
                    ? Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("UserRemoveError", Components.Constants.LocalResourcesFile))
                    : Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveDeletedUsers()
        {
            if (!UserInfo.IsSuperUser)
            {
                if (!UserInfo.IsInRole(PortalSettings.AdministratorRoleName))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
                }
            }
            UserController.RemoveDeletedUsers(PortalSettings.PortalId);
            var remaining = UserController.GetDeletedUsers(PortalSettings.PortalId);
            if (remaining.Count > 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("CouldNotRemoveAll", Components.Constants.LocalResourcesFile));
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.DeleteUser)]
        public HttpResponseMessage RestoreDeletedUser([FromUri] int userId)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);
                var restored = user.IsDeleted && UserController.RestoreUser(ref user);
                return !restored
                    ? Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("UserRestoreError", Components.Constants.LocalResourcesFile))
                    : Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage UpdateSuperUserStatus([FromUri] int userId, [FromUri] bool setSuperUser)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);

                user.IsSuperUser = setSuperUser;

                //Update User
                UserController.UpdateUser(PortalId, user);
                DataCache.ClearCache();

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.EditSettings)]
        public HttpResponseMessage UpdateUserBasicInfo(UserBasicDto userBasicDto)
        {
            try
            {
                Validate(userBasicDto);
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userBasicDto.UserId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);

                var upadtedUser = Components.UsersController.Instance.UpdateUserBasicInfo(userBasicDto);

                return Request.CreateResponse(HttpStatusCode.OK, upadtedUser);
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    Localization.GetString("UsernameNotUnique", Components.Constants.LocalResourcesFile));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AuthorizeUnAuthorizeUser)]
        public HttpResponseMessage UnlockUser([FromUri] int userId)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);
                HttpResponseMessage httpResponseMessage;
                if (IsCurrentUser(userId, out httpResponseMessage))
                    return httpResponseMessage;

                var unlocked = user.Membership.LockedOut && UserController.UnLockUser(user);
                return !unlocked
                    ? Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                        Localization.GetString("UserUnlockError", Components.Constants.LocalResourcesFile))
                    : Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region User Roles API

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage GetSuggestRoles(string keyword, int count)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new List<UserRoleInfo>());
                }
                var isAdmin = Components.UsersController.IsAdmin(PortalSettings);

                var roles = RoleController.Instance.GetRoles(PortalId,
                    x => x.RoleName.ToUpperInvariant().Contains(keyword.ToUpperInvariant()));
                var matchedRoles = roles
                    .Where(
                        r =>
                            (isAdmin || r.RoleID != PortalSettings.AdministratorRoleId) &&
                            r.Status == RoleStatus.Approved)
                    .ToList().Take(count).Select(u => new UserRoleInfo
                    {
                        RoleID = u.RoleID,
                        RoleName = $"{u.RoleName}",
                        SecurityMode = u.SecurityMode
                    });

                return Request.CreateResponse(HttpStatusCode.OK,
                    matchedRoles.ToList().Select(r => UserRoleDto.FromRoleInfo(PortalSettings, r)));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage GetUserRoles(string keyword, int userId, int pageIndex, int pageSize)
        {
            try
            {
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);
                int totalRoles;
                var userRoles = Components.UsersController.Instance.GetUserRoles(user, keyword, out totalRoles, pageIndex, pageSize)
                        .Select(r => UserRoleDto.FromRoleInfo(PortalSettings, r));

                return Request.CreateResponse(HttpStatusCode.OK, new { UserRoles = userRoles, TotalRecords = totalRoles });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage SaveUserRole(UserRoleDto userRoleDto, [FromUri] bool notifyUser,
            [FromUri] bool isOwner)
        {
            try
            {
                Validate(userRoleDto);
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userRoleDto.UserId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);

                var result = Components.UsersController.Instance.SaveUserRole(PortalId, UserInfo, userRoleDto,
                    notifyUser, isOwner);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage RemoveUserRole(UserRoleDto userRoleDto)
        {
            try
            {
                Validate(userRoleDto);
                KeyValuePair<HttpStatusCode, string> response;
                var user = Components.UsersController.GetUser(userRoleDto.UserId, PortalSettings, UserInfo, out response);
                if (user == null)
                    return Request.CreateErrorResponse(response.Key, response.Value);

                RoleController.Instance.UpdateUserRole(PortalId, userRoleDto.UserId, userRoleDto.RoleId,
                    RoleStatus.Approved, false, true);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Profiles API
        //        User profile is not implemented currently
        //        [HttpGet]
        //        public HttpResponseMessage GetProfileDefinitions()
        //        {
        //            var profileDefinitions = ProfileController.GetPropertyDefinitionsByPortal(PortalId)
        //                .Cast<ProfilePropertyDefinition>().Select(d => new ProfileDefinitionDto(d));
        //
        //            return Request.CreateResponse(HttpStatusCode.OK, profileDefinitions);
        //        }
        //
        //        [HttpGet]
        //        public HttpResponseMessage GetUserProfile(int userId)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new {Success = true});
        //        }

        #endregion

        /// <summary>
        /// Return Password security options from server 
        /// </summary>
        /// <returns>MembershipPasswordSettings</returns>
        [HttpGet]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageRoles)]
        public HttpResponseMessage PasswordStrengthOptions()
        {
            var settings = new MembershipPasswordSettings(PortalId);

            var passwordSettings = new PasswordSettingsDto{
                MinLength = settings.MinPasswordLength,
                MinNumberOfSpecialChars = settings.MinNonAlphanumericCharacters,
                ValidationExpression = settings.ValidationExpression
            };

            return Request.CreateResponse(HttpStatusCode.OK, passwordSettings);
        }

        private void Validate(UserRoleDto userRoleDto)
        {
            Requires.NotNegative("UserId", userRoleDto.UserId);
            Requires.NotNegative("RoleId", userRoleDto.RoleId);
        }

        private void Validate(UserBasicDto userBasicDto)
        {
            Requires.NotNegative("UserId", userBasicDto.UserId);
        }

        private bool IsCurrentUser(int userId, out HttpResponseMessage response)
        {
            response = null;
            if (userId == UserInfo.UserID)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    Localization.GetString("InSufficientPermissions", Components.Constants.LocalResourcesFile));
                return true;
            }
            return false;
        }
    }
}