#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
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

using System;
using System.Data.SqlTypes;
using System.Globalization;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;

namespace Dnn.ExportImport.Components.Scheduler
{
    /// <summary>
    /// Implements a SchedulerClient for the Exporting/Importing of site items.
    /// </summary>
    public class ExportImportScheduler : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportScheduler));

        private const string ReindexDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string LastSuccessExportName = "EXPORT_LastSuccessOn";
        private const string LastIndexKeyFormat = "{0}_{1}";

        private static DateTime FixSqlDateTime(DateTime datim)
        {
            if (datim <= SqlDateTime.MinValue.Value)
                datim = SqlDateTime.MinValue.Value.AddDays(1);
            else if (datim >= SqlDateTime.MaxValue.Value)
                datim = SqlDateTime.MaxValue.Value.AddDays(-1);
            return datim;
        }

        private DateTimeOffset GetLastSuccessfulIndexingDateTime(int scheduleId)
        {
            var settings = SchedulingProvider.Instance().GetScheduleItemSettings(scheduleId);
            var lastValue = settings[LastSuccessExportName] as string;

            if (string.IsNullOrEmpty(lastValue))
            {
                // try to fallback to old location where this was stored
                var name = string.Format(LastIndexKeyFormat, LastSuccessExportName, scheduleId);
                lastValue = HostController.Instance.GetString(name, Null.NullString);
            }

            DateTime lastTime;
            if (!string.IsNullOrEmpty(lastValue) &&
                DateTime.TryParseExact(lastValue, ReindexDateTimeFormat, null, DateTimeStyles.None, out lastTime))
            {
                // retrieves the date as UTC but returns to caller as local
                lastTime = FixSqlDateTime(lastTime).ToLocalTime().ToLocalTime();
                if (lastTime > DateTime.Now) lastTime = DateTime.Now;
            }
            else
            {
                lastTime = SqlDateTime.MinValue.Value.AddDays(1);
            }

            return lastTime;
        }

        public void SetLastSuccessfulIndexingDateTime(int scheduleId, DateTime startDateLocal)
        {
            SchedulingProvider.Instance().AddScheduleItemSetting(scheduleId,
                LastSuccessExportName, startDateLocal.ToUniversalTime().ToString(ReindexDateTimeFormat));
        }

        public override void DoWork()
        {
            try
            {
                var lastSuccessFulDateTime = GetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID);
                Logger.Trace("Export/Import: Site Crawler - Starting. Content change start time " + lastSuccessFulDateTime.ToString("g"));
                ScheduleHistoryItem.AddLogNote($"Starting. Content change start time <b>{lastSuccessFulDateTime:g}</b>");

                //var engine = new ExportImportEngine(ScheduleHistoryItem, lastSuccessFulDateTime);
                //try
                //{
                //    //TODO:
                //    throw new NotImplementedException();
                //}
                //finally
                //{
                //    engine.Commit();
                //}

                ScheduleHistoryItem.Succeeded = true;
                ScheduleHistoryItem.AddLogNote("<br/><b>Exporting Successful</b>");
                Logger.Trace("Export/Import: Site Crawler - Indexing Successful");
                SetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID, ScheduleHistoryItem.StartDate);
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("<br/>EXCEPTION: " + ex.Message);
                Errored(ref ex);
                if (ScheduleHistoryItem.ScheduleSource != ScheduleSource.STARTED_FROM_BEGIN_REQUEST)
                {
                    Exceptions.LogException(ex);
                }
            }
        }
    }
}