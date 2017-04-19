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
using System.Diagnostics;
using System.Linq;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using DotNetNuke.Common.Utilities;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Dto.Users;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Users;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Service to export/import users.
    /// </summary>
    public class UsersExportService : BasePortableService
    {
        public override string Category => Constants.Category_Users;

        public override string ParentCategory => null;

        public override uint Priority => 0;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var fromDate = exportDto.FromDate?.DateTime;
            var toDate = exportDto.ToDate;
            if (CheckCancelled(exportJob)) return;

            var portalId = exportJob.PortalId;
            var pageIndex = 0;
            const int pageSize = 500;
            var totalUsersExported = 0;
            var totalUserRolesExported = 0;
            var totalPortalsExported = 0;
            var totalProfilesExported = 0;
            var totalAuthenticationExported = 0;
            var totalAspnetUserExported = 0;
            var totalAspnetMembershipExported = 0;
            var totalUsers = DataProvider.Instance().GetUsersCount(portalId, exportDto.IncludeDeletions, toDate, fromDate);
            if (totalUsers == 0) return;
            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);

            //Skip the export if all the users has been processed already.
            if (CheckPoint.Stage >= totalPages)
                return;

            //Check if there is any pending stage or partially processed data.
            if (CheckPoint.Stage > 0)
            {
                pageIndex = CheckPoint.Stage;
            }
            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalUsers : CheckPoint.TotalItems;
            CheckPoint.ProcessedItems = CheckPoint.Stage * pageSize;
            if (CheckPointStageCallback(this)) return;
            var includeProfile = exportDto.IncludeProperfileProperties;
            try
            {
                while (pageIndex < totalPages)
                {
                    if (CheckCancelled(exportJob)) return;
                    var exportUsersList = new List<ExportUser>();
                    var exportAspnetUserList = new List<ExportAspnetUser>();
                    var exportAspnetMembershipList = new List<ExportAspnetMembership>();
                    var exportUserRoleList = new List<ExportUserRole>();
                    var exportUserPortalList = new List<ExportUserPortal>();
                    var exportUserAuthenticationList = new List<ExportUserAuthentication>();
                    var exportUserProfileList = new List<ExportUserProfile>();
                    try
                    {
                        using (var reader = DataProvider.Instance()
                            .GetAllUsers(portalId, pageIndex, pageSize, exportDto.IncludeDeletions, toDate, fromDate,
                                Util.ConvertToDbUtcTime(toDate) ?? Constants.MaxDbTime,
                                Util.ConvertToDbUtcTime(fromDate)))
                        {
                            CBO.FillCollection(reader, exportUsersList, false);
                            reader.NextResult();

                            CBO.FillCollection(reader, exportUserAuthenticationList, false);
                            reader.NextResult();

                            CBO.FillCollection(reader, exportUserRoleList, false);
                            reader.NextResult();

                            if (includeProfile)
                            {
                                CBO.FillCollection(reader, exportUserProfileList, false);
                            }
                            reader.NextResult();

                            CBO.FillCollection(reader, exportUserPortalList, false);
                            reader.NextResult();

                            CBO.FillCollection(reader, exportAspnetUserList, false);
                            reader.NextResult();

                            CBO.FillCollection(reader, exportAspnetMembershipList, true);
                        }

                        Repository.CreateItems(exportUsersList, null);
                        totalUsersExported += exportUsersList.Count;

                        exportUserAuthenticationList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        Repository.CreateItems(exportUserAuthenticationList, null);
                        totalAuthenticationExported += exportUserAuthenticationList.Count;

                        exportUserRoleList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        Repository.CreateItems(exportUserRoleList, null);
                        totalUserRolesExported += exportUserRoleList.Count;
                        if (includeProfile)
                        {
                            exportUserProfileList.ForEach(
                                x =>
                                {
                                    x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                                });
                            Repository.CreateItems(exportUserProfileList, null);
                            totalProfilesExported += exportUserProfileList.Count;
                        }
                        exportUserPortalList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        Repository.CreateItems(exportUserPortalList, null);
                        totalPortalsExported += exportUserPortalList.Count;

                        exportAspnetUserList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.Username == x.UserName)?.Id;
                            });
                        Repository.CreateItems(exportAspnetUserList, null);
                        totalAspnetUserExported += exportAspnetUserList.Count;

                        exportAspnetMembershipList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportAspnetUserList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        Repository.CreateItems(exportAspnetMembershipList, null);
                        totalAspnetMembershipExported += exportAspnetMembershipList.Count;

                        CheckPoint.ProcessedItems += exportUsersList.Count;
                    }
                    catch (Exception ex)
                    {
                        Result.AddLogEntry($"Exporting Users from {pageIndex * pageSize} to {pageIndex * pageSize + pageSize} exception", ex.Message, ReportLevel.Error);
                    }
                    CheckPoint.Progress = CheckPoint.ProcessedItems * 100.0 / totalUsers;
                    CheckPoint.Stage++;
                    if (CheckPointStageCallback(this)) return;

                    pageIndex++;
                }
                CheckPoint.Completed = true;
                CheckPoint.Progress = 100;
            }
            finally
            {
                CheckPointStageCallback(this);
                Result.AddSummary("Exported Users", totalUsersExported.ToString());
                Result.AddSummary("Exported User Portals", totalPortalsExported.ToString());
                Result.AddSummary("Exported User Roles", totalUserRolesExported.ToString());
                if (includeProfile)
                {
                    Result.AddSummary("Exported User Profiles", totalProfilesExported.ToString());
                }
                Result.AddSummary("Exported User Authentication", totalAuthenticationExported.ToString());
                Result.AddSummary("Exported Aspnet User", totalAspnetUserExported.ToString());
                Result.AddSummary("Exported Aspnet Membership", totalAspnetMembershipExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob)) return;

            const int pageSize = 500;// Constants.DefaultPageSize;
            var totalUsersImported = 0;
            var totalPortalsImported = 0;
            var totalAspnetUserImported = 0;
            var totalAspnetMembershipImported = 0;
            var totalUserAuthenticationCount = 0;
            var totalUsers = Repository.GetCount<ExportUser>();
            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);

            //Skip the import if all the users has been processed already.
            if (CheckPoint.Stage >= totalPages)
                return;

            var pageIndex = CheckPoint.Stage;

            var totalUsersToBeProcessed = totalUsers - pageIndex * pageSize;
            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? totalUsers : CheckPoint.TotalItems;
            CheckPoint.ProcessedItems = CheckPoint.Stage * pageSize;
            if (CheckPointStageCallback(this)) return;
            try
            {
                Repository.RebuildIndex<ExportUser>(x => x.Id, true);
                Repository.RebuildIndex<ExportUserPortal>(x => x.ReferenceId);
                Repository.RebuildIndex<ExportAspnetUser>(x => x.ReferenceId);
                Repository.RebuildIndex<ExportAspnetMembership>(x => x.ReferenceId);
                Repository.RebuildIndex<ExportUserAuthentication>(x => x.ReferenceId);
                var portalId = importJob.PortalId;
                var dataProvider = DotNetNuke.Data.DataProvider.Instance();
                using (var table = new DataTable("Users"))
                {
                    // must create the columns from scratch with each iteration
                    table.Columns.AddRange(
                        UsersDatasetColumns.Select(column => new DataColumn(column.Item1, column.Item2)).ToArray());
                    while (totalUsersImported < totalUsersToBeProcessed)
                    {
                        if (CheckCancelled(importJob)) return;
                        var users =
                            Repository.GetAllItems<ExportUser>(null, true, pageIndex * pageSize, pageSize).ToList();
                        var tempAspUserCount = 0;
                        var tempAspMembershipCount = 0;
                        var tempUserPortalCount = 0;
                        var tempUserAuthenticationCount = 0;
                        try
                        {
                            foreach (var user in users)
                            {
                                if (CheckCancelled(importJob)) return;
                                var row = table.NewRow();
                                var userPortal = Repository.GetRelatedItems<ExportUserPortal>(user.Id).FirstOrDefault();
                                tempUserPortalCount += userPortal != null ? 1 : 0;
                                var userAuthentication = Repository.GetRelatedItems<ExportUserAuthentication>(user.Id).FirstOrDefault();
                                tempUserAuthenticationCount += userAuthentication != null ? 1 : 0;

                                row["PortalId"] = portalId;
                                row["Username"] = user.Username;
                                row["FirstName"] = string.IsNullOrEmpty(user.FirstName) ? string.Empty : user.FirstName;
                                row["LastName"] = string.IsNullOrEmpty(user.LastName) ? string.Empty : user.LastName;
                                row["AffiliateId"] = dataProvider.GetNull(user.AffiliateId);
                                row["IsSuperUser"] = user.IsSuperUser;
                                row["Email"] = user.Email;
                                row["DisplayName"] = string.IsNullOrEmpty(user.DisplayName) ? string.Empty : user.DisplayName;
                                row["UpdatePassword"] = user.UpdatePassword;
                                row["Authorised"] = dataProvider.GetNull(userPortal?.Authorised);
                                row["CreatedByUserID"] = dataProvider.GetNull(Util.GetUserIdByName(importJob, user.CreatedByUserId, user.CreatedByUserName));
                                row["VanityUrl"] = dataProvider.GetNull(userPortal?.VanityUrl);
                                row["RefreshRoles"] = dataProvider.GetNull(userPortal?.RefreshRoles);
                                row["LastIPAddress"] = dataProvider.GetNull(user.LastIpAddress);
                                row["PasswordResetToken"] = dataProvider.GetNull(user.PasswordResetToken);
                                row["PasswordResetExpiration"] = dataProvider.GetNull(user.PasswordResetExpiration);
                                row["IsDeleted"] = dataProvider.GetNull(userPortal?.IsDeleted);
                                row["LastModifiedByUserID"] = dataProvider.GetNull(Util.GetUserIdByName(importJob, user.LastModifiedByUserId, user.LastModifiedByUserName));
                                row["AuthenticationType"] = dataProvider.GetNull(userAuthentication?.AuthenticationType);
                                row["AuthenticationToken"] = dataProvider.GetNull(userAuthentication?.AuthenticationToken);

                                //Aspnet Users and Membership
                                ExportAspnetMembership aspnetMembership = null;
                                var aspNetUser = Repository.GetRelatedItems<ExportAspnetUser>(user.Id).FirstOrDefault();
                                tempAspUserCount += aspNetUser != null ? 1 : 0;
                                if (aspNetUser != null)
                                {
                                    aspnetMembership =
                                        Repository.GetRelatedItems<ExportAspnetMembership>(aspNetUser.Id)
                                            .FirstOrDefault();
                                    tempAspMembershipCount += aspnetMembership != null ? 1 : 0;
                                }

                                row["ApplicationId"] = dataProvider.GetNull(GetApplicationId());
                                row["AspUserId"] = dataProvider.GetNull(aspNetUser?.UserId);
                                row["MobileAlias"] = dataProvider.GetNull(aspNetUser?.MobileAlias);
                                row["IsAnonymous"] = dataProvider.GetNull(aspNetUser?.IsAnonymous);
                                row["Password"] = string.IsNullOrEmpty(aspnetMembership?.Password) ? string.Empty : aspnetMembership.Password;
                                row["PasswordFormat"] = dataProvider.GetNull(aspnetMembership?.PasswordFormat);
                                row["PasswordSalt"] = dataProvider.GetNull(aspnetMembership?.PasswordSalt);
                                row["MobilePIN"] = dataProvider.GetNull(aspnetMembership?.MobilePin);
                                row["PasswordQuestion"] = dataProvider.GetNull(aspnetMembership?.PasswordQuestion);
                                row["PasswordAnswer"] = dataProvider.GetNull(aspnetMembership?.PasswordAnswer);
                                row["IsApproved"] = dataProvider.GetNull(aspnetMembership?.IsApproved);
                                row["IsLockedOut"] = dataProvider.GetNull(aspnetMembership?.IsLockedOut);
                                row["FailedPasswordAttemptCount"] = dataProvider.GetNull(aspnetMembership?.FailedPasswordAttemptCount);
                                row["FailedPasswordAnswerAttemptCount"] = dataProvider.GetNull(aspnetMembership?.FailedPasswordAnswerAttemptCount);
                                row["Comment"] = dataProvider.GetNull(aspnetMembership?.Comment);
                                table.Rows.Add(row);
                            }
                            var Overwrite = importDto.CollisionResolution == CollisionResolution.Overwrite;
                            //Bulk insert the data in DB
                            DotNetNuke.Data.DataProvider.Instance()
                                .BulkInsert("ExportImport_AddUpdateUsersBulk", "@DataTable", table);
                            totalUsersImported += users.Count;
                            totalAspnetUserImported += tempAspUserCount;
                            totalAspnetMembershipImported += tempAspMembershipCount;
                            totalPortalsImported += tempUserPortalCount;
                            totalUserAuthenticationCount += tempUserAuthenticationCount;
                            CheckPoint.ProcessedItems += users.Count;
                        }
                        catch (Exception ex)
                        {
                            Result.AddLogEntry($"Importing Users from {pageIndex * pageSize} to {pageIndex * pageSize + pageSize} exception", ex.Message, ReportLevel.Error);
                        }
                        table.Rows.Clear();
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
                Result.AddSummary("Imported Users", totalUsersImported.ToString());
                Result.AddSummary("Imported User Portals", totalPortalsImported.ToString());
                Result.AddSummary("Import User Authentications", totalUserAuthenticationCount.ToString());
                Result.AddSummary("Imported Aspnet Users", totalAspnetUserImported.ToString());
                Result.AddSummary("Imported Aspnet Memberships", totalAspnetMembershipImported.ToString());
            }
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportUser>();
        }

        private void ProcessUser(ExportImportJob importJob, ImportDto importDto, ExportUser user,
            ExportUserPortal userPortal, ImportAspnetUser aspnetUser, ImportAspnetMembership aspnetMembership)
        {
            if (user == null) return;
            var existingUser = CBO.FillObject<ExportUser>(DotNetNuke.Data.DataProvider.Instance().GetUserByUsername(importJob.PortalId, user.Username));
            var isUpdate = false;
            if (existingUser != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        Result.AddLogEntry("Ignored user", user.Username);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }
            user.FirstName = string.IsNullOrEmpty(user.FirstName) ? string.Empty : user.FirstName;
            user.LastName = string.IsNullOrEmpty(user.LastName) ? string.Empty : user.LastName;

            if (isUpdate)
            {
                var vanityUrl = userPortal != null ? userPortal.VanityUrl : UserController.Instance.GetUser(importJob.PortalId, existingUser.UserId)?.VanityUrl;
                if (string.IsNullOrEmpty(vanityUrl))
                {

                }
                var modifiedBy = Util.GetUserIdByName(importJob, user.LastModifiedByUserId, user.LastModifiedByUserName);
                user.UserId = existingUser.UserId;
                DotNetNuke.Data.DataProvider.Instance().UpdateUser(user.UserId, importJob.PortalId, user.FirstName, user.LastName, user.IsSuperUser,
                        user.Email, user.DisplayName, vanityUrl, user.UpdatePassword, aspnetMembership?.IsApproved ?? true,
                        false, user.LastIpAddress, user.PasswordResetToken ?? Guid.NewGuid(), user.PasswordResetExpiration ?? DateTime.Now.AddYears(10), user.IsDeletedPortal, modifiedBy);
            }
            else
            {
                var createdBy = Util.GetUserIdByName(importJob, user.CreatedByUserId, user.CreatedByUserName);
                user.UserId = DotNetNuke.Data.DataProvider.Instance().AddUser(importJob.PortalId, user.Username, user.FirstName, user.LastName, user.AffiliateId ?? Null.NullInteger,
                        user.IsSuperUser, user.Email, user.DisplayName, user.UpdatePassword, aspnetMembership?.IsApproved ?? true, createdBy);

                if (user.IsDeletedPortal)
                    EntitiesController.Instance.SetUserDeleted(importJob.PortalId, user.UserId, true);
            }
            ProcessUserMembership(aspnetUser, aspnetMembership);

        }

        private Guid GetApplicationId()
        {
            using (var db =
                new PetaPocoDataContext(DotNetNuke.Data.DataProvider.Instance().Settings["connectionStringName"],
                    "aspnet_"))
            {
                return db.ExecuteScalar<Guid>(CommandType.Text,
                    "SELECT TOP 1 ApplicationId FROM aspnet_Applications");
            }
        }

        private void ProcessUserMembership(ImportAspnetUser aspNetUser, ImportAspnetMembership aspnetMembership)
        {
            if (aspNetUser == null) return;
            using (var db =
                new PetaPocoDataContext(DotNetNuke.Data.DataProvider.Instance().Settings["connectionStringName"],
                    "aspnet_"))
            {
                var repAspnetUsers = db.GetRepository<ImportAspnetUser>();
                var repAspnetMembership = db.GetRepository<ImportAspnetMembership>();
                var step = 0;

                var existingAspnetUser =
                    CBO.FillObject<ExportAspnetUser>(DataProvider.Instance()
                        .GetAspNetUser(aspNetUser.UserName));
                if (existingAspnetUser != null)
                {
                    aspNetUser.LastActivityDate = existingAspnetUser.LastActivityDate;
                    aspNetUser.UserId = existingAspnetUser.UserId;
                    aspNetUser.ApplicationId = existingAspnetUser.ApplicationId;
                    repAspnetUsers.Update(aspNetUser);
                    step++;

                    var existingAspnetMembership = CBO.FillObject<ExportAspnetMembership>(DataProvider.Instance().GetUserMembership(existingAspnetUser.UserId));
                    if (existingAspnetMembership != null && aspnetMembership != null)
                    {
                        aspnetMembership.UserId = existingAspnetMembership.UserId;
                        aspnetMembership.ApplicationId = existingAspnetMembership.ApplicationId;
                        aspnetMembership.CreateDate = existingAspnetMembership.CreateDate;
                        repAspnetMembership.Update(aspnetMembership);
                        return;
                    }
                }

                var applicationId = db.ExecuteScalar<Guid>(CommandType.Text,
                    "SELECT TOP 1 ApplicationId FROM aspnet_Applications");

                if (step < 1)
                {
                    //AspnetUser
                    aspNetUser.UserId = Guid.Empty;
                    aspNetUser.ApplicationId = applicationId;
                    aspNetUser.LastActivityDate = DateUtils.GetDatabaseUtcTime();
                    repAspnetUsers.Insert(aspNetUser);
                }
                if (step < 2 && aspnetMembership != null)
                {
                    //AspnetMembership
                    aspnetMembership.UserId = aspNetUser.UserId;
                    aspnetMembership.ApplicationId = applicationId;
                    aspnetMembership.CreateDate = DateUtils.GetDatabaseUtcTime();
                    aspnetMembership.LastLoginDate =
                        aspnetMembership.LastPasswordChangedDate =
                            aspnetMembership.LastLockoutDate =
                                aspnetMembership.FailedPasswordAnswerAttemptWindowStart =
                                    aspnetMembership.FailedPasswordAttemptWindowStart =
                                        new DateTime(1754, 1, 1);
                    repAspnetMembership.Insert(aspnetMembership);
                }
            }
        }

        private static readonly Tuple<string, Type>[] UsersDatasetColumns =
        {
            new Tuple<string, Type>("PortalId", typeof (int)),
            new Tuple<string, Type>("Username", typeof (string)),
            new Tuple<string, Type>("FirstName", typeof (string)),
            new Tuple<string, Type>("LastName", typeof (string)),
            new Tuple<string, Type>("AffiliateId", typeof (int)),
            new Tuple<string, Type>("IsSuperUser", typeof (bool)),
            new Tuple<string, Type>("Email", typeof (string)),
            new Tuple<string, Type>("DisplayName", typeof (string)),
            new Tuple<string, Type>("UpdatePassword", typeof (bool)),
            new Tuple<string, Type>("Authorised", typeof (bool)),
            new Tuple<string, Type>("CreatedByUserID", typeof (int)),
            new Tuple<string, Type>("VanityUrl", typeof (string)),
            new Tuple<string, Type>("RefreshRoles", typeof (bool)),
            new Tuple<string, Type>("LastIPAddress", typeof (string)),
            new Tuple<string, Type>("PasswordResetToken", typeof (Guid)),
            new Tuple<string, Type>("PasswordResetExpiration", typeof (DateTime)),
            new Tuple<string, Type>("IsDeleted", typeof (bool)),
            new Tuple<string, Type>("LastModifiedByUserID", typeof (int)),
            new Tuple<string, Type>("ApplicationId", typeof (Guid)),
            new Tuple<string, Type>("AspUserId", typeof (Guid)),
            new Tuple<string, Type>("MobileAlias", typeof (string)),
            new Tuple<string, Type>("IsAnonymous", typeof (bool)),
            new Tuple<string, Type>("Password", typeof (string)),
            new Tuple<string, Type>("PasswordFormat", typeof (int)),
            new Tuple<string, Type>("PasswordSalt", typeof (string)),
            new Tuple<string, Type>("MobilePIN", typeof (string)),
            new Tuple<string, Type>("PasswordQuestion", typeof (string)),
            new Tuple<string, Type>("PasswordAnswer", typeof (string)),
            new Tuple<string, Type>("IsApproved", typeof (bool)),
            new Tuple<string, Type>("IsLockedOut", typeof (bool)),
            new Tuple<string, Type>("FailedPasswordAttemptCount", typeof (int)),
            new Tuple<string, Type>("FailedPasswordAnswerAttemptCount", typeof (int)),
            new Tuple<string, Type>("Comment", typeof (string)),
            new Tuple<string, Type>("AuthenticationType", typeof (string)),
            new Tuple<string, Type>("AuthenticationToken", typeof (string))
        };
    }
}