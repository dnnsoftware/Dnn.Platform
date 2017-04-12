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
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using Dnn.ExportImport.Components.Scheduler;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using Dnn.ExportImport.Dto.Pages;
using DotNetNuke.Entities.Host;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Scheduling;

namespace Dnn.ExportImport.Components.Controllers
{
    public class EntitiesController : ServiceLocator<IEntitiesController, EntitiesController>, IEntitiesController
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        protected override Func<IEntitiesController> GetFactory()
        {
            return () => new EntitiesController();
        }

        public ExportImportJob GetFirstActiveJob()
        {
            return CBO.Instance.FillObject<ExportImportJob>(_dataProvider.GetFirstActiveJob());
        }

        public ExportImportJob GetJobById(int jobId)
        {
            var job = CBO.Instance.FillObject<ExportImportJob>(_dataProvider.GetJobById(jobId));
            //System.Diagnostics.Trace.WriteLine($"xxxxxxxxx job id={job?.JobId} IsCancelled={job?.IsCancelled} xxxxxxxxx");
            return job;
        }

        public IList<ExportImportJobLog> GetJobSummaryLog(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportJobLog>(_dataProvider.GetJobSummaryLog(jobId));
        }

        public IList<ExportImportJobLog> GetJobFullLog(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportJobLog>(_dataProvider.GetJobFullLog(jobId));
        }

        public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
        {
            return _dataProvider.GetAllJobsCount(portalId, jobType, keywords);
        }

        public IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
            return CBO.Instance.FillCollection<ExportImportJob>(
                _dataProvider.GetAllJobs(portalId, pageSize, pageIndex, jobType, keywords));
        }

        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
            var lastJobTime = _dataProvider.GetLastJobTime(portalId, jobType);
            if (lastJobTime != null)
            {
                return Util.ConvertToDbLocalTime(new DateTime(
                    lastJobTime.Value.Year, lastJobTime.Value.Month, lastJobTime.Value.Day,
                    lastJobTime.Value.Hour, lastJobTime.Value.Minute, lastJobTime.Value.Second,
                    DateTimeKind.Utc));
            }
            return null;
        }

        public void UpdateJobInfo(ExportImportJob job)
        {
            _dataProvider.UpdateJobInfo(job.JobId, job.Name, job.Description);
        }

        public void UpdateJobStatus(ExportImportJob job)
        {
            _dataProvider.UpdateJobStatus(job.JobId, job.JobStatus);
        }

        public void SetJobCancelled(ExportImportJob job)
        {
            _dataProvider.SetJobCancelled(job.JobId);
        }

        public void RemoveJob(ExportImportJob job)
        {
            _dataProvider.RemoveJob(job.JobId);
        }

        public IList<ExportImportChekpoint> GetJobChekpoints(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportChekpoint>(_dataProvider.GetJobChekpoints(jobId));
        }

        public void UpdateJobChekpoint(ExportImportChekpoint checkpoint)
        {
            _dataProvider.UpsertJobChekpoint(checkpoint);
        }

        public IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabInfo>(
                _dataProvider.GetAllPortalTabs(portalId, includeDeleted, includeSystem, toDate, fromDate));
        }

        public IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabSetting>(
                _dataProvider.GetAllTabSettings(tabId, toDate, fromDate));
        }

        public IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabPermission>(
                _dataProvider.GetAllTabPermissions(tabId, toDate, fromDate));
        }

        public IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabUrl>(
                _dataProvider.GetAllTabUrls(tabId, toDate, fromDate));
        }

        public IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModule>(
                _dataProvider.GetAllModules(tabId, includeDeleted, toDate, fromDate));
        }

        public IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModuleSetting>(
                _dataProvider.GetAllModuleSettings(moduleId, toDate, fromDate));
        }

        public IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModulePermission>(
                _dataProvider.GetAllModulePermissions(moduleId, toDate, fromDate));
        }

        public IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabModule>(
                _dataProvider.GetAllTabModules(tabId, includeDeleted, toDate, fromDate));
        }

        public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabModuleSetting>(
                _dataProvider.GetAllTabModuleSettings(tabId, toDate, fromDate));
        }

        public PermissionInfo GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
        {
            return CBO.Instance.FillObject<PermissionInfo>(
                _dataProvider.GetPermissionInfo(permissionCode, permissionKey, permissionName));
        }

        public void SetTabDeleted(int tabId, bool isDeleted)
        {
            _dataProvider.SetTabDeleted(tabId, isDeleted);
        }

        public void SetTabModuleDeleted(int tabModuleId, bool isDeleted)
        {
            _dataProvider.SetTabModuleDeleted(tabModuleId, isDeleted);
        }

        public void SetUserDeleted(int portalId, int userId, bool isDeleted)
        {
            _dataProvider.SetUserDeleted(portalId, userId, isDeleted);
        }

        public void RunSchedule()
        {
            var executingServer = ServerController.GetExecutingServerName();

            var scheduleItem = SchedulingController.GetSchedule(GetSchedulerTypeFullName(), executingServer);
            if (scheduleItem != null)
            {
                SchedulingProvider.Instance().RunScheduleItemNow(scheduleItem, true);

                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                {
                    SchedulingProvider.Instance().ReStart("Change made to schedule.");
                }
            }
        }

        private string GetSchedulerTypeFullName()
        {
            var type = typeof (ExportImportScheduler);
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }
    }
}