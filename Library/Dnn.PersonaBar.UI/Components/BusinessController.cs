#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.UI.Components
{
    public class BusinessController : IUpgradeable
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(BusinessController));

        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.00.00":
                    UpdateControlPanel();
                    CreateAdminLinks();
                    break;
                case "01.04.00":
                    UpdateEditPermissions();
                    break;
                case "03.00.00":
                    UninstallPersonaBarExtensions();
                    break;
            }

            return "Success";
        }

        private void CreateAdminLinks()
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                CreatePageLinks(portal.PortalID, "Admin");
            }

            CreatePageLinks(Null.NullInteger, "Host");
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
                    menuItems.ForEach(i => SaveEditPermission(portalId, i));
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
                            AllowAccess = p.AllowAccess
                        };

                        MenuPermissionController.SaveMenuPermissions(portalId, menuItem, menuPermissionInfo);
                    }
                }
            });
        }

        private void UninstallPersonaBarExtensions()
        {
            UninstallPackage("Dnn.PersonaBar.AdminLogs", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.ConfigConsole", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Connectors", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.CssEditor", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Licensing", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Pages", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Prompt", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Recyclebin", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Roles", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Security", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Seo", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Servers", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.SiteImportExport", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Sites", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.SiteSettings", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.SqlConsole", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.TaskScheduler", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Themes", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Users", "PersonaBar");
            UninstallPackage("Dnn.PersonaBar.Vocabularies", "PersonaBar");
        }

        private static void UninstallPackage(string packageName, string packageType, bool deleteFiles = true, string version = "")
        {
            Logger.InstallLogInfo(string.Concat(Localization.GetString("LogStart", Localization.GlobalResourceFile), "Uninstallation of Package:", packageName, " Type:", packageType, " Version:", version));

            var searchInput = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase)
                && p.PackageType.Equals(packageType, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrEmpty(version) || p.Version.ToString() == version));
            if (searchInput != null)
            {
                var searchInputInstaller = new Installer(searchInput, Globals.ApplicationMapPath);
                searchInputInstaller.UnInstall(deleteFiles);
            }
        }
    }
}
