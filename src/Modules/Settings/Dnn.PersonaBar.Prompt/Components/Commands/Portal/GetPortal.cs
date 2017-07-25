using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    [ConsoleCommand("get-portal", "Retrieves information about the current portal", new[] { "id" })]
    public class GetPortal : ConsoleCommandBase
    {
        private const string FlagId = "id";


        public int? PortalIdFlagValue { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

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
                    string argId = null;
                    if (HasFlag(FlagId))
                    {
                        argId = Flag(FlagId);
                    }
                    else
                    {
                        if (!IsFlag(args[1]))
                        {
                            argId = args[1];
                        }
                    }
                    if (!string.IsNullOrEmpty(argId))
                    {
                        var tmpId = 0;
                        if (int.TryParse(argId, out tmpId))
                        {
                            PortalIdFlagValue = tmpId;
                        }
                    }
                }
                else
                {
                    // admins cannot access info on other portals.
                    sbErrors.Append("The get-portal command does not take any arguments or flags; ");
                }
            }

            if (!PortalIdFlagValue.HasValue)
            {
                sbErrors.Append("No valid Portal ID provided; ");
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var pc = new PortalController();
            var lst = new List<PortalModel>();

            if (PortalIdFlagValue.HasValue)
            {
                var portal = pc.GetPortal((int)PortalIdFlagValue);
                if (portal == null)
                {
                    return new ConsoleErrorResultModel($"Could not find a portal with ID of '{PortalIdFlagValue}'");
                }
                lst.Add(new PortalModel(portal));
            }

            return new ConsoleResultModel(string.Empty) { Data = lst, Records = lst.Count };
        }


    }
}