// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System.Collections;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ScheduleHistorySortStartDate Class is a custom IComparer Implementation
    /// used to sort the Schedule Items.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ScheduleHistorySortStartDate : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((ScheduleHistoryItem)y).StartDate.CompareTo(((ScheduleHistoryItem)x).StartDate);
        }
    }
}
