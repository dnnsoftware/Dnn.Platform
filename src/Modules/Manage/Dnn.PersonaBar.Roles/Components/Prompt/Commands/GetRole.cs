using System;
using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("get-role", "Retrieves a DNN security role for this portal")]
    public class GetRole : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagId = "id";

        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing);

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            RoleId = GetFlagValue(FlagId, "Role Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<RoleModel>();
            var role = RolesController.Instance.GetRole(PortalSettings, RoleId);
            if (role == null)
                return new ConsoleErrorResultModel(string.Format(LocalizeString("Prompt_NoRoleWithId"), RoleId));
            lst.Add(new RoleModel(role));
            return new ConsoleResultModel { Data = lst, Records = lst.Count };
        }
    }
}