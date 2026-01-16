// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ModuleCache
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    public class PurgeModuleCache : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PurgeModuleCache));

        /// <summary>Initializes a new instance of the <see cref="PurgeModuleCache"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        public PurgeModuleCache(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem; // REQUIRED
        }

        /// <inheritdoc/>
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
                            this.ScheduleHistoryItem.AddLogNote($"Purged Module cache for {kvp.Key}.  ");
                        }
                    }
                    catch (NotSupportedException exc)
                    {
                        // some Module caching providers don't use this feature
                        Logger.Debug(exc);
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote($"Purging Module cache task failed: {exc}.");

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
