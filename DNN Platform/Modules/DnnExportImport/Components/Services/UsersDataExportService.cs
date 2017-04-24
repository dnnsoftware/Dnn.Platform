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
using Dnn.ExportImport.Dto.Assets;
using Dnn.ExportImport.Dto.Users;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

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
            const int pageSize = Constants.DefaultPageSize;
            var totalUserRolesImported = 0;
            var totalProfilesImported = 0;
            var totalProcessed = 0;
            var totalUsers = Repository.GetCount<ExportUser>();
            if (totalUsers == 0)
            {
                CheckPoint.Completed = true;
                CheckPointStageCallback(this);
                return;
            }
            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);
            //Skip the import if all the users has been processed already.
            if (CheckPoint.Stage >= totalPages)
                return;

            pageIndex = CheckPoint.Stage;

            var totalUsersToBeProcessed = totalUsers - pageIndex * pageSize;

            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalUsers : CheckPoint.TotalItems;
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
                                        userRoleRow["IsSuperUser"] = user.IsSuperUser;
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
                                            if (profileDefinitionId == null || profileDefinitionId == -1) continue;
                                            var value = userProfile.PropertyValue;
                                            if (userProfile.PropertyName.Equals("photo", StringComparison.InvariantCultureIgnoreCase) && (value = GetUserPhotoId(portalId, value, user)) == null)
                                            {
                                                continue;
                                            }

                                            var userProfileRow = tableUserProfile.NewRow();
                                            userProfileRow["PortalId"] = importJob.PortalId;
                                            userProfileRow["UserId"] = userId;
                                            userProfileRow["PropertyDefinitionId"] = profileDefinitionId.Value;
                                            userProfileRow["PropertyValue"] = value;
                                            userProfileRow["PropertyText"] = userProfile.PropertyText;
                                            userProfileRow["Visibility"] = userProfile.Visibility;
                                            userProfileRow["ExtendedVisibility"] = userProfile.ExtendedVisibility;
                                            userProfileRow["IsSuperUser"] = user.IsSuperUser;
                                            tableUserProfile.Rows.Add(userProfileRow);
                                            tempUserProfileCount++;
                                        }
                                    }
                                }
                            }
                            var overwrite = importDto.CollisionResolution == CollisionResolution.Overwrite;
                            //Bulk insert the data in DB
                            DotNetNuke.Data.DataProvider.Instance()
                                .BulkInsert("ExportImport_AddUpdateUserRolesBulk", "@DataTable", tableUserRoles, new Dictionary<string, object> { { "Overwrite", overwrite } });
                            totalUserRolesImported += tempUserRolesCount;

                            if (includeProfile)
                            {
                                DotNetNuke.Data.DataProvider.Instance()
                                    .BulkInsert("ExportImport_AddUpdateUsersProfilesBulk", "@DataTable",
                                        tableUserProfile, new Dictionary<string, object> { { "Overwrite", overwrite } });
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

        private string GetUserPhotoId(int portalId, string importFileId, ExportUser user)
        {
            int profilePictureId;
            if (string.IsNullOrEmpty(importFileId) ||
                !int.TryParse(importFileId, out profilePictureId)) return null;
            var files =
                FolderManager.Instance.GetFiles(
                    FolderManager.Instance.GetUserFolder(
                        UserController.GetUserByName(portalId, user.Username)))
                    .ToList();
            if (!files.Any()) return null;
            var importUserFolder =
                Repository.GetItem<ExportFolder>(x => x.UserId == user.UserId);
            if (importUserFolder == null) return null;
            {
                var profilePicture =
                    Repository.GetRelatedItems<ExportFile>(importUserFolder.Id)
                        .FirstOrDefault(x => x.FileId == profilePictureId);
                if (profilePicture != null &&
                    files.Any(x => x.FileName == profilePicture.FileName))
                {
                    return Convert.ToString(
                        files.First(
                            x => x.FileName == profilePicture.FileName)
                            .FileId);
                }
            }
            return null;
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportUser>();
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
            new Tuple<string, Type>("IsOwner", typeof (bool)),
            new Tuple<string, Type>("IsSuperUser", typeof (bool)),
        };

        private static readonly Tuple<string, Type>[] UserProfileDatasetColumns =
        {
            new Tuple<string, Type>("PortalId", typeof (int)),
            new Tuple<string, Type>("UserId", typeof (int)),
            new Tuple<string, Type>("PropertyDefinitionId", typeof (int)),
            new Tuple<string, Type>("PropertyValue", typeof (string)),
            new Tuple<string, Type>("PropertyText", typeof (string)),
            new Tuple<string, Type>("Visibility", typeof (int)),
            new Tuple<string, Type>("ExtendedVisibility", typeof (string)),
            new Tuple<string, Type>("IsSuperUser", typeof (bool))
        };
    }
}