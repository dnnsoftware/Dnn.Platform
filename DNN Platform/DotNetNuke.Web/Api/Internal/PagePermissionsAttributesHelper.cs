// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using System.Threading;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;

    /// <summary>A helper class for auth filter attributes that are based on page permissions.</summary>
    public class PagePermissionsAttributesHelper
    {
        /// <summary>Whether the current user has access to the <paramref name="permissionKey"/> for the current page.</summary>
        /// <param name="permissionKey">The permission key.</param>
        /// <returns><see langword="true"/> if the user has access, otherwise <see langword="false"/>.</returns>
        public static bool HasTabPermission(string permissionKey)
        {
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            var currentPortal = PortalController.Instance.GetCurrentSettings();
            var currentUser = UserController.Instance.GetCurrentUserInfo();

            if (currentUser.IsSuperUser || PortalSecurity.IsInRole(currentPortal.AdministratorRoleName))
            {
                return true;
            }

            return TabPermissionController.HasTabPermission(permissionKey);
        }
    }
}
