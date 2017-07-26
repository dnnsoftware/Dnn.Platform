using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Roles.Components.Prompt.Models;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Roles.Components.Prompt.Commands
{
    [ConsoleCommand("get-role", "Retrieves a DNN security role for this portal")]
    public class GetRole : ConsoleCommandBase
    {
        private const string FlagId = "id";

        public int RoleId { get; private set; } = Convert.ToInt32(Globals.glbRoleNothing);

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                int tmp;
                if (int.TryParse(Flag(FlagId), out tmp))
                {
                    RoleId = tmp;
                }
                else
                {
                    sbErrors.Append(Localization.GetString("Prompt_RoleIdNotInt", Constants.LocalResourcesFile));
                }
            }
            else
            {
                // assume it's the first argument
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    int tmp;
                    if (int.TryParse(args[1], out tmp))
                    {
                        RoleId = tmp;
                    }
                }
            }

            if (RoleId == Convert.ToInt32(Globals.glbRoleNothing))
            {
                sbErrors.Append(Localization.GetString("Prompt_RoleIdIsRequired", Constants.LocalResourcesFile));
            }
            else if (RoleId <= 0)
            {
                // validate it's > 0
                sbErrors.AppendFormat(Localization.GetString("Prompt_RoleIdNegative", Constants.LocalResourcesFile));
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<RoleModel>();
            var role = RolesController.Instance.GetRole(PortalSettings, RoleId);
            if (role == null)
                return new ConsoleErrorResultModel(string.Format(Localization.GetString("Prompt_NoRoleWithId", Constants.LocalResourcesFile), RoleId));
            lst.Add(new RoleModel(role));
            return new ConsoleResultModel { Data = lst, Records = lst.Count };
        }
    }
}