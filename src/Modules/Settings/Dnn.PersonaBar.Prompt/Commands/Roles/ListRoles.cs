using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Prompt.Commands.Roles
{
    [ConsoleCommand("list-roles", "Retrieves a list of DNN security roles for this portal", new string[] { })]
    public class ListRoles : ConsoleCommandBase, IConsoleCommand
    {

        public string ValidationMessage { get; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            RoleController rc = new RoleController();
            List<RoleModelBase> lst = new List<RoleModelBase>();

            var results = rc.GetRoles(PortalId);
            if (results != null)
            {
                foreach (RoleInfo role in results)
                {
                    lst.Add(new RoleModelBase(role));
                }
            }

            return new ConsoleResultModel(string.Format("{0} role{1} found", lst.Count, (lst.Count != 1 ? "s" : ""))) { Data = lst };
        }


    }
}