﻿#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
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
using System.Data;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Permissions;

namespace Dnn.ExportImport.Components.Providers
{
    internal class DataProvider
    {
        #region Shared/Static Methods

        private static readonly DataProvider Provider;

        private readonly DotNetNuke.Data.DataProvider _dataProvider = DotNetNuke.Data.DataProvider.Instance();

        static DataProvider()
        {
            Provider = new DataProvider();
        }

        public static DataProvider Instance()
        {
            return Provider;
        }

        private DataProvider()
        {
            // so it can't be instantiated outside this class
        }

        #endregion

        public void UpdateRecordChangers(string tableName, string primaryKeyName, int primaryKeyId, int? createdBy, int? modifiedBy)
        {
            _dataProvider.ExecuteNonQuery(
                "Export_GenericUpdateRecordChangers", tableName, primaryKeyName, primaryKeyId, createdBy, modifiedBy);
        }

        public void UpdateUniqueId(string tableName, string primaryKeyName, int primaryKeyId, Guid uniqueId)
        {
            _dataProvider.ExecuteNonQuery("Export_UpdateUniqueId", tableName, primaryKeyName, primaryKeyId, uniqueId);
        }

        public void UpdateSettingRecordChangers(string tableName, string primaryKeyName, int parentKeyId, string settingName, int? createdBy, int? modifiedBy)
        {
            _dataProvider.ExecuteNonQuery(
                "Export_GenedicUpdateSettingsRecordChangers", tableName, primaryKeyName, parentKeyId, settingName, createdBy, modifiedBy);
        }

        public int AddNewJob(int portalId, int userId, JobType jobType,
            string jobName, string jobDescription, string directory, string serializedObject)
        {
            return _dataProvider.ExecuteScalar<int>("ExportImportJobs_Add", portalId,
                (int)jobType, userId, jobName, jobDescription, directory, serializedObject);
        }

        public void UpdateJobInfo(int jobId, string name, string description)
        {
            _dataProvider.ExecuteNonQuery("ExportImportJobs_UpdateInfo", jobId, name, description);
        }

        public void UpdateJobStatus(int jobId, JobStatus jobStatus)
        {
            DateTime? completeDate = null;
            if (jobStatus == JobStatus.Failed || jobStatus == JobStatus.Successful)
                completeDate = DateUtils.GetDatabaseUtcTime();

            _dataProvider.ExecuteNonQuery(
                "ExportImportJobs_UpdateStatus", jobId, jobStatus, completeDate);
        }

        public void SetJobCancelled(int jobId)
        {
            _dataProvider.ExecuteNonQuery("ExportImportJobs_SetCancelled", jobId);
        }

        public void RemoveJob(int jobId)
        {
            // using 60 sec timeout because cascading deletes in logs might take a lot of time
            _dataProvider.ExecuteNonQuery(60, "ExportImportJobs_Remove", jobId);
        }

        public IDataReader GetExportImportSettings()
        {
            return _dataProvider.ExecuteReader("ExportImport_Settings");
        }
        public void AddExportImportSetting(ExportImportSetting exportImportSetting)
        {
            _dataProvider.ExecuteNonQuery("ExportImport_AddSetting", exportImportSetting.SettingName,
                exportImportSetting.SettingValue, exportImportSetting.SettingIsSecure,
                exportImportSetting.CreatedByUserId);
        }

        public IDataReader GetFirstActiveJob()
        {
            return _dataProvider.ExecuteReader("ExportImportJobs_FirstActive");
        }

        public IDataReader GetJobById(int jobId)
        {
            return _dataProvider.ExecuteReader("ExportImportJobs_GetById", jobId);
        }

        public IDataReader GetJobSummaryLog(int jobId)
        {
            return _dataProvider.ExecuteReader("ExportImportJobLogs_Summary", jobId);
        }

