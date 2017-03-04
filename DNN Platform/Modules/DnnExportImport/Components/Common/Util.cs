using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Entities.Users;

namespace Dnn.ExportImport.Components.Common
{
    public static class Util
    {
        public static int GetUserIdOrName(ExportImportJob importJob, int? exportedUserId, string exportUsername)
        {
            if (!exportedUserId.HasValue)
                return -1;

            if (exportedUserId <= 0)
                return -1;

            if (exportedUserId == 1)
                return 1;

            var user = UserController.GetUserByName(importJob.PortalId, exportUsername);
            return user.UserID < 0 ? importJob.CreatedBy : user.UserID;
        }
    }
}