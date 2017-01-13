#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Security;
using Dnn.PersonaBar.Users.Components.Comparers;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using Dnn.PersonaBar.Users.Data;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Services.Search.Controllers;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Services.Localization;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

namespace Dnn.PersonaBar.Users.Components
{
    public class UsersController : ServiceLocator<IUsersController, UsersController>, IUsersController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Services.UsersController));
        private const int SearchPageSize = 500;

        private PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        protected override Func<IUsersController> GetFactory()
        {
            return () => new UsersController();
        }

        #region Public Methods

        public IEnumerable<UserBasicDto> GetUsers(GetUsersContract usersContract, bool isSuperUser, out int totalRecords)
        {
            return !string.IsNullOrEmpty(usersContract.SearchText) 
                        && usersContract.Filter == UserFilters.Authorized
                        && !HostController.Instance.GetBoolean("DisableUserCrawling", false)
                ? GetUsersFromLucene(usersContract, out totalRecords)
                : GetUsersFromDb(usersContract, isSuperUser, out totalRecords);
        }

        public IEnumerable<KeyValuePair<string, int>> GetUserFilters(bool isSuperUser= false)
        {
            var userFilters = new List<KeyValuePair<string, int>>();
            for (var i = 0; i < 6; i++)
            {
                userFilters.Add(
                    new KeyValuePair<string, int>(
                        Localization.GetString(Convert.ToString((UserFilters) i), Constants.LocalResourcesFile), i));
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

        public UserBasicDto UpdateUserBasicInfo(UserBasicDto userBasicDto)
        {
            var user = UserController.Instance.GetUser(PortalSettings.PortalId, userBasicDto.UserId);
            int portalId = PortalSettings.PortalId;
            if (user == null)
            {
                throw new ArgumentException("UserNotExist");
            }

            if (userBasicDto.UserId == PortalSettings.AdministratorId)
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

            //Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(PortalSettings.Registration.DisplayNameFormat))
            {
                user.UpdateDisplayName(PortalSettings.Registration.DisplayNameFormat);
            }
            //either update the username or update the user details

            if (CanUpdateUsername(user) && !PortalSettings.Registration.UseEmailAsUserName)
            {
                UserController.ChangeUsername(user.UserID, userBasicDto.Username);
                user.Username = userBasicDto.Username;
            }

            //DNN-5874 Check if unique display name is required
            if (PortalSettings.Registration.RequireUniqueDisplayName)
            {
                var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(portalId, 0, 2, "DisplayName", true, "DisplayName", user.DisplayName);
                if (usersWithSameDisplayName.Any(u => u.UserID != user.UserID))
                {
                    throw new ArgumentException("DisplayNameNotUnique");
                }
            }

            UserController.UpdateUser(portalId, user);

            if (PortalSettings.Registration.UseEmailAsUserName && (user.Username.ToLowerInvariant() != user.Email.ToLowerInvariant()))
            {
                UserController.ChangeUsername(user.UserID, user.Email);
            }
            return
                UserBasicDto.FromUserInfo(UserController.Instance.GetUser(PortalSettings.PortalId, userBasicDto.UserId));
        }

        public UserRoleDto SaveUserRole(int portalId, UserInfo userInfo, UserRoleDto userRoleDto, bool notifyUser,
            bool isOwner)
        {
            if (!UserRoleDto.AllowExpiredRole(PortalSettings, userRoleDto.UserId, userRoleDto.RoleId))
            {
                userRoleDto.StartTime = userRoleDto.ExpiresTime = Null.NullDate;
            }

            var user = UserController.Instance.GetUserById(portalId, userRoleDto.UserId);
            var role = RoleController.Instance.GetRoleById(portalId, userRoleDto.RoleId);
            if (role == null || role.Status != RoleStatus.Approved)
            {
                throw new Exception(Localization.GetString("RoleIsNotApproved", Constants.LocalResourcesFile));
            }

            if (userInfo.IsSuperUser || userInfo.Roles.Contains(PortalSettings.AdministratorRoleName) ||
                (!userInfo.IsSuperUser && !userInfo.Roles.Contains(PortalSettings.AdministratorRoleName) &&
                 role.RoleType != RoleType.Administrator))
            {
                if (role.SecurityMode != SecurityMode.SocialGroup && role.SecurityMode != SecurityMode.Both)
                    isOwner = false;

                RoleController.AddUserRole(user, role, PortalSettings, RoleStatus.Approved, userRoleDto.StartTime,
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
                    AllowExpired = UserRoleDto.AllowExpiredRole(PortalSettings, user.UserID, role.RoleID),
                    AllowDelete = RoleController.CanRemoveUserFromRole(PortalSettings, user.UserID, role.RoleID)
                };
            }
            throw new Exception(Localization.GetString("InSufficientPermissions", Constants.LocalResourcesFile));
        }

        #endregion

        #region Private Methods

        private static IEnumerable<UserBasicDto> GetUsersFromDb(GetUsersContract usersContract, bool isSuperUser, out int totalRecords)
        {
            totalRecords = 0;
            List<UserBasicDto> users = null;
            ArrayList dbUsers = null;
            IEnumerable<UserInfo> userInfos = null;

            var portalId = usersContract.PortalId;
            var pageIndex = usersContract.PageIndex;
            var pageSize = usersContract.PageSize;
            var searchText = usersContract.SearchText;
            var paged = false;

            switch (usersContract.Filter)
            {
                case UserFilters.All:

                    if (string.IsNullOrEmpty(searchText))
                    {
                        dbUsers = UserController.GetUsers(portalId, pageIndex, pageSize, ref totalRecords, true, false);
                    }
                    else
                    {
                        dbUsers = UserController.GetUsersByDisplayName(portalId, searchText + "%", pageIndex, pageSize,
                            ref totalRecords, true, false);
                    }
                    paged = true;
                    userInfos = dbUsers?.OfType<UserInfo>().ToList();
                    break;
                case UserFilters.SuperUsers:
                    if (isSuperUser)
                    {
                        dbUsers = UserController.GetUsers(Null.NullInteger, pageIndex, pageSize, ref totalRecords, false, true);
                        userInfos = dbUsers?.OfType<UserInfo>().ToList();
                    }
                    paged = true;
                    break;
                case UserFilters.UnAuthorized:
                    dbUsers = UserController.GetUnAuthorizedUsers(portalId, false, false);
                    userInfos = dbUsers?.OfType<UserInfo>().ToList();
                    if (!isSuperUser)
                    {
                        userInfos = userInfos?.Where(x => !x.IsSuperUser);
                    }
                    break;
                case UserFilters.Deleted:
                    dbUsers = UserController.GetDeletedUsers(portalId);
                    userInfos = dbUsers?.OfType<UserInfo>().ToList();
                    if (!isSuperUser)
                    {
                        userInfos = userInfos?.Where(x => !x.IsSuperUser);
                    }
                    break;
//                    case UserFilters.Online:
//                        dbUsers = UserController.GetOnlineUsers(usersContract.PortalId);
//                        break;
                case UserFilters.RegisteredUsers:
                    userInfos = RoleController.Instance.GetUsersByRole(portalId,
                        PortalController.Instance.GetCurrentPortalSettings().RegisteredRoleName);
                    if (!isSuperUser)
                    {
                        userInfos = userInfos?.Where(x => !x.IsSuperUser);
                    }
                    break;
                case UserFilters.Authorized:
                    if (string.IsNullOrEmpty(searchText))
                    {
                        var reader = DataProvider.Instance()
                            .ExecuteReader("Personabar_GetUsers", usersContract.PortalId,
                                string.IsNullOrEmpty(usersContract.SortColumn) ? "Joined" : usersContract.SortColumn,
                                usersContract.SortAscending,
                                usersContract.PageIndex,
                                usersContract.PageSize);
                        if (reader.Read())
                        {
                            totalRecords = reader.GetInt32(0);
                            reader.NextResult();
                        }
                        users = CBO.FillCollection<UserBasicDto>(reader);
                    }
                    else
                    {
                        dbUsers = UserController.GetUsersByDisplayName(portalId, searchText + "%", pageIndex, pageSize,
                            ref totalRecords, false, false);
                        users = dbUsers?.OfType<UserInfo>().Select(UserBasicDto.FromUserInfo).ToList();
                    }
                    paged = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (users == null && userInfos != null)
            {
                var enumerable = userInfos as UserInfo[] ?? userInfos.ToArray();
                totalRecords = paged ? totalRecords : enumerable.Length;
                var sorted = GetSortedUsers(enumerable, usersContract.SortColumn, usersContract.SortAscending);
                users = paged ? sorted.Select(UserBasicDto.FromUserInfo).ToList() : GetPagedUsers(sorted, pageSize, pageIndex)?.Select(UserBasicDto.FromUserInfo).ToList();
            }
            return users;
        }

        private static IEnumerable<UserInfo> GetSortedUsers(IEnumerable<UserInfo> users, string sortColumn,
            bool sortAscending = false)
        {
            switch (sortColumn?.ToLowerInvariant())
            {

                case "displayname":
                    return sortAscending
                        ? users.OrderBy(x => x.DisplayName)
                        : users.OrderByDescending(x => x.DisplayName);
                case "email":
                    return sortAscending
                        ? users.OrderBy(x => x.Email)
                        : users.OrderByDescending(x => x.Email);
                default:
                    return sortAscending
                        ? users.OrderBy(x => x.CreatedOnDate).ThenBy(x => x.LastModifiedOnDate)
                        : users.OrderByDescending(x => x.CreatedOnDate).ThenByDescending(x => x.LastModifiedOnDate);
            }
        }

        private static IEnumerable<UserInfo> GetPagedUsers(IEnumerable<UserInfo> users, int pageSize, int pageIndex)
        {
            return
                users.Skip(pageIndex*pageSize).Take(pageSize);
        }

        private static IList<UserBasicDto> GetUsersFromLucene(GetUsersContract usersContract, out int totalRecords)
        {
            var query = new SearchQuery
            {
                KeyWords = usersContract.SearchText, PortalIds = new List<int> {usersContract.PortalId}, PageIndex = 1, SearchTypeIds = new List<int> {SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId}, PageSize = SearchPageSize, WildCardSearch = true, CultureCode = null, NumericKeys = new Dictionary<string, int> {{"superuser", 0}}
            };

            var searchResults = SearchController.Instance.SiteSearch(query);
            var userIds = searchResults.Results.Distinct(new UserSearchResultComparer()).Take(SearchPageSize).Select(r =>
            {
                int userId;
                TryConvertToInt32(r.UniqueKey.Split('_')[0], out userId);
                return userId;
            }).Where(u => u > 0).ToList();

            var currentIds = string.Join(",", userIds.Skip(usersContract.PageIndex*usersContract.PageSize).Take(usersContract.PageSize));
            var users = UsersDataService.Instance.GetUsersByUserIds(usersContract.PortalId, currentIds).Where(u => UserController.GetUserById(usersContract.PortalId, u.UserId).Membership.Approved).ToList();
            totalRecords = users.Count;
            return users;
        }

        private static bool TryConvertToInt32(string paramValue, out int intValue)
        {
            if (!string.IsNullOrEmpty(paramValue) && Int32.TryParse(paramValue, out intValue))
            {
                return true;
            }

            intValue = Null.NullInteger;
            return false;
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


    #endregion
}
}