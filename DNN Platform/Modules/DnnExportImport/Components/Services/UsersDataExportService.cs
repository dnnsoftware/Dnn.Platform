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
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Dto.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Authentication;
using Newtonsoft.Json;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Supplementary service to import users additional data.
    /// </summary>
    public class UsersDataExportService : BasePortableService
    {
        public override string Category => Constants.Category_UsersData;

        public override string ParentCategory => Constants.Category_Users;

        public override uint Priority => 10;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            CheckPoint.Progress += 100;
            CheckPoint.TotalItems = 0;
            CheckPoint.ProcessedItems = 0;
            CheckPointStageCallback(this);
            //No implementation required in export users child as everything is exported in parent service.
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob)) return;

            var pageIndex = 0;
            const int pageSize = Constants.DefaultPageSize;
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

            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalUsers : CheckPoint.TotalItems;
            if (CheckPointStageCallback(this)) return;

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

                        //Find the correct userId from the system which was added/updated by UserExportService.
                        var userId = UserController.GetUserByName(user.Username)?.UserID;
                        if (userId != null)
                        {
                            ProcessUserRoles(importJob, importDto, userRoles, userId.Value);
                            totalUserRolesImported += userRoles.Count;

                            ProcessUserProfiles(importJob, importDto, userProfiles, userId.Value);
                            totalProfilesImported += userProfiles.Count;

                            ProcessUserAuthentications(importJob, importDto, userAuthentication, userId.Value);
                            if (userAuthentication != null) totalAuthenticationImported++;
                        }

                        currentIndex++;
                        CheckPoint.ProcessedItems++;
                        totalProcessed++;
                        CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalUsers;
                        CheckPoint.StageData = null;
                        //After every 100 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 100 == 0 && CheckPointStageCallback(this)) return;
                    }
                    currentIndex = 0;//Reset current index to 0
                    pageIndex++;
                    CheckPoint.Stage++;
                    CheckPoint.StageData = null;
                    if (CheckPointStageCallback(this)) return;
                }
                CheckPoint.Progress = 100;
            }
            finally
            {
                CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                CheckPointStageCallback(this);
                Result.AddSummary("Imported User Roles", totalUserRolesImported.ToString());
                Result.AddSummary("Imported User Profiles", totalProfilesImported.ToString());
                Result.AddSummary("Imported User Authentication", totalAuthenticationImported.ToString());
            }
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportUser>();
        }

        private void ProcessUserRoles(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportUserRole> userRoles, int userId)
        {
            foreach (var userRole in userRoles)
            {
                if (CheckCancelled(importJob)) return;
                var roleId = Util.GetRoleIdByName(importJob.PortalId, userRole.RoleName);
                if (roleId == null) continue;

                var existingUserRole = RoleController.Instance.GetUserRole(importJob.PortalId, userId, roleId.Value);
                var isUpdate = false;
                if (existingUserRole != null)
                {
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore:
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }

                if (isUpdate)
                {
                    var modifiedBy = Util.GetUserIdByName(importJob, userRole.LastModifiedByUserId, userRole.LastModifiedByUserName);
                    userRole.UserRoleId = existingUserRole.UserRoleID;
                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdateUserRole(userRole.UserRoleId, userRole.Status, userRole.IsOwner,
                            userRole.EffectiveDate ?? Null.NullDate, userRole.ExpiryDate ?? Null.NullDate, modifiedBy);
                }
                else
                {
                    var createdBy = Util.GetUserIdByName(importJob, userRole.CreatedByUserId, userRole.CreatedByUserName);
                    userRole.UserRoleId = DotNetNuke.Data.DataProvider.Instance()
                        .AddUserRole(importJob.PortalId, userId, roleId.Value, userRole.Status, userRole.IsOwner,
                            userRole.EffectiveDate ?? Null.NullDate, userRole.ExpiryDate ?? Null.NullDate, createdBy);
                }
            }
        }

        private void ProcessUserProfiles(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportUserProfile> userProfiles, int userId)
        {
            var allUserProfileProperties =
                CBO.FillCollection<ExportUserProfile>(DataProvider.Instance().GetUserProfile(importJob.PortalId, userId));
            foreach (var userProfile in userProfiles)
            {
                if (CheckCancelled(importJob)) return;
                var existingUserProfile =
                    allUserProfileProperties.FirstOrDefault(x => x.PropertyName == userProfile.PropertyName);
                var isUpdate = false;
                if (existingUserProfile != null)
                {
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore:
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                if (isUpdate)
                {
                    userProfile.PropertyDefinitionId = existingUserProfile.PropertyDefinitionId;
                    userProfile.ProfileId = existingUserProfile.ProfileId;
                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdateProfileProperty(userProfile.ProfileId, userId, userProfile.PropertyDefinitionId,
                            userProfile.PropertyValue
                            , userProfile.Visibility, userProfile.ExtendedVisibility, DateUtils.GetDatabaseLocalTime());
                }
                else
                {
                    var profileDefinitionId = Util.GetProfilePropertyId(importJob.PortalId,
                        userProfile.PropertyDefinitionId,
                        userProfile.PropertyName);
                    if (profileDefinitionId == null) continue;
                    userProfile.PropertyDefinitionId = profileDefinitionId.Value;

                    DotNetNuke.Data.DataProvider.Instance()
                        .UpdateProfileProperty(Null.NullInteger, userId, userProfile.PropertyDefinitionId,
                            userProfile.PropertyValue, userProfile.Visibility, userProfile.ExtendedVisibility,
                            DateUtils.GetDatabaseLocalTime());
                }
            }
        }

        private void ProcessUserAuthentications(ExportImportJob importJob, ImportDto importDto,
            ExportUserAuthentication userAuthentication, int userId)
        {
            if (userAuthentication == null) return;

            var existingUserAuthenticaiton = AuthenticationController.GetUserAuthentication(userId);
            var isUpdate = false;
            if (existingUserAuthenticaiton != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }
            if (isUpdate)
            {
                //TODO: Do we need this part?
                userAuthentication.UserAuthenticationId = existingUserAuthenticaiton.UserAuthenticationID;
                //No updates.
            }
            else
            {
                userAuthentication.UserAuthenticationId = 0;
                var createdById = Util.GetUserIdByName(importJob, userAuthentication.CreatedByUserId, userAuthentication.CreatedByUserName);
                userAuthentication.UserAuthenticationId = DotNetNuke.Data.DataProvider.Instance().AddUserAuthentication(userAuthentication.UserId, userAuthentication.AuthenticationType,
                        userAuthentication.AuthenticationToken, createdById);
            }
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