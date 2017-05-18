#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
