// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    public class TabPublishingController : ServiceLocator<ITabPublishingController, TabPublishingController>, ITabPublishingController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabPublishingController));
        private readonly IPermissionDefinitionService permissionDefinitionService;

        /// <summary>Initializes a new instance of the <see cref="TabPublishingController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public TabPublishingController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TabPublishingController"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public TabPublishingController(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        }

        /// <inheritdoc />
        public bool IsTabPublished(int tabID, int portalID)
        {
            var allUsersRoleId = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);
            var tab = TabController.Instance.GetTab(tabID, portalID);

            var existPermission = GetAlreadyPermission(this.permissionDefinitionService, tab, "VIEW", allUsersRoleId);
            return existPermission is { AllowAccess: true, };
        }

        /// <inheritdoc />
        public void SetTabPublishing(int tabID, int portalID, bool publish)
        {
            var tab = TabController.Instance.GetTab(tabID, portalID);
            if (!TabPermissionController.CanAdminPage(tab))
            {
                var errorMessage = Localization.GetExceptionMessage("PublishPagePermissionsNotMet", "Permissions are not met. The page has not been published.");
                var permissionsNotMetExc = new PermissionsNotMetException(tabID, errorMessage);
                Logger.Error(errorMessage, permissionsNotMetExc);
                throw permissionsNotMetExc;
            }

            if (publish)
            {
                PublishTabInternal(this.permissionDefinitionService, tab);
            }
            else
            {
                UnpublishTabInternal(tab);
            }
        }

        /// <inheritdoc />
        public bool CanPublishingBePerformed(int tabID, int portalID)
        {
            var tab = TabController.Instance.GetTab(tabID, portalID);
            if (!TabPermissionController.CanAdminPage(tab))
            {
                return false; // User has no permission
            }

            Hashtable settings = TabController.Instance.GetTabSettings(tabID);
            if (settings["WorkflowID"] != null)
            {
                return Convert.ToInt32(settings["WorkflowID"], CultureInfo.InvariantCulture) == 1; // If workflowID is 1, then the Page workflow is Direct Publish
            }

            // If workflowID is 1, then the Page workflow is Direct Publish
            // If WorkflowID is -1, then there is no Workflow setting
            var workflowId = Convert.ToInt32(PortalController.GetPortalSetting("WorkflowID", portalID, "-1"), CultureInfo.InvariantCulture);

            return workflowId is 1 or -1;
        }

        /// <inheritdoc />
        protected override Func<ITabPublishingController> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<ITabPublishingController>;
        }

        private static void PublishTabInternal(IPermissionDefinitionService permissionDefinitionService, TabInfo tab)
        {
            var allUsersRoleId = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);

            var existPermission = GetAlreadyPermission(permissionDefinitionService, tab, "VIEW", allUsersRoleId);
            if (existPermission != null)
            {
                tab.TabPermissions.Remove(existPermission);
            }

            tab.TabPermissions.Add(GetTabPermissionByRole(permissionDefinitionService, tab.TabID, "VIEW", allUsersRoleId));
            TabPermissionController.SaveTabPermissions(tab);
            ClearTabCache(tab);
        }

        private static void UnpublishTabInternal(TabInfo tab)
        {
            var administratorsRoleID = PortalController.Instance.GetPortal(tab.PortalID).AdministratorRoleId;
            var permissionsToRemove = new List<int>();
            permissionsToRemove.AddRange(tab.TabPermissions.Where(p => p.RoleID != administratorsRoleID).Select(p => p.TabPermissionID));
            foreach (var tabPermissionId in permissionsToRemove)
            {
                tab.TabPermissions.Remove(tab.TabPermissions.Cast<TabPermissionInfo>().SingleOrDefault(p => p.TabPermissionID == tabPermissionId));
            }

            TabPermissionController.SaveTabPermissions(tab);
            ClearTabCache(tab);
        }

        private static void ClearTabCache(TabInfo tabInfo)
        {
            TabController.Instance.ClearCache(tabInfo.PortalID);

            // Clear the Tab's Cached modules
            DataCache.ClearModuleCache(tabInfo.TabID);
        }

        private static TabPermissionInfo GetAlreadyPermission(IPermissionDefinitionService permissionDefinitionService, TabInfo tab, string permissionKey, int roleId)
        {
            var permission = permissionDefinitionService.GetDefinitionsByTab().Single(p => p.PermissionKey == permissionKey);
            return tab.TabPermissions.FirstOrDefault(tp => ((IPermissionInfo)tp).RoleId == roleId && ((IPermissionDefinitionInfo)tp).PermissionId == permission.PermissionId);
        }

        private static TabPermissionInfo GetTabPermissionByRole(IPermissionDefinitionService permissionDefinitionService, int tabID, string permissionKey, int roleID)
        {
            var permission = permissionDefinitionService.GetDefinitionsByTab().Single(p => p.PermissionKey == permissionKey);
            var tabPermission = new TabPermissionInfo
            {
                TabID = tabID,
                PermissionKey = permission.PermissionKey,
                PermissionName = permission.PermissionName,
                AllowAccess = true,
            };
            ((IPermissionInfo)tabPermission).PermissionId = permission.PermissionId;
            ((IPermissionInfo)tabPermission).RoleId = roleID;
            ((IPermissionInfo)tabPermission).UserId = Null.NullInteger;
            return tabPermission;
        }
    }
}
