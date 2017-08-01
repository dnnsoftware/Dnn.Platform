using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("get-user", "Prompt_GetUser_Description", new[]{
        "id",
        "email",
        "username"
    })]
    public class GetUser : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_GetUser_FlagId", "Integer")]
        private const string FlagId = "id";
        [FlagParameter("email", "Prompt_GetUser_FlagEmail", "String")]
        private const string FlagEmail = "email";
        [FlagParameter("username", "Prompt_GetUser_FlagUsername", "String")]
        private const string FlagUsername = "username";


        private int? UserId { get; set; }
        private string Email { get; set; }
        private string Username { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            UserId = GetFlagValue<int?>(FlagId, "User Id", null);
            Email = GetFlagValue(FlagEmail, "Email", string.Empty);
            Username = GetFlagValue(FlagUsername, "Username", string.Empty);

            if (args.Length != 1)
            {
                if (args.Length == 2 && !UserId.HasValue && string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Username))
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
                if (!UserId.HasValue && string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Username))
                {
                    AddMessage(LocalizeString("Prompt_SearchUserParameterRequired"));
                }
            }
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
                Records = lst.Count,
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