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
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Authentication;
using Newtonsoft.Json;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Supplementary service to import users additional data.
    /// </summary>
    public class UsersDataExportService : Portable2Base
    {
        public override string Category => Constants.Category_UsersData;

        public override string ParentCategory => Constants.Category_Users;

        public override uint Priority => 10;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            CheckPoint.Progress += 100;
            CheckPointStageCallback(this);
            //No implementation required in export users child as everything is exported in parent service.
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exportDto)
        {
            if (CheckCancelled(importJob)) return;

            var pageIndex = 0;
            const int pageSize = 1000;
            var totalUserRolesImported = 0;
            var totalProfilesImported = 0;
            var totalAuthenticationImported = 0;
            var totalProcessed = 0;
            var totalUsers = Repository.GetCount<ExportUser>();
            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);
            var skip = GetCurrentSkip();
            var currentIndex = skip;
            //Skip the import if all the users has been processed already.
            if (CheckPoint.Stage >= totalPages && skip == 0)
                return;

            pageIndex = CheckPoint.Stage;

            var totalUsersToBeProcessed = totalUsers - pageIndex * pageSize - skip;
            var progressStep = totalUsersToBeProcessed > 100 ? totalUsersToBeProcessed / 100 : 1;
            try
            {
                while (totalProcessed < totalUsersToBeProcessed)
                {
                    if (CheckCancelled(importJob)) return;
                    var users = Repository.GetAllItems<ExportUser>(null, true, pageIndex * pageSize + skip, pageSize).ToList();
                    skip = 0;
                    foreach (var user in users)
                    {
                        if (CheckCancelled(importJob)) return;
                        var userRoles = Repository.GetRelatedItems<ExportUserRole>(user.Id).ToList();
                        var userAuthentication =
                            Repository.GetRelatedItems<ExportUserAuthentication>(user.Id).FirstOrDefault();
                        var userProfiles = Repository.GetRelatedItems<ExportUserProfile>(user.Id).ToList();

                        using (var db = DataContext.Instance())
                        {
                            ProcessUserRoles(importJob, exportDto, db, userRoles, user.UserId, user.Username);
                            totalUserRolesImported += userRoles.Count;

                            ProcessUserProfiles(importJob, exportDto, db, userProfiles, user.UserId, user.Username);
                            totalProfilesImported += userProfiles.Count;

                            ProcessUserAuthentications(importJob, exportDto, db, userAuthentication, user.UserId, user.Username);
                            if (userAuthentication != null) totalAuthenticationImported++;
                            //Update the source repository local ids.
                            Repository.UpdateItems(userRoles);
                            Repository.UpdateItems(userProfiles);
                            Repository.UpdateItem(userAuthentication);
                            DataProvider.Instance()
                                .UpdateUserChangers(user.UserId, user.CreatedByUserName, user.LastModifiedByUserName);
                        }
                        currentIndex++;
                        if (currentIndex % progressStep == 0)
                            CheckPoint.Progress += 1;
                        if (CheckPointStageCallback(this)) return;
                    }
                    totalProcessed += currentIndex;
                    currentIndex = 0;
                    pageIndex++;

                    CheckPoint.Stage++;
                    CheckPoint.StageData = null;
                    if (CheckPointStageCallback(this)) return;
                }
                CheckPoint.Progress = 100;
                CheckPointStageCallback(this);
            }
            finally
            {
                CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                CheckPointStageCallback(this);
            }
            Result.AddSummary("Imported User Roles", totalUserRolesImported.ToString());
            Result.AddSummary("Imported User Profiles", totalProfilesImported.ToString());
            Result.AddSummary("Imported User Authentication", totalAuthenticationImported.ToString());
        }

        private void ProcessUserRoles(ExportImportJob importJob, ExportDto exportDto, IDataContext db,
            IEnumerable<ExportUserRole> userRoles, int userId, string username)
        {
            var repUserRoles = db.GetRepository<ExportUserRole>();

            foreach (var userRole in userRoles)
            {
                if (CheckCancelled(importJob)) return;
                var roleId = Util.GetRoleId(importJob.PortalId, userRole.RoleName);
                if (roleId == null) continue;

                var existingUserRole = RoleController.Instance.GetUserRole(importJob.PortalId, userId, roleId.Value);
                var isUpdate = false;
                if (existingUserRole != null)
                {
                    switch (exportDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore: //Just ignore the record
                            //Result.AddLogEntry("Ignored user role", $"{username}/{userRole.RoleName}");
                            continue;
                        case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                            //Result.AddLogEntry("Ignored duplicate user role", $"{username}/{userRole.RoleName}");
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                    }
                }

                var modifiedById = Util.GetUserIdOrName(importJob, userRole.LastModifiedByUserId,
                    userRole.LastModifiedByUserName);

                userRole.UserId = userId;
                userRole.RoleId = roleId.Value;
                userRole.LastModifiedOnDate = DateTime.UtcNow;
                userRole.EffectiveDate = userRole.EffectiveDate != null
                    ? (DateTime?)DateTime.UtcNow
                    : null;
                userRole.LastModifiedByUserId = modifiedById;
                if (isUpdate)
                {
                    userRole.CreatedByUserId = existingUserRole.CreatedByUserID;
                    userRole.CreatedOnDate = existingUserRole.CreatedOnDate;
                    userRole.UserRoleId = existingUserRole.UserRoleID;
                    repUserRoles.Update(userRole);
                    //Result.AddLogEntry("Updated user role", $"{username}/{userRole.RoleName}");
                }
                else
                {
                    var createdById = Util.GetUserIdOrName(importJob, userRole.CreatedByUserId,
                        userRole.CreatedByUserName);
                    userRole.UserRoleId = 0;
                    userRole.CreatedByUserId = createdById;
                    userRole.CreatedOnDate = DateTime.UtcNow;
                    repUserRoles.Insert(userRole);
                    //Result.AddLogEntry("Added user role", $"{username}/{userRole.RoleName}");
                }
                userRole.LocalId = userRole.UserRoleId;
            }
        }

        private void ProcessUserProfiles(ExportImportJob importJob, ExportDto exportDto, IDataContext db,
            IEnumerable<ExportUserProfile> userProfiles, int userId, string username)
        {
            var repUserProfile = db.GetRepository<ExportUserProfile>();
            foreach (var userProfile in userProfiles)
            {
                if (CheckCancelled(importJob)) return;
                var existingUserProfile =
                    CBO.FillCollection<ExportUserProfile>(
                        DataProvider.Instance().GetUserProfile(importJob.PortalId, userId)).FirstOrDefault(x => x.PropertyName == userProfile.PropertyName);
                var isUpdate = false;
                if (existingUserProfile != null)
                {
                    switch (exportDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore: //Just ignore the record
                            //Result.AddLogEntry("Ignored user profile", userProfile.PropertyName);
                            continue;
                        case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                            //Result.AddLogEntry("Ignored duplicate user profile", userProfile.PropertyName);
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                    }
                }
                userProfile.UserId = userId;
                userProfile.LastUpdatedDate = DateTime.UtcNow;
                if (isUpdate)
                {
                    userProfile.PropertyDefinitionId = existingUserProfile.PropertyDefinitionId;
                    userProfile.ProfileId = existingUserProfile.ProfileId;
                    repUserProfile.Update(userProfile);
                    //Result.AddLogEntry("Updated user profile", $"{username}/{userProfile.PropertyName}");
                }
                else
                {
                    userProfile.ProfileId = 0;
                    var profileDefinitionId = Util.GetProfilePropertyId(importJob.PortalId,
                        userProfile.PropertyDefinitionId,
                        userProfile.PropertyName);
                    if (profileDefinitionId == null) continue;

                    userProfile.PropertyDefinitionId = profileDefinitionId.Value;
                    repUserProfile.Insert(userProfile);
                    //Result.AddLogEntry("Added user profile", userProfile.PropertyName);
                }
                userProfile.LocalId = userProfile.ProfileId;
            }
        }

        private void ProcessUserAuthentications(ExportImportJob importJob, ExportDto exportDto, IDataContext db,
            ExportUserAuthentication userAuthentication, int userId, string username)
        {
            if (userAuthentication == null) return;

            var repUserAuthentication = db.GetRepository<ExportUserAuthentication>();
            var existingUserAuthenticaiton = AuthenticationController.GetUserAuthentication(userId);
            var isUpdate = false;
            if (existingUserAuthenticaiton != null)
            {
                switch (exportDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore: //Just ignore the record
                        //Result.AddLogEntry("Ignored user authentication", username);
                        return;
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        //Result.AddLogEntry("Ignored duplicate user authentication", username);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                }
            }
            var modifiedById = Util.GetUserIdOrName(importJob, userAuthentication.LastModifiedByUserId,
                userAuthentication.LastModifiedByUserName);
            userAuthentication.LastModifiedOnDate = DateTime.UtcNow;
            userAuthentication.LastModifiedByUserId = modifiedById;
            userAuthentication.UserId = userId;
            if (isUpdate)
            {
                userAuthentication.UserAuthenticationId = existingUserAuthenticaiton.UserAuthenticationID;
                repUserAuthentication.Update(userAuthentication);
                //Result.AddLogEntry("Updated user authentication", username);
            }
            else
            {
                userAuthentication.UserAuthenticationId = 0;
                var createdById = Util.GetUserIdOrName(importJob, userAuthentication.CreatedByUserId,
                 userAuthentication.CreatedByUserName);
                userAuthentication.CreatedOnDate = DateTime.UtcNow;
                userAuthentication.CreatedByUserId = createdById;
                repUserAuthentication.Insert(userAuthentication);
                //Result.AddLogEntry("Added user authentication", username);
            }
            userAuthentication.LocalId = userAuthentication.UserAuthenticationId;
        }

        private int GetCurrentSkip()
        {
            if (!string.IsNullOrEmpty(CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(CheckPoint.StageData);
                return Convert.ToInt32(stageData.skip) ?? 0;
            }
            return 0;
        }
    }
}