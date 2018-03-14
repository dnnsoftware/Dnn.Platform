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

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    [ConsoleCommand("get-task", Constants.SchedulerCategory, "Prompt_GetTask_Description")]
    public class GetTask : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GetTask));

        [FlagParameter("id", "Prompt_GetTask_FlagId", "Integer", true)]
        private const string FlagId = "id";

        private int TaskId { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            
            TaskId = GetFlagValue(FlagId, "Task Id", -1, true, true, true);
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var task = SchedulingProvider.Instance().GetSchedule(TaskId);
                if (task == null)
                    return new ConsoleErrorResultModel(string.Format(LocalizeString("Prompt_TaskNotFound"), TaskId));
                var tasks = new List<TaskModel> { new TaskModel(task) };
                return new ConsoleResultModel { Data = tasks, Records = tasks.Count, Output = string.Format(LocalizeString("Prompt_TaskFound"), TaskId) };
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return new ConsoleErrorResultModel(LocalizeString("Prompt_FetchTaskFailed"));
            }
        }
        public override string LocalResourceFile => Constants.LocalResourcesFile;
    }
}