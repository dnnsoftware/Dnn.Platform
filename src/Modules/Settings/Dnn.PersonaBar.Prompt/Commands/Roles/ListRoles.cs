using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Prompt.Commands.Roles
{
    [ConsoleCommand("list-roles", "Retrieves a list of DNN security roles for this portal")]
    public class ListRoles : ConsoleCommandBase
    {
        public override ConsoleResultModel Run()
        {
            var rc = new RoleController();
            var lst = new List<RoleModelBase>();

            var results = rc.GetRoles(PortalId);
            if (results != null)
            {
                foreach (var role in results)
                {
                    lst.Add(new RoleModelBase(role));
                }
            }
            return new ConsoleResultModel($"{lst.Count} role{(lst.Count != 1 ? "s" : "")} found") { Data = lst };
        }
    }
}