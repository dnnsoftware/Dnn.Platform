using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    [ConsoleCommand("list-portals", "Prompt_ListPortals_Description")]
    public class ListPortals : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            if (args.Length == 1)
            {
                // do nothing
            }
            else
            {
                AddMessage("The get-portal command does not take any arguments or flags; ");
            }
        }

        public override ConsoleResultModel Run()
        {
            var pc = PortalController.Instance;

            var alPortals = pc.GetPortals();
            var lst = (from PortalInfo portal in alPortals select new PortalModelBase(portal)).ToList();

            return new ConsoleResultModel(string.Empty) { Data = lst, Records = lst.Count };
        }


    }
}