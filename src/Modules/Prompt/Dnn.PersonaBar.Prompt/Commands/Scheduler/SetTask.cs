using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Scheduler
{
    [ConsoleCommand("set-task", "Updates a specific scheduled task with new information", new string[]{
        "id",
        "enabled"
    })]
    public class SetTask : ConsoleCommandBase, IConsoleCommand
    {

        private const string FLAG_ID = "id";
        private const string FLAG_ENABLED = "enabled";

        public string ValidationMessage { get; private set; }
        public int? TaskId { get; private set; }
        public bool? Enabled { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_ID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_ID), out tmpId))
                {
                    TaskId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag must be a number; ", FLAG_ID);
                }
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                int tmpId = 0;
                if (int.TryParse(args[1], out tmpId))
                {
                    TaskId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("You must specify the scheduled item's ID using the --{0} flag or by passing the number as the first argument; ", FLAG_ID);
                }
            }

            if (HasFlag(FLAG_ENABLED))
            {
                bool tmpEnabled = false;
                if (bool.TryParse(Flag(FLAG_ENABLED), out tmpEnabled))
                {
                    Enabled = tmpEnabled;
                }
                else
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag must be set to True or False", FLAG_ENABLED);
                }
            }
            else
            {
                sbErrors.AppendFormat("The --{0} flag is required", FLAG_ENABLED);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {

            var taskToUpdate = SchedulingController.GetSchedule(Convert.ToInt32(TaskId));
            List<TaskModel> lst = new List<TaskModel>();

            if (taskToUpdate != null)
            {
                taskToUpdate.Enabled = (bool)Enabled;
                var _with1 = taskToUpdate;

                SchedulingController.UpdateSchedule(_with1.ScheduleID, _with1.TypeFullName, _with1.TimeLapse, _with1.TimeLapseMeasurement, _with1.RetryTimeLapse, _with1.RetryTimeLapseMeasurement, _with1.RetainHistoryNum, _with1.AttachToEvent, _with1.CatchUpEnabled, _with1.Enabled,
                _with1.ObjectDependencies, _with1.Servers, _with1.FriendlyName);
                var updatedTask = SchedulingController.GetSchedule(Convert.ToInt32(TaskId));
                if (updatedTask != null)
                {
                    lst.Add(new TaskModel(updatedTask));
                }

            }

            return new ConsoleResultModel(string.Format("{0} task{1} updated", lst.Count, (lst.Count != 1 ? "s" : ""))) { Data = lst };
        }


    }
}