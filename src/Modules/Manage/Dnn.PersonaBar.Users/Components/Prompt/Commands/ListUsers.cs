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
using DotNetNuke.Services.Localization;

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

        private const string FlagEmail = "email";
        private const string FlagUsernme = "username";
        private const string FlagRole = "role";
        private const string FlagPage = "page";
        private const string FlagMax = "Max";


        public string Email { get; private set; }
        public string Username { get; private set; }
        public string Role { get; private set; }
        public int Page { get; private set; }
        public int Max { get; private set; } = 500;


        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagEmail))
                Email = Flag(FlagEmail);
            if (HasFlag(FlagUsernme))
                Username = Flag(FlagUsernme);
            if (HasFlag(FlagRole))
                Role = Flag(FlagRole);
            if (HasFlag(FlagPage))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagPage), out tmpId))
                    Page = tmpId;
            }
            if (HasFlag(FlagMax))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagMax), out tmpId))
                    Max = tmpId > 0 && tmpId < 500 ? tmpId : Max;
            }

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
                        sbErrors.AppendFormat(Localization.GetString("Prompt_OnlyOneFlagRequired", Constants.LocalResourcesFile), FlagEmail, FlagUsernme, FlagRole);
                    }
                }

            }
            ValidationMessage = sbErrors.ToString();
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
                Filter = UserFilters.Authorized
            };
            if (!string.IsNullOrEmpty(Username))
            {
                // do username lookup
                var searchTerm = Username.Replace("%", "").Replace("*", "%");
                getUsersContract.SearchText = searchTerm;
            }
            else if (!string.IsNullOrEmpty(Email))
            {
                // must be email
                var searchTerm = Email.Replace("%", "").Replace("*", "%");
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
                return new ConsoleResultModel(Localization.GetString("noUsers", Constants.LocalResourcesFile));
            }
            return new ConsoleResultModel(string.Empty) { Data = usersList, Output = string.Format(Localization.GetString("Prompt_ListUsersOutput", Constants.LocalResourcesFile), recCount, recCount / Max + (recCount % Max == 0 ? 0 : 1), (Page > 0 ? Page : 1), Max) };
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