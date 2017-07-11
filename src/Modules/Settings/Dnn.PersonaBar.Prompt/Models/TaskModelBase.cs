using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Services.Scheduling;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class TaskModelBase
    {
        public int ScheduleId;
        public string FriendlyName;
        public string NextStart;
        public bool Enabled;

        #region Constructors
        public TaskModelBase()
        {
        }

        public TaskModelBase(ScheduleItem item)
        {
            Enabled = item.Enabled;
            FriendlyName = item.FriendlyName;
            NextStart = item.NextStart.ToPromptShortDateAndTimeString();
            ScheduleId = item.ScheduleID;
        }
        #endregion

        #region CommandLinks
        #endregion
    }
}