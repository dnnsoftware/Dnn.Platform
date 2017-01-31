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

    public string ValidationMessage { get; private set; }
    public int? PortalIdFlagValue { get; private set; }

    public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
    {
        base.Initialize(args, portalSettings, userInfo, activeTabId);
        StringBuilder sbErrors = new StringBuilder();

        // default usage: return current page if nothing else specified
        if (args.Length == 1)
        {
            // currently prompt only works on current portal
            PortalIdFlagValue = PortalId;
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
        PortalController pc = new PortalController();
        List<PortalInfoModel> lst = new List<PortalInfoModel>();

        if (PortalIdFlagValue.HasValue)
        {
            lst.Add(PortalInfoModel.FromDnnPortalInfo(PortalSettings));
        }

        return new ConsoleResultModel(string.Empty) { data = lst };
    }


}
}