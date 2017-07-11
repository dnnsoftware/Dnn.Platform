using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Scheduler
{
    [ConsoleCommand("get-task", "Retrieves details for a specified scheduled task", new string[] { "id" })]
    public class GetTask : ConsoleCommandBase, IConsoleCommand
    {
        private const string FLAG_ID = "id";

        public string ValidationMessage { get; private set; }
        public int? TaskId { get; private set; }

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
            else if (args.Length == 2 && !IsFlag(args[1]))
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

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            var lstSchedule = SchedulingController.GetSchedule();
            List<TaskModel> lst = new List<TaskModel>();

            foreach (ScheduleItem task in lstSchedule)
            {
                if (TaskId == task.ScheduleID)
                {
                    lst.Add(new TaskModel(task));
                    break; 
                }
            }

            return new ConsoleResultModel(string.Format("{0} task{1} found", lst.Count, (lst.Count != 1 ? "s" : ""))) { Data = lst };
        }


    }
}