        public IDataReader GetJobFullLog(int jobId)
        {
            return _dataProvider.ExecuteReader("ExportImportJobLogs_Full", jobId);
        }

        public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
        {
            return _dataProvider.ExecuteScalar<int>("ExportImport_GetJobsCount", portalId, jobType, keywords);
        }

        public IDataReader GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
            return _dataProvider.ExecuteReader(
                "ExportImportJobs_GetAll", portalId, pageSize, pageIndex, jobType, keywords);
        }

        public IDataReader GetJobChekpoints(int jobId)
        {
            return _dataProvider.ExecuteReader("ExportImportCheckpoints_GetByJob", jobId);
        }

        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
            var datim = _dataProvider.ExecuteScalar<DateTime?>("ExportImportJobLogs_LastJobTime", portalId, jobType);
            if (datim.HasValue)
            {
                var d = datim.Value;
                datim = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond, DateTimeKind.Utc);
            }
            return datim;
        }

        public void UpsertJobChekpoint(ExportImportChekpoint checkpoint)
        {
            _dataProvider.ExecuteNonQuery("ExportImportCheckpoints_Upsert",
                checkpoint.JobId, checkpoint.AssemblyName, checkpoint.Category, checkpoint.Stage, checkpoint.StageData,
                Null.SetNullInteger(Math.Floor(checkpoint.Progress)), checkpoint.TotalItems, checkpoint.ProcessedItems, _dataProvider.GetNull(checkpoint.StartDate), checkpoint.Completed);
        }

        public IDataReader GetAllScopeTypes()
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_ScopeTypes");
        }

        public IDataReader GetAllVocabularyTypes()
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_VocabularyTypes");
        }

        public IDataReader GetAllTerms(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_Terms", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAllVocabularies(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_Vocabularies", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAllRoleGroups(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_RoleGroups", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAllRoles(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_Roles", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAllRoleSettings(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_RoleSettings", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public int GetRoleIdByName(int portalId, string roleName)
        {
            return _dataProvider.ExecuteScalar<int>("Export_RoleIdByName", _dataProvider.GetNull(portalId), roleName);
        }


        public void SetRoleAutoAssign(int roleId)
        {
            _dataProvider.ExecuteNonQuery("Export_RoleSetAutoAssign", roleId);
        }

        public IDataReader GetPropertyDefinitionsByPortal(int portalId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider
                .ExecuteReader("Export_GetPropertyDefinitionsByPortal", portalId, includeDeleted, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAllUsers(int portalId, int pageIndex, int pageSize, bool includeDeleted, DateTime toDateUtc, DateTime? fromDateUtc)
        {
            return _dataProvider
                .ExecuteReader("Export_GetAllUsers", portalId, pageIndex, pageSize, includeDeleted, toDateUtc,
                    _dataProvider.GetNull(fromDateUtc), false);
        }

        public int GetUsersCount(int portalId, bool includeDeleted, DateTime toDateUtc, DateTime? fromDateUtc)
        {
            return _dataProvider
                .ExecuteScalar<int>("Export_GetAllUsers", portalId, 0, 0, includeDeleted, toDateUtc, _dataProvider.GetNull(fromDateUtc), true);
        }

        public void UpdateUserChangers(int userId, string createdByUserName, string modifiedByUserName)
        {
            _dataProvider.ExecuteNonQuery(
                "Export_UpdateUsersChangers", userId, createdByUserName, modifiedByUserName);
        }

        public IDataReader GetPortalSettings(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetPortalSettings", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetPortalLanguages(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetPortalLanguages", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetPortalLocalizations(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetPortalLocalizations", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetFolders(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetFolders", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetFolderPermissionsByPath(int portalId, string folderPath, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider
                .ExecuteReader("Export_GetFolderPermissionsByPath", portalId, folderPath, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetFolderMappings(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetFolderMappings", portalId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetFiles(int portalId, int? folderId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetFiles", portalId, folderId, toDate, _dataProvider.GetNull(fromDate));
        }

        public int? GetPermissionId(string permissionCode, string permissionKey, string permissionName)
        {
            return
                CBO.GetCachedObject<IEnumerable<PermissionInfo>>(new CacheItemArgs(DataCache.PermissionsCacheKey,
                    DataCache.PermissionsCacheTimeout,
                    DataCache.PermissionsCachePriority),
                    c =>
                        CBO.FillCollection<PermissionInfo>(
                            _dataProvider.ExecuteReader("GetPermissions")))
                    .FirstOrDefault(x => x.PermissionCode == permissionCode &&
                    x.PermissionKey == permissionKey
                    && x.PermissionName.Equals(permissionName, StringComparison.InvariantCultureIgnoreCase))?.PermissionID;
        }

        public IDataReader GetAllPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_Tabs", portalId, includeDeleted, includeSystem, toDate, fromDate);
        }

        public IDataReader GetAllTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_TabSettings", tabId, toDate, fromDate);
        }

        public IDataReader GetAllTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_TabPermissions", tabId, toDate, fromDate);
        }

        public IDataReader GetAllTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_TabUrls", tabId, toDate, fromDate);
        }

        public IDataReader GetAllModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_Modules", tabId, includeDeleted, toDate, fromDate);
        }

        public IDataReader GetAllModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_ModuleSettings", moduleId, toDate, fromDate);
        }

        public IDataReader GetAllModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_ModulePermissions", moduleId, toDate, fromDate);
        }

        public IDataReader GetAllTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_TabModules", tabId, includeDeleted, toDate, fromDate);
        }

        public bool CheckTabModuleUniqueIdExists(Guid uniqueId)
        {
            return _dataProvider.ExecuteScalar<int?>("ExportImport_CheckTabModuleUniqueIdExists", uniqueId) > 0;
        }

        public bool CheckTabUniqueIdExists(Guid uniqueId)
        {
            return _dataProvider.ExecuteScalar<int?>("ExportImport_CheckTabUniqueIdExists", uniqueId) > 0;
        }

        public IDataReader GetAllTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_TabModuleSettings", tabId, includeDeleted, toDate, fromDate);
        }

        public void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible)
        {
            _dataProvider.ExecuteNonQuery("Export_SetTabSpecificData", tabId, isDeleted, isVisible);
        }

        public void SetTabModuleDeleted(int tabModuleId, bool isDeleted)
        {
            _dataProvider.ExecuteNonQuery("Export_SetTabModuleDeleted", tabModuleId, isDeleted);
        }

        public void SetUserDeleted(int portalId, int userId, bool isDeleted)
        {
            _dataProvider.ExecuteNonQuery("Export_SetUserDeleted", portalId, userId, isDeleted);
        }

        public IDataReader GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
        {
            return _dataProvider.ExecuteReader("Export_GetPermissionInfo", permissionCode, permissionKey, permissionName);
        }

        public void UpdateTabUrlChangers(int tabId, int seqNum, int? createdBy, int? modifiedBy)
        {
            _dataProvider.ExecuteNonQuery("Export_UpdateTabUrlChangers", tabId, seqNum, createdBy, modifiedBy);
        }

        public IDataReader GetAllWorkflows(int portalId, bool includeDeleted)
        {
            return _dataProvider.ExecuteReader("Export_ContentWorkflows", portalId, includeDeleted);
        }

        public IDataReader GetAllWorkflowSources(int workflowId)
        {
            return _dataProvider.ExecuteReader("Export_ContentWorkflowSources", workflowId);
        }

        public IDataReader GetAllWorkflowStates(int workflowId)
        {
            return _dataProvider.ExecuteReader("Export_ContentWorkflowStates", workflowId);
        }

        public IDataReader GetAllWorkflowStatePermissions(int workflowStateId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_ContentWorkflowStatePermissions", workflowStateId, toDate, fromDate);
        }
    }
}