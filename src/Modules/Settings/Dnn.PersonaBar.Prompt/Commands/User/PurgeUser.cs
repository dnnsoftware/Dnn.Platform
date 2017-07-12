using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Commands.User
{
    [ConsoleCommand("purge-user", "Completely removes a previously deleted user from the portal.", new[] { "id" })]
    public class PurgeUser : ConsoleCommandBase
    {

        private const string FlagId = "id";


        public int? UserId { get; private set; }
        public bool? Notify { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                var tmpId = 0;
                if (args.Length == 2 && int.TryParse(args[1], out tmpId))
                {
                    UserId = tmpId;
                }
            }

            if (!UserId.HasValue)
            {
                sbErrors.AppendFormat("You must specify a valid numeric User ID using the --{0} flag or by passing it as the first argument; ", FlagId);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<UserModel>();

            var sbErrors = new StringBuilder();
            if (UserId.HasValue)
            {
                // do lookup by user id
                var ui = UserController.GetUserById(PortalId, (int)UserId);
                if (ui != null)
                {
                    if (ui.IsDeleted)
                    {
                        if (UserController.RemoveUser(ui))
                        {
                            lst.Add(new UserModel(ui));
                            return new ConsoleResultModel("The User has been permanently removed from the site.") { Data = lst };
                        }
                    }
                    else
                    {
                        return new ConsoleErrorResultModel("Cannot purge user that has not been deleted first. Try delete-user.");
                    }
                }
                else
                {
                    return new ConsoleErrorResultModel($"No user found with the ID of '{UserId}'");
                }
            }

            // shouldn't get here.
            return new ConsoleResultModel("No user found to purge");
        }
    }
}