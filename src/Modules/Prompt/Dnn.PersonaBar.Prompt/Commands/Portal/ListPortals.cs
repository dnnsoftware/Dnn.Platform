using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Portal
{
    [ConsoleCommand("list-portals", "Retrieves a list of portals for the current DNN Installation", new string[] { })]
    public class ListPortals : BaseConsoleCommand, IConsoleCommand
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
            List<PortalInfoModelSlim> lst = new List<PortalInfoModelSlim>();

            var alPortals = pc.GetPortals();
            foreach (PortalInfo portal in alPortals)
            {
                lst.Add(PortalInfoModelSlim.FromDnnPortalInfo(portal));
            }

            return new ConsoleResultModel(string.Empty) { data = lst };
        }


    }
}