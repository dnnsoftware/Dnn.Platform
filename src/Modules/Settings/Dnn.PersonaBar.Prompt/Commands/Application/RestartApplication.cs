using System;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Prompt.Commands.Application
{
    [ConsoleCommand("restart-application", "Restarts the application and reloads the page", new string[] { })]
    public class RestartApplication : ConsoleCommandBase
    {
        public override ConsoleResultModel Run()
        {
            try
            {
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()
                };
                log.AddProperty("Message", Localization.GetString("UserRestart", "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Servers/App_LocalResources/Servers.resx"));
                LogController.Instance.AddLog(log);
                DotNetNuke.Common.Utilities.Config.Touch();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return new ConsoleErrorResultModel("An error occurred while attempting to restart the application.");
            }
            return new ConsoleResultModel("Application Restarted") { MustReload = true };
        }
    }
}