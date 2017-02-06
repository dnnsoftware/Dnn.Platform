using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using Dnn.PersonaBar.Prompt.Repositories;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System;
using System.Linq;

namespace Dnn.PersonaBar.Prompt.Commands.Commands
{
    [ConsoleCommand("list-commands", "Lists all available commands", new string[] { })]
    public class ListCommands : ConsoleCommandBase, IConsoleCommand
    {
        public string ValidationMessage { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
        }

        public bool IsValid()
        {
            return true;
        }

        public ConsoleResultModel Run()
        {

            try
            {
                var lstOut = CommandRepository.Instance.GetCommands().Values;
                return new ConsoleResultModel(string.Format("Found {0} commands", lstOut.Count()))
                {
                    data = lstOut,
                    fieldOrder = new string[] {
                    "Name", "Description", "Version", "NameSpace" }
                };
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to restart the application.");
            }
        }
    }
}