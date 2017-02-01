using System;

namespace Dnn.PersonaBar.Prompt.Models
{
    /// <summary>
    /// Represents an DNN ScheduleItem
    /// </summary>
    public class TaskModel
    {
        public int ScheduleId;
        public string FriendlyName;
        public string TypeName;
        public DateTime NextStart;
        public bool Enabled;
        public bool CatchUp;
        public DateTime Created;
        public DateTime StartDate;

        public static TaskModel FromDnnScheduleItem(DotNetNuke.Services.Scheduling.ScheduleItem item)
        {
            return new TaskModel
            {
                CatchUp = item.CatchUpEnabled,
                Created = item.CreatedOnDate,
                Enabled = item.Enabled,
                FriendlyName = item.FriendlyName,
                NextStart = item.NextStart,
                ScheduleId = item.ScheduleID,
                StartDate = item.ScheduleStartDate,
                TypeName = item.TypeFullName
            };

        }

    }
}