using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("purge-user", "Completely removes a previously deleted user from the portal.", new[] { "id" })]
    public class PurgeUser : ConsoleCommandBase
    {

        private const string FlagId = "id";
        public int? UserId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                int tmpId;
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
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;
            if ((errorResultModel = Utilities.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null) return errorResultModel;

            if (!userInfo.IsDeleted)
                return new ConsoleErrorResultModel("Cannot purge user that has not been deleted first. Try delete-user.");

            if (!UserController.RemoveUser(userInfo))
                return new ConsoleResultModel("No user found to purge");

            lst.Add(new UserModel(userInfo));
            return new ConsoleResultModel("The User has been permanently removed from the site.") { Data = lst };
        }
    }
}