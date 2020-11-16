// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("list-portals", Constants.PortalCategory, "Prompt_ListPortals_Description")]
    public class ListPortals : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            if (args.Length == 1)
            {
                // do nothing
            }
            else
            {
                this.AddMessage(this.LocalizeString("Prompt_ListPortals_NoArgs"));
            }
        }

        public override ConsoleResultModel Run()
        {
            var pc = PortalController.Instance;

            var alPortals = pc.GetPortals();
            var lst = (from PortalInfo portal in alPortals select new PortalModelBase(portal)).ToList();
            var count = lst.Count() > 0 ? lst.Count().ToString() : "No";
            var pluralSuffix = lst.Count() > 1 ? "s" : "";
            return new ConsoleResultModel(string.Empty) { Data = lst, Records = lst.Count, Output = string.Format(this.LocalizeString("Prompt_ListPortals_Results"), count, pluralSuffix) };
        }
    }
}
