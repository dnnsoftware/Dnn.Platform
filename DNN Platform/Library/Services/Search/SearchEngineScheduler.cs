// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search.Internals;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke
    /// Class:      SearchEngineScheduler
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SearchEngineScheduler implements a SchedulerClient for the Indexing of
    /// portal content.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SearchEngineScheduler : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchEngineScheduler));

        public SearchEngineScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DoWork runs the scheduled item
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public override void DoWork()
        {
            try
            {
                var lastSuccessFulDateTime = SearchHelper.Instance.GetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID);
                Logger.Trace("Search: Site Crawler - Starting. Content change start time " + lastSuccessFulDateTime.ToString("g"));
                ScheduleHistoryItem.AddLogNote(string.Format("Starting. Content change start time <b>{0:g}</b>", lastSuccessFulDateTime));

                var searchEngine = new SearchEngine(ScheduleHistoryItem, lastSuccessFulDateTime);
                try
                {
                    searchEngine.DeleteOldDocsBeforeReindex();
                    searchEngine.DeleteRemovedObjects();
                    searchEngine.IndexContent();
                    searchEngine.CompactSearchIndexIfNeeded(ScheduleHistoryItem);
                }
                finally
                {
                    searchEngine.Commit();
                }

                ScheduleHistoryItem.Succeeded = true;
                ScheduleHistoryItem.AddLogNote("<br/><b>Indexing Successful</b>");
                SearchHelper.Instance.SetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID, ScheduleHistoryItem.StartDate);

                Logger.Trace("Search: Site Crawler - Indexing Successful");
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("<br/>EXCEPTION: " + ex.Message);
                Errored(ref ex);
                if (ScheduleHistoryItem.ScheduleSource != ScheduleSource.STARTED_FROM_BEGIN_REQUEST)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
            }
        }
    }
}
