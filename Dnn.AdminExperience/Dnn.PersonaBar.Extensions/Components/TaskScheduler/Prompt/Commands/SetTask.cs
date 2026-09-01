// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    using Microsoft.Extensions.Logging;

    [ConsoleCommand("set-task", Constants.SchedulerCategory, "Prompt_SetTask_Description")]
    public class SetTask : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_SetTask_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("enabled", "Prompt_SetTask_FlagEnabled", "Boolean", true)]
        private const string FlagEnabled = "enabled";

        private static readonly ILogger Logger = DnnLoggingController.GetLogger<SetTask>();

        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int TaskId { get; set; }

        private bool Enabled { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            this.TaskId = this.GetFlagValue(FlagId, "Task Id", -1, true, true, true);
            this.Enabled = this.GetFlagValue(FlagEnabled, "Enabled", true, true);
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var taskToUpdate = SchedulingProvider.Instance().GetSchedule(this.TaskId);
                var tasks = new List<TaskModel>();

                if (taskToUpdate == null)
                {
                    return new ConsoleErrorResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Prompt_TaskNotFound"), this.TaskId));
                }

                if (taskToUpdate.Enabled == this.Enabled)
                {
                    return new ConsoleErrorResultModel(this.LocalizeString(this.Enabled ? "Prompt_TaskAlreadyEnabled" : "Prompt_TaskAlreadyDisabled"));
                }

                taskToUpdate.Enabled = this.Enabled;
                SchedulingProvider.Instance().UpdateSchedule(taskToUpdate);
                tasks.Add(new TaskModel(taskToUpdate));
                return new ConsoleResultModel(this.LocalizeString("Prompt_TaskUpdated"))
                {
                    Records = tasks.Count,
                    Data = tasks,
                };
            }
            catch (Exception exc)
            {
                Logger.SetTaskRunException(exc);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_TaskUpdateFailed"));
            }
        }
    }
}
