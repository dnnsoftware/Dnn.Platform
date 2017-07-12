using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("get-user", "Returns users that match the given expression", new[]{
        "id",
        "email",
        "username"
    })]
    public class GetUser : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagEmail = "email";
        private const string FlagUsername = "username";


        public int? UserId { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            // If no 'find flags (email, id, etc.) are specied, return current user. this is handled in Run so we don't have 
            // to do another datbase lookup

            if (HasFlag(FlagId))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            if (HasFlag(FlagEmail))
                Email = Flag(FlagEmail);
            if (HasFlag(FlagUsername))
                Username = Flag(FlagUsername);

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
                        var tmpId = 0;
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

        public override ConsoleResultModel Run()
        {
            var results = new ArrayList();
            var recCount = 0;

            var sbErrors = new StringBuilder();
            // if no argument, default to current user
            if (Args.Length == 1)
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


            var lst = new List<UserModel>();
            // this is a singular command. Only return the first result
            if (results.Count > 0)
            {
                var foundUser = (UserInfo)results[0];
                // ensure users cannot get info on super user accounts
                if (User.IsSuperUser || !foundUser.IsSuperUser)
                {
                    lst.Add(new UserModel((UserInfo)results[0]));
                }
            }

            if (lst.Count > 0)
            {
                return new ConsoleResultModel(string.Empty)
                {
                    Data = lst,
                    FieldOrder = new[]
                    {
                        "UserId",
                        "Username",
                        "DisplayName",
                        "FirstName",
                        "LastName",
                        "Email",
                        "LastActivity",
                        "LastLogin",
                        "LastLockout",
                        "LastPasswordChange",
                        "IsDeleted",
                        "IsAuthorized",
                        "IsLockedOut",
                        "Created"
                    }
                };
            }
            return new ConsoleResultModel("No user found");
        }


    }
}