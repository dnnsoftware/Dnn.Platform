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



#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Security;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;
using System.Net;
using DotNetNuke.Services.Mail;
using Dnn.PersonaBar.Users.Components.Helpers;
using System.Data;

namespace Dnn.PersonaBar.Users.Components
{
    public class UsersController : ServiceLocator<IUsersController, UsersController>, IUsersController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Services.UsersController));

        private PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        protected override Func<IUsersController> GetFactory()
        {
            return () => new UsersController();
        }

        #region Public Methods

        public IEnumerable<UserBasicDto> GetUsers(GetUsersContract usersContract, bool isSuperUser, out int totalRecords)
        {
            return GetUsersFromDb(usersContract, isSuperUser, out totalRecords) ?? new List<UserBasicDto>();
        }

        public IEnumerable<KeyValuePair<string, int>> GetUserFilters(bool isSuperUser = false)
        {
            var userFilters = new List<KeyValuePair<string, int>>();
            for (var i = 0; i < 6; i++)
            {
                userFilters.Add(
                    new KeyValuePair<string, int>(
                        Localization.GetString(Convert.ToString((UserFilters)i), Constants.LocalResourcesFile), i));
            }
            if (!isSuperUser)
            {
                userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.SuperUsers)));
            }
            userFilters.Remove(userFilters.FirstOrDefault(x => x.Value == Convert.ToInt32(UserFilters.RegisteredUsers)));//Temporarily removed registered users.
            return userFilters;
        }

        public UserDetailDto GetUserDetail(int portalId, int userId)
        {
            var user = UserController.Instance.GetUserById(portalId, userId);
            if (user == null)
            {
                return null;
            }
            user.PortalID = portalId;
            return new UserDetailDto(user);
        }

        public bool ChangePassword(int portalId, int userId, string newPassword)
        {
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                throw new Exception(Localization.GetString("CannotChangePassword", Constants.LocalResourcesFile));
            }

            var user = UserController.Instance.GetUserById(portalId, userId);
            if (user == null)
            {
                return false;
            }

            var membershipPasswordController = new MembershipPasswordController();
            var settings = new MembershipPasswordSettings(user.PortalID);

            if (settings.EnableBannedList)
            {
                if (membershipPasswordController.FoundBannedPassword(newPassword) || user.Username == newPassword)
                {
                    throw new Exception(Localization.GetString("PasswordResetFailed", Constants.LocalResourcesFile));
                }

            }

            //check new password is not in history
            if (membershipPasswordController.IsPasswordInHistory(user.UserID, user.PortalID, newPassword, false))
            {
                throw new Exception(Localization.GetString("PasswordResetFailed_PasswordInHistory", Constants.LocalResourcesFile));
            }

            try
            {
                var passwordChanged = UserController.ResetAndChangePassword(user, newPassword);
                if (!passwordChanged)
                {
                    throw new Exception(Localization.GetString("PasswordResetFailed", Constants.LocalResourcesFile));
                }

                return true;
            }
            catch (MembershipPasswordException exc)
            {
                //Password Answer missing
                Logger.Error(exc);
                throw new Exception(Localization.GetString("PasswordInvalid", Constants.LocalResourcesFile));
            }
            catch (ThreadAbortException)
            {
                return true;
            }
            catch (Exception exc)
            {
                //Fail
                Logger.Error(exc);
                throw new Exception(Localization.GetString("PasswordResetFailed", Constants.LocalResourcesFile));
            }
        }

        public UserBasicDto UpdateUserBasicInfo(UserBasicDto userBasicDto, int requestPortalId = -1)
        {
            int portalId = PortalSettings.PortalId;
            PortalSettings requestPortalSettings = PortalSettings;

            if (requestPortalId != -1)
            {
                portalId = requestPortalId;
                requestPortalSettings = new PortalSettings(portalId);
            }

            var user = UserController.Instance.GetUser(portalId, userBasicDto.UserId);

            if (user == null)
            {
                throw new ArgumentException("UserNotExist");
            }

            if (userBasicDto.UserId == requestPortalSettings.AdministratorId)
            {
                //Clear the Portal Cache
                DataCache.ClearPortalCache(portalId, true);
            }
            if (user.IsSuperUser)
            {
                DataCache.ClearHostCache(true);
            }
            user.DisplayName = userBasicDto.Displayname;
            user.Email = userBasicDto.Email;
            user.FirstName = !string.IsNullOrEmpty(userBasicDto.Firstname) ? userBasicDto.Firstname : user.FirstName;
            user.LastName = !string.IsNullOrEmpty(userBasicDto.Lastname) ? userBasicDto.Lastname : user.LastName;
            //Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(requestPortalSettings.Registration.DisplayNameFormat))
            {
                user.UpdateDisplayName(requestPortalSettings.Registration.DisplayNameFormat);
            }
            //either update the username or update the user details

            if (CanUpdateUsername(user) && !requestPortalSettings.Registration.UseEmailAsUserName)
            {
                UserController.ChangeUsername(user.UserID, userBasicDto.Username);
                user.Username = userBasicDto.Username;
            }

            //DNN-5874 Check if unique display name is required
            if (requestPortalSettings.Registration.RequireUniqueDisplayName)
            {
                var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(portalId, 0, 2, "DisplayName", true, "DisplayName", user.DisplayName);
                if (usersWithSameDisplayName.Any(u => u.UserID != user.UserID))
                {
                    throw new ArgumentException("DisplayNameNotUnique");
                }
            }

            UserController.UpdateUser(portalId, user);

            if (requestPortalSettings.Registration.UseEmailAsUserName && (user.Username.ToLowerInvariant() != user.Email.ToLowerInvariant()))
            {
                UserController.ChangeUsername(user.UserID, user.Email);
            }
            return
                UserBasicDto.FromUserInfo(UserController.Instance.GetUser(requestPortalSettings.PortalId, userBasicDto.UserId));
        }

        public UserRoleDto SaveUserRole(int portalId, UserInfo currentUserInfo, UserRoleDto userRoleDto, bool notifyUser,
            bool isOwner)
        {
            PortalSettings portalSettings = PortalSettings;

            if (PortalSettings.PortalId != portalId)
            {
                portalSettings = GetPortalSettings(portalId);
            }

            if (!UserRoleDto.AllowExpiredRole(portalSettings, userRoleDto.UserId, userRoleDto.RoleId))
            {
                userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
            }

            var user = UserController.Instance.GetUserById(portalId, userRoleDto.UserId);
            var role = RoleController.Instance.GetRoleById(portalId, userRoleDto.RoleId);
            if (role == null || role.Status != RoleStatus.Approved)
            {
                throw new Exception(Localization.GetString("RoleIsNotApproved", Constants.LocalResourcesFile));
            }

            if (currentUserInfo.IsSuperUser || currentUserInfo.Roles.Contains(portalSettings.AdministratorRoleName) ||
                (!currentUserInfo.IsSuperUser && !currentUserInfo.Roles.Contains(portalSettings.AdministratorRoleName) &&
                 role.RoleType != RoleType.Administrator))
            {
                if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
                    isOwner = false;

                RoleController.AddUserRole(user, role, portalSettings, RoleStatus.Approved, userRoleDto.StartTime,
                    userRoleDto.ExpiresTime, notifyUser, isOwner);
                var addedRole = RoleController.Instance.GetUserRole(portalId, userRoleDto.UserId, userRoleDto.RoleId);

                return new UserRoleDto
                {
                    UserId = addedRole.UserID,
                    RoleId = addedRole.RoleID,
                    DisplayName = addedRole.FullName,
                    RoleName = addedRole.RoleName,
                    StartTime = addedRole.EffectiveDate,
                    ExpiresTime = addedRole.ExpiryDate,
                    AllowExpired = UserRoleDto.AllowExpiredRole(portalSettings, user.UserID, role.RoleID),
                    AllowDelete = RoleController.CanRemoveUserFromRole(portalSettings, user.UserID, role.RoleID)
                };
            }
            throw new Exception(Localization.GetString("InSufficientPermissions", Constants.LocalResourcesFile));
        }        

        public bool ForceChangePassword(UserInfo userInfo, int portalId, bool notify)
        {
            if (MembershipProviderConfig.PasswordRetrievalEnabled || MembershipProviderConfig.PasswordResetEnabled)
            {
                var canSend = UserController.ResetPasswordToken(userInfo, notify);
                if (canSend || !notify)
                {
                    userInfo.Membership.UpdatePassword = true;

                    //Update User
                    UserController.UpdateUser(portalId, userInfo);
                    return true;
                }
            }
            return false;
        }

        public void AddUserToRoles(UserInfo currentUserInfo, int userId, int portalId, string roleNames, string roleDelimiter = ",", DateTime? effectiveDate = null, DateTime? expiryDate = null)
        {
            var effDate = effectiveDate.GetValueOrDefault(Null.NullDate);
            var expDate = expiryDate.GetValueOrDefault(Null.NullDate);

            // get the specified RoleName
            var roleController = new RoleController();
            var lstRoles = roleNames.Split(roleDelimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var curRole in lstRoles)
            {
                var role = roleController.GetRoleByName(portalId, curRole);
                if (role == null) continue;
                var userRoleDto = new UserRoleDto
                {
                    RoleId = role.RoleID,
                    UserId = userId,
                    StartTime = effDate,
                    ExpiresTime = expDate,
                    RoleName = curRole
                };
                Instance.SaveUserRole(portalId, currentUserInfo, userRoleDto, false, false);
            }
        }

        public static UserInfo GetUser(int userId, PortalSettings portalSettings, UserInfo userInfo, out KeyValuePair<HttpStatusCode, string> response)
        {
            response = new KeyValuePair<HttpStatusCode, string>();
            var user = UserController.Instance.GetUserById(portalSettings.PortalId, userId);
            if (user == null)
            {
                response = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("UserNotFound", Constants.LocalResourcesFile));
                return null;
            }
            if (!IsAdmin(user, portalSettings)) return user;

            if ((user.IsSuperUser && !userInfo.IsSuperUser) || !IsAdmin(portalSettings))
            {
                response = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.Unauthorized, Localization.GetString("InSufficientPermissions", Constants.LocalResourcesFile));
                return null;
            }
            if (user.IsSuperUser)
                user = UserController.Instance.GetUserById(Null.NullInteger, userId);
            return user;
        }

        public IList<UserRoleInfo> GetUserRoles(UserInfo user, string keyword, out int total, int pageIndex = -1, int pageSize = -1)
        {
            var roles = RoleController.Instance.GetUserRoles(user, true);
            if (!string.IsNullOrEmpty(keyword))
            {
                roles = roles.Where(u => u.FullName.StartsWith(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            total = roles.Count;
            pageSize = pageSize > 0 && pageSize < 500 ? pageSize : 500;
            return pageIndex == -1 ? roles : roles.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public IEnumerable<UserInfo> GetUsersInRole(PortalSettings portalSettings, string roleName, out int total, out KeyValuePair<HttpStatusCode, string> message, int pageIndex = -1, int pageSize = -1)
        {
            message = new KeyValuePair<HttpStatusCode, string>();
            total = 0;
            var role = RoleController.Instance.GetRoleByName(portalSettings.PortalId, roleName);
            if (role == null)
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("RoleNotFound", Constants.LocalResourcesFile));
                return null;
            }
            if (role.RoleID == PortalSettings.AdministratorRoleId && !IsAdmin(portalSettings))
            {
                message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, Localization.GetString("InvalidRequest", Constants.LocalResourcesFile));
                return null;
            }
            var users = RoleController.Instance.GetUsersByRole(portalSettings.PortalId, role.RoleName);
            total = users.Count;
            var startIndex = pageIndex * pageSize;
            return users.Skip(startIndex).Take(pageSize);
        }

        public void UpdateAuthorizeStatus(UserInfo userInfo, int portalId, bool authorized)
        {
            userInfo.Membership.Approved = authorized;

            //Update User
            UserController.UpdateUser(portalId, userInfo);
            if (authorized)
            {
                //Update User Roles if needed
                if (!userInfo.IsSuperUser && userInfo.IsInRole("Unverified Users") &&
                    PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    UserController.ApproveUser(userInfo);
                }

                Mail.SendMail(userInfo, MessageType.UserAuthorized, PortalSettings);
            }
            else if (PortalController.GetPortalSettingAsBoolean("AlwaysSendUserUnAuthorizedEmail", portalId,
                    false))
            {
                Mail.SendMail(userInfo, MessageType.UserUnAuthorized, PortalSettings);
            }
        }
        public static bool IsAdmin(PortalSettings portalSettings)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }

        #endregion

        #region Private Methods

        private IEnumerable<UserBasicDto> GetUsersFromDb(GetUsersContract usersContract, bool isSuperUser, out int totalRecords)
        {
            totalRecords = 0;
            IEnumerable<UserBasicDto> users = null;

            var portalId = usersContract.PortalId;
            var pageIndex = usersContract.PageIndex;
            var pageSize = usersContract.PageSize;

            switch (usersContract.Filter)
            {
                case UserFilters.All:
                    users = GetUsers(usersContract, null, null, isSuperUser ? null : (bool?)false, out totalRecords);
                    break;
                case UserFilters.Authorized:
                    users = GetUsers(usersContract, true, false, isSuperUser ? null : (bool?)false, out totalRecords);
                    break;
                case UserFilters.SuperUsers:
                    if (isSuperUser)
                    {
                        users = GetUsers(usersContract, null, null, true, out totalRecords);
                    }
                    break;
                case UserFilters.UnAuthorized:
                    users = GetUsers(usersContract, false, false, isSuperUser ? null : (bool?)false, out totalRecords);
                    break;
                case UserFilters.Deleted:
                    users = GetUsers(usersContract, null, true, isSuperUser ? null : (bool?)false, out totalRecords);
                    break;
                case UserFilters.RegisteredUsers:
                    {
                        IList<UserInfo> userInfos = RoleController.Instance.GetUsersByRole(portalId,
                            PortalController.Instance.GetCurrentPortalSettings().RegisteredRoleName);
                        if (!isSuperUser)
                        {
                            userInfos = (IList<UserInfo>)userInfos?.Where(x => !x.IsSuperUser);
                        }
                        if (userInfos != null)
                        {
                            totalRecords = userInfos.Count;
                            users = GetSortedUsers(
                                GetPagedUsers(userInfos, pageSize, pageIndex)?.Select(UserBasicDto.FromUserInfo),
                                usersContract.SortColumn, usersContract.SortAscending);
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return users;
        }

        private static IEnumerable<UserBasicDto> GetSortedUsers(IEnumerable<UserBasicDto> users, string sortColumn,
            bool sortAscending = false)
        {
            switch (sortColumn?.ToLowerInvariant())
            {

                case "displayname":
                    return sortAscending
                        ? users.OrderBy(x => x.Displayname)
                        : users.OrderByDescending(x => x.Displayname);
                case "email":
                    return sortAscending
                        ? users.OrderBy(x => x.Email)
                        : users.OrderByDescending(x => x.Email);
                default:
                    return sortAscending
                        ? users.OrderBy(x => x.CreatedOnDate)
                        : users.OrderByDescending(x => x.CreatedOnDate);
            }
        }

        private static IEnumerable<UserInfo> GetPagedUsers(IEnumerable<UserInfo> users, int pageSize, int pageIndex)
        {
            return
                users.Skip(pageIndex * pageSize).Take(pageSize);
        }

        private bool CanUpdateUsername(UserInfo user)
        {
            //can only update username if a host/admin and account being managed is not a superuser
            if (UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                //only allow updates for non-superuser accounts
                if (user.IsSuperUser == false)
                {
                    return true;
                }
            }

            //if an admin, check if the user is only within this portal
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(PortalSettings.AdministratorRoleName))
            {
                //only allow updates for non-superuser accounts
                if (user.IsSuperUser)
                {
                    return false;
                }
                if (PortalController.GetPortalsByUser(user.UserID).Count == 1) return true;
            }

            return false;
        }

        private static bool IsAdmin(UserInfo user, PortalSettings portalSettings)
        {
            return user.IsSuperUser || user.IsInRole(portalSettings.AdministratorRoleName);
        }

        private IEnumerable<UserBasicDto> GetUsers(GetUsersContract usersContract,
            bool? includeAuthorized, bool? includeDeleted, bool? includeSuperUsers, out int totalRecords)
        {

            var parsedSearchText = string.IsNullOrEmpty(usersContract.SearchText) ? "" : SearchTextFilter.CleanWildcards(usersContract.SearchText.Trim());

            usersContract.SearchText = string.Format("{0}*", parsedSearchText);

            List<UserBasicDto2> records = CBO.FillCollection<UserBasicDto2>(
                CallGetUsersBySearchTerm(
                    usersContract,
                    includeAuthorized, 
                    includeDeleted, 
                    includeSuperUsers));

            totalRecords = records.Count == 0 ? 0 : records[0].TotalCount;
            return records;
        }

        protected virtual IDataReader CallGetUsersBySearchTerm(GetUsersContract usersContract,
            bool? includeAuthorized, bool? includeDeleted, bool? includeSuperUsers)
        {
            var parsedSearchText = string.IsNullOrEmpty(usersContract.SearchText) ? "" : SearchTextFilter.CleanWildcards(usersContract.SearchText.Trim());

            return DataProvider.Instance().ExecuteReader(
                    "Personabar_GetUsersBySearchTerm",
                    usersContract.PortalId,
                    string.IsNullOrEmpty(usersContract.SortColumn) ? "Joined" : usersContract.SortColumn,
                    usersContract.SortAscending,
                    usersContract.PageIndex,
                    usersContract.PageSize,
                    parsedSearchText,
                    includeAuthorized,
                    includeDeleted,
                    includeSuperUsers);
        }

        private PortalSettings GetPortalSettings(int portalId)
        {
            var portalSettings = new PortalSettings(portalId);
            var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
            portalSettings.PrimaryAlias = portalAliases.FirstOrDefault(a => a.IsPrimary);
            portalSettings.PortalAlias = PortalAliasController.Instance.GetPortalAlias(portalSettings.DefaultPortalAlias);
            return portalSettings;
        }

        #endregion
    }
}