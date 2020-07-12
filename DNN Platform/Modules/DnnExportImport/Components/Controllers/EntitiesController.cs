// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Interfaces;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Components.Scheduler;
    using Dnn.ExportImport.Dto.Pages;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Scheduling;

    public class EntitiesController : ServiceLocator<IEntitiesController, EntitiesController>, IEntitiesController
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        public ExportImportJob GetFirstActiveJob()
        {
            return CBO.Instance.FillObject<ExportImportJob>(this._dataProvider.GetFirstActiveJob());
        }

        public ExportImportJob GetJobById(int jobId)
        {
            var job = CBO.Instance.FillObject<ExportImportJob>(this._dataProvider.GetJobById(jobId));

            // System.Diagnostics.Trace.WriteLine($"xxxxxxxxx job id={job?.JobId} IsCancelled={job?.IsCancelled} xxxxxxxxx");
            return job;
        }

        public IList<ExportImportJobLog> GetJobSummaryLog(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportJobLog>(this._dataProvider.GetJobSummaryLog(jobId));
        }

        public IList<ExportImportJobLog> GetJobFullLog(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportJobLog>(this._dataProvider.GetJobFullLog(jobId));
        }

        public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
        {
            return this._dataProvider.GetAllJobsCount(portalId, jobType, keywords);
        }

        public IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
            return CBO.Instance.FillCollection<ExportImportJob>(
                this._dataProvider.GetAllJobs(portalId, pageSize, pageIndex, jobType, keywords));
        }

        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
            return this._dataProvider.GetLastJobTime(portalId, jobType);
        }

        public void UpdateJobInfo(ExportImportJob job)
        {
            this._dataProvider.UpdateJobInfo(job.JobId, job.Name, job.Description);
        }

        public void UpdateJobStatus(ExportImportJob job)
        {
            this._dataProvider.UpdateJobStatus(job.JobId, job.JobStatus);
        }

        public void SetJobCancelled(ExportImportJob job)
        {
            this._dataProvider.SetJobCancelled(job.JobId);
        }

        public void RemoveJob(ExportImportJob job)
        {
            this._dataProvider.RemoveJob(job.JobId);
        }

        public IList<ExportImportChekpoint> GetJobChekpoints(int jobId)
        {
            return CBO.Instance.FillCollection<ExportImportChekpoint>(this._dataProvider.GetJobChekpoints(jobId));
        }

        public void UpdateJobChekpoint(ExportImportChekpoint checkpoint)
        {
            this._dataProvider.UpsertJobChekpoint(checkpoint);
        }

        public IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabInfo>(
                this._dataProvider.GetAllPortalTabs(portalId, includeDeleted, includeSystem, toDate, fromDate));
        }

        public IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabSetting>(
                this._dataProvider.GetAllTabSettings(tabId, toDate, fromDate));
        }

        public IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabPermission>(
                this._dataProvider.GetAllTabPermissions(tabId, toDate, fromDate));
        }

        public IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabUrl>(
                this._dataProvider.GetAllTabUrls(tabId, toDate, fromDate));
        }

        public IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModule>(
                this._dataProvider.GetAllModules(tabId, includeDeleted, toDate, fromDate));
        }

        public IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModuleSetting>(
                this._dataProvider.GetAllModuleSettings(moduleId, toDate, fromDate));
        }

        public IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportModulePermission>(
                this._dataProvider.GetAllModulePermissions(moduleId, toDate, fromDate));
        }

        public IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabModule>(
                this._dataProvider.GetAllTabModules(tabId, includeDeleted, toDate, fromDate));
        }

        public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return this.GetTabModuleSettings(tabId, true, toDate, fromDate);
        }

        public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabModuleSetting>(
                this._dataProvider.GetAllTabModuleSettings(tabId, includeDeleted, toDate, fromDate));
        }

        public PermissionInfo GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
        {
            return CBO.Instance.FillObject<PermissionInfo>(
                this._dataProvider.GetPermissionInfo(permissionCode, permissionKey, permissionName));
        }

        public void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible)
        {
            this._dataProvider.SetTabSpecificData(tabId, isDeleted, isVisible);
        }

        public void SetTabModuleDeleted(int tabModuleId, bool isDeleted)
        {
            this._dataProvider.SetTabModuleDeleted(tabModuleId, isDeleted);
        }

        public void SetUserDeleted(int portalId, int userId, bool isDeleted)
        {
            this._dataProvider.SetUserDeleted(portalId, userId, isDeleted);
        }

        public void RunSchedule()
        {
            var executingServer = ServerController.GetExecutingServerName();

            var scheduleItem = SchedulingController.GetSchedule(this.GetSchedulerTypeFullName(), executingServer);
            if (scheduleItem != null)
            {
                SchedulingProvider.Instance().RunScheduleItemNow(scheduleItem, true);

                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                {
                    SchedulingProvider.Instance().ReStart("Change made to schedule.");
                }
            }
        }

        protected override Func<IEntitiesController> GetFactory()
        {
            return () => new EntitiesController();
        }

        private string GetSchedulerTypeFullName()
        {
            var type = typeof(ExportImportScheduler);
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }
    }
}
