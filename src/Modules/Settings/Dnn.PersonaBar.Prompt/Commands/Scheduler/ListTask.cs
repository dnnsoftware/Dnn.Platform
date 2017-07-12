using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Scheduler
{
    [ConsoleCommand("list-tasks", "Retrieves a list of scheduled tasks", new[]{
        "enabled",
        "name"
    })]
    public class ListTasks : ConsoleCommandBase
    {
        private const string FlagEnabled = "enabled";
        private const string FlagName = "name";


        public bool? Enabled { get; private set; }
        public string TaskName { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();
            var bFirstArgProcessed = false;

            if (HasFlag(FlagEnabled))
            {
                var tmpEnabled = false;
                if (bool.TryParse(Flag(FlagEnabled), out tmpEnabled))
                {
                    Enabled = tmpEnabled;
                }
                else
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag must be True or False; ", FlagEnabled);
                }
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                // if the Enabled flag isn't used but the first argument is a boolean, assume then
                // user is passing Enabled as the first argument
                var tmpEnabled = false;
                if (bool.TryParse(args[1], out tmpEnabled))
                {
                    Enabled = tmpEnabled;
                    bFirstArgProcessed = true;
                    // don't let other code process first arugument
                }
            }

            if (HasFlag(FlagName))
            {
                TaskName = Flag(FlagName);
                if (string.IsNullOrEmpty(TaskName))
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag cannot be empty; ", FlagName);
                    TaskName = null;
                }
            }
            else if (!bFirstArgProcessed && args.Length >= 2 && !IsFlag(args[1]))
            {
                // only interpret first argument as the --name flag if the first arg is a value and 
                // has not already been interpreted as a boolean (for the Enabled flag)
                TaskName = args[1];
                bFirstArgProcessed = true;
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var lstSchedule = SchedulingController.GetSchedule();
            var lst = new List<TaskModelBase>();

            if (!string.IsNullOrEmpty(TaskName))
            {
                var search = TaskName.Replace("*", ".*");
                var query = from task in lstSchedule
                            where Regex.Match(task.FriendlyName, search, RegexOptions.IgnoreCase).Success
                            select task;
                foreach (var task in query)
                {
                    if (!Enabled.HasValue || Enabled == task.Enabled)
                    {
                        lst.Add(new TaskModelBase(task));
                    }
                }
            }
            else
            {
                foreach (var task in lstSchedule)
                {
                    // By default, if Enabled is not specified, return all scheduled tasks.
                    if (!Enabled.HasValue || (Enabled == task.Enabled))
                    {
                        lst.Add(new TaskModelBase(task));
                    }
                }
            }


            return new ConsoleResultModel(string.Format("{0} task{1} found", lst.Count, (lst.Count != 1 ? "s" : ""))) { Data = lst };
        }


    }
}