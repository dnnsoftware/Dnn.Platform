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
    [ConsoleCommand("get-role", Constants.RolesCategory, "Prompt_GetRole_Description")]
    public class GetRole : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_GetRole_FlagId", "Integer", true)]
        private const string FlagId = "id";

        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing);

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            RoleId = GetFlagValue(FlagId, "Role Id", -1, true, true);

            if (RoleId < 0)
            {
                AddMessage(LocalizeString("Prompt_RoleIdNegative"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<RoleModel>();
            var role = RolesController.Instance.GetRole(PortalSettings, RoleId);
            if (role == null)
                return new ConsoleErrorResultModel(string.Format(LocalizeString("Prompt_NoRoleWithId"), RoleId));
            lst.Add(new RoleModel(role));
            return new ConsoleResultModel { Data = lst, Records = lst.Count, Output = string.Format(LocalizeString("Prompt_RoleFound"),RoleId) };
        }
    }
}