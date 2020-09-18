// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.OutputCache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    public class PurgeOutputCache : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PurgeOutputCache));

        public PurgeOutputCache(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem; // REQUIRED
        }

        public override void DoWork()
        {
            try
            {
                var portals = PortalController.Instance.GetPortals();
                foreach (KeyValuePair<string, OutputCachingProvider> kvp in OutputCachingProvider.GetProviderList())
                {
                    try
                    {
                        foreach (PortalInfo portal in portals)
                        {
                            kvp.Value.PurgeExpiredItems(portal.PortalID);
                            this.ScheduleHistoryItem.AddLogNote(string.Format("Purged output cache for {0}.  ", kvp.Key));
                        }
                    }
                    catch (NotSupportedException exc)
                    {
                        // some output caching providers don't use this feature
                        Logger.Debug(exc);
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
            }
            catch (Exception exc) // REQUIRED
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote(string.Format("Purging output cache task failed: {0}.", exc.ToString())); // OPTIONAL

                // notification that we have errored
                this.Errored(ref exc);

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
