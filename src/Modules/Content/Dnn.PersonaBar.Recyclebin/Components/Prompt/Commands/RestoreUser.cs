using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    [ConsoleCommand("restore-user", "Recovers a user that has previously been deleted or 'unregistered'", new[] { "id" })]
    public class RestoreUser : ConsoleCommandBase
    {

        private const string FlagId = "id";
        public int UserId { get; private set; }

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

            if (UserId <= 0)
            {
                sbErrors.Append(Localization.GetString("Prompt_UserIdIsRequired", Constants.LocalResourcesFile));
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var userInfo = UserController.Instance.GetUser(PortalId, UserId);
            if (userInfo == null)
                return new ConsoleErrorResultModel(string.Format(Localization.GetString("UserNotFound", Constants.LocalResourcesFile), UserId));

            if (!userInfo.IsDeleted)
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_RestoreNotRequired", Constants.LocalResourcesFile));

            if (!UserController.RestoreUser(ref userInfo))
                return new ConsoleErrorResultModel(Localization.GetString("UserRestoreError", Constants.LocalResourcesFile));

            string message;
            var restoredUser = RecyclebinController.Instance.RestoreUser(userInfo, out message);
            return restoredUser
                ? new ConsoleResultModel(Localization.GetString("UserRestored", Constants.LocalResourcesFile)) { Records = 1 }
                : new ConsoleErrorResultModel(message);
        }
    }
}