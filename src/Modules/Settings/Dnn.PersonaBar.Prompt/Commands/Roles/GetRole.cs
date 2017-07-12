using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Prompt.Commands.Roles
{
    [ConsoleCommand("get-role", "Retrieves a DNN security role for this portal")]
    public class GetRole : ConsoleCommandBase
    {
        private const string FlagId = "id";


        public int? RoleId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);

            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                var tmp = 0;
                if (int.TryParse(Flag(FlagId), out tmp))
                {
                    RoleId = tmp;
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag must be an integer", FlagId);
                }
            }
            else
            {
                // assume it's the first argument
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    var tmp = 0;
                    if (int.TryParse(args[1], out tmp))
                    {
                        RoleId = tmp;
                    }
                }
            }

            if (!RoleId.HasValue)
            {
                sbErrors.AppendFormat("You must specify a Role ID using either the --{0} flag or by passing the Role ID as the first argument after the command name; ", FlagId);
            }
            else if (RoleId <= 0)
            {
                // validate it's > 0
                sbErrors.AppendFormat("The --{0} flag value must be greater than zero (0)", FlagId);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var rc = new RoleController();
            var lst = new List<RoleModel>();

            var role = RoleController.Instance.GetRoleById(PortalId, (int)RoleId);
            if (role != null)
            {
                lst.Add(new RoleModel(role));
            }
            else
            {
                return new ConsoleResultModel($"No role found with the ID of '{RoleId}'");
            }

            return new ConsoleResultModel($"{lst.Count} role{(lst.Count != 1 ? "s" : "")} found") { Data = lst };
        }


    }
}