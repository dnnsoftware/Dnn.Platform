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
    [ConsoleCommand("restore-user", "Recovers a user that has previously been deleted or 'unregistered'", new[] { "id" })]
    public class RestoreUser : ConsoleCommandBase
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
                        if (UserController.RestoreUser(ref ui))
                        {
                            var restoredUser = UserController.GetUserById(PortalId, (int)UserId);
                            lst.Add(new UserModel(restoredUser));
                            return new ConsoleResultModel("Successfully recovered the user.") { Data = lst };
                        }
                        else
                        {
                            return new ConsoleErrorResultModel("The system was unable to restore the user");
                        }
                    }
                    else
                    {
                        return new ConsoleResultModel("This user has not been deleted. Nothing to restore.");
                    }
                }
                else
                {
                    return new ConsoleErrorResultModel($"No user found with the ID of '{UserId}'");
                }
            }

            // shouldn't get here.
            return new ConsoleResultModel("No user found to restore");
        }
    }
}