using System;
using System.Collections.Generic;
using System.Reflection;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Services;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;

namespace Dnn.ExportImport.Components.Common
{
    public static class Util
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Util));

        private const long Kb = 1024;
        private const long Mb = Kb * Kb;
        private const long Gb = Mb * Kb;

        public static IEnumerable<BasePortableService> GetPortableImplementors()
        {
            var typeLocator = new TypeLocator();
            var types = typeLocator.GetAllMatchingTypes(
                t => t != null && t.IsClass && !t.IsAbstract && t.IsVisible &&
                     typeof(BasePortableService).IsAssignableFrom(t));

            foreach (var type in types)
            {
                BasePortableService portable2Type;
                try
                {
                    portable2Type = Activator.CreateInstance(type) as BasePortableService;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to create {0} while calling BasePortableService implementors. {1}",
                        type.FullName, e.Message);
                    portable2Type = null;
                }

                if (portable2Type != null)
                {
                    yield return portable2Type;
                }
            }
        }

        public static string FormatSize(long bytes)
        {
            if (bytes < Kb) return bytes + " B";
            if (bytes < Mb) return (1.0 * bytes / Kb).ToString("F1") + " KB";
            if (bytes < Gb) return (1.0 * bytes / Mb).ToString("F1") + " MB";
            return (1.0 * bytes / Gb).ToString("F1") + " GB";
        }

        public static string GetExpImpJobCacheKey(ExportImportJob job)
        {
            return string.Join(":", "ExpImpKey", job.PortalId.ToString(), job.JobId.ToString());
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

        public static int CalculateTotalPages(int totalRecords, int pageSize)
        {
            return totalRecords % pageSize == 0 ? totalRecords / pageSize : totalRecords / pageSize + 1;
        }

        //TODO: We should implement some base serializer to fix dates for all the entities.
        public static void FixDateTime<T>(T item)
        {
            var properties = item.GetType().GetRuntimeProperties();
            foreach (var property in properties)
            {
                if ((property.PropertyType == typeof (DateTime) || property.PropertyType == typeof(DateTime?)) &&
                    (property.GetValue(item) as DateTime?) == DateTime.MinValue)
                {
                    property.SetValue(item, FromEpoch());
                }
            }
        }

        private static DateTime FromEpoch()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch;
        }
    }
}