// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Services.Scheduling;

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models
{
    /// <summary>
    /// Represents a DNN ScheduleItem
    /// </summary>
    public class TaskModel : TaskModelBase
    {
        public string TypeName { get; set; }
        public bool CatchUp { get; set; }
        public string Created { get; set; }
        public string StartDate { get; set; }

        #region Constructors
        public TaskModel()
        {
        }
        public TaskModel(ScheduleItem item):base(item)
        {
            NextStart = item.NextStart.ToPromptLongDateString();
            CatchUp = item.CatchUpEnabled;
            Created = item.CreatedOnDate.ToPromptLongDateString();
            StartDate = item.ScheduleStartDate.ToPromptLongDateString();
            TypeName = item.TypeFullName;
        }
        #endregion
    }
}
