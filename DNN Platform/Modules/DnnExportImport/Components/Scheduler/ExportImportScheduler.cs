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
using System.Text;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Components.Engines;
using Dnn.ExportImport.Components.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;

namespace Dnn.ExportImport.Components.Scheduler
{
    /// <summary>
    /// Implements a SchedulerClient for the Exporting/Importing of site items.
    /// </summary>
    public class ExportImportScheduler : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportScheduler));

        public ExportImportScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                //TODO: do some clean-up for very old import/export jobs/logs

                var job = EntitiesController.Instance.GetFirstActiveJob();
                if (job == null)
                {
                    ScheduleHistoryItem.Succeeded = true;
                    ScheduleHistoryItem.AddLogNote("<br/>No Site Export/Import jobs queued for processing.");
                }
                else if (job.IsCancelled)
                {
                    job.JobStatus = JobStatus.Cancelled;
                    EntitiesController.Instance.UpdateJobStatus(job);
                    ScheduleHistoryItem.Succeeded = true;
                    ScheduleHistoryItem.AddLogNote("<br/>Site Export/Import jobs was previously cancelled.");
                }
                else
                {
                    job.JobStatus = JobStatus.InProgress;
                    EntitiesController.Instance.UpdateJobStatus(job);
                    var result = new ExportImportResult
                    {
                        JobId = job.JobId,
                    };
                    var engine = new ExportImportEngine();

                    switch (job.JobType)
                    {
                        case JobType.Export:
                            try
                            {
                                engine.Export(job, result, ScheduleHistoryItem);
                            }
                            catch (Exception ex)
                            {
                                result.AddLogEntry("EXCEPTION", ex.Message, ReportLevel.Error);
                                engine.AddLogsToDatabase(job.JobId, result.CompleteLog);
                                throw;
                            }
                            EntitiesController.Instance.UpdateJobStatus(job);
                            break;
                        case JobType.Import:
                            try
                            {
                                engine.Import(job, result, ScheduleHistoryItem);
                            }
                            catch (Exception ex)
                            {
                                result.AddLogEntry("EXCEPTION", ex.Message, ReportLevel.Error);
                                engine.AddLogsToDatabase(job.JobId, result.CompleteLog);
                                throw;
                            }
                            EntitiesController.Instance.UpdateJobStatus(job);
                            if (job.JobStatus == JobStatus.Successful || job.JobStatus == JobStatus.Cancelled)
                            {
                                // clear everything to be sure imported items take effect
                                DataCache.ClearCache();
                            }
                            break;
                        default:
                            throw new Exception("Unknown job type: " + job.JobType);
                    }

                    ScheduleHistoryItem.Succeeded = true;
                    var sb = new StringBuilder();
                    var jobType = Localization.GetString("JobType_" + job.JobType, Constants.SharedResources);
                    var jobStatus = Localization.GetString("JobStatus_" + job.JobStatus, Constants.SharedResources);
                    sb.AppendFormat("<br/><b>{0} {1}</b>", jobType, jobStatus);
                    var summary = result.Summary;
                    if (summary.Count > 0)
                    {
                        sb.Append("<br/><b>Summary:</b><ul>");
                        foreach (var entry in summary)
                        {
                            sb.Append($"<li>{entry.Name}: {entry.Value}</li>");
                        }
                        sb.Append("</ul>");
                    }

                    ScheduleHistoryItem.AddLogNote(sb.ToString());
                    engine.AddLogsToDatabase(job.JobId, result.CompleteLog);

                    Logger.Trace("Site Export/Import: Job Finished");
                }
                //SetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID, ScheduleHistoryItem.StartDate);
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote("<br/>Export/Import EXCEPTION: " + ex.Message);
                Errored(ref ex);
                // this duplicates the logging
                //if (ScheduleHistoryItem.ScheduleSource != ScheduleSource.STARTED_FROM_BEGIN_REQUEST)
                //{
                //    Exceptions.LogException(ex);
                //}
            }
        }
    }
}