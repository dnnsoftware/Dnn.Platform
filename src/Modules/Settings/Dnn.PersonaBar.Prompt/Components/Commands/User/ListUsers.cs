using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Prompt.Components.Commands.User
{
    [ConsoleCommand("list-users", "Returns users that match the given expression", new[]{
        "email",
        "username",
        "role"
    })]
    public class ListUsers : ConsoleCommandBase
    {

        private const string FlagEmail = "email";
        private const string FlagUsernme = "username";
        private const string FlagRole = "role";


        public string Email { get; private set; }
        public string Username { get; private set; }
        public string Role { get; private set; }


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

            if (args.Length != 1)
            {
                // if only one value passed and it's not a flag, try to interpret as username or email
                if (args.Length == 2 && !IsFlag(args[1]))
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
                        sbErrors.AppendFormat("You must specify one and only one flag: --{0}, --{1}, or --{2}; ", FlagEmail, FlagUsernme, FlagRole);
                    }
                }

            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<UserModelBase>();
            var results = new ArrayList();
            var recCount = 0;

            var sbErrors = new StringBuilder();
            // if no argument, default to listing all users in current portal
            if (Args.Length == 1)
            {
                results = UserController.GetUsers(PortalId);
                lst = ConvertList(results);
            }
            else
            {
                if (!string.IsNullOrEmpty(Username))
                {
                    // do username lookup
                    var searchTerm = Username.Replace("%", "").Replace("*", "%");
                    results = UserController.GetUsersByUserName(PortalId, searchTerm, -1, int.MaxValue, ref recCount);
                    lst = ConvertList(results);
                }
                else if (!string.IsNullOrEmpty(Role))
                {
                    //exact match only allowed at this time. Listing users in multiple roles would require
                    // 1) getting all ID's of roles matching search phrase;
                    // 2) getting all users in each of those roles;
                    // 3) de-duplicating the users list;
                    // for large user bases this could take a really long time.
                    var lstUsers = RoleController.Instance.GetUsersByRole(PortalId, Role);
                    lst = ConvertList(lstUsers);
                }
                else
                {
                    // must be email
                    var searchTerm = Email.Replace("%", "").Replace("*", "%");
                    results = UserController.GetUsersByEmail(PortalId, searchTerm, -1, int.MaxValue, ref recCount);
                    lst = ConvertList(results);
                }
            }

            if (lst == null || lst.Count == 0)
            {
                return new ConsoleResultModel("No users found");
            }
            else
            {
                return new ConsoleResultModel(string.Empty) { Data = lst };
            }

        }

        private List<UserModelBase> ConvertList(IEnumerable lst)
        {
            var lstUsers = new List<UserModelBase>();
            foreach (UserInfo ui in lst)
            {
                lstUsers.Add(new UserModelBase(ui));
            }
            return lstUsers;
        }

    }
}