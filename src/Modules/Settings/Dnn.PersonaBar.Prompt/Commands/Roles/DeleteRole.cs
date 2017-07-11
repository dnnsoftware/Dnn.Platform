using System;
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
    [ConsoleCommand("delete-role", "Deletes the specified DNN security role for this portal", new string[] { })]
    public class DeleteRole : ConsoleCommandBase, IConsoleCommand
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
            List<RoleModel> lst = new List<RoleModel>();

            var role = RoleController.Instance.GetRoleById(PortalId, (int)RoleId);

            if (role != null)
            {
                switch (role.RoleType)
                {
                    case RoleType.Administrator:
                    case RoleType.RegisteredUser:
                    case RoleType.Subscriber:
                    case RoleType.UnverifiedUser:
                        return new ConsoleErrorResultModel("You cannot delete built-in DNN roles.");
                }
                try
                {
                    rc.DeleteRole(role);
                    lst.Add(new RoleModel(role));
                    return new ConsoleResultModel(string.Format("Successfully deleted role '{0}' ({1})", role.RoleName, role.RoleID));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return new ConsoleErrorResultModel("An error occurred while deleting the role. See the DNN Event Viewer for details.");
                }
            }
            else
            {
                return new ConsoleResultModel(string.Format("No role found with the ID of '{0}'", RoleId));
            }

        }


    }
}