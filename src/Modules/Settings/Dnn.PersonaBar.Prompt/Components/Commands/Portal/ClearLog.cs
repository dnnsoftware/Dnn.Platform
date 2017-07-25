using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    [ConsoleCommand("clear-log", "Clears the Event Logo for the current portal")]
    public class ClearLog : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClearLog));
        public override ConsoleResultModel Run()
        {
            try
            {
                EventLogController.Instance.ClearLog();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to clear the Event Log.");
            }
            return new ConsoleResultModel("Event Log Cleared");

        }
    }
}
