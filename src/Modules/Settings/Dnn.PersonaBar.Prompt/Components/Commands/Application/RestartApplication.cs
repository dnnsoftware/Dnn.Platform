using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Application
{
    [ConsoleCommand("restart-application", "Prompt_RestartApplication_Description")]
    public class RestartApplication : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RestartApplication));

        public override ConsoleResultModel Run()
        {
            try
            {
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()
                };
                log.AddProperty("Message", LocalizeString("Prompt_UserRestart"));
                LogController.Instance.AddLog(log);
                DotNetNuke.Common.Utilities.Config.Touch();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to restart the application.");
            }
            return new ConsoleResultModel("Application Restarted") { MustReload = true };
        }

        public override string LocalResourceFile => Constants.LocalResourcesFile;
    }
}