using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DotNetNuke;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Portal
{
    [ConsoleCommand("get-portal", "Retrieves information about the current portal", new string[] { "id" })]
    public class GetPortal : BaseConsoleCommand, IConsoleCommand
    {
        private const string FLAG_ID = "id";

        public string ValidationMessage { get; private set; }
        public int? PortalIdFlagValue { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

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
                    if (HasFlag(FLAG_ID))
                    {
                        argId = Flag(FLAG_ID);
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
                        int tmpId = 0;
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

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            PortalController pc = new PortalController();
            List<PortalModel> lst = new List<PortalModel>();

            if (PortalIdFlagValue.HasValue)
            {
                PortalInfo portal = pc.GetPortal((int)PortalIdFlagValue);
                if (portal == null)
                {
                    return new ConsoleErrorResultModel($"Could not find a portal with ID of '{PortalIdFlagValue}'");
                }
                lst.Add(new PortalModel(portal));
            }

            return new ConsoleResultModel(string.Empty) { data = lst };
        }


    }
}