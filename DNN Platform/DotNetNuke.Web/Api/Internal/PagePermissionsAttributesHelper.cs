// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using System.Threading;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;

    public class PagePermissionsAttributesHelper
    {
        public static bool HasTabPermission(string permissionKey)
        {
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            var currentPortal = PortalController.Instance.GetCurrentPortalSettings();

            bool isAdminUser = currentPortal.UserInfo.IsSuperUser || PortalSecurity.IsInRole(currentPortal.AdministratorRoleName);
            if (isAdminUser)
            {
                return true;
            }

            return TabPermissionController.HasTabPermission(permissionKey);
        }
    }
}
