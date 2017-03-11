using Dnn.ExportImport.Components.Entities;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace Dnn.ExportImport.Components.Common
{
    public static class Util
    {
        private const long Kb = 1024;
        private const long Mb = Kb * Kb;
        private const long Gb = Mb * Kb;

        public static string FormatSize(long bytes)
        {
            if (bytes < Kb) return bytes + " B";
            if (bytes < Mb) return (1.0 * bytes / Kb).ToString("F1") + " KB";
            if (bytes < Gb) return (1.0 * bytes / Mb).ToString("F1") + " MB";
            return (1.0 * bytes / Gb).ToString("F1") + " GB";
        }

        public static int GetUserIdOrName(ExportImportJob importJob, int? exportedUserId, string exportUsername)
        {
            if (!exportedUserId.HasValue || exportedUserId <= 0)
                return -1;

            if (exportedUserId == 1)
                return 1; // default HOST user

            if (string.IsNullOrEmpty(exportUsername))
                return -1;

            var user = UserController.GetUserByName(importJob.PortalId, exportUsername);
            return user.UserID < 0 ? importJob.CreatedByUserId : user.UserID;
        }

        public static int? GetRoleId(int portalId, string exportRolename)
        {
            var role = RoleController.Instance.GetRoleByName(portalId, exportRolename);
            return role?.RoleID;
        }

        public static int? GetProfilePropertyId(int portalId, int? exportedProfilePropertyId,
            string exportProfilePropertyname)
        {
            if (!exportedProfilePropertyId.HasValue || exportedProfilePropertyId <= 0)
                return -1;

            var property = ProfileController.GetPropertyDefinitionByName(portalId, exportProfilePropertyname);
            return property?.PropertyDefinitionId;
        }
    }
}