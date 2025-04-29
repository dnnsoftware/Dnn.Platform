// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Controllers;

using System;
using System.Collections.Generic;

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

/// <inheritdoc cref="IEntitiesController"/>
/// <seealso cref="ServiceLocator{TContract,TSelf}"/>
public class EntitiesController : ServiceLocator<IEntitiesController, EntitiesController>, IEntitiesController
{
    private readonly DataProvider dataProvider = DataProvider.Instance();

    /// <inheritdoc/>
    public ExportImportJob GetFirstActiveJob()
    {
        return CBO.Instance.FillObject<ExportImportJob>(this.dataProvider.GetFirstActiveJob());
    }

    /// <inheritdoc/>
    public ExportImportJob GetJobById(int jobId)
    {
        var job = CBO.Instance.FillObject<ExportImportJob>(this.dataProvider.GetJobById(jobId));

        // System.Diagnostics.Trace.WriteLine($"xxxxxxxxx job id={job?.JobId} IsCancelled={job?.IsCancelled} xxxxxxxxx");
        return job;
    }

    /// <inheritdoc/>
    public IList<ExportImportJobLog> GetJobSummaryLog(int jobId)
    {
        return CBO.Instance.FillCollection<ExportImportJobLog>(this.dataProvider.GetJobSummaryLog(jobId));
    }

    /// <inheritdoc/>
    public IList<ExportImportJobLog> GetJobFullLog(int jobId)
    {
        return CBO.Instance.FillCollection<ExportImportJobLog>(this.dataProvider.GetJobFullLog(jobId));
    }

    /// <inheritdoc/>
    public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
    {
        return this.dataProvider.GetAllJobsCount(portalId, jobType, keywords);
    }

    /// <inheritdoc/>
    public IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
    {
        return CBO.Instance.FillCollection<ExportImportJob>(
            this.dataProvider.GetAllJobs(portalId, pageSize, pageIndex, jobType, keywords));
    }

    /// <inheritdoc/>
    public DateTime? GetLastJobTime(int portalId, JobType jobType)
    {
        return this.dataProvider.GetLastJobTime(portalId, jobType);
    }

    /// <inheritdoc/>
    public void UpdateJobInfo(ExportImportJob job)
    {
        this.dataProvider.UpdateJobInfo(job.JobId, job.Name, job.Description);
    }

    /// <inheritdoc/>
    public void UpdateJobStatus(ExportImportJob job)
    {
        this.dataProvider.UpdateJobStatus(job.JobId, job.JobStatus);
    }

    /// <inheritdoc/>
    public void SetJobCancelled(ExportImportJob job)
    {
        this.dataProvider.SetJobCancelled(job.JobId);
    }

    /// <inheritdoc/>
    public void RemoveJob(ExportImportJob job)
    {
        this.dataProvider.RemoveJob(job.JobId);
    }

    /// <inheritdoc/>
    public IList<ExportImportChekpoint> GetJobChekpoints(int jobId)
    {
        return CBO.Instance.FillCollection<ExportImportChekpoint>(this.dataProvider.GetJobChekpoints(jobId));
    }

    /// <inheritdoc/>
    public void UpdateJobChekpoint(ExportImportChekpoint checkpoint)
    {
        this.dataProvider.UpsertJobChekpoint(checkpoint);
    }

    /// <inheritdoc/>
    public IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportTabInfo>(
            this.dataProvider.GetAllPortalTabs(portalId, includeDeleted, includeSystem, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportTabSetting>(
            this.dataProvider.GetAllTabSettings(tabId, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportTabPermission>(
            this.dataProvider.GetAllTabPermissions(tabId, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportTabUrl>(
            this.dataProvider.GetAllTabUrls(tabId, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportModule>(
            this.dataProvider.GetAllModules(tabId, includeDeleted, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportModuleSetting>(
            this.dataProvider.GetAllModuleSettings(moduleId, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportModulePermission>(
            this.dataProvider.GetAllModulePermissions(moduleId, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportTabModule>(
            this.dataProvider.GetAllTabModules(tabId, includeDeleted, toDate, fromDate));
    }

    /// <inheritdoc/>
    public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate)
    {
        return this.GetTabModuleSettings(tabId, true, toDate, fromDate);
    }

    /// <inheritdoc/>
    public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
    {
        return CBO.Instance.FillCollection<ExportTabModuleSetting>(
            this.dataProvider.GetAllTabModuleSettings(tabId, includeDeleted, toDate, fromDate));
    }

    /// <inheritdoc/>
    public PermissionInfo GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
    {
        return CBO.Instance.FillObject<PermissionInfo>(
            this.dataProvider.GetPermissionInfo(permissionCode, permissionKey, permissionName));
    }

    /// <inheritdoc/>
    public void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible)
    {
        this.dataProvider.SetTabSpecificData(tabId, isDeleted, isVisible);
    }

    /// <inheritdoc/>
    public void SetTabModuleDeleted(int tabModuleId, bool isDeleted)
    {
        this.dataProvider.SetTabModuleDeleted(tabModuleId, isDeleted);
    }

    /// <inheritdoc/>
    public void SetUserDeleted(int portalId, int userId, bool isDeleted)
    {
        this.dataProvider.SetUserDeleted(portalId, userId, isDeleted);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
