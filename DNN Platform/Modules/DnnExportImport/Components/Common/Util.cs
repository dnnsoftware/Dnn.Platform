// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Components.Services;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using Newtonsoft.Json;

    public static class Util
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Util));
        private static int _noRole = Convert.ToInt32(Globals.glbRoleNothing);

        // some string extension helpers
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

        public static bool HasValue(this string s) => !string.IsNullOrEmpty(s);

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
                    Logger.ErrorFormat(
                        "Unable to create {0} while calling BasePortableService implementors. {1}",
                        type.FullName, e.Message);
                    portable2Type = null;
                }

                if (portable2Type != null)
                {
                    yield return portable2Type;
                }
            }
        }

        public static string FormatSize(long bytes, byte decimals = 1)
        {
            const long kb = 1024;
            const long mb = kb * kb;
            const long gb = mb * kb;

            if (bytes < kb)
            {
                return bytes + " B";
            }

            if (bytes < mb)
            {
                return (1.0 * bytes / kb).ToString("F" + decimals) + " KB";
            }

            if (bytes < gb)
            {
                return (1.0 * bytes / mb).ToString("F" + decimals) + " MB";
            }

            return (1.0 * bytes / gb).ToString("F" + decimals) + " GB";
        }

        public static string GetExpImpJobCacheKey(ExportImportJob job)
        {
            return string.Join(":", "ExpImpKey", job.PortalId.ToString(), job.JobId.ToString());
        }

        public static int GetUserIdByName(ExportImportJob importJob, int? exportedUserId, string exportUsername)
        {
            if (!exportedUserId.HasValue || exportedUserId <= 0)
            {
                return -1;
            }

            if (exportedUserId == 1)
            {
                return 1; // default HOST user
            }

            if (string.IsNullOrEmpty(exportUsername))
            {
                return -1;
            }

            var user = UserController.GetUserByName(importJob.PortalId, exportUsername);
            if (user == null)
            {
                return -1;
            }

            return user.UserID < 0 ? importJob.CreatedByUserId : user.UserID;
        }

        public static int? GetRoleIdByName(int portalId, int exportRoleId, string exportRolename)
        {
            if (string.IsNullOrEmpty(exportRolename))
            {
                return null;
            }

            var roleId = DataProvider.Instance().GetRoleIdByName(exportRoleId >= 0 ? portalId : -1, exportRolename);
            return roleId == _noRole ? null : (int?)roleId;
        }

        public static int? GeModuleDefIdByFriendltName(string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName))
            {
                return null;
            }

            var moduleDefInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName);
            return moduleDefInfo?.ModuleDefID;
        }

        public static int? GePermissionIdByName(string permissionCode, string permissionKey, string permissionName)
        {
            if (string.IsNullOrEmpty(permissionCode) ||
                string.IsNullOrEmpty(permissionKey) ||
                string.IsNullOrEmpty(permissionName))
            {
                return null;
            }

            var permission = EntitiesController.Instance.GetPermissionInfo(permissionCode, permissionKey, permissionName);
            return permission?.PermissionID;
        }

        public static int? GetProfilePropertyId(int portalId, int? exportedProfilePropertyId,
            string exportProfilePropertyname)
        {
            if (!exportedProfilePropertyId.HasValue || exportedProfilePropertyId <= 0)
            {
                return -1;
            }

            var property = ProfileController.GetPropertyDefinitionByName(portalId, exportProfilePropertyname);
            return property?.PropertyDefinitionId;
        }

        public static int CalculateTotalPages(int totalRecords, int pageSize)
        {
            return totalRecords % pageSize == 0 ? totalRecords / pageSize : (totalRecords / pageSize) + 1;
        }

        public static void WriteJson<T>(string filePath, T item)
        {
            var content = JsonConvert.SerializeObject(item);
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        public static void ReadJson<T>(string filePath, ref T item)
        {
            if (File.Exists(filePath))
            {
                var content = File.ReadAllText(filePath);

                // TODO: This might throw error if file is corrupt. Should we handle error here?
                item = JsonConvert.DeserializeObject<T>(content);
            }
        }

        public static void FixDateTime<T>(T item)
        {
            var properties = item.GetType().GetRuntimeProperties();
            foreach (var property in properties)
            {
                if ((property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?)) &&
                    (property.GetValue(item) as DateTime?) == DateTime.MinValue)
                {
                    property.SetValue(item, Constants.MinDbTime);
                }
            }
        }

        public static DateTime ToLocalDateTime(DateTime dateTime, UserInfo userInfo)
        {
            if (dateTime.Kind != DateTimeKind.Local)
            {
                dateTime = new DateTime(
                    dateTime.Year, dateTime.Month, dateTime.Day,
                    dateTime.Hour, dateTime.Minute, dateTime.Second,
                    dateTime.Millisecond, DateTimeKind.Utc);
                return userInfo.LocalTime(dateTime);
            }

            return dateTime;
        }

        public static DateTime? ToLocalDateTime(DateTime? dateTime, UserInfo userInfo)
        {
            if (dateTime != null && dateTime.Value.Kind != DateTimeKind.Local)
            {
                return userInfo.LocalTime(dateTime.Value);
            }

            return dateTime;
        }

        /// <summary>
        /// Convert the UTC time to Database local time.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToDbLocalTime(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }

            if (dateTime.Value.Kind != DateTimeKind.Utc)
            {
                return dateTime;
            }

            var differenceInUtcTimes =
                TimeZone.CurrentTimeZone.GetUtcOffset(DateUtils.GetDatabaseUtcTime()).TotalMilliseconds;
            var d = dateTime.Value.ToLocalTime().AddMilliseconds(differenceInUtcTimes);
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond, DateTimeKind.Local);
        }

        /// <summary>
        /// Convert the Local time to Database Utc time.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToDbUtcTime(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }

            if (dateTime.Value.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }

            var differenceInUtcTimes =
                TimeZone.CurrentTimeZone.GetUtcOffset(DateUtils.GetDatabaseUtcTime()).TotalMilliseconds;
            var d = dateTime.Value.ToUniversalTime().AddMilliseconds(differenceInUtcTimes);
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond, DateTimeKind.Utc);
        }

        public static string GetDateTimeString(DateTime? dateTime)
        {
            return dateTime?.ToString(Thread.CurrentThread.CurrentUICulture) ?? string.Empty;
        }

        public static string FormatNumber(int? number)
        {
            return number?.ToString("n0", Thread.CurrentThread.CurrentUICulture);
        }
    }
}
