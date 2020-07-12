// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    [ConsoleCommand("get-task", Constants.SchedulerCategory, "Prompt_GetTask_Description")]
    public class GetTask : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_GetTask_FlagId", "Integer", true)]
        private const string FlagId = "id";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GetTask));
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int TaskId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {

            this.TaskId = this.GetFlagValue(FlagId, "Task Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var task = SchedulingProvider.Instance().GetSchedule(this.TaskId);
                if (task == null)
                    return new ConsoleErrorResultModel(string.Format(this.LocalizeString("Prompt_TaskNotFound"), this.TaskId));
                var tasks = new List<TaskModel> { new TaskModel(task) };
                return new ConsoleResultModel { Data = tasks, Records = tasks.Count, Output = string.Format(this.LocalizeString("Prompt_TaskFound"), this.TaskId) };
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_FetchTaskFailed"));
            }
        }
    }
}
