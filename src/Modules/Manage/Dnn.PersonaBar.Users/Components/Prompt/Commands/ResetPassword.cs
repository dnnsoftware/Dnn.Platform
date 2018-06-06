using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    [ConsoleCommand("reset-password", Constants.UsersCategory, "Prompt_ResetPassword_Description")]
    public class ResetPassword : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_ResetPassword_FlagId", "Integer", true)]
        private const string FlagId = "id";
        [FlagParameter("notify", "Prompt_ResetPassword_FlagNotify", "Boolean", "false")]
        private const string FlagNotify = "notify";

        private IUserValidator _userValidator;

        private bool Notify { get; set; }
        private int? UserId { get; set; }

        public ResetPassword() : this(new UserValidator())
        {
        }

        public ResetPassword(IUserValidator userValidator)
        {
            this._userValidator = userValidator;
        }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            UserId = GetFlagValue(FlagId, "User Id", -1, true, true, true);
            Notify = GetFlagValue(FlagNotify, "Notify", false);
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            if (
                (errorResultModel = _userValidator.ValidateUser(
                    UserId,
                    PortalSettings,
                    User,
                    out userInfo)
                ) != null
               )
            {
                return errorResultModel;
            }

            //Don't allow self password change.
            if (userInfo.UserID == User.UserID)
            {
                return new ConsoleErrorResultModel(LocalizeString("InSufficientPermissions"));
            }

            var success = UsersController.Instance.ForceChangePassword(userInfo, PortalId, Notify);
            return success
                ? new ConsoleResultModel(LocalizeString("Prompt_PasswordReset") + (Notify ? LocalizeString("Prompt_EmailSent") : "")) { Records = 1 }
                : new ConsoleErrorResultModel(LocalizeString("OptionUnavailable"));
        }
    }
}