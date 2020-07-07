// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Pages;
    using DotNetNuke.Security.Permissions;

    public interface IEntitiesController
    {
        ExportImportJob GetFirstActiveJob();

        ExportImportJob GetJobById(int jobId);

        IList<ExportImportJobLog> GetJobSummaryLog(int jobId);

        IList<ExportImportJobLog> GetJobFullLog(int jobId);

        int GetAllJobsCount(int? portalId, int? jobType, string keywords);

        IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords);

        DateTime? GetLastJobTime(int portalId, JobType jobType);

        void UpdateJobInfo(ExportImportJob job);

        void UpdateJobStatus(ExportImportJob job);

        void SetJobCancelled(ExportImportJob job);

        void RemoveJob(ExportImportJob job);

        IList<ExportImportChekpoint> GetJobChekpoints(int jobId);

        void UpdateJobChekpoint(ExportImportChekpoint checkpoint);

        IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate);

        IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate);

        IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate);

        IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate);

        IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);

        IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate);

        IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate);

        IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);

        IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate);

        IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);

        PermissionInfo GetPermissionInfo(string permissionCode, string permissionKey, string permissionName);

        void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible);

        void SetTabModuleDeleted(int tabModuleId, bool isDeleted);

        void SetUserDeleted(int portalId, int userId, bool isDeleted);

        void RunSchedule();
    }
}
