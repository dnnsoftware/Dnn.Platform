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
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Dto.Users;
using DotNetNuke.Common;
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
            CheckPoint.Completed = true;
            CheckPoint.TotalItems = 0;
            CheckPoint.ProcessedItems = 0;
            CheckPointStageCallback(this);
            //No implementation required in export users child as everything is exported in parent service.
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob)) return;

            var pageIndex = 0;
            const int pageSize = 500;//Constants.DefaultPageSize;
            var totalUserRolesImported = 0;
            var totalProfilesImported = 0;
            var totalProcessed = 0;
            var totalUsers = Repository.GetCount<ExportUser>();
            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);
            //Skip the import if all the users has been processed already.
            if (CheckPoint.Stage >= totalPages)
                return;

            pageIndex = CheckPoint.Stage;

            var totalUsersToBeProcessed = totalUsers - pageIndex * pageSize;

            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalUsers : CheckPoint.TotalItems;
            CheckPoint.ProcessedItems = CheckPoint.Stage * pageSize;
            if (CheckPointStageCallback(this)) return;
            var includeProfile = importDto.ExportDto.IncludeProperfileProperties;
            try
            {
                Repository.RebuildIndex<ExportUserRole>(x => x.ReferenceId);
                if (includeProfile)
                    Repository.RebuildIndex<ExportUserProfile>(x => x.ReferenceId);
                var portalId = importJob.PortalId;
                using (var tableUserProfile = new DataTable("UserProfile"))
                using (var tableUserRoles = new DataTable("UserRoles"))
                {
                    // must create the columns from scratch with each iteration
                    tableUserProfile.Columns.AddRange(UserProfileDatasetColumns.Select(column => new DataColumn(column.Item1, column.Item2)).ToArray());
                    tableUserRoles.Columns.AddRange(UserRolesDatasetColumns.Select(column => new DataColumn(column.Item1, column.Item2)).ToArray());
                    var dataProvider = DotNetNuke.Data.DataProvider.Instance();
                    while (totalProcessed < totalUsersToBeProcessed)
                    {
                        if (CheckCancelled(importJob)) return;
                        var users = Repository.GetAllItems<ExportUser>(null, true, pageIndex * pageSize, pageSize).ToList();
                        var tempUserRolesCount = 0;
                        var tempUserProfileCount = 0;
                        try
                        {
                            foreach (var user in users)
                            {
                                if (CheckCancelled(importJob)) return;
                                //Find the correct userId from the system which was added/updated by UserExportService.
                                var userId = UserController.GetUserByName(user.Username)?.UserID;
                                if (userId != null)
                                {
                                    var userRoles = Repository.GetRelatedItems<ExportUserRole>(user.Id).ToList();
                                    foreach (var userRole in userRoles)
                                    {
                                        var roleId = Util.GetRoleIdByName(importJob.PortalId, userRole.RoleName);
                                        if (roleId == null) continue;
                                        if (!(roleId > Convert.ToInt32(Globals.glbRoleNothing))) continue;
                                        var userRoleRow = tableUserRoles.NewRow();
                                        userRoleRow["PortalId"] = portalId;
                                        userRoleRow["UserId"] = userId;
                                        userRoleRow["RoleId"] = roleId;
                                        userRoleRow["ExpiryDate"] = dataProvider.GetNull(userRole.ExpiryDate);
                                        userRoleRow["IsTrialUsed"] = userRole.IsTrialUsed;
                                        userRoleRow["EffectiveDate"] = dataProvider.GetNull(userRole.EffectiveDate);
                                        userRoleRow["CreatedByUserId"] = Util.GetUserIdByName(importJob, user.CreatedByUserId, user.CreatedByUserName);
                                        userRoleRow["LastModifiedByUserId"] = Util.GetUserIdByName(importJob, user.LastModifiedByUserId, user.LastModifiedByUserName);
                                        userRoleRow["Status"] = userRole.Status;
                                        userRoleRow["IsOwner"] = userRole.IsOwner;
                                        tableUserRoles.Rows.Add(userRoleRow);
                                        tempUserRolesCount++;
                                    }
                                    if (includeProfile)
                                    {
                                        var userProfiles =
                                            Repository.GetRelatedItems<ExportUserProfile>(user.Id).ToList();
                                        foreach (var userProfile in userProfiles)
                                        {
                                            var profileDefinitionId = Util.GetProfilePropertyId(importJob.PortalId,
                                                userProfile.PropertyDefinitionId, userProfile.PropertyName);
                                            if (profileDefinitionId == null) continue;
                                            var userProfileRow = tableUserProfile.NewRow();
                                            userProfileRow["PortalId"] = importJob.PortalId;
                                            userProfileRow["UserId"] = userId;
                                            userProfileRow["PropertyDefinitionId"] = profileDefinitionId;
                                            userProfileRow["PropertyValue"] = userProfile.PropertyValue;
                                            userProfileRow["PropertyText"] = userProfile.PropertyText;
                                            userProfileRow["Visibility"] = userProfile.Visibility;
                                            userProfileRow["ExtendedVisibility"] = userProfile.ExtendedVisibility;
                                            tableUserProfile.Rows.Add(userProfileRow);
                                            tempUserProfileCount++;
                                        }
                                    }
                                }
                            }
                            var Overwrite = importDto.CollisionResolution == CollisionResolution.Overwrite;
                            //Bulk insert the data in DB
                            DotNetNuke.Data.DataProvider.Instance()
                                .BulkInsert("ExportImport_AddUpdateUserRolesBulk", "@DataTable", tableUserRoles);
                            totalUserRolesImported += tempUserRolesCount;

                            if (includeProfile)
                            {
                                DotNetNuke.Data.DataProvider.Instance()
                                    .BulkInsert("ExportImport_AddUpdateUsersProfilesBulk", "@DataTable",
                                        tableUserProfile, Overwrite);
                                totalProfilesImported += tempUserProfileCount;
                            }

                            CheckPoint.ProcessedItems += users.Count;
                            totalProcessed += users.Count;
                            CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalUsers;
                            CheckPoint.StageData = null;
                        }
                        catch (Exception ex)
                        {
                            Result.AddLogEntry($"Importing Users Data from {pageIndex * pageSize} to {pageIndex * pageSize + pageSize} exception", ex.Message, ReportLevel.Error);
                        }
                        tableUserRoles.Rows.Clear();
                        tableUserProfile.Rows.Clear();
                        pageIndex++;
                        CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalUsers;
                        CheckPoint.Stage++;
                        CheckPoint.StageData = null;
                        if (CheckPointStageCallback(this)) return;
                    }
                }
                CheckPoint.Completed = true;
                CheckPoint.Progress = 100;
            }
            finally
            {
                CheckPointStageCallback(this);
                Result.AddSummary("Imported User Roles", totalUserRolesImported.ToString());
                if (includeProfile)
                {
                    Result.AddSummary("Imported User Profiles", totalProfilesImported.ToString());
                }
            }
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportUser>();
        }

        private int ProcessUserRoles(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportUserRole> userRoles, int userId)
        {
            var total = 0;
            foreach (var userRole in userRoles)
            {
                if (CheckCancelled(importJob)) return total;
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
                total++;
            }
            return total;
        }

        private int ProcessUserProfiles(ExportImportJob importJob, ImportDto importDto,
            IEnumerable<ExportUserProfile> userProfiles, int userId)
        {
            var total = 0;
            var allUserProfileProperties = CBO.FillCollection<ExportUserProfile>(DataProvider.Instance().GetUserProfile(importJob.PortalId, userId));
            foreach (var userProfile in userProfiles)
            {
                if (CheckCancelled(importJob)) return total;
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
                total++;
            }
            return total;
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

        private static readonly Tuple<string, Type>[] UserRolesDatasetColumns =
        {
            new Tuple<string, Type>("PortalId", typeof (int)),
            new Tuple<string, Type>("UserId", typeof (int)),
            new Tuple<string, Type>("RoleId", typeof (int)),
            new Tuple<string, Type>("ExpiryDate", typeof (DateTime)),
            new Tuple<string, Type>("IsTrialUsed", typeof (bool)),
            new Tuple<string, Type>("EffectiveDate", typeof (DateTime)),
            new Tuple<string, Type>("CreatedByUserId", typeof (int)),
            new Tuple<string, Type>("LastModifiedByUserId", typeof (int)),
            new Tuple<string, Type>("Status", typeof (int)),
            new Tuple<string, Type>("IsOwner", typeof (bool))
        };

        private static readonly Tuple<string, Type>[] UserProfileDatasetColumns =
        {
            new Tuple<string, Type>("PortalId", typeof (int)),
            new Tuple<string, Type>("UserId", typeof (int)),
            new Tuple<string, Type>("PropertyDefinitionId", typeof (int)),
            new Tuple<string, Type>("PropertyValue", typeof (string)),
            new Tuple<string, Type>("PropertyText", typeof (string)),
            new Tuple<string, Type>("Visibility", typeof (int)),
            new Tuple<string, Type>("ExtendedVisibility", typeof (string))
        };
    }
}