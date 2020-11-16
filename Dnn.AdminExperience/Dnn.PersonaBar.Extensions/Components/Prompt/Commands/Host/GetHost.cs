// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Host
{
    using System.Collections.Generic;
    using System.Text;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

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
                sbErrors.Append(this.LocalizeString("Prompt_GetHost_Unauthorized"));
            }
            else
            {
                // default usage: return current page if nothing else specified
                if (args.Length != 1)
                {
                    sbErrors.Append(this.LocalizeString("Prompt_GetHost__NoArgs"));
                }
            }
            this.AddMessage(sbErrors.ToString());
        }

        public override ConsoleResultModel Run()
        {
            var lst = new List<HostModel>();
            // double-check host access:
            if (this.User.IsSuperUser)
            {
                lst.Add(HostModel.Current());
            }

            return new ConsoleResultModel(string.Empty) { Data = lst, Output = this.LocalizeString("Prompt_GetHost_OkMessage") };
        }
    }
}

