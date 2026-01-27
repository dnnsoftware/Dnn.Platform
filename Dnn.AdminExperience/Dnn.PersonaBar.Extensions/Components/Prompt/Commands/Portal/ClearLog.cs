// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    using System;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    [ConsoleCommand("clear-log", Constants.PortalCategory, "Prompt_ClearLog_Description")]
    public class ClearLog : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClearLog));
        private readonly IEventLogService eventLogService;

        /// <summary>Initializes a new instance of the <see cref="ClearLog"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogService. Scheduled removal in v12.0.0.")]
        public ClearLog()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ClearLog"/> class.</summary>
        /// <param name="eventLogService">The event logger.</param>
        public ClearLog(IEventLogService eventLogService)
        {
            this.eventLogService = eventLogService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogService>();
        }

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
            try
            {
                this.eventLogService.ClearLog();
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
