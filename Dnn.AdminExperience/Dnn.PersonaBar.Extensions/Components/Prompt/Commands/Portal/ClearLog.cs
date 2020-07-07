// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    using System;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;

    [ConsoleCommand("clear-log", Constants.PortalCategory, "Prompt_ClearLog_Description")]
    public class ClearLog : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClearLog));

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        public override ConsoleResultModel Run()
        {
            try
            {
                EventLogController.Instance.ClearLog();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_ClearLog_Error"));
            }
            return new ConsoleResultModel(this.LocalizeString("Prompt_ClearLog_Success"));

        }
    }
}
