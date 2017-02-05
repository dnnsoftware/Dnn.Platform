using DotNetNuke.Services.Scheduling;
using System;

namespace Dnn.PersonaBar.Prompt.Models
{
    /// <summary>
    /// Represents a DNN ScheduleItem
    /// </summary>
    public class TaskModel : TaskModelBase
    {
        public string TypeName { get; set; }
        public bool CatchUp { get; set; }
        public DateTime Created { get; set; }
        public DateTime StartDate { get; set; }

        #region Constructors
        public TaskModel()
        {
        }
        public TaskModel(ScheduleItem item):base(item)
        {
            CatchUp = item.CatchUpEnabled;
            Created = item.CreatedOnDate;
            StartDate = item.ScheduleStartDate;
            TypeName = item.TypeFullName;
        }
        #endregion
    }
}