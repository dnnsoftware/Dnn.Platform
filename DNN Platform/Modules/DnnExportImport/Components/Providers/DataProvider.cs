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
using System.Data;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Permissions;

namespace Dnn.ExportImport.Components.Providers
{
    public class DataProvider
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
            return _dataProvider.ExecuteScalar<DateTime?>("ExportImportJobLogs_LastJobTime", portalId, jobType);
        }

        public void UpsertJobChekpoint(ExportImportChekpoint checkpoint)
        {
            _dataProvider.ExecuteNonQuery("ExportImportCheckpoints_Upsert",
                checkpoint.JobId, checkpoint.Category, checkpoint.Stage, checkpoint.StageData,
                Null.SetNullInteger(Math.Floor(checkpoint.Progress)), checkpoint.TotalItems, checkpoint.ProcessedItems, _dataProvider.GetNull(checkpoint.StartDate));
        }

        public IDataReader GetAllScopeTypes()
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_ScopeTypes");
        }

        public IDataReader GetAllVocabularyTypes()
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_VocabularyTypes");
        }

        public IDataReader GetAllTerms(DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_Terms", toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAllVocabularies(DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("ExportTaxonomy_Vocabularies", toDate, _dataProvider.GetNull(fromDate));
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

        public void SetRoleAutoAssign(int roleId)
        {
            _dataProvider.ExecuteNonQuery("Export_RoleSetAutoAssign", roleId);
        }

        public IDataReader GetPropertyDefinitionsByPortal(int portalId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider
                .ExecuteReader("Export_GetPropertyDefinitionsByPortal", portalId, includeDeleted, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAllUsers(int portalId, int pageIndex, int pageSize, bool includeDeleted, DateTime toDate,
            DateTime? fromDate)
        {
            return _dataProvider
                .ExecuteReader("Export_GetAllUsers", portalId, pageIndex, pageSize, includeDeleted, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetAspNetUser(string username, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetAspNetUser", username, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetUserMembership(Guid userId, Guid applicationId)
        {
            return _dataProvider.ExecuteReader("Export_GetUserMembership", userId, applicationId);
        }

        public IDataReader GetUserRoles(int portalId, int userId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetUserRoles", portalId, userId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetUserPortal(int portalId, int userId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetUserPortal", portalId, userId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetUserAuthentication(int userId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetUserAuthentication", userId, toDate, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetUserProfile(int portalId, int userId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetUserProfile", portalId, userId, toDate, _dataProvider.GetNull(fromDate));
        }

        public void UpdateUserChangers(int userId, string createdByUserName, string modifiedByUserName)
        {
            _dataProvider.ExecuteNonQuery(
                "Export_UpdateUsersChangers", userId, createdByUserName, modifiedByUserName);
        }

        public IDataReader GetPortalSettings(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetPortalSettings", portalId, _dataProvider.GetNull(fromDate));
        }

        public IDataReader GetPortalPermissions(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetPortalSettings", portalId, _dataProvider.GetNull(fromDate));
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
                                         && x.PermissionName == permissionName)?.PermissionID;
        }

        public IDataReader GetAllPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_GetAllPortalTabs", portalId, includeDeleted, includeSystem, toDate, fromDate);
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

        public IDataReader GetAllTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
            return _dataProvider.ExecuteReader("Export_TabModuleSettings", tabId, toDate, fromDate);
        }

        public void SetTabModuleDeleted(int tabModuleId)
        {
            _dataProvider.ExecuteNonQuery("Export_SetTabModuleDeleted", tabModuleId);
        }

        public IDataReader GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
        {
            return _dataProvider.ExecuteReader("Export_GetPermissionInfo", permissionCode, permissionKey, permissionName);
        }

        public void UpdateTabUrlChangers(int tabId, int seqNum, int? createdBy, int? modifiedBy)
        {
            _dataProvider.ExecuteNonQuery("Export_UpdateTabUrlChangers", tabId, seqNum, createdBy, modifiedBy);
        }
    }
}