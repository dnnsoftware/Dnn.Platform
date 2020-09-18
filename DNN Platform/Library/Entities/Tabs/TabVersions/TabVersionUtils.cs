// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;

    /// <summary>
    /// This Class ofers a set of methods useful and reusable along the Tab Version API.
    /// </summary>
    internal static class TabVersionUtils
    {
        /// <summary>
        /// Try to get the version number from the current URL.
        /// </summary>
        /// <param name="versionInt">Version number obtained. Null Integer if it is not available.</param>
        /// <returns>True if version number is available and valid from URL. Otherwise, False.</returns>
        internal static bool TryGetUrlVersion(out int versionInt)
        {
            var version = GetTabVersionQueryStringValue();
            if (string.IsNullOrEmpty(version))
            {
                versionInt = Null.NullInteger;
                return false;
            }

            return int.TryParse(version, out versionInt);
        }

        /// <summary>
        /// Check if current user can see the current page.
        /// </summary>
        /// <returns>True if current user can see the current page. Otherwise, False.</returns>
        internal static bool CanSeeVersionedPages()
        {
            return CanSeeVersionedPages(TabController.CurrentPage);
        }

        /// <summary>
        /// Check if current user can see a specific page.
        /// </summary>
        /// <param name="tab"> The TabInfo to be checked.</param>
        /// <returns>True if current user can see the specific page. Otherwise, False.</returns>
        internal static bool CanSeeVersionedPages(TabInfo tab)
        {
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                return false;
            }

            var currentPortalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (currentPortalSettings == null)
            {
                return false;
            }

            var isAdminUser = currentPortalSettings.UserInfo.IsSuperUser || PortalSecurity.IsInRole(currentPortalSettings.AdministratorRoleName);
            if (isAdminUser)
            {
                return true;
            }

            return TabPermissionController.HasTabPermission(tab.TabPermissions, "EDIT,CONTENT,MANAGE");
        }

        private static string GetTabVersionQueryStringValue()
        {
            var currentPortal = PortalController.Instance.GetCurrentPortalSettings();
            return currentPortal == null ?
                string.Empty :
                HttpContext.Current.Request.QueryString[TabVersionSettings.Instance.GetTabVersionQueryStringParameter(currentPortal.PortalId)];
        }
    }
}
