using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Data;
using DotNetNuke.Common.Utilities;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using UserProfile = Dnn.ExportImport.Components.Dto.Users.UserProfile;

namespace Dnn.ExportImport.Components.Services
{
    public class UsersExportService : IPortable2
    {
        private int _progressPercentage;

        public string Category => "USERS";

        public uint Priority => 3;

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
        public void ExportData(ExportImportJob exportJob, IExportImportRepository repository)
        {
            var portalId = exportJob.PortalId;
            var pageIndex = 0;
            var pageSize = 1000;
            var totalProcessed = 0;
            ProgressPercentage = 0;
            try
            {
                var dataReader = DataProvider.Instance()
                    .ExecuteReader("ExportImport_GetAllUsers", portalId, pageIndex, pageSize, false);
                var allUser = CBO.FillCollection<Users>(dataReader).ToList();
                var firstOrDefault = allUser.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var totalUsers = allUser.Any() ? firstOrDefault.Total : 0;
                    var progressStep = totalUsers < pageSize ? 100 : pageSize/totalUsers*100;
                    do
                    {
                        foreach (var user in allUser)
                        {
                            var aspnetUser =
                                CBO.FillObject<AspnetUsers>(DataProvider.Instance()
                                    .ExecuteReader("ExportImport_GetAspNetUser", user.Username));
                            var aspnetMembership = CBO.FillObject<AspnetMembership>(DataProvider.Instance()
                                .ExecuteReader("ExportImport_GetUserMembership", aspnetUser.UserId,
                                    aspnetUser.ApplicationId));
                            var userRoles = CBO.FillCollection<UserRoles>(DataProvider.Instance()
                                .ExecuteReader("ExportImport_GetUserRoles", portalId, user.UserId));
                            var userPortal = CBO.FillObject<UserPortals>(DataProvider.Instance()
                                .ExecuteReader("ExportImport_GetUserPortal", portalId, user.UserId));
                            var userAuthentications = CBO.FillCollection<UserAuthentication>(DataProvider.Instance()
                                .ExecuteReader("GetUserAuthentication", user.UserId));
                            
                            repository.CreateItem(user, null);
                            repository.CreateItem(aspnetUser, user.Id);
                            repository.CreateItem(aspnetMembership, user.Id);
                            repository.CreateItem(userPortal, user.Id);
                            repository.CreateItems(userRoles, user.Id);
                            repository.CreateItems(userAuthentications, user.Id);
                        }
                        totalProcessed += pageSize;
                        pageIndex++;
                        ProgressPercentage += progressStep;
                        dataReader = DataProvider.Instance()
                            .ExecuteReader("ExportImport_GetAllUsers", portalId, pageIndex, pageSize, false);
                        allUser =
                            CBO.FillCollection<Users>(dataReader).ToList();
                    } while (totalProcessed < totalUsers);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository)
        {
            ProgressPercentage = 0;
            var portalId = importJob.PortalId;
            var totalUsersToImport = repository.GetCount<Users>();
            var users = repository.GetAllItems<Users>(null, true, 0, 100);
            foreach (var user in users)
            {
                var userRoles = repository.GetRelatedItems<UserRoles>(user.Id).ToList();
                var userAuthentications = repository.GetRelatedItems<UserAuthentication>(user.Id).ToList();
                var aspNetUser = repository.GetRelatedItems<AspnetUsers>(user.Id).FirstOrDefault();
                var aspnetMembership = repository.GetRelatedItems<AspnetMembership>(user.Id).FirstOrDefault();
                var userPortal = repository.GetRelatedItems<UserPortals>(user.Id).FirstOrDefault();
                var existingUser = UserController.GetUserByName(portalId, user.Username);

                if (existingUser != null)
                {
                    switch (exporteDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            //UserController.CreateUser()
                            break;
                        case CollisionResolution.Ignore: //Just ignore the record
                            //TODO: Log that user was ignored.
                            break;
                        case CollisionResolution.Duplicate: //Duplicate option will not work for users.
                            //TODO: Log that users was ignored as duplicate not possible for users.
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    if (aspNetUser != null && aspnetMembership != null)
                    {
                        using (var db = new PetaPocoDataContext(DataProvider.Instance().Settings["connectionStringName"], "aspnet_"))
                        {
                            var applicationName = Globals.GetApplicationName(portalId);
                            var applicationId = db.ExecuteScalar<Guid>(CommandType.Text,
                                $"SELECT TOP 1 ApplicationId FROM aspnet_Applications WHERE ApplicationName='DotNetNuke'");
                            aspNetUser.UserId = Guid.Empty;
                            aspNetUser.ApplicationId = applicationId;
                            aspNetUser.LastActivityDate = DateTime.UtcNow;
                            var repAspnetUsers = db.GetRepository<AspnetUsers>();
                            repAspnetUsers.Insert(aspNetUser);
                            var repAspnetMembership = db.GetRepository<AspnetMembership>();
                            aspnetMembership.UserId = aspNetUser.UserId;
                            aspnetMembership.ApplicationId = applicationId;
                            aspnetMembership.CreateDate = DateTime.UtcNow;
                            aspnetMembership.LastLoginDate =
                                aspnetMembership.LastPasswordChangedDate =
                                    aspnetMembership.LastLockoutDate =
                                        aspnetMembership.FailedPasswordAnswerAttemptWindowStart =
                                            aspnetMembership.FailedPasswordAttemptWindowStart = new DateTime(1970, 1, 1);
//                            aspnetMembership.FailedPasswordAnswerAttemptCount =
//                                aspnetMembership.FailedPasswordAttemptCount = 0;
                            repAspnetMembership.Insert(aspnetMembership);
                        }
                        using (var db = DataContext.Instance())
                        {
                            user.Id = 0;
                            user.CreatedByUserId = GetUserId(importJob, user.CreatedByUserId, user.CreatedByUserName);
                            user.LastModifiedByUserId = GetUserId(importJob, user.LastModifiedByUserId,
                                user.LastModifiedByUserName);
                            user.CreatedOnDate = user.LastModifiedOnDate = DateTime.UtcNow;
                            var repUser = db.GetRepository<Users>();
                            repUser.Insert(user);
                            if (userPortal != null)
                            {
                                var repUserPortal = db.GetRepository<UserPortals>();
                                userPortal.UserId = user.UserId;
                                userPortal.PortalId = portalId;
                                userPortal.CreatedDate = DateTime.UtcNow;
                                repUserPortal.Insert(userPortal);
                            }
                            if (!userRoles.Any()) continue;

                            var repUserRoles = db.GetRepository<UserRoles>();

                            foreach (var userRole in userRoles)
                            {
                                var roleId = GetRoleId(importJob, userRole.RoleId, userRole.RoleName);
                                if (roleId == null) continue;

                                userRole.UserId = user.UserId;
                                userRole.RoleId = roleId.Value;
                                userRole.CreatedOnDate = DateTime.UtcNow;
                                userRole.LastModifiedOnDate = DateTime.UtcNow;
                                userRole.EffectiveDate = userRole.EffectiveDate != null
                                    ? (DateTime?) DateTime.UtcNow
                                    : null;
                                userRole.CreatedByUserId = GetUserId(importJob, user.CreatedByUserId, user.CreatedByUserName);
                                userRole.LastModifiedByUserId = GetUserId(importJob, user.LastModifiedByUserId, user.LastModifiedByUserName);
                                repUserRoles.Insert(userRole);
                            }
                            if (!userAuthentications.Any()) continue;

                            var repUserAuthentication = db.GetRepository<UserAuthentication>();
                            foreach (var userAuthentication in userAuthentications)
                            {
                                userAuthentication.UserId = user.UserId;
                                userAuthentication.CreatedOnDate = DateTime.UtcNow;
                                userAuthentication.LastModifiedOnDate = DateTime.UtcNow;
                                userAuthentication.CreatedByUserId = GetUserId(importJob, user.CreatedByUserId, user.CreatedByUserName);
                                userAuthentication.LastModifiedByUserId = GetUserId(importJob, user.LastModifiedByUserId, user.LastModifiedByUserName);
                                repUserAuthentication.Insert(userAuthentication);
                            }
                        }
                    }
                }
            }
        }

        private Users UpdateUser(Users user)
        {
            using (var db = DataContext.Instance())
            {
                var rep = db.GetRepository<Users>();
                rep.Insert(user);
                var dataService = Util.GetDataService();
            }
            return user;
        }

        private Users AddUser(Users user)
        {
            //var dataService = Util.GetDataService();
            using (var db = DataContext.Instance())
            {
                var rep = db.GetRepository<Users>();
                rep.Insert(user);
            }
            return user;
        }

        //        private UserInfo MapUserInfoFromUser(UserInfo userInfo, Users user, int portalId)
        //        {
        //            userInfo.PortalID = portalId;
        //            userInfo.DisplayName = user.DisplayName;
        //            userInfo.Email = user.Email;
        //            userInfo.FirstName = user.FirstName;
        //            userInfo.IsDeleted = user.IsDeleted;
        //            userInfo.IsSuperUser = user.IsSuperUser;
        //            userInfo.LastIPAddress = user.LastIpAddress;
        //            userInfo.LastName = user.LastName;
        //            //userInfo.AffiliateID = user.AffiliateId;//??
        //            userInfo.PasswordResetExpiration = user.PasswordResetExpiration?? DateTime.Now.AddMinutes(1440);
        //            userInfo.PasswordResetToken = user.PasswordResetToken;
        //            userInfo.VanityUrl = user.UserPortals.VanityUrl;
        //            userInfo.Membership = new UserMembership(userInfo)
        //            {
        //                Approved = user.AspnetMembership.IsApproved,
        //                CreatedDate = user.AspnetMembership.CreateDate,
        //                IsDeleted = user.IsDeleted,
        //                IsOnLine = false,
        //                LastActivityDate = user.AspnetUser.LastActivityDate ?? user.AspnetMembership.CreateDate,
        //                LastLockoutDate = user.AspnetMembership.LastLockoutDate,
        //                LastLoginDate = user.AspnetMembership.LastLoginDate,
        //                LastPasswordChangeDate = user.AspnetMembership.LastPasswordChangedDate,
        //                Password = user.AspnetMembership.Password,//We need to get the unencrypted password?
        //                PasswordConfirm = user.AspnetMembership.Password,//We need to get the unencrypted password?
        //                PasswordAnswer = user.AspnetMembership.PasswordAnswer,
        //                PasswordQuestion = user.AspnetMembership.PasswordQuestion,
        //                UpdatePassword = user.UpdatePassword,
        //                LockedOut = user.AspnetMembership.IsLockedOut
        //            };
        //            userInfo.Social
        //            userInfo.Roles = user.UserRoles.Select(x => x.RoleName).ToArray();
        //            //userInfo.Profile
        //            //userInfo.Social
        //            
        //            return userInfo;
        //        }

        private int GetUserId(ExportImportJob importJob, int exportedUserId, string exportUsername)
        {
            if (exportedUserId <= 0)
                return -1;
            if (exportedUserId == 1)
                return 1;

            var user = UserController.GetUserByName(importJob.PortalId, exportUsername);
            return user.UserID < 0 ? importJob.CreatedBy : user.UserID;
        }

        private int? GetRoleId(ExportImportJob importJob, int exportedRoleId, string exportRolename)
        {
            var role = RoleController.Instance.GetRoleByName(importJob.PortalId, exportRolename);
            return role?.RoleID;
        }
    }
}
