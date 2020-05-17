// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Services.Scheduling
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ScheduleStatusSortRemainingTimeDescending Class is a custom IComparer Implementation
    /// used to sort the Schedule Items
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ScheduleStatusSortRemainingTimeDescending : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            return ((ScheduleHistoryItem) x).RemainingTime.CompareTo(((ScheduleHistoryItem) y).RemainingTime);
        }

        #endregion
    }
}
