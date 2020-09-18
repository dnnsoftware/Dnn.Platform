// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Commands
{
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("reset-password", Constants.UsersCategory, "Prompt_ResetPassword_Description")]
    public class ResetPassword : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_ResetPassword_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("notify", "Prompt_ResetPassword_FlagNotify", "Boolean", "false")]
        private const string FlagNotify = "notify";

        private IUserValidator _userValidator;

        public ResetPassword() : this(new UserValidator())
        {
        }

        public ResetPassword(IUserValidator userValidator)
        {
            this._userValidator = userValidator;
        }

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private bool Notify { get; set; }
        private int? UserId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
            this.Notify = this.GetFlagValue(FlagNotify, "Notify", false);
        }

        public override ConsoleResultModel Run()
        {
            ConsoleErrorResultModel errorResultModel;
            UserInfo userInfo;

            if (
                (errorResultModel = this._userValidator.ValidateUser(
                    this.UserId,
                    this.PortalSettings,
                    this.User,
                    out userInfo)
                ) != null
               )
            {
                return errorResultModel;
            }

            //Don't allow self password change.
            if (userInfo.UserID == this.User.UserID)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("InSufficientPermissions"));
            }

            var success = UsersController.Instance.ForceChangePassword(userInfo, this.PortalId, this.Notify);
            return success
                ? new ConsoleResultModel(this.LocalizeString("Prompt_PasswordReset") + (this.Notify ? this.LocalizeString("Prompt_EmailSent") : "")) { Records = 1 }
                : new ConsoleErrorResultModel(this.LocalizeString("OptionUnavailable"));
        }
    }
}
