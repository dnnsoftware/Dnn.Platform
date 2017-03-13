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
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Models;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Authentication;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Supplementary service to import users additional data.
    /// </summary>
    public class UsersDataExportService : IPortable2
    {
        private int _progressPercentage;

        public string Category => "USERS_DATA";

        public string ParentCategory => "USERS";

        public uint Priority => 4;

        public bool CanCancel => true;

        public bool CanRollback => false;

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            private set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _progressPercentage = value;
            }
        }

        public void ExportData(ExportImportJob exportJob, ExportDto exportDto, IExportImportRepository repository, ExportImportResult result)
        {
            ProgressPercentage = 100;
            //No implementation required in export users child as everything is exported in parent service.
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository, ExportImportResult result)
        {
            ProgressPercentage = 0;
            var pageIndex = 0;
            var pageSize = 1000;
            var totalUserRolesImported = 0;
            var totalProfilesImported = 0;
            var totalAuthenticationImported = 0;
            var totalProcessed = 0;
            var totalUsers = repository.GetCount<ExportUser>();
            var progressStep = totalUsers < pageSize ? 100 : pageSize/totalUsers*100;
            while (totalProcessed < totalUsers)
            {
                var users = repository.GetAllItems<ExportUser>(null, true, pageIndex*pageSize, pageSize).ToList();
                foreach (var user in users)
                {
                    var userRoles = repository.GetRelatedItems<ExportUserRole>(user.Id).ToList();
                    var userAuthentication =
                        repository.GetRelatedItems<ExportUserAuthentication>(user.Id).FirstOrDefault();
                    var userProfiles = repository.GetRelatedItems<ExportUserProfile>(user.Id).ToList();

                    using (var db = DataContext.Instance())
                    {
                        ProcessUserRoles(importJob, exporteDto, db, userRoles, user.UserId, user.Username, result);
                        totalUserRolesImported += userRoles.Count;

                        ProcessUserProfiles(importJob, exporteDto, db, userProfiles, user.UserId, user.Username, result);
                        totalProfilesImported += userProfiles.Count;

                        ProcessUserAuthentications(importJob, exporteDto, db, userAuthentication, user.UserId, user.Username, result);
                        if (userAuthentication != null) totalAuthenticationImported++;
                        //Update the source repository local ids.
                        repository.UpdateItems(userRoles);
                        repository.UpdateItems(userProfiles);
                        repository.UpdateItem(userAuthentication);
                        DataProvider.Instance()
                            .UpdateUserChangers(user.UserId, user.CreatedByUserName, user.LastModifiedByUserName);
                    }
                }
                totalProcessed += pageSize > users.Count ? users.Count : pageSize;
                ProgressPercentage += progressStep;
                pageIndex++;
            }
            result.AddSummary("Imported User Roles", totalUserRolesImported.ToString());
            result.AddSummary("Imported User Profiles", totalProfilesImported.ToString());
            result.AddSummary("Imported User Authentication", totalAuthenticationImported.ToString());
        }

        private static void ProcessUserRoles(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            IEnumerable<ExportUserRole> userRoles, int userId, string username, ExportImportResult result)
        {
            var repUserRoles = db.GetRepository<ExportUserRole>();

            foreach (var userRole in userRoles)
            {
                var roleId = Common.Util.GetRoleId(importJob.PortalId, userRole.RoleName);
                if (roleId == null) continue;

                var existingUserRole = RoleController.Instance.GetUserRole(importJob.PortalId, userId, roleId.Value);
                var isUpdate = false;
                if (existingUserRole != null)
                {
                    switch (exporteDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore: //Just ignore the record
                            result.AddLogEntry("Ignored user role", $"{username}/{userRole.RoleName}");
                            continue;
                        case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                            result.AddLogEntry("Ignored duplicate user role", $"{username}/{userRole.RoleName}");
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                    }
                }

                var modifiedById = Common.Util.GetUserIdOrName(importJob, userRole.LastModifiedByUserId,
                    userRole.LastModifiedByUserName);

                userRole.UserId = userId;
                userRole.RoleId = roleId.Value;
                userRole.LastModifiedOnDate = DateTime.UtcNow;
                userRole.EffectiveDate = userRole.EffectiveDate != null
                    ? (DateTime?) DateTime.UtcNow
                    : null;
                userRole.LastModifiedByUserId = modifiedById;
                if (isUpdate)
                {
                    userRole.CreatedByUserId = existingUserRole.CreatedByUserID;
                    userRole.CreatedOnDate = existingUserRole.CreatedOnDate;
                    userRole.UserRoleId = existingUserRole.UserRoleID;
                    repUserRoles.Update(userRole);
                    result.AddLogEntry("Updated user role", $"{username}/{userRole.RoleName}");
                }
                else
                {
                    var createdById = Common.Util.GetUserIdOrName(importJob, userRole.CreatedByUserId,
                        userRole.CreatedByUserName);
                    userRole.UserRoleId = 0;
                    userRole.CreatedByUserId = createdById;
                    userRole.CreatedOnDate = DateTime.UtcNow;
                    repUserRoles.Insert(userRole);
                    result.AddLogEntry("Added user role", $"{username}/{userRole.RoleName}");
                }
                userRole.LocalId = userRole.UserRoleId;
            }
        }

        private static void ProcessUserProfiles(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            IEnumerable<ExportUserProfile> userProfiles, int userId, string username, ExportImportResult result)
        {
            var repUserProfile = db.GetRepository<ExportUserProfile>();
            foreach (var userProfile in userProfiles)
            {
                var existingUserProfile =
                    CBO.FillCollection<ExportUserProfile>(
                        DataProvider.Instance().GetUserProfile(userId)).FirstOrDefault(x => x.PropertyName == userProfile.PropertyName);
                var isUpdate = false;
                if (existingUserProfile != null)
                {
                    switch (exporteDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore: //Just ignore the record
                            result.AddLogEntry("Ignored user profile", userProfile.PropertyName);
                            continue;
                        case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                            result.AddLogEntry("Ignored duplicate user profile", userProfile.PropertyName);
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                    }
                }
                userProfile.UserId = userId;
                userProfile.LastUpdatedDate = DateTime.UtcNow;
                if (isUpdate)
                {
                    userProfile.PropertyDefinitionId = existingUserProfile.PropertyDefinitionId;
                    userProfile.ProfileId = existingUserProfile.ProfileId;
                    repUserProfile.Update(userProfile);
                    result.AddLogEntry("Updated user profile", $"{username}/{userProfile.PropertyName}");
                }
                else
                {
                    userProfile.ProfileId = 0;
                    var profileDefinitionId = Common.Util.GetProfilePropertyId(importJob.PortalId,
                        userProfile.PropertyDefinitionId,
                        userProfile.PropertyName);
                    if (profileDefinitionId == null) continue;

                    userProfile.PropertyDefinitionId = profileDefinitionId.Value;
                    repUserProfile.Insert(userProfile);
                    result.AddLogEntry("Added user profile", userProfile.PropertyName);
                }
                userProfile.LocalId = userProfile.ProfileId;
            }
        }

        private static void ProcessUserAuthentications(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            ExportUserAuthentication userAuthentication, int userId, string username, ExportImportResult result)
        {
            if (userAuthentication == null) return;

            var repUserAuthentication = db.GetRepository<ExportUserAuthentication>();
            var existingUserAuthenticaiton = AuthenticationController.GetUserAuthentication(userId);
            var isUpdate = false;
            if (existingUserAuthenticaiton != null)
            {
                switch (exporteDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore: //Just ignore the record
                        result.AddLogEntry("Ignored user authentication", username);
                        return;
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        result.AddLogEntry("Ignored duplicate user authentication", username);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                }
            }
            var modifiedById = Common.Util.GetUserIdOrName(importJob, userAuthentication.LastModifiedByUserId,
                userAuthentication.LastModifiedByUserName);
            userAuthentication.LastModifiedOnDate = DateTime.UtcNow;
            userAuthentication.LastModifiedByUserId = modifiedById;
            userAuthentication.UserId = userId;
            if (isUpdate)
            {
                userAuthentication.UserAuthenticationId = existingUserAuthenticaiton.UserAuthenticationID;
                repUserAuthentication.Update(userAuthentication);
                result.AddLogEntry("Updated user authentication", username);
            }
            else
            {
                userAuthentication.UserAuthenticationId = 0;
                   var createdById = Common.Util.GetUserIdOrName(importJob, userAuthentication.CreatedByUserId,
                    userAuthentication.CreatedByUserName);
                userAuthentication.CreatedOnDate = DateTime.UtcNow;
                userAuthentication.CreatedByUserId = createdById;
                repUserAuthentication.Insert(userAuthentication);
                result.AddLogEntry("Added user authentication", username);
            }
            userAuthentication.LocalId = userAuthentication.UserAuthenticationId;
        }
    }
}