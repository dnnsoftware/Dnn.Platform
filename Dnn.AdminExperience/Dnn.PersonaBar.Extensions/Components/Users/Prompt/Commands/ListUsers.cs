// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components.Contracts;
    using Dnn.PersonaBar.Users.Components.Dto;
    using Dnn.PersonaBar.Users.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("list-users", Constants.UsersCategory, "Prompt_ListUsers_Description")]
    public class ListUsers : ConsoleCommandBase
    {
        [FlagParameter("email", "Prompt_ListUsers_FlagEmail", "String")]
        private const string FlagEmail = "email";

        [FlagParameter("username", "Prompt_ListUsers_FlagUsername", "String")]
        private const string FlagUsername = "username";

        [FlagParameter("role", "Prompt_ListUsers_FlagRole", "String")]
        private const string FlagRole = "role";

        [FlagParameter("page", "Prompt_ListUsers_FlagPage", "Integer", "0")]
        private const string FlagPage = "page";

        [FlagParameter("max", "Prompt_ListUsers_FlagMax", "Integer", "10")]
        private const string FlagMax = "max";

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private string Email { get; set; }
        private string Username { get; set; }
        private string Role { get; set; }
        private int Page { get; set; }
        private int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.Email = this.GetFlagValue(FlagEmail, "Email", string.Empty);
            this.Username = this.GetFlagValue(FlagUsername, "Username", string.Empty);
            this.Role = this.GetFlagValue(FlagRole, "Role", string.Empty);
            this.Page = this.GetFlagValue(FlagPage, "Page", 0);
            this.Max = this.GetFlagValue(FlagMax, "Max", 10);
            if (args.Length != 1 && !(args.Length == 3 && (this.HasFlag(FlagPage) || this.HasFlag(FlagMax))) && !(args.Length == 5 && this.HasFlag(FlagPage) && this.HasFlag(FlagMax)))
            {
                // if only one value passed and it's not a flag, try to interpret as username or email
                if (args.Length >= 2 && !this.IsFlag(args[1]))
                {
                    if (args[1].Contains("@"))
                    {
                        // assume it's an email
                        this.Email = args[1];
                    }
                    else
                    {
                        //assume it's a username
                        this.Username = args[1];
                    }
                }
                else
                {
                    // ensure only one filter is used
                    var numFilters = 0;
                    if (!string.IsNullOrEmpty(this.Email))
                        numFilters += 1;
                    if (!string.IsNullOrEmpty(this.Username))
                        numFilters += 1;
                    if (!string.IsNullOrEmpty(this.Role))
                        numFilters += 1;

                    if (numFilters != 1)
                    {
                        this.AddMessage(string.Format(this.LocalizeString("Prompt_OnlyOneFlagRequired"), FlagEmail, FlagUsername, FlagRole));
                    }
                }

            }
        }

        public override ConsoleResultModel Run()
        {
            var usersList = new List<UserModelBase>();
            var recCount = 0;
            var max = this.Max <= 0 ? 10 : (this.Max > 500 ? 500 : this.Max);

            // We need to cover the site group scenario for child portal 
            var portalId = PortalController.GetEffectivePortalId(this.PortalId);
            var getUsersContract = new GetUsersContract
            {
                SearchText = null,
                PageIndex = this.Page > 0 ? this.Page - 1 : 0,
                PageSize = max,
                SortColumn = "displayname",
                SortAscending = true,
                PortalId = portalId,
                Filter = UserFilters.All
            };
            if (!string.IsNullOrEmpty(this.Username))
            {
                // do username lookup
                getUsersContract.SearchText = this.Username;
            }
            else if (!string.IsNullOrEmpty(this.Email))
            {
                // must be email
                getUsersContract.SearchText = this.Email;
            }
            else if (!string.IsNullOrEmpty(this.Role))
            {
                //exact match only allowed at this time. Listing users in multiple roles would require
                // 1) getting all ID's of roles matching search phrase;
                // 2) getting all users in each of those roles;
                // 3) de-duplicating the users list;
                // for large user bases this could take a really long time.
                getUsersContract = null;
                KeyValuePair<HttpStatusCode, string> response;
                var users = UsersController.Instance.GetUsersInRole(this.PortalSettings, this.Role, out recCount, out response, this.Page, max);
                if (users != null)
                    usersList = ConvertList(users);
                else
                {
                    return new ConsoleErrorResultModel(response.Value);
                }
            }

            if (getUsersContract != null)
            {
                usersList = ConvertList(UsersController.Instance.GetUsers(getUsersContract, this.User.IsSuperUser, out recCount), this.PortalId);
            }
            if ((usersList == null || usersList.Count == 0) && recCount == 0)
            {
                return new ConsoleResultModel(this.LocalizeString("noUsers"));
            }
            var totalPages = recCount / max + (recCount % max == 0 ? 0 : 1);
            var pageNo = this.Page > 0 ? this.Page : 1;
            return new ConsoleResultModel
            {
                Data = usersList,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    PageSize = max
                },
                Records = usersList?.Count ?? 0,
                Output = usersList?.Count == 0 ? this.LocalizeString("noUsers") : ""
            };
        }

        private static List<UserModelBase> ConvertList(IEnumerable<UserInfo> lstUserInfos)
        {
            return (from UserInfo ui in lstUserInfos select new UserModelBase(ui)).ToList();
        }

        private static List<UserModelBase> ConvertList(IEnumerable<UserBasicDto> lstBasicDtos, int portalId)
        {
            return (from UserBasicDto ui in lstBasicDtos select new UserModelBase(UserController.Instance.GetUser(portalId, ui.UserId))).ToList();
        }
    }
}
