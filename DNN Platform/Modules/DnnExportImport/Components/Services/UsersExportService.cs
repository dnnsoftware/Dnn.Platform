// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Users;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data.PetaPoco;

    using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

    /// <summary>
    /// Service to export/import users.
    /// </summary>
    public class UsersExportService : BasePortableService
    {
        private static readonly Tuple<string, Type>[] UsersDatasetColumns =
        {
            new Tuple<string, Type>("PortalId", typeof(int)),
            new Tuple<string, Type>("Username", typeof(string)),
            new Tuple<string, Type>("FirstName", typeof(string)),
            new Tuple<string, Type>("LastName", typeof(string)),
            new Tuple<string, Type>("AffiliateId", typeof(int)),
            new Tuple<string, Type>("IsSuperUser", typeof(bool)),
            new Tuple<string, Type>("Email", typeof(string)),
            new Tuple<string, Type>("DisplayName", typeof(string)),
            new Tuple<string, Type>("UpdatePassword", typeof(bool)),
            new Tuple<string, Type>("Authorised", typeof(bool)),
            new Tuple<string, Type>("CreatedByUserID", typeof(int)),
            new Tuple<string, Type>("VanityUrl", typeof(string)),
            new Tuple<string, Type>("RefreshRoles", typeof(bool)),
            new Tuple<string, Type>("LastIPAddress", typeof(string)),
            new Tuple<string, Type>("PasswordResetToken", typeof(Guid)),
            new Tuple<string, Type>("PasswordResetExpiration", typeof(DateTime)),
            new Tuple<string, Type>("IsDeleted", typeof(bool)),
            new Tuple<string, Type>("LastModifiedByUserID", typeof(int)),
            new Tuple<string, Type>("ApplicationId", typeof(Guid)),
            new Tuple<string, Type>("AspUserId", typeof(Guid)),
            new Tuple<string, Type>("MobileAlias", typeof(string)),
            new Tuple<string, Type>("IsAnonymous", typeof(bool)),
            new Tuple<string, Type>("Password", typeof(string)),
            new Tuple<string, Type>("PasswordFormat", typeof(int)),
            new Tuple<string, Type>("PasswordSalt", typeof(string)),
            new Tuple<string, Type>("MobilePIN", typeof(string)),
            new Tuple<string, Type>("PasswordQuestion", typeof(string)),
            new Tuple<string, Type>("PasswordAnswer", typeof(string)),
            new Tuple<string, Type>("IsApproved", typeof(bool)),
            new Tuple<string, Type>("IsLockedOut", typeof(bool)),
            new Tuple<string, Type>("FailedPasswordAttemptCount", typeof(int)),
            new Tuple<string, Type>("FailedPasswordAnswerAttemptCount", typeof(int)),
            new Tuple<string, Type>("Comment", typeof(string)),
            new Tuple<string, Type>("AuthenticationType", typeof(string)),
            new Tuple<string, Type>("AuthenticationToken", typeof(string)),
        };

        public override string Category => Constants.Category_Users;

        public override string ParentCategory => null;

        public override uint Priority => 0;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            var fromDateUtc = exportDto.FromDateUtc;
            var toDateUtc = exportDto.ToDateUtc;

            var portalId = exportJob.PortalId;
            var pageIndex = 0;
            const int pageSize = Constants.DefaultPageSize;
            var totalUsersExported = 0;
            var totalUserRolesExported = 0;
            var totalPortalsExported = 0;
            var totalProfilesExported = 0;
            var totalAuthenticationExported = 0;
            var totalAspnetUserExported = 0;
            var totalAspnetMembershipExported = 0;
            var totalUsers = DataProvider.Instance().GetUsersCount(portalId, exportDto.IncludeDeletions, toDateUtc, fromDateUtc);
            if (totalUsers == 0)
            {
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
                return;
            }

            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);

            // Skip the export if all the users has been processed already.
            if (this.CheckPoint.Stage >= totalPages)
            {
                return;
            }

            // Check if there is any pending stage or partially processed data.
            if (this.CheckPoint.Stage > 0)
            {
                pageIndex = this.CheckPoint.Stage;
            }

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalUsers : this.CheckPoint.TotalItems;
            this.CheckPoint.ProcessedItems = this.CheckPoint.Stage * pageSize;
            if (this.CheckPointStageCallback(this))
            {
                return;
            }

            var includeProfile = exportDto.IncludeProperfileProperties;
            try
            {
                while (pageIndex < totalPages)
                {
                    if (this.CheckCancelled(exportJob))
                    {
                        return;
                    }

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
                            .GetAllUsers(portalId, pageIndex, pageSize, exportDto.IncludeDeletions, toDateUtc, fromDateUtc))
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

                        this.Repository.CreateItems(exportUsersList, null);
                        totalUsersExported += exportUsersList.Count;

                        exportUserAuthenticationList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        this.Repository.CreateItems(exportUserAuthenticationList, null);
                        totalAuthenticationExported += exportUserAuthenticationList.Count;

                        exportUserRoleList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        this.Repository.CreateItems(exportUserRoleList, null);
                        totalUserRolesExported += exportUserRoleList.Count;
                        if (includeProfile)
                        {
                            exportUserProfileList.ForEach(
                                x =>
                                {
                                    x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                                });
                            this.Repository.CreateItems(exportUserProfileList, null);
                            totalProfilesExported += exportUserProfileList.Count;
                        }

                        exportUserPortalList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        this.Repository.CreateItems(exportUserPortalList, null);
                        totalPortalsExported += exportUserPortalList.Count;

                        exportAspnetUserList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportUsersList.FirstOrDefault(user => user.Username == x.UserName)?.Id;
                            });
                        this.Repository.CreateItems(exportAspnetUserList, null);
                        totalAspnetUserExported += exportAspnetUserList.Count;

                        exportAspnetMembershipList.ForEach(
                            x =>
                            {
                                x.ReferenceId = exportAspnetUserList.FirstOrDefault(user => user.UserId == x.UserId)?.Id;
                            });
                        this.Repository.CreateItems(exportAspnetMembershipList, null);
                        totalAspnetMembershipExported += exportAspnetMembershipList.Count;

                        this.CheckPoint.ProcessedItems += exportUsersList.Count;
                    }
                    catch (Exception ex)
                    {
                        this.Result.AddLogEntry($"Exporting Users from {pageIndex * pageSize} to {(pageIndex * pageSize) + pageSize} exception", ex.Message, ReportLevel.Error);
                    }

                    this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalUsers;
                    this.CheckPoint.Stage++;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    // Rebuild the indexes in the exported database.
                    this.Repository.RebuildIndex<ExportUser>(x => x.Id, true);
                    this.Repository.RebuildIndex<ExportUserPortal>(x => x.ReferenceId);
                    this.Repository.RebuildIndex<ExportAspnetUser>(x => x.ReferenceId);
                    this.Repository.RebuildIndex<ExportAspnetMembership>(x => x.ReferenceId);
                    this.Repository.RebuildIndex<ExportUserAuthentication>(x => x.ReferenceId);
                    this.Repository.RebuildIndex<ExportUserRole>(x => x.ReferenceId);
                    if (includeProfile)
                    {
                        this.Repository.RebuildIndex<ExportUserProfile>(x => x.ReferenceId);
                    }

                    pageIndex++;
                }

                this.CheckPoint.Completed = true;
                this.CheckPoint.Progress = 100;
            }
            finally
            {
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Users", totalUsersExported.ToString());
                this.Result.AddSummary("Exported User Portals", totalPortalsExported.ToString());
                this.Result.AddSummary("Exported User Roles", totalUserRolesExported.ToString());
                if (includeProfile)
                {
                    this.Result.AddSummary("Exported User Profiles", totalProfilesExported.ToString());
                }

                this.Result.AddSummary("Exported User Authentication", totalAuthenticationExported.ToString());
                this.Result.AddSummary("Exported Aspnet User", totalAspnetUserExported.ToString());
                this.Result.AddSummary("Exported Aspnet Membership", totalAspnetMembershipExported.ToString());
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckCancelled(importJob))
            {
                return;
            }

            const int pageSize = Constants.DefaultPageSize;
            var totalUsersImported = 0;
            var totalPortalsImported = 0;
            var totalAspnetUserImported = 0;
            var totalAspnetMembershipImported = 0;
            var totalUserAuthenticationCount = 0;
            var totalUsers = this.Repository.GetCount<ExportUser>();
            if (totalUsers == 0)
            {
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
                return;
            }

            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);

            // Skip the import if all the users has been processed already.
            if (this.CheckPoint.Stage >= totalPages)
            {
                return;
            }

            var pageIndex = this.CheckPoint.Stage;

            var totalUsersToBeProcessed = totalUsers - (pageIndex * pageSize);

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalUsers : this.CheckPoint.TotalItems;
            if (this.CheckPointStageCallback(this))
            {
                return;
            }

            try
            {
                this.Repository.RebuildIndex<ExportUser>(x => x.Id, true);
                this.Repository.RebuildIndex<ExportUserPortal>(x => x.ReferenceId);
                this.Repository.RebuildIndex<ExportAspnetUser>(x => x.ReferenceId);
                this.Repository.RebuildIndex<ExportAspnetMembership>(x => x.ReferenceId);
                this.Repository.RebuildIndex<ExportUserAuthentication>(x => x.ReferenceId);
                var portalId = importJob.PortalId;
                var dataProvider = DotNetNuke.Data.DataProvider.Instance();
                using (var table = new DataTable("Users"))
                {
                    // must create the columns from scratch with each iteration
                    table.Columns.AddRange(
                        UsersDatasetColumns.Select(column => new DataColumn(column.Item1, column.Item2)).ToArray());
                    while (totalUsersImported < totalUsersToBeProcessed)
                    {
                        if (this.CheckCancelled(importJob))
                        {
                            return;
                        }

                        var users =
                            this.Repository.GetAllItems<ExportUser>(null, true, pageIndex * pageSize, pageSize).ToList();
                        var tempAspUserCount = 0;
                        var tempAspMembershipCount = 0;
                        var tempUserPortalCount = 0;
                        var tempUserAuthenticationCount = 0;
                        try
                        {
                            foreach (var user in users)
                            {
                                if (this.CheckCancelled(importJob))
                                {
                                    return;
                                }

                                var row = table.NewRow();

                                var userPortal = this.Repository.GetRelatedItems<ExportUserPortal>(user.Id).FirstOrDefault();
                                var userAuthentication = this.Repository.GetRelatedItems<ExportUserAuthentication>(user.Id).FirstOrDefault();

                                // Aspnet Users and Membership
                                var aspNetUser = this.Repository.GetRelatedItems<ExportAspnetUser>(user.Id).FirstOrDefault();
                                var aspnetMembership = aspNetUser != null
                                    ? this.Repository.GetRelatedItems<ExportAspnetMembership>(aspNetUser.Id).FirstOrDefault()
                                    : null;

                                row["PortalId"] = portalId;
                                row["Username"] = user.Username;
                                row["FirstName"] = string.IsNullOrEmpty(user.FirstName) ? string.Empty : user.FirstName;
                                row["LastName"] = string.IsNullOrEmpty(user.LastName) ? string.Empty : user.LastName;
                                row["AffiliateId"] = dataProvider.GetNull(user.AffiliateId);
                                row["IsSuperUser"] = user.IsSuperUser;
                                row["Email"] = user.Email;
                                row["DisplayName"] = string.IsNullOrEmpty(user.DisplayName) ? string.Empty : user.DisplayName;
                                row["UpdatePassword"] = user.UpdatePassword;
                                row["CreatedByUserID"] = Util.GetUserIdByName(importJob, user.CreatedByUserId, user.CreatedByUserName);
                                row["LastIPAddress"] = dataProvider.GetNull(user.LastIpAddress);
                                row["PasswordResetToken"] = dataProvider.GetNull(user.PasswordResetToken);
                                row["PasswordResetExpiration"] = dataProvider.GetNull(user.PasswordResetExpiration);
                                row["LastModifiedByUserID"] = Util.GetUserIdByName(importJob, user.LastModifiedByUserId, user.LastModifiedByUserName);

                                if (userPortal != null)
                                {
                                    tempUserPortalCount += 1;
                                    row["Authorised"] = userPortal.Authorised;
                                    row["VanityUrl"] = userPortal.VanityUrl;
                                    row["RefreshRoles"] = userPortal.RefreshRoles;
                                    row["IsDeleted"] = userPortal.IsDeleted;
                                }
                                else
                                {
                                    row["Authorised"] = DBNull.Value;
                                    row["VanityUrl"] = DBNull.Value;
                                    row["RefreshRoles"] = DBNull.Value;
                                    row["IsDeleted"] = DBNull.Value;
                                }

                                if (userAuthentication != null)
                                {
                                    tempUserAuthenticationCount += 1;
                                    row["AuthenticationType"] = userAuthentication?.AuthenticationType;
                                    row["AuthenticationToken"] = userAuthentication?.AuthenticationToken;
                                }
                                else
                                {
                                    row["AuthenticationType"] = DBNull.Value;
                                    row["AuthenticationToken"] = DBNull.Value;
                                }

                                if (aspNetUser != null)
                                {
                                    tempAspUserCount += 1;
                                    row["ApplicationId"] = this.GetApplicationId();
                                    row["AspUserId"] = aspNetUser.UserId;
                                    row["MobileAlias"] = aspNetUser.MobileAlias;
                                    row["IsAnonymous"] = aspNetUser.IsAnonymous;
                                    if (aspnetMembership != null)
                                    {
                                        tempAspMembershipCount += 1;
                                        row["Password"] = string.IsNullOrEmpty(aspnetMembership.Password) ? string.Empty : aspnetMembership.Password;
                                        row["PasswordFormat"] = aspnetMembership.PasswordFormat;
                                        row["PasswordSalt"] = aspnetMembership.PasswordSalt;
                                        row["MobilePIN"] = aspnetMembership.MobilePin;
                                        row["PasswordQuestion"] = aspnetMembership.PasswordQuestion;
                                        row["PasswordAnswer"] = aspnetMembership.PasswordAnswer;
                                        row["IsApproved"] = aspnetMembership.IsApproved;
                                        row["IsLockedOut"] = aspnetMembership.IsLockedOut;
                                        row["FailedPasswordAttemptCount"] = aspnetMembership.FailedPasswordAttemptCount;
                                        row["FailedPasswordAnswerAttemptCount"] = aspnetMembership.FailedPasswordAnswerAttemptCount;
                                        row["Comment"] = aspnetMembership.Comment;
                                    }
                                }
                                else
                                {
                                    row["ApplicationId"] = DBNull.Value;
                                    row["AspUserId"] = DBNull.Value;
                                    row["MobileAlias"] = DBNull.Value;
                                    row["IsAnonymous"] = DBNull.Value;
                                    row["Password"] = DBNull.Value;
                                    row["PasswordFormat"] = DBNull.Value;
                                    row["PasswordSalt"] = DBNull.Value;
                                    row["MobilePIN"] = DBNull.Value;
                                    row["PasswordQuestion"] = DBNull.Value;
                                    row["PasswordAnswer"] = DBNull.Value;
                                    row["IsApproved"] = DBNull.Value;
                                    row["IsLockedOut"] = DBNull.Value;
                                    row["FailedPasswordAttemptCount"] = DBNull.Value;
                                    row["FailedPasswordAnswerAttemptCount"] = DBNull.Value;
                                    row["Comment"] = DBNull.Value;
                                }

                                table.Rows.Add(row);
                            }

                            var overwrite = importDto.CollisionResolution == CollisionResolution.Overwrite;

                            // Bulk insert the data in DB
                            DotNetNuke.Data.DataProvider.Instance()
                                .BulkInsert("ExportImport_AddUpdateUsersBulk", "@DataTable", table, new Dictionary<string, object> { { "Overwrite", overwrite } });
                            totalUsersImported += users.Count;
                            totalAspnetUserImported += tempAspUserCount;
                            totalAspnetMembershipImported += tempAspMembershipCount;
                            totalPortalsImported += tempUserPortalCount;
                            totalUserAuthenticationCount += tempUserAuthenticationCount;
                            this.CheckPoint.ProcessedItems += users.Count;
                        }
                        catch (Exception ex)
                        {
                            this.Result.AddLogEntry($"Importing Users from {pageIndex * pageSize} to {(pageIndex * pageSize) + pageSize} exception", ex.Message, ReportLevel.Error);
                        }

                        table.Rows.Clear();
                        pageIndex++;
                        this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalUsers;
                        this.CheckPoint.Stage++;
                        this.CheckPoint.StageData = null;
                        if (this.CheckPointStageCallback(this))
                        {
                            return;
                        }
                    }
                }

                this.CheckPoint.Completed = true;
                this.CheckPoint.Progress = 100;
            }
            finally
            {
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Imported Users", totalUsersImported.ToString());
                this.Result.AddSummary("Imported User Portals", totalPortalsImported.ToString());
                this.Result.AddSummary("Import User Authentications", totalUserAuthenticationCount.ToString());
                this.Result.AddSummary("Imported Aspnet Users", totalAspnetUserImported.ToString());
                this.Result.AddSummary("Imported Aspnet Memberships", totalAspnetMembershipImported.ToString());
            }
        }

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<ExportUser>();
        }

        private Guid GetApplicationId()
        {
            using (var db =
                new PetaPocoDataContext(
                    DotNetNuke.Data.DataProvider.Instance().Settings["connectionStringName"],
                    "aspnet_"))
            {
                return db.ExecuteScalar<Guid>(
                    CommandType.Text,
                    "SELECT TOP 1 ApplicationId FROM aspnet_Applications");
            }
        }
    }
}
