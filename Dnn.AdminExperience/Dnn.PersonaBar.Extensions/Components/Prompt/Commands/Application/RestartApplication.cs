// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Application
{
    using System;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;

    [ConsoleCommand("restart-application", Constants.HostCategory, "Prompt_RestartApplication_Description")]
    public class RestartApplication : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RestartApplication));

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public override ConsoleResultModel Run()
        {
            try
            {
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()
                };
                log.AddProperty("Message", this.LocalizeString("Prompt_UserRestart"));
                LogController.Instance.AddLog(log);
                DotNetNuke.Common.Utilities.Config.Touch();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_UserRestart_Error"));
            }
            return new ConsoleResultModel(this.LocalizeString("Prompt_UserRestart_Success")) { MustReload = true };
        }
    }
}
