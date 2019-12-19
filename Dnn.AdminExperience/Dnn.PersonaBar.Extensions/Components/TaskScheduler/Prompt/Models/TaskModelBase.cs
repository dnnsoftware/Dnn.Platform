// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Prompt.Common;
using DotNetNuke.Services.Scheduling;

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models
{
    public class TaskModelBase
    {
        public int ScheduleId { get; set; }
        public string FriendlyName { get; set; }
        public string NextStart { get; set; }
        public bool Enabled { get; set; }

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
