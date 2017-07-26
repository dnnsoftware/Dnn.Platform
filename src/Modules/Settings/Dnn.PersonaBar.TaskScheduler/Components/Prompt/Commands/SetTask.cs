using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    [ConsoleCommand("set-task", "Updates a specific scheduled task with new information", new[]{
        "id",
        "enabled"
    })]
    public class SetTask : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SetTask));

        private const string FlagId = "id";
        private const string FlagEnabled = "enabled";

        private int TaskId { get; set; }
        private bool Enabled { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                int tmpId;
                if (int.TryParse(Flag(FlagId), out tmpId))
                {
                    TaskId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagMustBeNumber", Constants.LocalResourcesFile), FlagId);
                }
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                int tmpId;
                if (int.TryParse(args[1], out tmpId))
                {
                    TaskId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_ScheduleFlagRequired", Constants.LocalResourcesFile), FlagId);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagRequired", Constants.LocalResourcesFile), FlagId);
            }

            if (HasFlag(FlagEnabled))
            {
                bool tmpEnabled;
                if (bool.TryParse(Flag(FlagEnabled), out tmpEnabled))
                {
                    Enabled = tmpEnabled;
                }
                else
                {
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagMustBeTrueFalse", Constants.LocalResourcesFile), FlagEnabled);
                }
            }
            else
            {
                sbErrors.AppendFormat(Localization.GetString("Prompt_FlagRequired", Constants.LocalResourcesFile), FlagEnabled);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            try
            {
                var taskToUpdate = SchedulingProvider.Instance().GetSchedule(TaskId);
                var tasks = new List<TaskModel>();

                if (taskToUpdate == null)
                    return new ConsoleErrorResultModel(string.Format(Localization.GetString("Prompt_TaskNotFound", Constants.LocalResourcesFile), TaskId));
                if (taskToUpdate.Enabled == Enabled)
                    return new ConsoleErrorResultModel(Localization.GetString(Enabled ? "Prompt_TaskAlreadyEnabled" : "Prompt_TaskAlreadyDisabled", Constants.LocalResourcesFile));

                taskToUpdate.Enabled = Enabled;
                SchedulingProvider.Instance().UpdateSchedule(taskToUpdate);
                tasks.Add(new TaskModel(taskToUpdate));
                return new ConsoleResultModel(Localization.GetString("Prompt_TaskUpdated", Constants.LocalResourcesFile))
                {
                    Records = tasks.Count,
                    Data = tasks
                };
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return new ConsoleErrorResultModel(Localization.GetString("Prompt_TaskUpdateFailed", Constants.LocalResourcesFile));
            }
        }
    }
}