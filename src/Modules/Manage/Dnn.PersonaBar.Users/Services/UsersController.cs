#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
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
                    PortalId = PortalId,
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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;

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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;
                if (IsCurrentUser(userId, out response))
                    return response;

                if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
                {
                    UserController.ResetPasswordToken(user);
                }
                var canSend = Mail.SendMail(user, MessageType.PasswordReminder, PortalSettings) == string.Empty;
                if (canSend)
                {
                    user.Membership.UpdatePassword = true;

                    //Update User
                    UserController.UpdateUser(PortalId, user);
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                }

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;

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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;
                if (IsCurrentUser(userId, out response))
                    return response;
                if (user.Membership.Approved == authorized)//Do nothing if the new status is same as current status.
                    return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });

                user.Membership.Approved = authorized;

                //Update User
                UserController.UpdateUser(PortalId, user);
                if (authorized)
                {
                    //Update User Roles if needed
                    if (!user.IsSuperUser && user.IsInRole("Unverified Users") &&
                        PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                    {
                        UserController.ApproveUser(user);
                    }

                    Mail.SendMail(user, MessageType.UserAuthorized, PortalSettings);
                }
                else if (PortalController.GetPortalSettingAsBoolean("AlwaysSendUserUnAuthorizedEmail", PortalId,
                    false))
                {
                    Mail.SendMail(user, MessageType.UserUnAuthorized, PortalSettings);
                }
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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;

                var deleted = UserController.DeleteUser(ref user, true, false);
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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;

                var deleted = UserController.RemoveUser(user);

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
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.DeleteUser)]
        public HttpResponseMessage RestoreDeletedUser([FromUri] int userId)
        {
            try
            {
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;

                var restored = UserController.RestoreUser(ref user);

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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;

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
                HttpResponseMessage response;
                var user = GetUser(userBasicDto.UserId, out response);
                if (user == null)
                    return response;

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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;
                if (IsCurrentUser(userId, out response))
                    return response;

                var unlocked = !user.Membership.LockedOut || UserController.UnLockUser(user);
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
                var isAdmin = IsAdmin();

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
                HttpResponseMessage response;
                var user = GetUser(userId, out response);
                if (user == null)
                    return response;

                var allUserRoles = RoleController.Instance.GetUserRoles(user, true);
                if (!string.IsNullOrEmpty(keyword))
                {
                    allUserRoles =
                        allUserRoles.Where(
                            u => u.FullName.StartsWith(keyword, StringComparison.InvariantCultureIgnoreCase))
                            .ToList();
                }

                var userRoles = allUserRoles
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .Select(r => UserRoleDto.FromRoleInfo(PortalSettings, r));

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { UserRoles = userRoles, TotalRecords = allUserRoles.Count });
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
                HttpResponseMessage response;
                var user = GetUser(userRoleDto.UserId, out response);
                if (user == null)
                    return response;

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
                HttpResponseMessage response;
                var user = GetUser(userRoleDto.UserId, out response);
                if (user == null)
                    return response;

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

        private void Validate(UserRoleDto userRoleDto)
        {
            Requires.NotNegative("UserId", userRoleDto.UserId);
            Requires.NotNegative("RoleId", userRoleDto.RoleId);
        }

        private void Validate(UserBasicDto userBasicDto)
        {
            Requires.NotNegative("UserId", userBasicDto.UserId);
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