using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

namespace Dnn.PersonaBar.Prompt.Commands.Portal
{
    [ConsoleCommand("clear-log", "Clears the Event Logo for the current portal", new string[] { })]
    public class ClearLog : ConsoleCommandBase
    {
        public override ConsoleResultModel Run()
        {
            try
            {
                EventLogController.Instance.ClearLog();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to clear the Event Log.");
            }
            return new ConsoleResultModel("Event Log Cleared") { };

        }
    }
}
