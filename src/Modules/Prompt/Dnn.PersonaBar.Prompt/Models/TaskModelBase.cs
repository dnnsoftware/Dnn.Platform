using System;
using DotNetNuke.Services.Scheduling;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class TaskModelBase
    {
        public int ScheduleId;
        public string FriendlyName;
        public DateTime NextStart;
        public bool Enabled;

        #region Constructors
        public TaskModelBase()
        {
        }

        public TaskModelBase(ScheduleItem item)
        {
            Enabled = item.Enabled;
            FriendlyName = item.FriendlyName;
            NextStart = item.NextStart;
            ScheduleId = item.ScheduleID;
        }
        #endregion

        #region CommandLinks
        #endregion
    }
}