using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("list-users", "Returns users that match the given expression", new[]{
        "email",
        "username",
        "role",
        "page",
        "max"
    })]
    public class ListUsers : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagEmail = "email";
        private const string FlagUsername = "username";
        private const string FlagRole = "role";
        private const string FlagPage = "page";
        private const string FlagMax = "max";

        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int Page { get; set; }
        public int Max { get; set; } = 10;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            Email = GetFlagValue(FlagEmail, "Email", string.Empty);
            Username = GetFlagValue(FlagUsername, "Username", string.Empty);
            Page = GetFlagValue(FlagPage, "Page", 1);
            Max = GetFlagValue(FlagMax, "Max", 10);
            if (args.Length != 1 && !(args.Length == 3 && (HasFlag(FlagPage) || HasFlag(FlagMax))) && !(args.Length == 5 && HasFlag(FlagPage) && HasFlag(FlagMax)))
            {
                // if only one value passed and it's not a flag, try to interpret as username or email
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    if (args[1].Contains("@"))
                    {
                        // assume it's an email
                        Email = args[1];
                    }
                    else
                    {
                        //assume it's a username
                        Username = args[1];
                    }
                }
                else
                {
                    // ensure only one filter is used
                    var numFilters = 0;
                    if (!string.IsNullOrEmpty(Email))
                        numFilters += 1;
                    if (!string.IsNullOrEmpty(Username))
                        numFilters += 1;
                    if (!string.IsNullOrEmpty(Role))
                        numFilters += 1;

                    if (numFilters != 1)
                    {
                        AddMessage(string.Format(LocalizeString("Prompt_OnlyOneFlagRequired"), FlagEmail, FlagUsername, FlagRole));
                    }
                }

            }
        }

        public override ConsoleResultModel Run()
        {
            var usersList = new List<UserModelBase>();
            var recCount = 0;
            var getUsersContract = new GetUsersContract
            {
                SearchText = null,
                PageIndex = Page > 0 ? Page - 1 : 0,
                PageSize = Max,
                SortColumn = "displayname",
                SortAscending = true,
                PortalId = PortalId,
                Filter = UserFilters.All
            };
            if (!string.IsNullOrEmpty(Username))
            {
                // do username lookup
                var searchTerm = Username.Replace("%", "").Replace("*", "");
                getUsersContract.SearchText = searchTerm;
            }
            else if (!string.IsNullOrEmpty(Email))
            {
                // must be email
                var searchTerm = Email.Replace("%", "").Replace("*", "");
                getUsersContract.SearchText = searchTerm;
            }
            else if (!string.IsNullOrEmpty(Role))
            {
                //exact match only allowed at this time. Listing users in multiple roles would require
                // 1) getting all ID's of roles matching search phrase;
                // 2) getting all users in each of those roles;
                // 3) de-duplicating the users list;
                // for large user bases this could take a really long time.
                getUsersContract = null;
                KeyValuePair<HttpStatusCode, string> response;
                var users = UsersController.Instance.GetUsersInRole(PortalSettings, Role, out recCount, out response, Page, Max);
                if (users != null)
                    usersList = ConvertList(users);
                else
                {
                    return new ConsoleErrorResultModel(response.Value);
                }
            }

            if (getUsersContract != null)
            {
                usersList = ConvertList(UsersController.Instance.GetUsers(getUsersContract, User.IsSuperUser, out recCount), PortalId);
            }
            if ((usersList == null || usersList.Count == 0) && recCount == 0)
            {
                return new ConsoleResultModel(LocalizeString("noUsers"));
            }
            var totalPages = recCount / Max + (recCount % Max == 0 ? 0 : 1);
            var pageNo = Page > 0 ? Page : 1;
            return new ConsoleResultModel
            {
                Data = usersList,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    PageSize = Max
                },
                Records = usersList?.Count ?? 0,
                Output = pageNo <= totalPages ? LocalizeString("Prompt_ListUsersOutput") : LocalizeString("noUsers")
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