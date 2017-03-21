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
using System.Linq;
using System.Threading;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using DotNetNuke.Common.Utilities;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Users;
using DotNetNuke.Data;
using DotNetNuke.Security.Membership;
using Newtonsoft.Json;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Service to export/import users.
    /// </summary>
    public class UsersExportService : Potable2Base
    {
        private int _progressPercentage;

        public override string Category => Constants.Category_Users;

        public override string ParentCategory => null;

        public override uint Priority => 1;

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

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var sinceDate = exportDto.SinceTime?.DateTime;
            var tillDate = exportJob.CreatedOnDate;
            if (CheckCancelled(exportJob)) return;

            var portalId = exportJob.PortalId;
            var pageIndex = 0;
            const int pageSize = 1000;
            var totalUsersExported = 0;
            var totalUserRolesExported = 0;
            var totalPortalsExported = 0;
            var totalProfilesExported = 0;
            var totalAuthenticationExported = 0;
            var totalAspnetUserExported = 0;
            var totalAspnetMembershipExported = 0;
            ProgressPercentage = 0;
            var dataReader = DataProvider.Instance()
                .GetAllUsers(portalId, pageIndex, pageSize, exportDto.IncludeDeletions,tillDate,
                    sinceDate);
            var allUser = CBO.FillCollection<ExportUser>(dataReader).ToList();
            var firstOrDefault = allUser.FirstOrDefault();
            if (firstOrDefault == null) return;

            var totalUsers = allUser.Any() ? firstOrDefault.Total : 0;
            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);

            var skip = GetCurrentSkip();
            var currentIndex = skip;

            //Skip the export if all the users has been processed already.
            if (CheckPoint.Stage >= totalPages && skip == 0)
                return;

            //Check if there is any pending stage or partially processed data.
            if (CheckPoint.Stage > 0 || skip > 0)
            {
                pageIndex = CheckPoint.Stage;
                if (pageIndex > 0)
                {
                    dataReader = DataProvider.Instance()
                        .GetAllUsers(portalId, pageIndex, pageSize, false,tillDate, sinceDate);
                    allUser =
                        CBO.FillCollection<ExportUser>(dataReader).ToList();
                }
                allUser = allUser.Skip(skip).ToList();
            }

            var totalUsersToBeProcessed = totalUsers - pageIndex * pageSize - skip;
            var progressStep = totalUsersToBeProcessed < pageSize ? 100 : pageSize / totalUsersToBeProcessed * 100;
            try
            {
                do
                {
                    if (CheckCancelled(exportJob)) break;
                    foreach (var user in allUser)
                    {
                        if (CheckCancelled(exportJob)) break;
                        var aspnetUser =
                            CBO.FillObject<ExportAspnetUser>(DataProvider.Instance().GetAspNetUser(user.Username));
                        var aspnetMembership =
                            CBO.FillObject<ExportAspnetMembership>(
                                DataProvider.Instance()
                                    .GetUserMembership(aspnetUser.UserId, aspnetUser.ApplicationId));
                        var userRoles =
                            CBO.FillCollection<ExportUserRole>(DataProvider.Instance()
                                .GetUserRoles(portalId, user.UserId));
                        var userPortal =
                            CBO.FillObject<ExportUserPortal>(DataProvider.Instance()
                                .GetUserPortal(portalId, user.UserId));
                        var userAuthentication =
                            CBO.FillObject<ExportUserAuthentication>(
                                DataProvider.Instance().GetUserAuthentication(user.UserId));
                        var userProfiles =
                            CBO.FillCollection<ExportUserProfile>(DataProvider.Instance()
                                .GetUserProfile(portalId, user.UserId));

                        Repository.CreateItem(user, null);
                        Repository.CreateItem(aspnetUser, user.Id);
                        totalAspnetUserExported += 1;

                        Repository.CreateItem(aspnetMembership, user.Id);
                        totalAspnetMembershipExported += aspnetMembership != null ? 1 : 0;

                        Repository.CreateItem(userPortal, user.Id);
                        totalPortalsExported += userPortal != null ? 1 : 0;

                        Repository.CreateItems(userProfiles, user.Id);
                        totalProfilesExported += userProfiles.Count;

                        Repository.CreateItems(userRoles, user.Id);
                        totalUserRolesExported += userRoles.Count;

                        Repository.CreateItem(userAuthentication, user.Id);
                        totalAuthenticationExported += userAuthentication != null ? 1 : 0;
                        currentIndex++;
                    }
                    totalUsersExported += currentIndex;
                    currentIndex = 0;
                    CheckPoint.Stage++;
                    CheckPoint.StageData = null;
                    if (CheckPointStageCallback(this)) break;

                    pageIndex++;
                    ProgressPercentage += progressStep;
                    dataReader = DataProvider.Instance()
                        .GetAllUsers(portalId, pageIndex, pageSize, false,tillDate, sinceDate);
                    allUser =
                        CBO.FillCollection<ExportUser>(dataReader).ToList();
                } while (totalUsersExported < totalUsersToBeProcessed);
            }
            finally
            {
                CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                CheckPointStageCallback(this);
            }

            Result.AddSummary("Exported Users", totalUsersExported.ToString());
            Result.AddSummary("Exported User Portals", totalPortalsExported.ToString());
            Result.AddSummary("Exported User Roles", totalUserRolesExported.ToString());
            Result.AddSummary("Exported User Profiles", totalProfilesExported.ToString());
            Result.AddSummary("Exported User Authentication", totalAuthenticationExported.ToString());
            Result.AddSummary("Exported Aspnet User", totalAspnetUserExported.ToString());
            Result.AddSummary("Exported Aspnet Membership", totalAspnetMembershipExported.ToString());
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exportDto)
        {
            if (CheckCancelled(importJob)) return;

            ProgressPercentage = 0;
            var pageIndex = 0;
            const int pageSize = 1000;
            var totalUsersImported = 0;
            var totalPortalsImported = 0;
            var totalAspnetUserImported = 0;
            var totalAspnetMembershipImported = 0;

            var totalUsers = Repository.GetCount<ExportUser>();
            var totalPages = Util.CalculateTotalPages(totalUsers, pageSize);

            var skip = GetCurrentSkip();
            var currentIndex = skip;
            //Skip the import if all the users has been processed already.
            if (CheckPoint.Stage >= totalPages && skip == 0)
                return;

            pageIndex = CheckPoint.Stage;

            var totalUsersToBeProcessed = totalUsers - pageIndex * pageSize - skip;
            var progressStep = totalUsersToBeProcessed < pageSize ? 100 : pageSize / totalUsersToBeProcessed * 100;
            try
            {
                while (totalUsersImported < totalUsersToBeProcessed)
                {
                    if (CheckCancelled(importJob)) break;
                    var users =
                        Repository.GetAllItems<ExportUser>(null, true, pageIndex * pageSize + skip, pageSize).ToList();
                    skip = 0;
                    foreach (var user in users)
                    {
                        if (CheckCancelled(importJob)) break;
                        using (var db = DataContext.Instance())
                        {
                            var aspNetUser = Repository.GetRelatedItems<ExportAspnetUser>(user.Id).FirstOrDefault();
                            if (aspNetUser == null)
                            {
                                currentIndex++;
                                continue;
                            }

                            var aspnetMembership =
                                Repository.GetRelatedItems<ExportAspnetMembership>(user.Id).FirstOrDefault();
                            if (aspnetMembership == null)
                            {
                                currentIndex++;
                                continue;
                            }

                            var userPortal = Repository.GetRelatedItems<ExportUserPortal>(user.Id).FirstOrDefault();
                            ProcessUser(importJob, exportDto, db, user, userPortal, aspNetUser, aspnetMembership);
                            totalAspnetUserImported += 1;
                            totalAspnetMembershipImported += 1;
                            ProcessUserPortal(importJob, exportDto, db, userPortal, user.UserId, user.Username);
                            totalPortalsImported += userPortal != null ? 1 : 0;

                            //Update the source repository local ids.
                            Repository.UpdateItem(user);
                            Repository.UpdateItem(userPortal);
                        }
                        currentIndex++;
                    }
                    totalUsersImported += currentIndex;
                    currentIndex = 0;
                    CheckPoint.Stage++;
                    CheckPoint.StageData = null;
                    if (CheckPointStageCallback(this)) break;

                    ProgressPercentage += progressStep;
                    pageIndex++;
                }
            }
            finally
            {
                CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                CheckPointStageCallback(this);
            }
            Result.AddSummary("Imported Users", totalUsersImported.ToString());
            Result.AddSummary("Imported User Portals", totalPortalsImported.ToString());
            Result.AddSummary("Imported Aspnet Users", totalAspnetUserImported.ToString());
            Result.AddSummary("Imported Aspnet Memberships", totalAspnetMembershipImported.ToString());
        }

        private void ProcessUser(ExportImportJob importJob, ExportDto exportDto, IDataContext db, ExportUser user,
            ExportUserPortal userPortal, ExportAspnetUser aspnetUser, ExportAspnetMembership aspnetMembership)
        {
            if (user == null) return;
            var existingUser = UserController.GetUserByName(user.Username);
            var isUpdate = false;
            var repUser = db.GetRepository<ExportUser>();

            if (existingUser != null)
            {
                switch (exportDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore: //Just ignore the record
                        Result.AddLogEntry("Ignored user", user.Username);
                        return;
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        Result.AddLogEntry("Ignored duplicate user", user.Username);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                }
            }
            if (isUpdate)
            {
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.DisplayName = user.DisplayName;
                existingUser.Email = user.Email;
                existingUser.IsDeleted = user.IsDeleted;
                existingUser.IsSuperUser = user.IsSuperUser;
                existingUser.VanityUrl = userPortal?.VanityUrl;
                MembershipProvider.Instance().UpdateUser(existingUser);
                DataCache.ClearCache(DataCache.UserCacheKey);
                user.UserId = existingUser.UserID;
                Result.AddLogEntry("Updated user", user.Username);
            }
            else
            {
                ProcessUserMembership(aspnetUser, aspnetMembership);
                user.UserId = 0;
                user.FirstName = string.IsNullOrEmpty(user.FirstName) ? string.Empty : user.FirstName;
                user.LastName = string.IsNullOrEmpty(user.LastName) ? string.Empty : user.LastName;
                user.CreatedOnDate = user.LastModifiedOnDate = DateTime.UtcNow;
                repUser.Insert(user);
                Result.AddLogEntry("Added user", user.Username);
            }
            user.LocalId = user.UserId;
        }

        private void ProcessUserPortal(ExportImportJob importJob, ExportDto exportDto, IDataContext db,
            ExportUserPortal userPortal, int userId, string username)
        {
            if (userPortal == null) return;
            var repUserPortal = db.GetRepository<ExportUserPortal>();
            var existingPortal =
                CBO.FillObject<ExportUserPortal>(DataProvider.Instance().GetUserPortal(importJob.PortalId, userId));
            var isUpdate = false;
            if (existingPortal != null)
            {
                switch (exportDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore: //Just ignore the record
                        //Result.AddLogEntry("Ignored user portal", $"{username}/{userPortal.PortalId}");
                        return;
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        //Result.AddLogEntry("Ignored duplicate user portal", $"{username}/{userPortal.PortalId}");
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                }
            }
            userPortal.UserId = userId;
            userPortal.PortalId = importJob.PortalId;
            if (isUpdate)
            {
                userPortal.UserPortalId = existingPortal.UserPortalId;
                repUserPortal.Update(userPortal);
                //Result.AddLogEntry("Updated user portal", $"{username}/{userPortal.PortalId}");
            }
            else
            {
                userPortal.UserPortalId = 0;
                userPortal.CreatedDate = DateTime.UtcNow;
                repUserPortal.Insert(userPortal);
                //Result.AddLogEntry("Added user portal", $"{username}/{userPortal.PortalId}");
            }
            userPortal.LocalId = userPortal.UserPortalId;
        }

        private void ProcessUserMembership(ExportAspnetUser aspNetUser, ExportAspnetMembership aspnetMembership)
        {
            using (var db =
                new PetaPocoDataContext(DotNetNuke.Data.DataProvider.Instance().Settings["connectionStringName"],
                    "aspnet_"))
            {
                var applicationId = db.ExecuteScalar<Guid>(CommandType.Text,
                    "SELECT TOP 1 ApplicationId FROM aspnet_Applications");

                //AspnetUser

                aspNetUser.UserId = Guid.Empty;
                aspNetUser.ApplicationId = applicationId;
                aspNetUser.LastActivityDate = DateTime.UtcNow;
                var repAspnetUsers = db.GetRepository<ExportAspnetUser>();
                repAspnetUsers.Insert(aspNetUser);
                //aspNetUser.LocalId = aspNetUser.UserId;

                //AspnetMembership
                var repAspnetMembership = db.GetRepository<ExportAspnetMembership>();
                aspnetMembership.UserId = aspNetUser.UserId;
                aspnetMembership.ApplicationId = applicationId;
                aspnetMembership.CreateDate = DateTime.UtcNow;
                aspnetMembership.LastLoginDate =
                    aspnetMembership.LastPasswordChangedDate =
                        aspnetMembership.LastLockoutDate =
                            aspnetMembership.FailedPasswordAnswerAttemptWindowStart =
                                aspnetMembership.FailedPasswordAttemptWindowStart =
                                    new DateTime(1754, 1, 1);
                //aspnetMembership.FailedPasswordAnswerAttemptCount =
                //    aspnetMembership.FailedPasswordAttemptCount = 0;
                repAspnetMembership.Insert(aspnetMembership);
                //aspnetMembership.LocalId = aspnetMembership.UserId;
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