// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.Cache
{
    public class PurgeCache : SchedulerClient
    {
        public PurgeCache(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem; //REQUIRED
        }

        public override void DoWork()
        {
            try
            {
                string str = CachingProvider.Instance().PurgeCache();

                ScheduleHistoryItem.Succeeded = true; //REQUIRED
                ScheduleHistoryItem.AddLogNote(str);
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED

                ScheduleHistoryItem.AddLogNote(string.Format("Purging cache task failed: {0}.", exc.ToString()));

                //notification that we have errored
                Errored(ref exc); //REQUIRED
				
				//log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }
    }
}
