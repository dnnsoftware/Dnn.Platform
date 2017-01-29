using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using System;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Localization;
using Dnn.PersonaBar.Prompt.Models;

namespace Dnn.PersonaBar.Prompt.Commands.Application
{
    [ConsoleCommand("restart-application", "Restarts the application and reloads the page", new string[] { })]
    public class RestartApplication : BaseConsoleCommand, IConsoleCommand
    {
        public string ValidationMessage { get; }

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
            return new ConsoleResultModel("Application Restarted") { mustReload = true };

        }
    }
}