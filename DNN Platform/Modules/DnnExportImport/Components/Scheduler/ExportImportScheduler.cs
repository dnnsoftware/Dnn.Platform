// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Scheduler
{
    using System;
    using System.Text;
    using System.Threading;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Engines;
    using Dnn.ExportImport.Components.Models;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Scheduling;

    /// <summary>
    /// Implements a SchedulerClient for the Exporting/Importing of site items.
    /// </summary>
    public class ExportImportScheduler : SchedulerClient
    {
        private const int EmergencyScheduleFrequency = 120;
        private const int DefaultScheduleFrequency = 1;
        private const string EmergencyScheduleFrequencyUnit = "m";
        private const string DefaultScheduleFrequencyUnit = "d";

        private const int EmergencyScheduleRetry = 90;
        private const int DefaultScheduleRetry = 1;
        private const string EmergencyScheduleRetryUnit = "s";
        private const string DefaultScheduleRetryUnit = "h";

        private const int EmergencyHistoryNumber = 1;
        private const int DefaultHistoryNumber = 60;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportScheduler));

        public ExportImportScheduler(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                // TODO: do some clean-up for very old import/export jobs/logs
                var job = EntitiesController.Instance.GetFirstActiveJob();
                if (job == null)
                {
                    this.ScheduleHistoryItem.Succeeded = true;
                    this.ScheduleHistoryItem.AddLogNote("<br/>No Site Export/Import jobs queued for processing.");
                }
                else if (job.IsCancelled)
                {
                    job.JobStatus = JobStatus.Cancelled;
                    EntitiesController.Instance.UpdateJobStatus(job);
                    this.ScheduleHistoryItem.Succeeded = true;
                    this.ScheduleHistoryItem.AddLogNote("<br/>Site Export/Import jobs was previously cancelled.");
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
                    var succeeded = true;

                    switch (job.JobType)
                    {
                        case JobType.Export:
                            try
                            {
                                engine.Export(job, result, this.ScheduleHistoryItem);
                            }
                            catch (Exception ex)
                            {
                                result.AddLogEntry("EXCEPTION exporting job #" + job.JobId, ex.Message, ReportLevel.Error);
                                engine.AddLogsToDatabase(job.JobId, result.CompleteLog);
                                throw;
                            }

                            EntitiesController.Instance.UpdateJobStatus(job);
                            break;
                        case JobType.Import:
                            try
                            {
                                engine.Import(job, result, this.ScheduleHistoryItem);
                            }
                            catch (ThreadAbortException)
                            {
                                this.ScheduleHistoryItem.TimeLapse = EmergencyScheduleFrequency;
                                this.ScheduleHistoryItem.TimeLapseMeasurement = EmergencyScheduleFrequencyUnit;
                                this.ScheduleHistoryItem.RetryTimeLapse = EmergencyScheduleRetry;
                                this.ScheduleHistoryItem.RetryTimeLapseMeasurement = EmergencyScheduleRetryUnit;
                                this.ScheduleHistoryItem.RetainHistoryNum = EmergencyHistoryNumber;

                                SchedulingController.UpdateSchedule(this.ScheduleHistoryItem);

                                SchedulingController.PurgeScheduleHistory();

                                Logger.Error("The Schduler item stopped because main thread stopped, set schedule into emergency mode so it will start after app restart.");
                                succeeded = false;
                            }
                            catch (Exception ex)
                            {
                                result.AddLogEntry("EXCEPTION importing job #" + job.JobId, ex.Message, ReportLevel.Error);
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

                    this.ScheduleHistoryItem.Succeeded = true;

                    // restore schedule item running timelapse to default.
                    if (succeeded
                        && this.ScheduleHistoryItem.TimeLapse == EmergencyScheduleFrequency
                        && this.ScheduleHistoryItem.TimeLapseMeasurement == EmergencyScheduleFrequencyUnit)
                    {
                        this.ScheduleHistoryItem.TimeLapse = DefaultScheduleFrequency;
                        this.ScheduleHistoryItem.TimeLapseMeasurement = DefaultScheduleFrequencyUnit;
                        this.ScheduleHistoryItem.RetryTimeLapse = DefaultScheduleRetry;
                        this.ScheduleHistoryItem.RetryTimeLapseMeasurement = DefaultScheduleRetryUnit;
                        this.ScheduleHistoryItem.RetainHistoryNum = DefaultHistoryNumber;

                        SchedulingController.UpdateSchedule(this.ScheduleHistoryItem);
                    }

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

                    this.ScheduleHistoryItem.AddLogNote(sb.ToString());
                    engine.AddLogsToDatabase(job.JobId, result.CompleteLog);

                    Logger.Trace("Site Export/Import: Job Finished");
                }

                // SetLastSuccessfulIndexingDateTime(ScheduleHistoryItem.ScheduleID, ScheduleHistoryItem.StartDate);
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("<br/>Export/Import EXCEPTION: " + ex.Message);
                this.Errored(ref ex);

                // this duplicates the logging
                // if (ScheduleHistoryItem.ScheduleSource != ScheduleSource.STARTED_FROM_BEGIN_REQUEST)
                // {
                //    Exceptions.LogException(ex);
                // }
            }
        }
    }
}
