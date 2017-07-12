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
    [ConsoleCommand("set-task", "Updates a specific scheduled task with new information", new[]{
        "id",
        "enabled"
    })]
    public class SetTask : ConsoleCommandBase
    {

        private const string FlagId = "id";
        private const string FlagEnabled = "enabled";


        public int? TaskId { get; private set; }
        public bool? Enabled { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagId))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagId), out tmpId))
                {
                    TaskId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag must be a number; ", FlagId);
                }
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                var tmpId = 0;
                if (int.TryParse(args[1], out tmpId))
                {
                    TaskId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("You must specify the scheduled item's ID using the --{0} flag or by passing the number as the first argument; ", FlagId);
                }
            }

            if (HasFlag(FlagEnabled))
            {
                var tmpEnabled = false;
                if (bool.TryParse(Flag(FlagEnabled), out tmpEnabled))
                {
                    Enabled = tmpEnabled;
                }
                else
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag must be set to True or False", FlagEnabled);
                }
            }
            else
            {
                sbErrors.AppendFormat("The --{0} flag is required", FlagEnabled);
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {

            var taskToUpdate = SchedulingController.GetSchedule(Convert.ToInt32(TaskId));
            var lst = new List<TaskModel>();

            if (taskToUpdate != null)
            {
                taskToUpdate.Enabled = (bool)Enabled;
                var with1 = taskToUpdate;

                SchedulingController.UpdateSchedule(with1.ScheduleID, with1.TypeFullName, with1.TimeLapse, with1.TimeLapseMeasurement, with1.RetryTimeLapse, with1.RetryTimeLapseMeasurement, with1.RetainHistoryNum, with1.AttachToEvent, with1.CatchUpEnabled, with1.Enabled,
                with1.ObjectDependencies, with1.Servers, with1.FriendlyName);
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