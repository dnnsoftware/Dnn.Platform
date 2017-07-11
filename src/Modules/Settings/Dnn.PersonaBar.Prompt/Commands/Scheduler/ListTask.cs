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
    [ConsoleCommand("list-tasks", "Retrieves a list of scheduled tasks", new string[]{
        "enabled",
        "name"
    })]
    public class ListTasks : ConsoleCommandBase, IConsoleCommand
    {
        private const string FLAG_ENABLED = "enabled";
        private const string FLAG_NAME = "name";

        public string ValidationMessage { get; private set; }
        public bool? Enabled { get; private set; }
        public string TaskName { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();
            bool bFirstArgProcessed = false;

            if (HasFlag(FLAG_ENABLED))
            {
                bool tmpEnabled = false;
                if (bool.TryParse(Flag(FLAG_ENABLED), out tmpEnabled))
                {
                    Enabled = tmpEnabled;
                }
                else
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag must be True or False; ", FLAG_ENABLED);
                }
            }
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                // if the Enabled flag isn't used but the first argument is a boolean, assume then
                // user is passing Enabled as the first argument
                bool tmpEnabled = false;
                if (bool.TryParse(args[1], out tmpEnabled))
                {
                    Enabled = tmpEnabled;
                    bFirstArgProcessed = true;
                    // don't let other code process first arugument
                }
            }

            if (HasFlag(FLAG_NAME))
            {
                TaskName = Flag(FLAG_NAME);
                if (string.IsNullOrEmpty(TaskName))
                {
                    sbErrors.AppendFormat("When specified, the --{0} flag cannot be empty; ", FLAG_NAME);
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

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            var lstSchedule = SchedulingController.GetSchedule();
            List<TaskModelBase> lst = new List<TaskModelBase>();

            if (!string.IsNullOrEmpty(TaskName))
            {
                var search = TaskName.Replace("*", ".*");
                var query = from task in lstSchedule
                            where Regex.Match(task.FriendlyName, search, RegexOptions.IgnoreCase).Success
                            select task;
                foreach (ScheduleItem task in query)
                {
                    if (!Enabled.HasValue || Enabled == task.Enabled)
                    {
                        lst.Add(new TaskModelBase(task));
                    }
                }
            }
            else
            {
                foreach (ScheduleItem task in lstSchedule)
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