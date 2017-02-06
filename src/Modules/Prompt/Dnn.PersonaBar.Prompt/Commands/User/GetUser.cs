using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("get-user", "Returns users that match the given expression", new string[]{
        "id",
        "email",
        "username"
    })]
    public class GetUser : ConsoleCommandBase, IConsoleCommand
    {
        private const string FLAG_ID = "id";
        private const string FLAG_EMAIL = "email";
        private const string FLAG_USERNAME = "username";

        public string ValidationMessage { get; private set; }
        public int? UserId { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            // If no 'find flags (email, id, etc.) are specied, return current user. this is handled in Run so we don't have 
            // to do another datbase lookup

            if (HasFlag(FLAG_ID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_ID), out tmpId))
                    UserId = tmpId;
            }
            if (HasFlag(FLAG_EMAIL))
                Email = Flag(FLAG_EMAIL);
            if (HasFlag(FLAG_USERNAME))
                Username = Flag(FLAG_USERNAME);

            if (args.Length != 1)
            {
                if (args.Length == 2 && !UserId.HasValue && Email == null && Username == null)
                {
                    // only one value passed and it's not a flagged value. Try to interpret it.
                    if (args[1].Contains("@"))
                    {
                        Email = args[1];
                    }
                    else
                    {
                        int tmpId = 0;
                        if (int.TryParse(args[1], out tmpId))
                        {
                            UserId = tmpId;
                        }
                        else
                        {
                            // assume it's a username
                            Username = args[1];
                        }
                    }
                }
                if (!UserId.HasValue && Email == null && Username == null)
                {
                    sbErrors.Append("To search for a user, you must specify either --id (UserId), --email (User Email), or --name (Username)");
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
            ArrayList results = new ArrayList();
            int recCount = 0;

            StringBuilder sbErrors = new StringBuilder();
            // if no argument, default to current user
            if (args.Length == 1)
            {
                results.Add(User);
            }
            else
            {
                if (UserId.HasValue)
                {
                    // do lookup by user id
                    var ui = UserController.GetUserById(PortalId, (int)UserId);
                    if (ui != null)
                    {
                        results.Add(ui);
                    }
                }
                else if (!string.IsNullOrEmpty(Username))
                {
                    // do username lookup
                    var searchTerm = Username.Replace("%", "").Replace("*", "%");
                    results = UserController.GetUsersByUserName(PortalId, searchTerm, -1, int.MaxValue, ref recCount);
                }
                else
                {
                    // must be email
                    var searchTerm = Email.Replace("%", "").Replace("*", "%");
                    results = UserController.GetUsersByEmail(PortalId, searchTerm, -1, int.MaxValue, ref recCount);
                }
            }


            List<UserModel> lst = new List<UserModel>();
            // this is a singular command. Only return the first result
            if (results.Count > 0)
            {
                UserInfo foundUser = (UserInfo)results[0];
                // ensure users cannot get info on super user accounts
                if (User.IsSuperUser || !foundUser.IsSuperUser)
                {
                    lst.Add(new UserModel((UserInfo)results[0]));
                }
            }

            if (lst.Count > 0)
            {
                return new ConsoleResultModel(string.Empty) { data = lst };
            }
            return new ConsoleResultModel("No user found");
        }


    }
}