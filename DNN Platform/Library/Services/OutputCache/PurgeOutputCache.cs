// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.OutputCache
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    using Microsoft.Extensions.Logging;

    /// <summary>A scheduled task to purge the output cache.</summary>
    public partial class PurgeOutputCache : SchedulerClient
    {
        private static readonly ILogger Logger = DnnLoggingController.GetLogger<PurgeOutputCache>();

        /// <summary>Initializes a new instance of the <see cref="PurgeOutputCache"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        public PurgeOutputCache(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem; // REQUIRED
        }

        /// <inheritdoc />
        public override void DoWork()
        {
            try
            {
                var portals = PortalController.Instance.GetPortals();
                foreach (KeyValuePair<string, OutputCachingProvider> kvp in OutputCachingProvider.GetProviderList())
                {
                    try
                    {
                        foreach (IPortalInfo portal in portals)
                        {
                            kvp.Value.PurgeExpiredItems(portal.PortalId);
                            this.ScheduleHistoryItem.AddLogNote($"Purged output cache for {kvp.Key}.  ");
                        }
                    }
                    catch (NotSupportedException exc)
                    {
                        // some output caching providers don't use this feature
                        Logger.PurgeOutputCachePurgeNotSupportedException(exc);
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote($"Purging output cache task failed: {exc}."); // OPTIONAL

                // notification that we have errored
                this.Errored(ref exc);

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
