// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Scheduling
{
    public class PurgeScheduleHistory : SchedulerClient
    {
        public PurgeScheduleHistory(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                //notification that the event is progressing
                Progressing();

                SchedulingProvider.Instance().PurgeScheduleHistory();

                //update the result to success since no exception was thrown
                ScheduleHistoryItem.Succeeded = true;
                ScheduleHistoryItem.AddLogNote("Schedule history purged.");
            }
            catch (Exception exc)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("Schedule history purge failed." + exc);
                ScheduleHistoryItem.Succeeded = false;

                //notification that we have errored
                Errored(ref exc);
				
				//log the exception
                Exceptions.Exceptions.LogException(exc);
            }
        }
    }
}
