using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    [ConsoleCommand("list-tasks", "Retrieves a list of scheduled tasks", new[]{
        "enabled",
        "name"
    })]
    public class ListTasks : ConsoleCommandBase
    {
        private const string FlagEnabled = "enabled";
        private const string FlagName = "name";


        private bool? Enabled { get; set; }
        private string TaskName { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();
            var bFirstArgProcessed = false;

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
            else if (args.Length >= 2 && !IsFlag(args[1]))
            {
                // if the Enabled flag isn't used but the first argument is a boolean, assume then
                // user is passing Enabled as the first argument
                bool tmpEnabled;
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
                    sbErrors.AppendFormat(Localization.GetString("Prompt_FlagCantBeEmpty", Constants.LocalResourcesFile), FlagName);
                    TaskName = null;
                }
            }
            else if (!bFirstArgProcessed && args.Length >= 2 && !IsFlag(args[1]))
            {
                // only interpret first argument as the --name flag if the first arg is a value and 
                // has not already been interpreted as a boolean (for the Enabled flag)
                TaskName = args[1];
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var controller = new TaskSchedulerController();
            var tasks = new List<TaskModelBase>();
            var schedulerItems = controller.GetScheduleItems(Enabled, "", TaskName?.Replace("*", ""));
            tasks.AddRange(schedulerItems.Select(x => new TaskModelBase(x)));
            return new ConsoleResultModel(string.Format(Localization.GetString("Prompt_TasksFound", Constants.LocalResourcesFile), tasks.Count))
            {
                Data = tasks,
                Records = tasks.Count
            };
        }
    }
}