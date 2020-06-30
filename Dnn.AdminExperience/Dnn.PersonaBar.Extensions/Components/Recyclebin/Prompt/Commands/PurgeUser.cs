// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("purge-user", Constants.RecylcleBinCategory, "Prompt_PurgeUser_Description")]
    public class PurgeUser : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_PurgeUser_FlagId", "Integer", true)]
        private const string FlagId = "id";

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int UserId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.UserId = this.GetFlagValue(FlagId, "User Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            var userInfo = UserController.Instance.GetUser(this.PortalId, this.UserId);
            if (userInfo == null)
                return new ConsoleErrorResultModel(string.Format(this.LocalizeString("UserNotFound"), this.UserId));
            if (!userInfo.IsDeleted)
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_CannotPurgeUser"));

            RecyclebinController.Instance.DeleteUsers(new List<UserInfo> { userInfo });
            return new ConsoleResultModel(this.LocalizeString("Prompt_UserPurged")) { Records = 1 };
        }
    }
}
