using System;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Library.Prompt.Attributes;

namespace Dnn.PersonaBar.Prompt.Commands.Client
{
    [ConsoleCommand("clear-history", "Clears history of commands used in current session", new string[] { })]
    public class ClearHistory : IConsoleCommand
    {

        public string ValidationMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public ConsoleResultModel Run()
        {
            throw new NotImplementedException();
        }

    }
}