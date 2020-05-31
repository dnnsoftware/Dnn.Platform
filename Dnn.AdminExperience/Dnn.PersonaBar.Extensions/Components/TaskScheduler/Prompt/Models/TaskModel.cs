// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
