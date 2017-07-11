using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Models;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Host
{
    [ConsoleCommand("get-host", "Retrieves information about the current DNN Installation", new string[] { "id" })]
    public class GetHost : ConsoleCommandBase, IConsoleCommand
    {

        public string ValidationMessage { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            // HOST-ONLY ACCESS
            if (!userInfo.IsSuperUser)
            {
                sbErrors.Append("You do not have authorization to access this functionality");
            }
            else
            {
                // default usage: return current page if nothing else specified
                if (args.Length != 1)
                {
                    sbErrors.Append("The get-host command does not take any arguments or flags; ");
                }
            }



            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            List<HostModel> lst = new List<HostModel>();
            // double-check host access:
            if (base.User.IsSuperUser)
            {
                lst.Add(HostModel.Current());
            }

            return new ConsoleResultModel(string.Empty) { Data = lst };
        }


    }
}

