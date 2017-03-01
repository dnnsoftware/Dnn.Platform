using System;
using System.Linq;
using Dnn.ExportImport.Components.Dto.Users;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.LiteDBRepository;
using DotNetNuke.Data;
using DotNetNuke.Common.Utilities;
using Dnn.ExportImport.Components.Entities;

namespace Dnn.ExportImport.Components.Services
{
    public class UsersExportService : IPortable2
    {
        public string Category => "USERS";

        public uint Priority => 3;

        public bool CanCancel => true;

        public bool CanRollback => false;

        public int ProgressPercentage => GetProgress();

        public void ExportData(ExportImportJob exportJob)
        {
            var portalId = exportJob.PortalId;
            var pageIndex = 0;
            var PageSize = 100;
            var totalProcessed = 0;
            try
            {
                //var dataReader = PetaPocoHelper.ExecuteReader(ConnectionString, CommandType.Text,
                //    $"Select U.* FROM {MakeTableName("Users")} U INNER JOIN {MakeTableName("UserPortals")} UP ON U.UserID=UP.UserId WHERE UP.PortalId={portalId}");
                var dataReader = DataProvider.Instance()
                .ExecuteReader("ExportImport_GetAllUsers", portalId, pageIndex, PageSize, false);
                var allUser =
                    CBO.FillCollection<Users>(dataReader).ToList();
                var firstOrDefault = allUser.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    var totalUsers = allUser.Any() ? firstOrDefault.Total : 0;
                    do
                    {
                        foreach (var user in allUser)
                        {
                            //PetaPocoHelper.ExecuteReader()
                            var aspnetUser =
                                CBO.FillObject<AspnetUsers>(DataProvider.Instance()
                                    .ExecuteReader("ExportImport_GetAspNetUser", user.Username));
                            var aspnetMembership = CBO.FillObject<AspnetMembership>(DataProvider.Instance()
                                .ExecuteReader("ExportImport_GetUserMembership", aspnetUser.UserId, aspnetUser.ApplicationId));
                            var userRoles = CBO.FillCollection<UserRoles>(DataProvider.Instance()
                                .ExecuteReader("ExportImport_GetUserRoles", portalId, user.UserId));
                            var userPortal = CBO.FillObject<UserPortals>(DataProvider.Instance()
                                .ExecuteReader("ExportImport_GetUserPortal", portalId, user.UserId));

                            LiteDbSingleCollectionRepository.Instance.CreateItem(aspnetUser, portalId);
                            LiteDbSingleCollectionRepository.Instance.CreateItem(aspnetMembership, portalId);

                            LiteDbSingleCollectionRepository.Instance.CreateItem(userPortal, portalId);
                            user.AspnetUsers = aspnetUser;
                            user.AspnetMembership = aspnetMembership;
                            user.UserPortals = userPortal;
                            LiteDbSingleCollectionRepository.Instance.CreateItem(user, portalId);
                            userRoles.ForEach(x => { x.ReferenceUserId = user.Id; });
                            LiteDbSingleCollectionRepository.Instance.CreateItems(userRoles, portalId);
                        }
                        totalProcessed += PageSize;
                        pageIndex++;
                        dataReader = DataProvider.Instance()
                            .ExecuteReader("ExportImport_GetAllUsers", portalId, pageIndex, PageSize, false);
                        allUser =
                            CBO.FillCollection<Users>(dataReader).ToList();
                    } while (totalProcessed < totalUsers);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void ImportData(ExportImportJob importJob)
        {
            var portalId = importJob.PortalId;
            var users = LiteDbSingleCollectionRepository.Instance.GetAllItems<Users>(new Users().CollectionName,
                portalId)
                .Include(x => x.AspnetUsers)
                .Include(x => x.AspnetMembership)
                .Include(x => x.UserPortals).FindAll().ToList();
            var userRoles = LiteDbSingleCollectionRepository.Instance.GetAllItems<UserRoles>(new UserRoles().CollectionName,
                portalId).FindAll().ToList();
            users.ForEach(x => { x.UserRoles = userRoles.Where(y => y.ReferenceUserId == x.Id); });
        }

        private int GetProgress()
        {
            return 20;
        }
    }
}
