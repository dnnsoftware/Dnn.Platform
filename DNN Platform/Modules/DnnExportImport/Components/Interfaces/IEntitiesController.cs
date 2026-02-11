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

    /// <summary>A contract specifying the ability to manage entities.</summary>
    public interface IEntitiesController
    {
        /// <summary>Gets the first active job.</summary>
        /// <returns>The job or <see langword="null"/>.</returns>
        ExportImportJob GetFirstActiveJob();

        /// <summary>Gets the job by ID.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>The job or <see langword="null"/>.</returns>
        ExportImportJob GetJobById(int jobId);

        /// <summary>Gets a summary log of the job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A list of log entries.</returns>
        IList<ExportImportJobLog> GetJobSummaryLog(int jobId);

        /// <summary>Gets a full log of the job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A list of log entries.</returns>
        IList<ExportImportJobLog> GetJobFullLog(int jobId);

        /// <summary>Gets the count of all jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type ID.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>The count.</returns>
        int GetAllJobsCount(int? portalId, int? jobType, string keywords);

        /// <summary>Gets all the jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="jobType">The job type ID.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>A list of jobs.</returns>
        IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords);

        /// <summary>Gets the last job time.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type.</param>
        /// <returns>The date or <see langword="null"/>.</returns>
        DateTime? GetLastJobTime(int portalId, JobType jobType);

        /// <summary>Updates a job.</summary>
        /// <param name="job">The job info.</param>
        void UpdateJobInfo(ExportImportJob job);

        /// <summary>Updates a job's status.</summary>
        /// <param name="job">The job info.</param>
        void UpdateJobStatus(ExportImportJob job);

        /// <summary>Marks a job as canceled.</summary>
        /// <param name="job">The job info.</param>
        void SetJobCancelled(ExportImportJob job);

        /// <summary>Removes a job.</summary>
        /// <param name="job">The job info.</param>
        void RemoveJob(ExportImportJob job);

        /// <summary>Gets the checkpoints for a job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A list of checkpoints.</returns>
        IList<ExportImportChekpoint> GetJobChekpoints(int jobId);

        /// <summary>Updates a job checkpoint.</summary>
        /// <param name="checkpoint">The checkpoint info.</param>
        void UpdateJobChekpoint(ExportImportChekpoint checkpoint);

        /// <summary>Gets the portal tabs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted tabs.</param>
        /// <param name="includeSystem">Whether to include system tabs.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportTabInfo"/> instances.</returns>
        IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the tab settings.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportTabSetting"/> instances.</returns>
        IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the tab permissions.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportTabPermission"/> instances.</returns>
        IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the tab URLs.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportTabUrl"/> instances.</returns>
        IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the modules on a page.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include deleted modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportModule"/> instances.</returns>
        IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the module settings.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportModuleSetting"/> instances.</returns>
        IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the module permissions.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportModulePermission"/> instances.</returns>
        IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the tab modules.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include deleted tab-modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportTabModule"/> instances.</returns>
        IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the tab module settings.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportTabModuleSetting"/> instances.</returns>
        IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the tab module settings.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include settings from deleted tab-modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A list of <see cref="ExportTabModuleSetting"/> instances.</returns>
        IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate);

        /// <summary>Gets the permission info.</summary>
        /// <param name="permissionCode">The permission code.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <param name="permissionName">The permission name.</param>
        /// <returns>The <see cref="PermissionInfo"/> or <see langword="null"/>.</returns>
        PermissionInfo GetPermissionInfo(string permissionCode, string permissionKey, string permissionName);

        /// <summary>Set specific data on a tab.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="isDeleted">Whether the tab is deleted.</param>
        /// <param name="isVisible">Whether the tab is visible.</param>
        void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible);

        /// <summary>Set whether a tab module is deleted.</summary>
        /// <param name="tabModuleId">The tab-module ID.</param>
        /// <param name="isDeleted">Whether the tab-module is deleted.</param>
        void SetTabModuleDeleted(int tabModuleId, bool isDeleted);

        /// <summary>Set whether a user is deleted.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="isDeleted">Whether the user is deleted.</param>
        void SetUserDeleted(int portalId, int userId, bool isDeleted);

        /// <summary>Run the import/export scheduled task.</summary>
        void RunSchedule();
    }
}
