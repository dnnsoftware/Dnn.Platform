using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

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
                sbErrors.Append(Localization.GetString("Prompt_UserIdIsRequired", Constants.LocalResourcesFile) + " ");
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
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_CannotPurgeUser", Constants.LocalResourcesFile));

            if (!UserController.RemoveUser(userInfo))
                return new ConsoleErrorResultModel(Localization.GetString("UserRemoveError", Constants.LocalResourcesFile));

            lst.Add(new UserModel(userInfo));
            return new ConsoleResultModel(Localization.GetString("Prompt_UserPurged", Constants.LocalResourcesFile)) { Data = lst };
        }
    }
}