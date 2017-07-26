using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("reset-password", "Resets the user's password", new[]{
        "id",
        "notify"
    })]
    public class ResetPassword : ConsoleCommandBase
    {
        private const string FlagId = "id";
        private const string FlagNotify = "notify";


        public bool Notify { get; private set; } = true;
        public int? UserId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            int tmpId;
            if (HasFlag(FlagId))
            {
                if (int.TryParse(Flag(FlagId), out tmpId))
                    UserId = tmpId;
            }
            else
            {
                if (int.TryParse(args[1], out tmpId))
                    UserId = tmpId;
            }

            if (HasFlag(FlagNotify))
            {
                bool tmpNotify;
                if (bool.TryParse(Flag(FlagNotify), out tmpNotify))
                {
                    Notify = tmpNotify;
                }
                else
                {
                    sbErrors.Append(string.Format(Localization.GetString("Prompt_IfSpecifiedMustHaveValue", Constants.LocalResourcesFile), FlagNotify) + " ");
                }
            }

            if (!UserId.HasValue)
            {
                sbErrors.Append(Localization.GetString("Prompt_UserIdIsRequired", Constants.LocalResourcesFile));
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;
            if ((errorResultModel = Utilities.ValidateUser(UserId, PortalSettings, User, out userInfo)) != null) return errorResultModel;
            //Don't allow self password change.
            if (userInfo.UserID == User.UserID)
            {
                return new ConsoleErrorResultModel(Localization.GetString("InSufficientPermissions", Constants.LocalResourcesFile));
            }
            var success = UsersController.Instance.ForceChangePassword(userInfo, PortalId, Notify);
            return success
                ? new ConsoleResultModel(Localization.GetString("Prompt_PasswordReset", Constants.LocalResourcesFile) + (Notify ? Localization.GetString("Prompt_EmailSent", Constants.LocalResourcesFile) : "")) { Records = 1 }
                : new ConsoleErrorResultModel(Localization.GetString("OptionUnavailable", Constants.LocalResourcesFile));
        }
    }
}