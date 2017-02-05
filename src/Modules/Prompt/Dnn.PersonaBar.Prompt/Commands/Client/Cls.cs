using System;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Commands.Client
{
    [ConsoleCommand("cls", "Clears the console screen", new string[] { })]
    public class Cls : IConsoleCommand
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