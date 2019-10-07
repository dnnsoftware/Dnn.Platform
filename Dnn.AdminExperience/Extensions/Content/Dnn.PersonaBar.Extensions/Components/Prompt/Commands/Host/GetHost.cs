using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Host
{
    [ConsoleCommand("get-host", Constants.HostCategory, "Prompt_GetHost_Description")]
    public class GetHost : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            var sbErrors = new StringBuilder();

            // HOST-ONLY ACCESS
            if (!userInfo.IsSuperUser)
            {
                sbErrors.Append(LocalizeString("Prompt_GetHost_Unauthorized"));
            }
            else
            {
                // default usage: return current page if nothing else specified
                if (args.Length != 1)
                {
                    sbErrors.Append(LocalizeString("Prompt_GetHost__NoArgs"));
                }
            }
            AddMessage(sbErrors.ToString());
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<HostModel>();
            // double-check host access:
            if (User.IsSuperUser)
            {
                lst.Add(HostModel.Current());
            }

            return new ConsoleResultModel(string.Empty) { Data = lst, Output = LocalizeString("Prompt_GetHost_OkMessage") };
        }


    }
}

