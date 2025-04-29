// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Common;

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
using DotNetNuke.Internal.SourceGenerators;
using Newtonsoft.Json;

/// <summary>A collection of utilities for import/export.</summary>
public static partial class Util
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Util));
    private static int noRole = Convert.ToInt32(Globals.glbRoleNothing);

    /// <summary>Checks if a string is either null or empty ("").</summary>
    /// <param name="s">The string to check.</param>
    /// <returns>A value indicating whether the string is null or empty.</returns>
    [DnnDeprecated(9, 8, 0, "Use string.IsNullOrEmpty from System.String instead")]
    public static partial bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

    /// <summary>Checks if a string is either null or contains only whitespace (" ").</summary>
    /// <param name="s">The string to check.</param>
    /// <returns>A value indicating whether the string is null or contains only whtespace.</returns>
    [DnnDeprecated(9, 8, 0, "Use string.IsNullOrWhiteSpace from System.String instead")]
    public static partial bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);

    /// <summary>Check if a given string is not null or empty (contains any value).</summary>
    /// <param name="s">The string to check.</param>
    /// <returns>A value indicating whether the string contains any value.</returns>
    [DnnDeprecated(9, 8, 0, "Use !string.IsNullOrEmpty from System.String instead")]
    public static partial bool HasValue(this string s) => !string.IsNullOrEmpty(s);

    /// <summary>Gets the types that implement BasePortableService.</summary>
    /// <returns>An enumeration of the types that implement BasePortableService.</returns>
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
                    type.FullName,
                    e.Message);
                portable2Type = null;
            }

            if (portable2Type != null)
            {
                yield return portable2Type;
            }
        }
    }

    /// <summary>Formats a size to a human readable format.</summary>
    /// <param name="bytes">The amount of bytes to represent.</param>
    /// <param name="decimals">How many decimal places to use in the resulting string.</param>
    /// <returns>A human readable size format, for instance 1024 would return 1 KB.</returns>
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

    /// <summary>Gets the export/import job cache key.</summary>
    /// <param name="job">The job to generate the key for.</param>
    /// <returns>A string representing the cacke key.</returns>
    public static string GetExpImpJobCacheKey(ExportImportJob job)
    {
        return string.Join(":", "ExpImpKey", job.PortalId.ToString(), job.JobId.ToString());
    }

    /// <summary>Get the id of a user for populating audit control values.</summary>
    /// <param name="importJob">A reference to the import job.</param>
    /// <param name="exportedUserId">The user id for the user that created the export.</param>
    /// <param name="exportUsername">The user name for the user that creted the export.</param>
    /// <returns>-1 if not found, 1 if the user is HOST, the user id if found on the imported site.</returns>
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

    /// <summary>Get the new id of a role in an export.</summary>
    /// <param name="portalId">The id of the portal (site).</param>
    /// <param name="exportRoleId">The id of the role in the export.</param>
    /// <param name="exportRolename">The name of the role in the export.</param>
    /// <returns>The found role id or null.</returns>
    public static int? GetRoleIdByName(int portalId, int exportRoleId, string exportRolename)
    {
        if (string.IsNullOrEmpty(exportRolename))
        {
            return null;
        }

        var roleId = DataProvider.Instance().GetRoleIdByName(exportRoleId >= 0 ? portalId : -1, exportRolename);
        return roleId == noRole ? null : (int?)roleId;
    }

    /// <summary>Gets a module definition id by it's friendly name.</summary>
    /// <param name="friendlyName">The module definition frienly name.</param>
    /// <returns>The found module definition id or null.</returns>
    public static int? GeModuleDefIdByFriendltName(string friendlyName)
    {
        if (string.IsNullOrEmpty(friendlyName))
        {
            return null;
        }

        var moduleDefInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName);
        return moduleDefInfo?.ModuleDefID;
    }

    /// <summary>Gets the id of a permission by it's permission name.</summary>
    /// <param name="permissionCode">The code of the permission.</param>
    /// <param name="permissionKey">The key of the permission.</param>
    /// <param name="permissionName">The name of the permission.</param>
    /// <returns>The id of the permission if found, null if not found.</returns>
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

    /// <summary>Gets the id of a profile property from an export.</summary>
    /// <param name="portalId">The id of the portal (site).</param>
    /// <param name="exportedProfilePropertyId">The exported profile property.</param>
    /// <param name="exportProfilePropertyname">The name of the exported profile property.</param>
    /// <returns>The id of the profile property or null if not found.</returns>
    public static int? GetProfilePropertyId(
        int portalId,
        int? exportedProfilePropertyId,
        string exportProfilePropertyname)
    {
        if (!exportedProfilePropertyId.HasValue || exportedProfilePropertyId <= 0)
        {
            return -1;
        }

        var property = ProfileController.GetPropertyDefinitionByName(portalId, exportProfilePropertyname);
        return property?.PropertyDefinitionId;
    }

    /// <summary>Calculates the total number of pages.</summary>
    /// <param name="totalRecords">The total amount of records.</param>
    /// <param name="pageSize">The number of items on a page.</param>
    /// <returns>A value indicating the total amount of pages required to containe the amount of items.</returns>
    public static int CalculateTotalPages(int totalRecords, int pageSize)
    {
        return totalRecords % pageSize == 0 ? totalRecords / pageSize : (totalRecords / pageSize) + 1;
    }

    /// <summary>Writes an item to a file as json.</summary>
    /// <typeparam name="T">The type of the item to write.</typeparam>
    /// <param name="filePath">The file to save to.</param>
    /// <param name="item">The item to write.</param>
    public static void WriteJson<T>(string filePath, T item)
    {
        var content = JsonConvert.SerializeObject(item);
        File.WriteAllText(filePath, content, Encoding.UTF8);
    }

    /// <summary>Reads an item from a json file.</summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="filePath">The file to read from.</param>
    /// <param name="item">The item to extract.</param>
    public static void ReadJson<T>(string filePath, ref T item)
    {
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);

            // TODO: This might throw error if file is corrupt. Should we handle error here?
            item = JsonConvert.DeserializeObject<T>(content);
        }
    }

    /// <summary>Replaces missing DateTime values to give them <see cref="Constants.MinDbTime"/> value.</summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="item">The item to fix.</param>
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

    /// <summary>Converts non local dates and times to the provided user local time.</summary>
    /// <param name="dateTime">The date to convert from.</param>
    /// <param name="userInfo">The user for whitch to adjust the provided date and item.</param>
    /// <returns>The adjusted date and time.</returns>
    public static DateTime ToLocalDateTime(DateTime dateTime, UserInfo userInfo)
    {
        if (dateTime.Kind != DateTimeKind.Local)
        {
            dateTime = new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond,
                DateTimeKind.Utc);
            return userInfo.LocalTime(dateTime);
        }

        return dateTime;
    }

    /// <summary>Converts a date and time to the user local time.</summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <param name="userInfo">The user for which to adjust the date and time for.</param>
    /// <returns>The provided date time adjusted for the user local time.</returns>
    public static DateTime? ToLocalDateTime(DateTime? dateTime, UserInfo userInfo)
    {
        if (dateTime != null && dateTime.Value.Kind != DateTimeKind.Local)
        {
            return userInfo.LocalTime(dateTime.Value);
        }

        return dateTime;
    }

    /// <summary>Convert the UTC time to Database local time.</summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <returns>The converted date and time.</returns>
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

    /// <summary>Convert the Local time to Database Utc time.</summary>
    /// <param name="dateTime">the date and time to convert.</param>
    /// <returns>The converted date and time.</returns>
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

    /// <summary>Gets a string representation of a date and time formatted for the current thread culture.</summary>
    /// <param name="dateTime">The date and time to convert.</param>
    /// <returns>A human readable representation of the date and time.</returns>
    public static string GetDateTimeString(DateTime? dateTime)
    {
        return dateTime?.ToString(Thread.CurrentThread.CurrentUICulture) ?? string.Empty;
    }

    /// <summary>Gets a string representation of a number formatted for the current thread culture.</summary>
    /// <param name="number">The number to format.</param>
    /// <returns>A string representing a number in the current thread culture format.</returns>
    public static string FormatNumber(int? number)
    {
        return number?.ToString("n0", Thread.CurrentThread.CurrentUICulture);
    }
}
