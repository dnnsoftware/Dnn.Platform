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
using System.Data;
using Dnn.ExportImport.Components.Common;
using PlatformDataProvider = DotNetNuke.Data.DataProvider;

namespace Dnn.ExportImport.Components.Providers
{
    public class DataProvider
    {
        #region Shared/Static Methods

        private static readonly DataProvider Provider;

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

        #region Public Methods

        public int AddNewJob(int portalId, int userId, JobType jobType, string exportFile, string serializedObject)
        {
            return PlatformDataProvider.Instance().ExecuteScalar<int>(
                "ExportImportJobs_Add", portalId, (int) jobType, userId, exportFile, serializedObject);
        }

        public void UpdateJobStatus(int jobId, JobStatus jobStatus)
        {
            DateTime? completeDate = null;
            if (jobStatus == JobStatus.DoneFailure || jobStatus == JobStatus.DoneSuccess)
                completeDate = DateTime.UtcNow;

            PlatformDataProvider.Instance().ExecuteNonQuery("ExportImportJobs_UpdateStatus", jobId, jobStatus, completeDate);
        }

        public IDataReader GetFirstActiveJob()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportImportJobs_FirstActive");
        }

        public IDataReader GetAllJobs(int? portalId, int? pageSize, int? pageIndex)
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportImportJobs_GetAll", portalId, pageSize, pageIndex);
        }

        public IDataReader GetAllScopeTypes()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_ScopeTypes");
        }

        public IDataReader GetAllVocabularyTypes()
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_VocabularyTypes");
        }

        public IDataReader GetAllTerms(DateTime? sinceDate)
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_Terms", sinceDate);
        }

        public IDataReader GetAllVocabularies(DateTime? sinceDate)
        {
            return PlatformDataProvider.Instance().ExecuteReader("ExportTaxonomy_Vocabularies", sinceDate);
        }

        public IDataReader GetAllRoleGroups(int portalId, DateTime? sinceDate)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_RoleGroups", portalId, sinceDate);
        }

        public IDataReader GetAllRoles(int portalId, DateTime? sinceDate)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_Roles", portalId, sinceDate);
        }

        public IDataReader GetAllRoleSettings(int portalId, DateTime? sinceDate)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_RoleSettings", portalId, sinceDate);
        }

        public IDataReader GetPropertyDefinitionsByPortal(int portalId, bool includeDeleted, DateTime? sinceDate)
        {
            return PlatformDataProvider.Instance()
                .ExecuteReader("Export_GetPropertyDefinitionsByPortal", portalId, includeDeleted, sinceDate);
        }

        public void UpdateRoleGroupChangers(int roleGroupId, int createdBy, int modifiedBy)
        {
            PlatformDataProvider.Instance().ExecuteNonQuery(
                "Export_UpdateRoleGroupChangers", roleGroupId, createdBy, modifiedBy);
        }

        public void UpdateRoleChangers(int roleId, int createdBy, int modifiedBy)
        {
            PlatformDataProvider.Instance().ExecuteNonQuery(
                "Export_UpdateRoleChangers", roleId, createdBy, modifiedBy);
        }

        public void UpdateRoleSettingChangers(int roleId, string settingName, int createdBy, int modifiedBy)
        {
            PlatformDataProvider.Instance().ExecuteNonQuery(
                "Export_UpdateRoleSettingChangers", roleId, settingName, createdBy, modifiedBy);
        }

        public IDataReader GetAllUsers(int portalId, int pageIndex, int pageSize, bool includeDeleted, DateTime? sinceDate)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_GetAllUsers", portalId, pageIndex, pageSize, includeDeleted, sinceDate);
        }

        public IDataReader GetAspNetUser(string username)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_GetAspNetUser", username);
        }

        public IDataReader GetUserMembership(Guid userId, Guid applicationId)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_GetUserMembership", userId, applicationId);
        }

        public IDataReader GetUserRoles(int portalId, int userId)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_GetUserRoles", portalId, userId);
        }

        public IDataReader GetUserPortal(int portalId, int userId)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_GetUserPortal", portalId, userId);
        }
        public IDataReader GetUserAuthentication(int userId)
        {
            return PlatformDataProvider.Instance().ExecuteReader("GetUserAuthentication", userId);
        }

        public IDataReader GetUserProfile(int userId)
        {
            return PlatformDataProvider.Instance().ExecuteReader("Export_GetUserProfile", userId);
        }

        public void UpdateUserChangers(int userId, string createdByUserName, string modifiedByUserName)
        {
            PlatformDataProvider.Instance().ExecuteNonQuery(
                "Export_UpdateUsersChangers", userId, createdByUserName, modifiedByUserName);
        }
        #region Users

        #endregion  
        #endregion
    }
}