// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search
{
    using System;
    using System.Globalization;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Internals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The SearchEngineScheduler implements a SchedulerClient for the Indexing of portal content.</summary>
    public class SearchEngineScheduler : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchEngineScheduler));
        private readonly IBusinessControllerProvider businessControllerProvider;

        /// <summary>Initializes a new instance of the <see cref="SearchEngineScheduler"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public SearchEngineScheduler(ScheduleHistoryItem objScheduleHistoryItem)
            : this(objScheduleHistoryItem, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SearchEngineScheduler"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        public SearchEngineScheduler(ScheduleHistoryItem objScheduleHistoryItem, IBusinessControllerProvider businessControllerProvider)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// <summary>DoWork runs the scheduled item.</summary>
        public override void DoWork()
        {
            try
            {
                var lastSuccessFulDateTime = SearchHelper.Instance.GetLastSuccessfulIndexingDateTime(this.ScheduleHistoryItem.ScheduleID);
                Logger.Trace("Search: Site Crawler - Starting. Content change start time " + lastSuccessFulDateTime.ToString("g", CultureInfo.InvariantCulture));
                this.ScheduleHistoryItem.AddLogNote(string.Format(CultureInfo.InvariantCulture, "Starting. Content change start time <b>{0:g}</b>", lastSuccessFulDateTime));

                var searchEngine = new SearchEngine(this.ScheduleHistoryItem, lastSuccessFulDateTime, this.businessControllerProvider);
                try
                {
                    searchEngine.DeleteOldDocsBeforeReindex();
                    searchEngine.DeleteRemovedObjects();
                    searchEngine.IndexContent();
                    searchEngine.CompactSearchIndexIfNeeded(this.ScheduleHistoryItem);
                }
                finally
                {
                    searchEngine.Commit();
                }

                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.AddLogNote("<br/><b>Indexing Successful</b>");
                SearchHelper.Instance.SetLastSuccessfulIndexingDateTime(this.ScheduleHistoryItem.ScheduleID, this.ScheduleHistoryItem.StartDate);

                Logger.Trace("Search: Site Crawler - Indexing Successful");
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("<br/>EXCEPTION: " + ex.Message);
                this.Errored(ref ex);
                if (this.ScheduleHistoryItem.ScheduleSource != ScheduleSource.STARTED_FROM_BEGIN_REQUEST)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
            }
        }
    }
}
