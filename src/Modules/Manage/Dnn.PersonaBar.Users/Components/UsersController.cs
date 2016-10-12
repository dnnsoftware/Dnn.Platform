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
using System.Collections.Generic;
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

namespace Dnn.PersonaBar.Users.Components
{
    public class UsersController : ServiceLocator<IUsersController, UsersController>, IUsersController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Services.UsersController));
        private const int SearchPageSize = 500;

        protected override Func<IUsersController> GetFactory()
        {
            return () => new UsersController();
        }

        #region Public Methods

        public IList<UserBasicDto> GetUsers(GetUsersContract usersContract, out int totalRecords)
        {
            return !string.IsNullOrEmpty(usersContract.SearchText) ? GetUsersFromLucene(usersContract, out totalRecords) : GetUsersFromDb(usersContract, out totalRecords);
        }

        public UserDetailDto GetUserDetail(int portalId, int userId)
        {
            var user = UserController.Instance.GetUserById(portalId, userId);
            if (user == null)
            {
                return null;
            }

            return new UserDetailDto(user);
        }

        public bool ChangePassword(int portalId, int userId, string newPassword, out string errorMessage)
        {
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                errorMessage = "ChangePasswordNotAvailable";
                return false;
            }

            errorMessage = string.Empty;
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
                    errorMessage = "BannedPasswordUsed";
                    return false;
                }

            }

            //check new password is not in history
            if (membershipPasswordController.IsPasswordInHistory(user.UserID, user.PortalID, newPassword, false))
            {
                errorMessage = "PasswordResetFailed";
                return false;
            }

            var oldPassword = UserController.GetPassword(ref user, string.Empty);
            if (oldPassword == newPassword)
            {
                errorMessage = "PasswordNotDifferent";
                return false;
            }

            try
            {
                var passwordChanged = UserController.ResetAndChangePassword(user, newPassword);
                if (!passwordChanged)
                {
                    errorMessage = "PasswordResetFailed";
                }

                return passwordChanged;
            }
            catch (MembershipPasswordException exc)
            {
                //Password Answer missing
                Logger.Error(exc);
                errorMessage = "InvalidPasswordAnswer";
                return false;
            }
            catch (ThreadAbortException)
            {
                return true;
            }
            catch (Exception exc)
            {
                //Fail
                Logger.Error(exc);
                errorMessage = "PasswordResetFailed";
                return false;
            }
        }

        #endregion

        #region Private Methods

        private static IList<UserBasicDto> GetUsersFromDb(GetUsersContract usersContract, out int totalRecords)
        {
            totalRecords = 0;
            using (var reader = DataProvider.Instance()
                                        .ExecuteReader("Personabar_GetUsers", usersContract.PortalId,
                                                       usersContract.SortColumn,
                                                       usersContract.SortAscending,
                                                       usersContract.PageIndex,
                                                       usersContract.PageSize))
            {
                if (reader.Read())
                {
                    totalRecords = reader.GetInt32(0);
                    reader.NextResult();
                }
                return CBO.FillCollection<UserBasicDto>(reader);
            }
        }

        private static IList<UserBasicDto> GetUsersFromLucene(GetUsersContract usersContract, out int totalRecords)
        {
            var query = new SearchQuery
            {
                KeyWords = usersContract.SearchText,
                PortalIds = new List<int> { usersContract.PortalId },
                PageIndex = 1,
                SearchTypeIds = new List<int> { SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId },
                PageSize = SearchPageSize,
                WildCardSearch = true,
                CultureCode = null,
                NumericKeys = new Dictionary<string, int> { { "superuser", 0 } }
            };

            var searchResults = SearchController.Instance.SiteSearch(query);
            var userIds = searchResults.Results.Distinct(new UserSearchResultComparer()).Take(SearchPageSize)
                            .Select(r => 
                                        {
                                            int userId;
                                            TryConvertToInt32(r.UniqueKey.Split('_')[0], out userId);
                                            return userId;
                                        })
                                        .Where(u => u > 0).ToList();
            totalRecords = userIds.Count;
            
            var currentIds = string.Join(",", userIds.Skip(usersContract.PageIndex * usersContract.PageSize).Take(usersContract.PageSize));
            return UsersDataService.Instance.GetUsersByUserIds(usersContract.PortalId, currentIds);
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

        #endregion
    }
}