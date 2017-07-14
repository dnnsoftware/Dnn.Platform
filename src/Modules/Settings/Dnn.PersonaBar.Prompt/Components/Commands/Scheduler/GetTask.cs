using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Scheduler
{
    [ConsoleCommand("get-task", "Retrieves details for a specified scheduled task", new[] { "id" })]
    public class GetTask : ConsoleCommandBase
    {
        private const string FlagId = "id";


        public int? TaskId { get; private set; }

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
            else if (args.Length == 2 && !IsFlag(args[1]))
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

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lstSchedule = SchedulingController.GetSchedule();
            var lst = new List<TaskModel>();

            foreach (var task in lstSchedule)
            {
                if (TaskId == task.ScheduleID)
                {
                    lst.Add(new TaskModel(task));
                    break;
                }
            }

            return new ConsoleResultModel($"{lst.Count} task{(lst.Count != 1 ? "s" : "")} found") { Data = lst };
        }


    }
}