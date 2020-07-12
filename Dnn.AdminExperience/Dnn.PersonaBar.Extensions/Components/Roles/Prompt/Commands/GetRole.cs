// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Roles.Components.Prompt.Models;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("get-role", Constants.RolesCategory, "Prompt_GetRole_Description")]
    public class GetRole : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_GetRole_FlagId", "Integer", true)]
        private const string FlagId = "id";

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing);

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.RoleId = this.GetFlagValue(FlagId, "Role Id", -1, true, true);

            if (this.RoleId < 0)
            {
                this.AddMessage(this.LocalizeString("Prompt_RoleIdNegative"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<RoleModel>();
            var role = RolesController.Instance.GetRole(this.PortalSettings, this.RoleId);
            if (role == null)
                return new ConsoleErrorResultModel(string.Format(this.LocalizeString("Prompt_NoRoleWithId"), this.RoleId));
            lst.Add(new RoleModel(role));
            return new ConsoleResultModel { Data = lst, Records = lst.Count, Output = string.Format(this.LocalizeString("Prompt_RoleFound"), this.RoleId) };
        }
    }
}
