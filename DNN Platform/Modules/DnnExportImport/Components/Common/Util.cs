using System.Web.Profile;
using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

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

        public static int? GetRoleId(ExportImportJob importJob, int? exportedRoleId, string exportRolename)
        {
            if (!exportedRoleId.HasValue)
                return -1;

            if (exportedRoleId < -3)
                return -1;

            var role = RoleController.Instance.GetRoleByName(importJob.PortalId, exportRolename);
            return role?.RoleID;
        }

        public static int? GetProfilePropertyId(ExportImportJob importJob, int? exportedProfilePropertyId,
            string exportProfilePropertyname)
        {
            if (!exportedProfilePropertyId.HasValue)
                return -1;

            if (exportedProfilePropertyId <= 0)
                return -1;

            var property = ProfileController.GetPropertyDefinitionByName(importJob.PortalId, exportProfilePropertyname);
            return property?.PropertyDefinitionId;
        }
    }
}