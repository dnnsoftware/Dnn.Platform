// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.ModuleCache
{
    public class PurgeModuleCache : SchedulerClient
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (PurgeModuleCache));
        public PurgeModuleCache(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem; //REQUIRED
        }

        public override void DoWork()
        {
            try
            {
                var portals = PortalController.Instance.GetPortals();
                foreach (KeyValuePair<string, ModuleCachingProvider> kvp in ModuleCachingProvider.GetProviderList())
                {
                    try
                    {
                        foreach (PortalInfo portal in portals)
                        {
                            kvp.Value.PurgeExpiredItems(portal.PortalID);
                            ScheduleHistoryItem.AddLogNote(string.Format("Purged Module cache for {0}.  ", kvp.Key));
                        }
                    }
                    catch (NotSupportedException exc)
                    {
						//some Module caching providers don't use this feature
                        Logger.Debug(exc);

                    }
                }
                ScheduleHistoryItem.Succeeded = true; //REQUIRED
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED

                ScheduleHistoryItem.AddLogNote(string.Format("Purging Module cache task failed: {0}.", exc.ToString()));

                //notification that we have errored
                Errored(ref exc); //REQUIRED
				
				//log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }
    }
}
