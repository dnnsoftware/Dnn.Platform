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
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Common.Utilities;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Models;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Users;
using DotNetNuke.Data;
using DotNetNuke.Security.Membership;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Service to export/import users.
    /// </summary>
    public class UsersExportService : IPortable2
    {
        private int _progressPercentage;

        public string Category => "USERS";

        public string ParentCategory => null;

        public uint Priority => 1;

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
            var portalId = exportJob.PortalId;
            var pageIndex = 0;
            var pageSize = 1000;
            var totalUsersExported = 0;
            var totalUserRolesExported = 0;
            var totalPortalsExported = 0;
            var totalProfilesExported = 0;
            var totalAuthenticationExported = 0;
            var totalAspnetUserExported = 0;
            var totalAspnetMembershipExported = 0;
            ProgressPercentage = 0;
            var dataReader = DataProvider.Instance().GetAllUsers(portalId, pageIndex, pageSize, exportDto.IncludeDeletions, exportDto.ExportTime?.UtcDateTime);
            var allUser = CBO.FillCollection<ExportUser>(dataReader).ToList();
            var firstOrDefault = allUser.FirstOrDefault();
            if (firstOrDefault == null) return;

            var totalUsers = allUser.Any() ? firstOrDefault.Total : 0;
            var progressStep = totalUsers < pageSize ? 100 : pageSize/totalUsers*100;
            do
            {
                foreach (var user in allUser)
                {
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
                        CBO.FillObject<ExportUserPortal>(DataProvider.Instance().GetUserPortal(portalId, user.UserId));
                    var userAuthentication =
                        CBO.FillObject<ExportUserAuthentication>(
                            DataProvider.Instance().GetUserAuthentication(user.UserId));
                    var userProfiles =
                        CBO.FillCollection<ExportUserProfile>(DataProvider.Instance().GetUserProfile(user.UserId));

                    repository.CreateItem(user, null);
                    repository.CreateItem(aspnetUser, user.Id);
                    totalAspnetUserExported += 1;

                    repository.CreateItem(aspnetMembership, user.Id);
                    totalAspnetMembershipExported += aspnetMembership != null ? 1 : 0;

                    repository.CreateItem(userPortal, user.Id);
                    totalPortalsExported += userPortal != null ? 1 : 0;

                    repository.CreateItems(userProfiles, user.Id);
                    totalProfilesExported += userProfiles.Count;

                    repository.CreateItems(userRoles, user.Id);
                    totalUserRolesExported += userRoles.Count;

                    repository.CreateItem(userAuthentication, user.Id);
                    totalAuthenticationExported += userAuthentication != null ? 1 : 0;
                }
                totalUsersExported += allUser.Count;
                pageIndex++;
                ProgressPercentage += progressStep;
                dataReader = DataProvider.Instance().GetAllUsers(portalId, pageIndex, pageSize, false, exportDto.ExportTime?.UtcDateTime);
                allUser =
                    CBO.FillCollection<ExportUser>(dataReader).ToList();
            } while (totalUsersExported < totalUsers);
            result.AddSummary("Exported Users", totalUsersExported.ToString());
            result.AddSummary("Exported User Portals", totalPortalsExported.ToString());
            result.AddSummary("Exported User Roles", totalUserRolesExported.ToString());
            result.AddSummary("Exported User Profiles", totalProfilesExported.ToString());
            result.AddSummary("Exported User Authentication", totalAuthenticationExported.ToString());
            result.AddSummary("Exported Aspnet User", totalAspnetUserExported.ToString());
            result.AddSummary("Exported Aspnet Membership", totalAspnetMembershipExported.ToString());
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository, ExportImportResult result)
        {
            ProgressPercentage = 0;
            var pageIndex = 0;
            var pageSize = 1000;
            var totalUsersImported = 0;
            var totalPortalsImported = 0;
            var totalAspnetUserImported = 0;
            var totalAspnetMembershipImported = 0;

            var totalUsers = repository.GetCount<ExportUser>();
            var progressStep = totalUsers < pageSize ? 100 : pageSize/totalUsers*100;
            while (totalUsersImported < totalUsers)
            {
                var users = repository.GetAllItems<ExportUser>(null, true, pageIndex*pageSize, pageSize).ToList();
                foreach (var user in users)
                {
                    using (var db = DataContext.Instance())
                    {
                        var aspNetUser = repository.GetRelatedItems<ExportAspnetUser>(user.Id).FirstOrDefault();
                        if (aspNetUser == null) continue;

                        var aspnetMembership =
                            repository.GetRelatedItems<ExportAspnetMembership>(user.Id).FirstOrDefault();
                        if (aspnetMembership == null) continue;

                        var userPortal = repository.GetRelatedItems<ExportUserPortal>(user.Id).FirstOrDefault();
                        ProcessUser(importJob, exporteDto, db, user, userPortal, aspNetUser, aspnetMembership);
                        totalAspnetUserImported += 1;
                        totalAspnetMembershipImported += 1;
                        ProcessUserPortal(importJob, exporteDto, db, userPortal, user.UserId);
                        totalPortalsImported += userPortal != null ? 1 : 0;

                        //Update the source repository local ids.
                        repository.UpdateItem(user);
                        repository.UpdateItem(userPortal);
                    }
                }
                totalUsersImported += pageSize > users.Count ? users.Count : pageSize;
                ProgressPercentage += progressStep;
                pageIndex++;
            }
            result.AddSummary("Imported Users", totalUsersImported.ToString());
            result.AddSummary("Imported User Portals", totalPortalsImported.ToString());
            result.AddSummary("Imported Aspnet Users", totalAspnetUserImported.ToString());
            result.AddSummary("Imported Aspnet Memberships", totalAspnetMembershipImported.ToString());

        }

        private static void ProcessUserUpdate(UserInfo existingUser, ExportUser user, ExportUserPortal userPortal)
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
        }

        private static void ProcessUser(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            ExportUser user, ExportUserPortal userPortal, ExportAspnetUser aspnetUser, ExportAspnetMembership aspnetMembership)
        {
            if (user == null) return;

            var existingUser = UserController.GetUserByName(importJob.PortalId, user.Username);
            var isUpdate = false;
            var repUser = db.GetRepository<ExportUser>();

            if (existingUser != null)
            {
                switch (exporteDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore: //Just ignore the record
                    //TODO: Log that user was ignored.
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        //TODO: Log that users was ignored as duplicate not possible for users.
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                }
            }
            if (isUpdate)
            {
                ProcessUserUpdate(existingUser, user, userPortal);
                user.UserId = existingUser.UserID;
            }
            else
            {
                ProcessUserMembership(aspnetUser, aspnetMembership);
                user.UserId = 0;
                user.FirstName = string.IsNullOrEmpty(user.FirstName) ? string.Empty : user.FirstName;
                user.LastName = string.IsNullOrEmpty(user.LastName) ? string.Empty : user.LastName;
                user.CreatedOnDate = user.LastModifiedOnDate = DateTime.UtcNow;
                repUser.Insert(user);
            }
            user.LocalId = user.UserId;
        }

        private static void ProcessUserPortal(ExportImportJob importJob, ExportDto exporteDto, IDataContext db,
            ExportUserPortal userPortal, int userId)
        {
            if (userPortal == null) return;
            var repUserPortal = db.GetRepository<ExportUserPortal>();
            var existingPortal =
                CBO.FillObject<ExportUserPortal>(DataProvider.Instance().GetUserPortal(importJob.PortalId, userId));
            var isUpdate = false;
            if (existingPortal != null)
            {
                switch (exporteDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore: //Just ignore the record
                    case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                }
            }
            userPortal.UserId = userId;
            userPortal.PortalId = importJob.PortalId;
            if (isUpdate)
            {
                userPortal.UserPortalId = existingPortal.UserPortalId;
                repUserPortal.Update(userPortal);
            }
            else
            {
                userPortal.UserPortalId = 0;
                userPortal.CreatedDate = DateTime.UtcNow;
                repUserPortal.Insert(userPortal);
            }
            userPortal.LocalId = userPortal.UserPortalId;
        }

        private static void ProcessUserMembership(ExportAspnetUser aspNetUser, ExportAspnetMembership aspnetMembership)
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
                //                            aspnetMembership.FailedPasswordAnswerAttemptCount =
                //                                aspnetMembership.FailedPasswordAttemptCount = 0;
                repAspnetMembership.Insert(aspnetMembership);
                //aspnetMembership.LocalId = aspnetMembership.UserId;
            }
        }
    }
}