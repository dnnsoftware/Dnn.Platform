// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("get-portal", Constants.PortalCategory, "Prompt_GetPortal_Description")]
    public class GetPortal : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_GetPortal_FlagId", "Integer")]
        private const string FlagId = "id";

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        int PortalIdFlagValue { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            // default usage: return current portal if nothing else specified
            if (args.Length == 1)
            {
                this.PortalIdFlagValue = this.PortalId;
            }
            else
            {
                // allow hosts to get info on other portals
                if (this.User.IsSuperUser && args.Length >= 2)
                {
                    this.PortalIdFlagValue = this.GetFlagValue(FlagId, "Portal Id", this.PortalId, true, true);
                }
                else
                {
                    // admins cannot access info on other portals.
                    this.AddMessage(this.LocalizeString("Prompt_GetPortal_NoArgs"));
                }
            }
        }

        public override ConsoleResultModel Run()
        {
            var pc = new PortalController();
            var lst = new List<PortalModel>();

            var portal = pc.GetPortal((int)this.PortalIdFlagValue);
            if (portal == null)
            {
                return new ConsoleErrorResultModel(string.Format(this.LocalizeString("Prompt_GetPortal_NotFound"), this.PortalIdFlagValue));
            }
            lst.Add(new PortalModel(portal));
            return new ConsoleResultModel(string.Empty) { Data = lst, Records = lst.Count, Output = string.Format(this.LocalizeString("Prompt_GetPortal_Found"), this.PortalIdFlagValue) };
        }
    }
}
