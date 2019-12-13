// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
