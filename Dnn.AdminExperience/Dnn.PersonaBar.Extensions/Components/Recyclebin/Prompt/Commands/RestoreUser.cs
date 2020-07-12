// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    using Constants = Dnn.PersonaBar.Recyclebin.Components.Constants;

    [ConsoleCommand("restore-user", Constants.RecylcleBinCategory, "Prompt_RestoreUser_Description")]
    public class RestoreUser : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_RestoreUser_FlagId", "Integer", true)]
        private const string FlagId = "id";

        private IUserValidator _userValidator;
        private IRecyclebinController _recyclebinController;

        public RestoreUser() : this(new UserValidator(), RecyclebinController.Instance)
        {
        }

        public RestoreUser(IUserValidator userValidator, IRecyclebinController instance)
        {
            this._userValidator = userValidator;
            this._recyclebinController = instance;
        }

        public override string LocalResourceFile => Constants.LocalResourcesFile;
        private int UserId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            UserInfo userInfo;
            this._userValidator.ValidateUser(this.UserId, this.PortalSettings, this.User, out userInfo);

            if (userInfo == null)
                return new ConsoleErrorResultModel(string.Format(this.LocalizeString("UserNotFound"), this.UserId));

            if (!userInfo.IsDeleted)
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_RestoreNotRequired"));

            string message;
            var restoredUser = this._recyclebinController.RestoreUser(userInfo, out message);
            return restoredUser
                ? new ConsoleResultModel(this.LocalizeString("UserRestored")) { Records = 1 }
                : new ConsoleErrorResultModel(message);
        }
    }
}
