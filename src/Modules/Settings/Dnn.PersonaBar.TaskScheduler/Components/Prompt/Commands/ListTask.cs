using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    [ConsoleCommand("list-tasks", "Retrieves a list of scheduled tasks", new[]{
        "enabled",
        "name"
    })]
    public class ListTasks : ConsoleCommandBase
    {
        protected override string LocalResourceFile => Constants.LocalResourcesFile;

        private const string FlagEnabled = "enabled";
        private const string FlagName = "name";

        private bool? Enabled { get; set; }
        private string TaskName { get; set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            Enabled = GetFlagValue<bool?>(FlagEnabled, "Enabled", null, false, true);
            TaskName = GetFlagValue(FlagName, "Task Name", string.Empty, false, !Enabled.HasValue);
        }

        public override ConsoleResultModel Run()
        {
            var controller = new TaskSchedulerController();
            var tasks = new List<TaskModelBase>();
            var schedulerItems = controller.GetScheduleItems(Enabled, "", TaskName?.Replace("*", ""));
            tasks.AddRange(schedulerItems.Select(x => new TaskModelBase(x)));
            return new ConsoleResultModel(string.Format(LocalizeString("Prompt_TasksFound"), tasks.Count))
            {
                Data = tasks,
                Records = tasks.Count
            };
        }
    }
}