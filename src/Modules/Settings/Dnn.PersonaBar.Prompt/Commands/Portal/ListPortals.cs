using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Portal
{
    [ConsoleCommand("list-portals", "Retrieves a list of portals for the current DNN Installation", new string[] { })]
    public class ListPortals : ConsoleCommandBase, IConsoleCommand
    {
        public string ValidationMessage { get; private set; }
        public int? PortalIdFlagValue { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (args.Length == 1)
            {
                // do nothing
            }
            else
            {
                sbErrors.Append("The get-portal command does not take any arguments or flags; ");
            }


            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            var pc = PortalController.Instance;
            List<PortalModelBase> lst = new List<PortalModelBase>();

            var alPortals = pc.GetPortals();
            foreach (PortalInfo portal in alPortals)
            {
                lst.Add(new PortalModelBase(portal));
            }

            return new ConsoleResultModel(string.Empty) { Data = lst };
        }


    }
}