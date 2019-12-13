// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Threading;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    /// <summary>
    /// This Class ofers a set of methods useful and reusable along the Tab Version API
    /// </summary>
    internal static class TabVersionUtils
    {
        #region Internal Methods
        
        /// <summary>
        /// Try to get the version number from the current URL
        /// </summary>
        /// <param name="versionInt">Version number obtained. Null Integer if it is not available</param>
        /// <returns>True if version number is available and valid from URL. Otherwise, False</returns>
        internal static bool TryGetUrlVersion(out int versionInt)
        {
            var version = GetTabVersionQueryStringValue();
            if (String.IsNullOrEmpty(version))
            {
                versionInt = Null.NullInteger;
                return false;
            }
            return int.TryParse(version, out versionInt);
        }

        /// <summary>
        /// Check if current user can see the current page
        /// </summary>
        /// <returns>True if current user can see the current page. Otherwise, False</returns>
        internal static bool CanSeeVersionedPages()
        {
            return CanSeeVersionedPages(TabController.CurrentPage);
        }

        /// <summary>
        /// Check if current user can see a specific page
        /// </summary>
        /// <param name="tab"> The TabInfo to be checked</param>
        /// <returns>True if current user can see the specific page. Otherwise, False</returns>
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
        #endregion

        #region Private Methods
        private static string GetTabVersionQueryStringValue()
        {
            var currentPortal = PortalController.Instance.GetCurrentPortalSettings();
            return currentPortal == null ? 
                string.Empty : 
                HttpContext.Current.Request.QueryString[TabVersionSettings.Instance.GetTabVersionQueryStringParameter(currentPortal.PortalId)];
        }
        #endregion
    }
}
