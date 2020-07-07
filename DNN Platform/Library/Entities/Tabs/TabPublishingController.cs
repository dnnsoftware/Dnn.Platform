// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    public class TabPublishingController : ServiceLocator<ITabPublishingController, TabPublishingController>, ITabPublishingController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabPublishingController));

        public bool IsTabPublished(int tabID, int portalID)
        {
            var allUsersRoleId = int.Parse(Globals.glbRoleAllUsers);
            var tab = TabController.Instance.GetTab(tabID, portalID);

            var existPermission = this.GetAlreadyPermission(tab, "VIEW", allUsersRoleId);
            return existPermission != null && existPermission.AllowAccess;
        }

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
                this.PublishTabInternal(tab);
            }
            else
            {
                this.UnpublishTabInternal(tab);
            }
        }

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
                return Convert.ToInt32(settings["WorkflowID"]) == 1; // If workflowID is 1, then the Page workflow is Direct Publish
            }

            // If workflowID is 1, then the Page workflow is Direct Publish
            // If WorkflowID is -1, then there is no Workflow setting
            var workflowID = Convert.ToInt32(PortalController.GetPortalSetting("WorkflowID", portalID, "-1"));

            return (workflowID == 1) || (workflowID == -1);
        }

        protected override Func<ITabPublishingController> GetFactory()
        {
            return () => new TabPublishingController();
        }

        private void PublishTabInternal(TabInfo tab)
        {
            var allUsersRoleId = int.Parse(Globals.glbRoleAllUsers);

            var existPermission = this.GetAlreadyPermission(tab, "VIEW", allUsersRoleId);
            if (existPermission != null)
            {
                tab.TabPermissions.Remove(existPermission);
            }

            tab.TabPermissions.Add(this.GetTabPermissionByRole(tab.TabID, "VIEW", allUsersRoleId));
            TabPermissionController.SaveTabPermissions(tab);
            this.ClearTabCache(tab);
        }

        private void UnpublishTabInternal(TabInfo tab)
        {
            var administratorsRoleID = PortalController.Instance.GetPortal(tab.PortalID).AdministratorRoleId;
            var permissionsToRemove = new List<int>();
            permissionsToRemove.AddRange(tab.TabPermissions.Where(p => p.RoleID != administratorsRoleID).Select(p => p.TabPermissionID));
            foreach (var tabPermissionId in permissionsToRemove)
            {
                tab.TabPermissions.Remove(tab.TabPermissions.Cast<TabPermissionInfo>().SingleOrDefault(p => p.TabPermissionID == tabPermissionId));
            }

            TabPermissionController.SaveTabPermissions(tab);
            this.ClearTabCache(tab);
        }

        private void ClearTabCache(TabInfo tabInfo)
        {
            TabController.Instance.ClearCache(tabInfo.PortalID);

            // Clear the Tab's Cached modules
            DataCache.ClearModuleCache(tabInfo.TabID);
        }

        private TabPermissionInfo GetAlreadyPermission(TabInfo tab, string permissionKey, int roleId)
        {
            var permission = PermissionController.GetPermissionsByTab().Cast<PermissionInfo>().SingleOrDefault<PermissionInfo>(p => p.PermissionKey == permissionKey);

            return
                tab.TabPermissions.Cast<TabPermissionInfo>()
                    .FirstOrDefault(tp => tp.RoleID == roleId && tp.PermissionID == permission.PermissionID);
        }

        private TabPermissionInfo GetTabPermissionByRole(int tabID, string permissionKey, int roleID)
        {
            var permission = PermissionController.GetPermissionsByTab().Cast<PermissionInfo>().SingleOrDefault<PermissionInfo>(p => p.PermissionKey == permissionKey);
            var tabPermission = new TabPermissionInfo
            {
                TabID = tabID,
                PermissionID = permission.PermissionID,
                PermissionKey = permission.PermissionKey,
                PermissionName = permission.PermissionName,
                RoleID = roleID,
                UserID = Null.NullInteger,
                AllowAccess = true,
            };
            return tabPermission;
        }
    }
}
