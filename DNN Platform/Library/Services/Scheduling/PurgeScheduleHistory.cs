// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;

    public class PurgeScheduleHistory : SchedulerClient
    {
        public PurgeScheduleHistory(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                // notification that the event is progressing
                this.Progressing();

                SchedulingProvider.Instance().PurgeScheduleHistory();

                // update the result to success since no exception was thrown
                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.AddLogNote("Schedule history purged.");
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("Schedule history purge failed." + exc);
                this.ScheduleHistoryItem.Succeeded = false;

                // notification that we have errored
                this.Errored(ref exc);

                // log the exception
                Exceptions.Exceptions.LogException(exc);
            }
        }
    }
}
