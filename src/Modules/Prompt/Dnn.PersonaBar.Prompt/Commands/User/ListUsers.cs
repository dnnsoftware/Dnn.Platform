using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("list-users", "Returns users that match the given expression", new string[]{
        "email",
        "username",
        "role"
    })]
    public class ListUsers : BaseConsoleCommand, IConsoleCommand
    {

        private const string FLAG_EMAIL = "email";
        private const string FLAG_USERNME = "username";
        private const string FLAG_ROLE = "role";

        public string ValidationMessage { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }
        public string Role { get; private set; }


        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_EMAIL))
                Email = Flag(FLAG_EMAIL);
            if (HasFlag(FLAG_USERNME))
                Username = Flag(FLAG_USERNME);
            if (HasFlag(FLAG_ROLE))
                Role = Flag(FLAG_ROLE);

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
                    int numFilters = 0;
                    if (!string.IsNullOrEmpty(Email))
                        numFilters += 1;
                    if (!string.IsNullOrEmpty(Username))
                        numFilters += 1;
                    if (!string.IsNullOrEmpty(Role))
                        numFilters += 1;

                    if (numFilters != 1)
                    {
                        sbErrors.AppendFormat("You must specify one and only one flag: --{0}, --{1}, or --{2}; ", FLAG_EMAIL, FLAG_USERNME, FLAG_ROLE);
                    }
                }

            }
            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            List<UserInfoModelSlim> lst = new List<UserInfoModelSlim>();
            ArrayList results = new ArrayList();
            int recCount = 0;

            StringBuilder sbErrors = new StringBuilder();
            // if no argument, default to listing all users in current portal
            if (args.Length == 1)
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
                return new ConsoleResultModel(string.Empty) { data = lst };
            }

        }

        private List<UserInfoModelSlim> ConvertList(IEnumerable lst)
        {
            List<UserInfoModelSlim> lstUsers = new List<UserInfoModelSlim>();
            foreach (UserInfo ui in lst)
            {
                lstUsers.Add(UserInfoModelSlim.FromDnnUserInfo(ui));
            }
            return lstUsers;
        }

    }
}