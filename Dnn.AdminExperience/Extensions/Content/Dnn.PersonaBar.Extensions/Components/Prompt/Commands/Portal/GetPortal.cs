using System.Collections.Generic;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    [ConsoleCommand("get-portal", Constants.PortalCategory, "Prompt_GetPortal_Description")]
    public class GetPortal : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        [FlagParameter("id", "Prompt_GetPortal_FlagId", "Integer")]
        private const string FlagId = "id";

        int PortalIdFlagValue { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            // default usage: return current portal if nothing else specified
            if (args.Length == 1)
            {
                PortalIdFlagValue = PortalId;
            }
            else
            {
                // allow hosts to get info on other portals
                if (User.IsSuperUser && args.Length >= 2)
                {
                    PortalIdFlagValue = GetFlagValue(FlagId, "Portal Id", PortalId, true, true);
                }
                else
                {
                    // admins cannot access info on other portals.
                    AddMessage(LocalizeString("Prompt_GetPortal_NoArgs"));
                }
            }
        }

        public override ConsoleResultModel Run()
        {
            var pc = new PortalController();
            var lst = new List<PortalModel>();

            var portal = pc.GetPortal((int)PortalIdFlagValue);
            if (portal == null)
            {
                return new ConsoleErrorResultModel(string.Format(LocalizeString("Prompt_GetPortal_NotFound"), PortalIdFlagValue));
            }
            lst.Add(new PortalModel(portal));
            return new ConsoleResultModel(string.Empty) { Data = lst, Records = lst.Count, Output = string.Format(LocalizeString("Prompt_GetPortal_Found"), PortalIdFlagValue) };
        }


    }
}