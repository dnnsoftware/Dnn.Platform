// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components
{
    using System;
    using System.Linq;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Library.Repository;
    using Dnn.PersonaBar.UI.Components.Controllers;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Installers;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    public class BusinessController : IUpgradeable
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(BusinessController));
        private static readonly DotNetNuke.Services.Installer.Log.Logger InstallLogger = new DotNetNuke.Services.Installer.Log.Logger();

        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.00.00":
                    this.UpdateControlPanel();
                    this.CreateAdminLinks();
                    break;
                case "01.04.00":
                    this.UpdateEditPermissions();
                    break;
                case "03.00.00":
                    this.RemovePersonaBarOldAssemblies();
                    break;
            }

            return "Success";
        }

        private static void RemoveAssembly(string assemblyName)
        {
            Logger.InstallLogInfo(string.Concat(Localization.GetString("LogStart", Localization.GlobalResourceFile), "Removal of assembly:", assemblyName));

            var packageInfo = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                p.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)
                && p.PackageType.Equals("PersonaBar", StringComparison.OrdinalIgnoreCase));
            if (packageInfo != null)
            {
                var fileName = assemblyName + ".dll";
                if (DataProvider.Instance().UnRegisterAssembly(packageInfo.PackageID, fileName))
                {
                    Logger.InstallLogInfo(Util.ASSEMBLY_UnRegistered + " - " + fileName);
                }
            }
            else
            {
                Logger.InstallLogInfo(Util.ASSEMBLY_InUse + " - " + assemblyName);
            }
        }

        private void CreateAdminLinks()
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                this.CreatePageLinks(portal.PortalID, "Admin");
            }

            this.CreatePageLinks(Null.NullInteger, "Host");
        }

        private void CreatePageLinks(int portalId, string parentPath)
        {
            var parentTab = TabController.GetTabByTabPath(portalId, "//" + parentPath, string.Empty);
            if (parentTab == Null.NullInteger)
            {
                return;
            }

            var adminTabs = TabController.GetTabsByParent(parentTab, portalId);
            foreach (var tab in adminTabs)
            {
                AdminMenuController.Instance.CreateLinkMenu(tab);
            }
        }

        private void UpdateControlPanel()
        {
            HostController.Instance.Update("ControlPanel", "DesktopModules/admin/Dnn.PersonaBar/UserControls/PersonaBarContainer.ascx");
        }

        private void UpdateEditPermissions()
        {
            var menuItems = PersonaBarRepository.Instance.GetMenu().AllItems;
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                var portalId = portal.PortalID;
                if (MenuPermissionController.PermissionAlreadyInitialized(portalId))
                {
                    menuItems.ForEach(i => this.SaveEditPermission(portalId, i));
                }
            }
        }

        private void SaveEditPermission(int portalId, MenuItem menuItem)
        {
            var viewPermission = MenuPermissionController.GetPermissions(menuItem.MenuId).FirstOrDefault(p => p.PermissionKey == "VIEW");
            var editPermission = MenuPermissionController.GetPermissions(menuItem.MenuId).FirstOrDefault(p => p.PermissionKey == "EDIT");

            if (viewPermission == null || editPermission == null)
            {
                return;
            }

            var permissions = MenuPermissionController.GetMenuPermissions(portalId, menuItem.Identifier).ToList();
            permissions.ForEach(p =>
            {
                if (p.PermissionID == viewPermission.PermissionId)
                {
                    if (!permissions.Any(c => c.PermissionID == editPermission.PermissionId && c.RoleID == p.RoleID && c.UserID == p.UserID))
                    {
                        var menuPermissionInfo = new MenuPermissionInfo
                        {
                            MenuPermissionId = Null.NullInteger,
                            MenuId = menuItem.MenuId,
                            PermissionID = editPermission.PermissionId,
                            RoleID = p.RoleID,
                            UserID = p.UserID,
                            AllowAccess = p.AllowAccess,
                        };

                        MenuPermissionController.SaveMenuPermissions(portalId, menuItem, menuPermissionInfo);
                    }
                }
            });
        }

        private void RemovePersonaBarOldAssemblies()
        {
            string[] assemblies =
            {
                "Dnn.PersonaBar.AdminLogs",
                "Dnn.PersonaBar.ConfigConsole",
                "Dnn.PersonaBar.Connectors",
                "Dnn.PersonaBar.CssEditor",
                "Dnn.PersonaBar.Licensing",
                "Dnn.PersonaBar.Pages",
                "Dnn.PersonaBar.Prompt",
                "Dnn.PersonaBar.Recyclebin",
                "Dnn.PersonaBar.Roles",
                "Dnn.PersonaBar.Security",
                "Dnn.PersonaBar.Seo",
                "Dnn.PersonaBar.Servers",
                "Dnn.PersonaBar.SiteImportExport",
                "Dnn.PersonaBar.Sites",
                "Dnn.PersonaBar.SiteSettings",
                "Dnn.PersonaBar.SqlConsole",
                "Dnn.PersonaBar.TaskScheduler",
                "Dnn.PersonaBar.Themes",
                "Dnn.PersonaBar.Users",
                "Dnn.PersonaBar.Vocabularies",
            };

            foreach (string assemblyName in assemblies)
            {
                RemoveAssembly(assemblyName);
            }
        }
    }
}
