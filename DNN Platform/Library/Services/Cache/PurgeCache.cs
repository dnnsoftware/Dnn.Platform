// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Cache
{
    using System;

    using DotNetNuke.Services.Scheduling;

    public class PurgeCache : SchedulerClient
    {
        public PurgeCache(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem; // REQUIRED
        }

        public override void DoWork()
        {
            try
            {
                string str = CachingProvider.Instance().PurgeCache();

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
                this.ScheduleHistoryItem.AddLogNote(str);
            }
            catch (Exception exc) // REQUIRED
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote(string.Format("Purging cache task failed: {0}.", exc.ToString()));

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
