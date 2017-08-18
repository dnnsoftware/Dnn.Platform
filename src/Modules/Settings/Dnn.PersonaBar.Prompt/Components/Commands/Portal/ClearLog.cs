using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    [ConsoleCommand("clear-log", Constants.PortalCategory, "Prompt_ClearLog_Description")]
    public class ClearLog : ConsoleCommandBase
    {
        public override string LocalResourceFile => Constants.LocalResourcesFile;

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
                return new ConsoleErrorResultModel(LocalizeString("Prompt_ClearLog_Error"));
            }
            return new ConsoleResultModel(LocalizeString("Prompt_ClearLog_Success"));

        }
    }
}
