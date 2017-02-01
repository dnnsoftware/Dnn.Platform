using System;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class TaskModelSlim
    {
        public int ScheduleId;
        public string FriendlyName;
        public DateTime NextStart;

        public bool Enabled;
        public static TaskModelSlim FromDnnScheduleItem(DotNetNuke.Services.Scheduling.ScheduleItem item)
        {
            return new TaskModelSlim
            {
                Enabled = item.Enabled,
                FriendlyName = item.FriendlyName,
                NextStart = item.NextStart,
                ScheduleId = item.ScheduleID
            };
        }

    }
}