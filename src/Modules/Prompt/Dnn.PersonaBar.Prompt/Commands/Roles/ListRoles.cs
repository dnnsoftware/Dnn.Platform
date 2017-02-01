using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using System.Collections.Generic;

namespace Dnn.PersonaBar.Prompt.Commands.Role
{
    [ConsoleCommand("list-roles", "Retrieves a list of DNN security roles for this portal", new string[] { })]
    public class ListRoles : BaseConsoleCommand, IConsoleCommand
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
            List<RoleInfoModelSlim> lst = new List<RoleInfoModelSlim>();

            var results = rc.GetRoles(PortalId);
            if (results != null)
            {
                foreach (RoleInfo role in results)
                {
                    lst.Add(RoleInfoModelSlim.FromDnnRoleInfo(role));
                }
            }

            return new ConsoleResultModel(string.Format("{0} role{1} found", lst.Count, (lst.Count != 1 ? "s" : ""))) { data = lst };
        }


    }
}