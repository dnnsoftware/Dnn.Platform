using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Security;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Users;
using Dnn.ExportImport.Components.Interfaces;
using DotNetNuke.Common.Utilities;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Common;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Data;
using DotNetNuke.Security.Membership;
using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

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
            var dataReader = DataProvider.Instance().GetAllUsers(portalId, pageIndex, pageSize, false);
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
                    var userAuthentications =
                        CBO.FillCollection<ExportUserAuthentication>(
                            DataProvider.Instance().GetUserAuthentication(user.UserId));
                    var userProfiles =
                        CBO.FillCollection<ExportUserProfile>(DataProvider.Instance().GetUserProfile(user.UserId));

                    repository.CreateItem(user, null);
                    repository.CreateItem(aspnetUser, user.Id);
                    repository.CreateItem(aspnetMembership, user.Id);
                    repository.CreateItem(userPortal, user.Id);
                    repository.CreateItems(userProfiles, user.Id);
                    repository.CreateItems(userRoles, user.Id);
                    repository.CreateItems(userAuthentications, user.Id);
                }
                totalProcessed += allUser.Count;
                pageIndex++;
                ProgressPercentage += progressStep;
                dataReader = DataProvider.Instance().GetAllUsers(portalId, pageIndex, pageSize, false);
                allUser =
                    CBO.FillCollection<ExportUser>(dataReader).ToList();
            } while (totalProcessed < totalUsers);
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository)
        {
            ProgressPercentage = 0;
            var portalId = importJob.PortalId;
            var pageIndex = 0;
            var pageSize = 1000;
            var totalProcessed = 0;
            var totalUsers = repository.GetCount<ExportUser>();
            var progressStep = totalUsers < pageSize ? 100 : pageSize/totalUsers*100;
            while (totalProcessed < totalUsers)
            {
                var users = repository.GetAllItems<ExportUser>(null, true, pageIndex*pageSize, pageSize).ToList();
                foreach (var user in users)
                {
                    var userRoles = repository.GetRelatedItems<ExportUserRole>(user.Id).ToList();
                    var userAuthentications = repository.GetRelatedItems<ExportUserAuthentication>(user.Id).ToList();
                    var aspNetUser = repository.GetRelatedItems<ExportAspnetUser>(user.Id).FirstOrDefault();
                    if (aspNetUser == null) continue;

                    var aspnetMembership = repository.GetRelatedItems<ExportAspnetMembership>(user.Id).FirstOrDefault();
                    if (aspnetMembership == null) continue;

                    var userPortal = repository.GetRelatedItems<ExportUserPortal>(user.Id).FirstOrDefault();
                    var userProfiles = repository.GetRelatedItems<ExportUserProfile>(user.Id).ToList();
                    var existingUser = UserController.GetUserByName(portalId, user.Username);

                    if (existingUser != null)
                    {
                        switch (exporteDto.CollisionResolution)
                        {
                            case CollisionResolution.Overwrite:
                                ProcessUserUpdate(existingUser, user, userPortal);
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
                        var createdById = Common.Util.GetUserIdOrName(importJob, user.CreatedByUserId,
                            user.CreatedByUserName);
                        var modifiedById = Common.Util.GetUserIdOrName(importJob, user.LastModifiedByUserId,
                            user.LastModifiedByUserName);
                        ProcessUserMembership(aspNetUser, aspnetMembership);

                        using (var db = DataContext.Instance())
                        {
                            ProcessUser(db, user, createdById, modifiedById);
                            //TODO: All the steps listed below should be done as Pass 2?
                            ProcessUserPortal(importJob, db, userPortal, user.UserId);
                            ProcessUserRoles(importJob, db, userRoles, user.UserId, createdById, modifiedById);
                            ProcessUserProfiles(importJob, db, userProfiles, user.UserId);
                            ProcessUserAuthentications(db, userAuthentications, user.UserId, createdById,
                                modifiedById);

                            //Update the source repository local ids.
                            repository.UpdateItem(user);
                            repository.UpdateItem(userPortal);
                            repository.UpdateItems(userRoles);
                            repository.UpdateItems(userProfiles);
                            repository.UpdateItems(userAuthentications);
                        }
                    }
                }
                totalProcessed += pageSize > users.Count ? users.Count : pageSize;
                ProgressPercentage += progressStep;
            }
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

        private static void ProcessUser(IDataContext db,
            ExportUser user, int createdById, int modifiedById)
        {
            user.UserId = 0;
            user.CreatedByUserId = createdById;
            user.LastModifiedByUserId = modifiedById;
            user.CreatedOnDate = user.LastModifiedOnDate = DateTime.UtcNow;
            var repUser = db.GetRepository<ExportUser>();
            repUser.Insert(user);
            user.LocalId = user.UserId;

        }

        private static void ProcessUserPortal(ExportImportJob importJob, IDataContext db,
            ExportUserPortal userPortal, int userId)
        {
            if (userPortal == null) return;

            var portalId = importJob.PortalId;
            var repUserPortal = db.GetRepository<ExportUserPortal>();
            userPortal.UserId = userId;
            userPortal.PortalId = portalId;
            userPortal.CreatedDate = DateTime.UtcNow;
            repUserPortal.Insert(userPortal);
            userPortal.LocalId = userPortal.UserPortalId;
        }

        private static void ProcessUserRoles(ExportImportJob importJob, IDataContext db,
            IEnumerable<ExportUserRole> userRoles, int userId, int createdById, int modifiedById)
        {
            var repUserRoles = db.GetRepository<ExportUserRole>();

            foreach (var userRole in userRoles)
            {
                var roleId = Common.Util.GetRoleId(importJob.PortalId, userRole.RoleName);
                if (roleId == null) continue;

                userRole.UserId = userId;
                userRole.RoleId = roleId.Value;
                userRole.CreatedOnDate = DateTime.UtcNow;
                userRole.LastModifiedOnDate = DateTime.UtcNow;
                userRole.EffectiveDate = userRole.EffectiveDate != null
                    ? (DateTime?) DateTime.UtcNow
                    : null;
                userRole.CreatedByUserId = createdById;
                userRole.LastModifiedByUserId = modifiedById;
                repUserRoles.Insert(userRole);
                userRole.LocalId = userRole.UserRoleId;
            }
        }

        private static void ProcessUserProfiles(ExportImportJob importJob, IDataContext db,
            IEnumerable<ExportUserProfile> userProfiles, int userId)
        {
            var repUserProfile = db.GetRepository<ExportUserProfile>();

            foreach (var userProfile in userProfiles)
            {
                var profileDefinitionId = Common.Util.GetProfilePropertyId(importJob.PortalId, userProfile.PropertyDefinitionId,
                    userProfile.PropertyName);
                if (profileDefinitionId == null) continue;

                userProfile.UserId = userId;
                userProfile.PropertyDefinitionId = profileDefinitionId.Value;
                userProfile.LastUpdatedDate = DateTime.UtcNow;
                repUserProfile.Insert(userProfile);
                userProfile.LocalId = userProfile.ProfileId;
            }
        }

        private static void ProcessUserAuthentications(IDataContext db,
            IEnumerable<ExportUserAuthentication> userAuthentications, int userId, int createdById, int modifiedById)
        {
            var repUserAuthentication = db.GetRepository<ExportUserAuthentication>();
            foreach (var userAuthentication in userAuthentications)
            {
                userAuthentication.UserId = userId;
                userAuthentication.CreatedOnDate = DateTime.UtcNow;
                userAuthentication.LastModifiedOnDate = DateTime.UtcNow;
                userAuthentication.CreatedByUserId = createdById;
                userAuthentication.LastModifiedByUserId = modifiedById;
                repUserAuthentication.Insert(userAuthentication);
                userAuthentication.LocalId = userAuthentication.UserAuthenticationId;
            }
        }

        private static void ProcessUserMembership(ExportAspnetUser aspNetUser, ExportAspnetMembership aspnetMembership)
        {
            using (var db =
                new PetaPocoDataContext(DotNetNuke.Data.DataProvider.Instance().Settings["connectionStringName"],
                    "aspnet_"))
            {
                var applicationId = db.ExecuteScalar<Guid>(CommandType.Text,
                    $"SELECT TOP 1 ApplicationId FROM aspnet_Applications");

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