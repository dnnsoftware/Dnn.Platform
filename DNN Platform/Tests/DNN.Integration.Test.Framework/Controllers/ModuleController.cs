// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using DNN.Integration.Test.Framework.Helpers;
    using DNN.Integration.Test.Framework.Scripts;

    public static class ModuleController
    {
        private const string PortalIdMarker = @"'$[portal_id]'";
        private const string RoleIdMarker = @"'$[role_id]'";
        private const string ModuleIdMarker = @"'$[module_id]'";
        private const string PermissionCodeMarker = @"$[permission_code]";
        private const string PermissionKeyMarker = @"$[permission_key]";
        private const string FriendlyNameMarker = @"$[friendly_name]";

        [Obsolete("typo in name, use AddModulePermission instead. Scheduled removal in v10.0.0.")]
        public static int AddModuleOPermission(int roleId, int moduleId, string permissionCode, string permissionKey, int portalId = 0)
        {
            return AddModulePermission(roleId, moduleId, permissionCode, permissionKey, portalId);
        }

        public static int AddModulePermission(int roleId, int moduleId, string permissionCode, string permissionKey, int portalId = 0)
        {
            var fileContent = SqlScripts.AddModulePermission;
            var masterScript = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            // make sure to create at least one SU (see comments at end of files below)
            // the create super-users is proportional to number of total users
            var script = new StringBuilder(masterScript)
                .Replace(RoleIdMarker, roleId.ToString(CultureInfo.InvariantCulture))
                .Replace(ModuleIdMarker, moduleId.ToString(CultureInfo.InvariantCulture))
                .Replace(PermissionCodeMarker, permissionCode.Replace("'", "''"))
                .Replace(PermissionKeyMarker, permissionKey.Replace("'", "''"))
                .ToString();

            var result = DatabaseHelper.ExecuteQuery(script);

            if (result.Count > 0 && result[0].ContainsKey("ModulePermissionId"))
            {
                return int.Parse(result[0]["ModulePermissionId"].ToString());
            }

            return -1;
        }

        public static void DeleteModulePermission(int modulePermissionId)
        {
            DatabaseHelper.ExecuteStoredProcedure("DeleteModulePermission", modulePermissionId);
        }

        /// <summary>
        /// Deletes a module from the specified tab and clears host cache.
        /// </summary>
        /// <param name="tabId">tabId on which module is deleted.</param>
        /// <param name="moduleId">moduleId that is deleted.</param>
        /// <param name="softDelete">if True, then softdeleted, otherwise harddeleted.</param>
        public static void DeleteTabModule(int tabId, int moduleId, bool softDelete)
        {
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabModule", tabId, moduleId, softDelete);
            WebApiTestHelper.ClearHostCache();
        }

        public static string GetModuleSettingValue(int moduleId, string settingName)
        {
            var results = DatabaseHelper.ExecuteStoredProcedure("GetModuleSetting", moduleId, settingName);

            if (results.Count > 0 && results[0].ContainsKey("SettingValue"))
            {
                return results[0]["SettingValue"].ToString();
            }

            return null;
        }

        public static void SetModuleSettingValue(int moduleId, string settingName, string settingValue)
        {
            DatabaseHelper.ExecuteStoredProcedure("DeleteModuleSetting", moduleId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("UpdateModuleSetting", moduleId, settingName, settingValue, -1);
        }

        public static IList<IDictionary<string, object>> GetModulesByFriendlyName(int portalId, string friendlyName)
        {
            var fileContent = SqlScripts.Modulenformation;
            var masterScript = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var script = new StringBuilder(masterScript)
                .Replace(FriendlyNameMarker, friendlyName)
                .ToString();

            var result = DatabaseHelper.ExecuteQuery(script);
            return result;
        }

        public static void SetTabModuleSettingValue(int tabModuleId, string settingName, string settingValue)
        {
            DatabaseHelper.ExecuteStoredProcedure("DeleteTabModuleSetting", tabModuleId, settingName);
            DatabaseHelper.ExecuteStoredProcedure("UpdateTabModuleSetting", tabModuleId, settingName, settingValue, -1);
        }
    }
}
