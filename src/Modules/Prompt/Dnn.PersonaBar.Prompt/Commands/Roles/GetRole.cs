using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Role
{
    [ConsoleCommand("get-role", "Retrieves a DNN security role for this portal", new string[] { })]
    public class GetRole : BaseConsoleCommand, IConsoleCommand
    {
        private const string FLAG_ID = "id";

        public string ValidationMessage { get; private set; }
        public int? RoleId { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);

            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_ID))
            {
                int tmp = 0;
                if (int.TryParse(Flag(FLAG_ID), out tmp))
                {
                    RoleId = tmp;
                }
                else
                {
                    sbErrors.AppendFormat("The --{0} flag must be an integer", FLAG_ID);
                }
            }
            else
            {
                // assume it's the first argument
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    int tmp = 0;
                    if (int.TryParse(args[1], out tmp))
                    {
                        RoleId = tmp;
                    }
                }
            }

            if (!RoleId.HasValue)
            {
                sbErrors.AppendFormat("You must specify a Role ID using either the --{0} flag or by passing the Role ID as the first argument after the command name; ", FLAG_ID);
            }
            else if (RoleId <= 0)
            {
                // validate it's > 0
                sbErrors.AppendFormat("The --{0} flag value must be greater than zero (0)", FLAG_ID);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            RoleController rc = new RoleController();
            List<RoleInfoModel> lst = new List<RoleInfoModel>();

            var role = RoleController.Instance.GetRoleById(PortalId, (int)RoleId);
            if (role != null)
            {
                lst.Add(RoleInfoModel.FromDnnRoleInfo(role));
            }
            else
            {
                return new ConsoleResultModel(string.Format("No role found with the ID of '{0}'", RoleId));
            }

            return new ConsoleResultModel(string.Format("{0} role{1} found", lst.Count, (lst.Count != 1 ? "s" : ""))) { data = lst };
        }


    }
}