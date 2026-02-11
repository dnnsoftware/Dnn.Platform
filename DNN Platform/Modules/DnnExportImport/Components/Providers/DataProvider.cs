// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Entities;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A data provider.</summary>
    internal sealed class DataProvider
    {
        private static readonly DataProvider Provider;

        private readonly DotNetNuke.Data.DataProvider dataProvider = DotNetNuke.Data.DataProvider.Instance();
        private readonly IHostSettings hostSettings;

        static DataProvider()
        {
            Provider = new DataProvider(Globals.DependencyProvider.GetRequiredService<IHostSettings>());
        }

        private DataProvider(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings;
        }

        /// <summary>Gets the singleton instance of <see cref="DataProvider"/>.</summary>
        /// <returns>The instance.</returns>
        public static DataProvider Instance()
        {
            return Provider;
        }

        /// <summary>Update record changers.</summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="primaryKeyName">The primary key name.</param>
        /// <param name="primaryKeyId">The primary key ID.</param>
        /// <param name="createdBy">The ID of the creating user.</param>
        /// <param name="modifiedBy">The ID of the modifying user.</param>
        public void UpdateRecordChangers(string tableName, string primaryKeyName, int primaryKeyId, int? createdBy, int? modifiedBy)
        {
            this.dataProvider.ExecuteNonQuery(
                "Export_GenericUpdateRecordChangers", tableName, primaryKeyName, primaryKeyId, createdBy, modifiedBy);
        }

        /// <summary>Update the unique ID.</summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="primaryKeyName">The primary key name.</param>
        /// <param name="primaryKeyId">The primary key ID.</param>
        /// <param name="uniqueId">The unique ID.</param>
        public void UpdateUniqueId(string tableName, string primaryKeyName, int primaryKeyId, Guid uniqueId)
        {
            this.dataProvider.ExecuteNonQuery("Export_UpdateUniqueId", tableName, primaryKeyName, primaryKeyId, uniqueId);
        }

        /// <summary>Update setting record changers.</summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="primaryKeyName">The primary key name.</param>
        /// <param name="parentKeyId">The parent key ID.</param>
        /// <param name="settingName">The setting name.</param>
        /// <param name="createdBy">The ID of the creating user.</param>
        /// <param name="modifiedBy">The ID of the modifying user.</param>
        public void UpdateSettingRecordChangers(string tableName, string primaryKeyName, int parentKeyId, string settingName, int? createdBy, int? modifiedBy)
        {
            this.dataProvider.ExecuteNonQuery(
                "Export_GenedicUpdateSettingsRecordChangers", tableName, primaryKeyName, parentKeyId, settingName, createdBy, modifiedBy);
        }

        /// <summary>Add a new import/export job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="jobType">The job type.</param>
        /// <param name="jobName">The job name.</param>
        /// <param name="jobDescription">The job description.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="serializedObject">The job object as JSON.</param>
        /// <returns>The job ID.</returns>
        public int AddNewJob(int portalId, int userId, JobType jobType, string jobName, string jobDescription, string directory, string serializedObject)
        {
            return this.dataProvider.ExecuteScalar<int>(
                "ExportImportJobs_Add",
                portalId,
                (int)jobType,
                userId,
                jobName,
                jobDescription,
                directory,
                serializedObject);
        }

        /// <summary>Update a job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="name">The job name.</param>
        /// <param name="description">The job description.</param>
        public void UpdateJobInfo(int jobId, string name, string description)
        {
            this.dataProvider.ExecuteNonQuery("ExportImportJobs_UpdateInfo", jobId, name, description);
        }

        /// <summary>Update a job status.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="jobStatus">The job status.</param>
        public void UpdateJobStatus(int jobId, JobStatus jobStatus)
        {
            DateTime? completeDate = null;
            if (jobStatus == JobStatus.Failed || jobStatus == JobStatus.Successful)
            {
                completeDate = DateUtils.GetDatabaseUtcTime();
            }

            this.dataProvider.ExecuteNonQuery(
                "ExportImportJobs_UpdateStatus", jobId, jobStatus, completeDate);
        }

        /// <summary>Set a job as canceled.</summary>
        /// <param name="jobId">The job ID.</param>
        public void SetJobCancelled(int jobId)
        {
            this.dataProvider.ExecuteNonQuery("ExportImportJobs_SetCancelled", jobId);
        }

        /// <summary>Remove a job.</summary>
        /// <param name="jobId">The job ID.</param>
        public void RemoveJob(int jobId)
        {
            // using 60 sec timeout because cascading deletes in logs might take a lot of time
            this.dataProvider.ExecuteNonQuery(60, "ExportImportJobs_Remove", jobId);
        }

        /// <summary>Get the import/export settings.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetExportImportSettings()
        {
            return this.dataProvider.ExecuteReader("ExportImport_Settings");
        }

        /// <summary>Add a setting.</summary>
        /// <param name="exportImportSetting">The setting.</param>
        public void AddExportImportSetting(ExportImportSetting exportImportSetting)
        {
            this.dataProvider.ExecuteNonQuery(
                "ExportImport_AddSetting",
                exportImportSetting.SettingName,
                exportImportSetting.SettingValue,
                exportImportSetting.SettingIsSecure,
                exportImportSetting.CreatedByUserId);
        }

        /// <summary>Get the first active job.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetFirstActiveJob()
        {
            return this.dataProvider.ExecuteReader("ExportImportJobs_FirstActive");
        }

        /// <summary>Gets a job by ID.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobById(int jobId)
        {
            return this.dataProvider.ExecuteReader("ExportImportJobs_GetById", jobId);
        }

        /// <summary>Gets the summary logs for a job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobSummaryLog(int jobId)
        {
            return this.dataProvider.ExecuteReader("ExportImportJobLogs_Summary", jobId);
        }

        /// <summary>Gets full logs for a job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobFullLog(int jobId)
        {
            return this.dataProvider.ExecuteReader("ExportImportJobLogs_Full", jobId);
        }

        /// <summary>Get the count of all jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type ID.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>The count.</returns>
        public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
        {
            return this.dataProvider.ExecuteScalar<int>("ExportImport_GetJobsCount", portalId, jobType, keywords);
        }

        /// <summary>Get all jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="jobType">The job type ID.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
            return this.dataProvider.ExecuteReader(
                "ExportImportJobs_GetAll", portalId, pageSize, pageIndex, jobType, keywords);
        }

        /// <summary>Get a job's checkpoints.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobChekpoints(int jobId)
        {
            return this.dataProvider.ExecuteReader("ExportImportCheckpoints_GetByJob", jobId);
        }

        /// <summary>Get the last job time.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type.</param>
        /// <returns>The date/time or <see langword="null"/>.</returns>
        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
            var datim = this.dataProvider.ExecuteScalar<DateTime?>("ExportImportJobLogs_LastJobTime", portalId, jobType);
            if (datim.HasValue)
            {
                var d = datim.Value;
                datim = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond, DateTimeKind.Utc);
            }

            return datim;
        }

        /// <summary>Updates or inserts a job checkpoint.</summary>
        /// <param name="checkpoint">The checkpoint.</param>
        public void UpsertJobChekpoint(ExportImportChekpoint checkpoint)
        {
            this.dataProvider.ExecuteNonQuery(
                "ExportImportCheckpoints_Upsert",
                checkpoint.JobId,
                checkpoint.AssemblyName,
                checkpoint.Category,
                checkpoint.Stage,
                checkpoint.StageData,
                Null.SetNullInteger(Math.Floor(checkpoint.Progress)),
                checkpoint.TotalItems,
                checkpoint.ProcessedItems,
                this.dataProvider.GetNull(checkpoint.StartDate),
                checkpoint.Completed);
        }

        /// <summary>Get all scope types.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllScopeTypes()
        {
            return this.dataProvider.ExecuteReader("ExportTaxonomy_ScopeTypes");
        }

        /// <summary>Gets all vocabulary types.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllVocabularyTypes()
        {
            return this.dataProvider.ExecuteReader("ExportTaxonomy_VocabularyTypes");
        }

        /// <summary>Get all terms.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTerms(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("ExportTaxonomy_Terms", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all vocabularies.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllVocabularies(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("ExportTaxonomy_Vocabularies", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all role groups.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllRoleGroups(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_RoleGroups", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all roles.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllRoles(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_Roles", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all role settings.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllRoleSettings(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_RoleSettings", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Gets the ID of a role.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="roleName">The role name.</param>
        /// <returns>The ID or <see cref="Globals.glbRoleNothing"/>.</returns>
        public int GetRoleIdByName(int portalId, string roleName)
        {
            return this.dataProvider.ExecuteScalar<int>("Export_RoleIdByName", this.dataProvider.GetNull(portalId), roleName);
        }

        /// <summary>Set role auto-assign.</summary>
        /// <param name="roleId">The role ID.</param>
        public void SetRoleAutoAssign(int roleId)
        {
            this.dataProvider.ExecuteNonQuery("Export_RoleSetAutoAssign", roleId);
        }

        /// <summary>Get property definitions for the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted properties.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPropertyDefinitionsByPortal(int portalId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader(
                "Export_GetPropertyDefinitionsByPortal",
                portalId,
                includeDeleted,
                toDate,
                this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all users.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="includeDeleted">Whether to include deleted users.</param>
        /// <param name="toDateUtc">The end date.</param>
        /// <param name="fromDateUtc">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllUsers(int portalId, int pageIndex, int pageSize, bool includeDeleted, DateTime toDateUtc, DateTime? fromDateUtc)
        {
            return this.dataProvider.ExecuteReader(
                "Export_GetAllUsers",
                portalId,
                pageIndex,
                pageSize,
                includeDeleted,
                toDateUtc,
                this.dataProvider.GetNull(fromDateUtc),
                false);
        }

        /// <summary>Gets the users count.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted users.</param>
        /// <param name="toDateUtc">The end date.</param>
        /// <param name="fromDateUtc">The start date or <see langword="null"/>.</param>
        /// <returns>The count.</returns>
        public int GetUsersCount(int portalId, bool includeDeleted, DateTime toDateUtc, DateTime? fromDateUtc)
        {
            return this.dataProvider
                .ExecuteScalar<int>("Export_GetAllUsers", portalId, 0, 0, includeDeleted, toDateUtc, this.dataProvider.GetNull(fromDateUtc), true);
        }

        /// <summary>Update users changers.</summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="createdByUserName">The username of the creating user.</param>
        /// <param name="modifiedByUserName">The username of the modifying user.</param>
        public void UpdateUserChangers(int userId, string createdByUserName, string modifiedByUserName)
        {
            this.dataProvider.ExecuteNonQuery(
                "Export_UpdateUsersChangers", userId, createdByUserName, modifiedByUserName);
        }

        /// <summary>Get portal settings.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPortalSettings(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_GetPortalSettings", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get portal languages.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPortalLanguages(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_GetPortalLanguages", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get portal localizations.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPortalLocalizations(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_GetPortalLocalizations", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get folders.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFolders(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_GetFolders", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get folder permissions by path.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFolderPermissionsByPath(int portalId, string folderPath, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider
                .ExecuteReader("Export_GetFolderPermissionsByPath", portalId, folderPath, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get folder mappings.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFolderMappings(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_GetFolderMappings", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get files.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="folderId">The folder ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFiles(int portalId, int? folderId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_GetFiles", portalId, folderId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Gets the permission ID.</summary>
        /// <param name="permissionCode">The permission code.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <param name="permissionName">The permission name.</param>
        /// <returns>The permission ID or <see langword="null"/>.</returns>
        public int? GetPermissionId(string permissionCode, string permissionKey, string permissionName)
        {
            var permissions = CBO.GetCachedObject<IEnumerable<PermissionInfo>>(
                this.hostSettings,
                new CacheItemArgs(DataCache.PermissionsCacheKey, DataCache.PermissionsCacheTimeout, DataCache.PermissionsCachePriority),
                c => CBO.FillCollection<PermissionInfo>(this.dataProvider.ExecuteReader("GetPermissions")));
            return
                (from IPermissionDefinitionInfo x in permissions
                where x.PermissionCode == permissionCode
                where x.PermissionKey == permissionKey
                where x.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase)
                select (int?)x.PermissionId)
                .FirstOrDefault();
        }

        /// <summary>Get all portal tabs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted tabs.</param>
        /// <param name="includeSystem">Whether to include system tabs.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_Tabs", portalId, includeDeleted, includeSystem, toDate, fromDate);
        }

        /// <summary>Get all tab settings.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_TabSettings", tabId, toDate, fromDate);
        }

        /// <summary>Get all tab permissions.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_TabPermissions", tabId, toDate, fromDate);
        }

        /// <summary>Get all tab URLs.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_TabUrls", tabId, toDate, fromDate);
        }

        /// <summary>Get all modules for a page.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include deleted modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_Modules", tabId, includeDeleted, toDate, fromDate);
        }

        /// <summary>Get all module settings.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_ModuleSettings", moduleId, toDate, fromDate);
        }

        /// <summary>Get all module permissions.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_ModulePermissions", moduleId, toDate, fromDate);
        }

        /// <summary>Get all tab modules.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include deleted tab-modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_TabModules", tabId, includeDeleted, toDate, fromDate);
        }

        /// <summary>Check if a tab-module unique ID exists.</summary>
        /// <param name="uniqueId">The unique ID.</param>
        /// <returns><see langword="true"/> if it exists, otherwise <see langword="false"/>.</returns>
        public bool CheckTabModuleUniqueIdExists(Guid uniqueId)
        {
            return this.dataProvider.ExecuteScalar<int?>("ExportImport_CheckTabModuleUniqueIdExists", uniqueId) > 0;
        }

        /// <summary>Check if a tab unique ID exists.</summary>
        /// <param name="uniqueId">The unique ID.</param>
        /// <returns><see langword="true"/> if it exists, otherwise <see langword="false"/>.</returns>
        public bool CheckTabUniqueIdExists(Guid uniqueId)
        {
            return this.dataProvider.ExecuteScalar<int?>("ExportImport_CheckTabUniqueIdExists", uniqueId) > 0;
        }

        /// <summary>Get all tab-module settings.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include settings for deleted tab-modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_TabModuleSettings", tabId, includeDeleted, toDate, fromDate);
        }

        /// <summary>Sets specific data on the tab.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="isDeleted">Whether the tab is deleted.</param>
        /// <param name="isVisible">Whether the tab is visible.</param>
        public void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible)
        {
            this.dataProvider.ExecuteNonQuery("Export_SetTabSpecificData", tabId, isDeleted, isVisible);
        }

        /// <summary>Sets whether a tab-module is deleted.</summary>
        /// <param name="tabModuleId">The tab-module ID.</param>
        /// <param name="isDeleted">Whether it is deleted.</param>
        public void SetTabModuleDeleted(int tabModuleId, bool isDeleted)
        {
            this.dataProvider.ExecuteNonQuery("Export_SetTabModuleDeleted", tabModuleId, isDeleted);
        }

        /// <summary>Sets whether a user is deleted.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="isDeleted">Whether it is deleted.</param>
        public void SetUserDeleted(int portalId, int userId, bool isDeleted)
        {
            this.dataProvider.ExecuteNonQuery("Export_SetUserDeleted", portalId, userId, isDeleted);
        }

        /// <summary>Gets the permission info.</summary>
        /// <param name="permissionCode">The permission code.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <param name="permissionName">The permission name.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
        {
            return this.dataProvider.ExecuteReader("Export_GetPermissionInfo", permissionCode, permissionKey, permissionName);
        }

        /// <summary>Update the tab URL changers.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="seqNum">The sequence number.</param>
        /// <param name="createdBy">The ID of the creating user.</param>
        /// <param name="modifiedBy">The ID of the modifying user.</param>
        public void UpdateTabUrlChangers(int tabId, int seqNum, int? createdBy, int? modifiedBy)
        {
            this.dataProvider.ExecuteNonQuery("Export_UpdateTabUrlChangers", tabId, seqNum, createdBy, modifiedBy);
        }

        /// <summary>Gets all workflows.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted workflows.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflows(int portalId, bool includeDeleted)
        {
            return this.dataProvider.ExecuteReader("Export_ContentWorkflows", portalId, includeDeleted);
        }

        /// <summary>Gets all workflow sources.</summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflowSources(int workflowId)
        {
            return this.dataProvider.ExecuteReader("Export_ContentWorkflowSources", workflowId);
        }

        /// <summary>Gets all workflow states.</summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflowStates(int workflowId)
        {
            return this.dataProvider.ExecuteReader("Export_ContentWorkflowStates", workflowId);
        }

        /// <summary>Gets all workflow state permissions.</summary>
        /// <param name="workflowStateId">The state ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflowStatePermissions(int workflowStateId, DateTime toDate, DateTime? fromDate)
        {
            return this.dataProvider.ExecuteReader("Export_ContentWorkflowStatePermissions", workflowStateId, toDate, fromDate);
        }
    }
}
