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
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto.Jobs;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Log.EventLog;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Scheduler;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;

namespace Dnn.ExportImport.Components.Controllers
{
    public class BaseController
    {
        protected void AddEventLog(int portalId, int userId, int jobId, string logTypeKey)
        {
            var objSecurity = new PortalSecurity();
            var portalInfo = PortalController.Instance.GetPortal(portalId);
            var userInfo = UserController.Instance.GetUser(portalId, userId);
            var username = objSecurity.InputFilter(userInfo.Username,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);

            var log = new LogInfo
            {
                LogTypeKey = logTypeKey,
                LogPortalID = portalId,
                LogPortalName = portalInfo.PortalName,
                LogUserName = username,
                LogUserID = userId,
            };

            log.AddProperty("JobID", jobId.ToString());
            LogController.Instance.AddLog(log);
        }

        public bool CancelJob(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || job.PortalId != portalId)
                return false;

            controller.SetJobCancelled(job);
            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
            return true;
        }

        public bool RemoveJob(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || job.PortalId != portalId)
                return false;

            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
            // if the job is running; then it will create few exceptions in the log file
            controller.RemoveJob(job);
            return true;
        }

        /// <summary>
        /// Retrieves one page of paginated proceessed jobs
        /// </summary>
        public IEnumerable<JobItem> GetAllJobs(int portalId, int pageSize, int pageIndex)
        {
            if (pageIndex < 0) pageIndex = 0;
            if (pageSize < 1) pageSize = 1;
            else if (pageSize > 100) pageSize = 100;

            var jobs = EntitiesController.Instance.GetAllJobs(portalId, pageSize, pageIndex);
            return jobs.Select(ToJobItem);
        }

        public JobItem GetJobSummary(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || job.PortalId != portalId)
                return null;

            var jobItem = ToJobItem(job);
            var summaryItems = controller.GetJobSummaryLog(jobId);
            jobItem.Summary = summaryItems.Select(
                s => new LogItem
                {
                    CreatedOnDate = s.CreatedOnDate,
                    Name = s.Name,
                    Value = s.Value,
                    IsSummary = s.IsSummary,
                });
            return jobItem;
        }

        public JobItem GetJobDetails(int portalId, int jobId)
        {
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
            if (job == null || job.PortalId != portalId)
                return null;

            var jobItem = ToJobItem(job);
            var summaryItems = controller.GetJobFullLog(jobId);
            jobItem.Summary = summaryItems.Select(
                s => new LogItem
                {
                    CreatedOnDate = s.CreatedOnDate,
                    Name = s.Name,
                    Value = s.Value,
                    IsSummary = s.IsSummary,
                });
            return jobItem;
        }

        private static JobItem ToJobItem(ExportImportJob job)
        {
            var user = UserController.Instance.GetUserById(job.PortalId, job.CreatedByUserId);
            return new JobItem
            {
                JobId = job.JobId,
                PortalId = job.PortalId,
                User = user?.DisplayName ?? user?.Username ?? job.CreatedByUserId.ToString(),
                JobType = Localization.GetString("JobType_" + job.JobType, Constants.SharedResources),
                JobStatus = Localization.GetString("JobStatus_" + job.JobStatus, Constants.SharedResources),
                CreatedOn = job.CreatedOnDate,
                CompletedOn = job.CompletedOnDate,
                ExportFile = job.CompletedOnDate.HasValue ? job.ExportFile : null
            };
        }

        /// <summary>
        /// Get the last time a successful export job has started.
        /// This date/time is in uts and can be used to set the next
        /// differntial date/time to start the job from.
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastExportUtcTime()
        {
            var lastTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var scheduleProvider = SchedulingProvider.Instance();
            var exportSchedulerClient = scheduleProvider.GetSchedule(typeof(ExportImportScheduler).FullName, null);
            if (exportSchedulerClient != null)
            {
                var settings = SchedulingProvider.Instance().GetScheduleItemSettings(exportSchedulerClient.ScheduleID);
                var lastValue = settings[Constants.LastJobStartTimeKey] as string;

                if (!string.IsNullOrEmpty(lastValue) &&
                    DateTime.TryParseExact(lastValue, Constants.JobRunDateTimeFormat, null, DateTimeStyles.None, out lastTime))
                {
                    lastTime = FixSqlDateTime(lastTime);
                    if (lastTime > DateTime.UtcNow) lastTime = DateTime.UtcNow;
                }
                else
                {
                    lastTime = FixSqlDateTime(DateTime.MinValue);
                }
            }

            return lastTime.ToUniversalTime();
        }

        private static DateTime FixSqlDateTime(DateTime datim)
        {
            if (datim <= SqlDateTime.MinValue.Value)
                datim = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            else if (datim >= SqlDateTime.MaxValue.Value)
                datim = new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return datim;
        }

    }
}