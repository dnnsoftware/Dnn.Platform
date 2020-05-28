// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
