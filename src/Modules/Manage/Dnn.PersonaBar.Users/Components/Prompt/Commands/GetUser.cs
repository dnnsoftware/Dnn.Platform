using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
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
                int tmpId;
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
                        int tmpId;
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
            var lst = new List<UserModel>();

            // if no argument, default to current user
            if (Args.Length == 1)
            {
                lst.Add(new UserModel(User));
            }
            else
            {
                var recCount = 0;
                var userId = UserId;
                if (!userId.HasValue && !string.IsNullOrEmpty(Username))
                {
                    // do username lookup
                    var searchTerm = Username.Replace("%", "").Replace("*", "%");
                    userId = (UserController.GetUsersByUserName(PortalId, searchTerm, -1, int.MaxValue, ref recCount, true, false).ToArray().FirstOrDefault() as UserInfo)?.UserID ?? 0;
                }
                else if (!userId.HasValue && !string.IsNullOrEmpty(Email))
                {
                    // must be email
                    var searchTerm = Email.Replace("%", "").Replace("*", "%");
                    userId = (UserController.GetUsersByEmail(PortalId, searchTerm, -1, int.MaxValue, ref recCount, true, false).ToArray().FirstOrDefault() as UserInfo)?.UserID ?? 0;
                }

                ConsoleErrorResultModel errorResultModel;
                UserInfo userInfo;
                if ((errorResultModel = Utilities.ValidateUser(userId, PortalSettings, User, out userInfo)) != null) return errorResultModel;
                lst.Add(new UserModel(userInfo));
            }

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
    }
}