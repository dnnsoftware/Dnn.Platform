using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Utilities
{
    [ConsoleCommand("echo", "Echos back the first argument received", new string[] { })]
    public class Echo : ConsoleCommandBase, IConsoleCommand
    {

        public string ValidationMessage { get; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
        }

        public bool IsValid()
        {
            return true;
        }

        public ConsoleResultModel Run()
        {
            if (args.Length > 1)
            {
                return new ConsoleResultModel(args[1]);
            }


            return new ConsoleErrorResultModel("Nothing to echo back");
        }

    }
}