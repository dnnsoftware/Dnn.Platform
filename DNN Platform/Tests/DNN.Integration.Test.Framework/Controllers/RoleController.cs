// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DNN.Integration.Test.Framework.Helpers;
    using DNN.Integration.Test.Framework.Scripts;

    public static class RoleController
    {
        private const string PortalIdMarker = @"'$[portal_id]'";
        private const string RoleNameMarker = @"$[role_name]";
        private const string RoleDescriptionMarker = @"$[role_description]";
        private const string RoleIdMarker = @"'$[role_id]'";
        private const string UserIdMarker = @"'$[user_id]'";

        public static int CreateBasicRole(string roleName, string roleDescription = "")
        {
            return CreateRole(roleName, roleDescription);
        }

        public static int CreateRoleIfNotPresent(string roleName, int portalId = 0)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var roleId = GetRoleId(roleName, portalId);
                return roleId < 0 ? CreateRole(roleName, roleName, portalId) : roleId;
            }

            return -1;
        }

        /// <summary>
        /// Get RoleId for role "Registered Users".
        /// </summary>
        /// <returns></returns>
        public static int GetRegisteredUsersRoleId(int portalId = 0)
        {
            return GetRoleId("Registered Users", portalId);
        }

        /// <summary>
        /// Get RoleId for role "Administrators".
        /// </summary>
        /// <returns></returns>
        public static int GetAdministratorsRoleId(int portalId = 0)
        {
            return GetRoleId("Administrators", portalId);
        }

        public static int GetRoleId(string roleName, int portalId = 0)
        {
            // The fix for DNN-4288 prevented virtual roles from getting virtual
            // roles (i.e., with RoleID <= 0 which includes Administrator role).
            // var results = DatabaseHelper.ExecuteStoredProcedure("GetRolesBasicSearch", portalId, 0, 10, roleName);
            roleName = roleName.Replace("'", string.Empty);
            var query = string.Format("SELECT RoleID FROM {{objectQualifier}}Roles WHERE RoleName = N'{0}' AND PortalID={1};", roleName, portalId);
            var results = DatabaseHelper.ExecuteQuery(query).ToArray();

            if (!results.Any())
            {
                return -1;
            }

            // we could have more than a single role with same name when we have more than one portal
            return results.SelectMany(x => x.Where(y => y.Key == "RoleID").Select(y => (int)y.Value)).FirstOrDefault();
        }

        public static void DeleteRole(int roleId)
        {
            DatabaseHelper.ExecuteStoredProcedure("DeleteRole", roleId);
        }

        public static void AssignRoleToUser(int roleId, int userId)
        {
            var fileContent = SqlScripts.AssignRoleToUser;
            var masterScript = new StringBuilder(fileContent)
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var script = new StringBuilder(masterScript)
                .Replace(RoleIdMarker, roleId.ToString(CultureInfo.InvariantCulture))
                .Replace(UserIdMarker, userId.ToString(CultureInfo.InvariantCulture));

            DatabaseHelper.ExecuteQuery(script.ToString());
        }

        private static int CreateRole(string roleName, string roleDescription, int portalId = 0)
        {
            var fileContent = SqlScripts.SingleRoleCreation;
            var masterScript = new StringBuilder(fileContent)
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var script = new StringBuilder(masterScript)
                .Replace(RoleNameMarker, roleName.Replace("'", "''"))
                .Replace(RoleDescriptionMarker, roleDescription.Replace("'", "''"));

            DatabaseHelper.ExecuteQuery(script.ToString());
            WebApiTestHelper.ClearHostCache();
            return GetRoleId(roleName);
        }
    }
}
