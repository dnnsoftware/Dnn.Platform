// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components
{
    using System;
    using System.Linq;

    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Library.Repository;
    using Dnn.PersonaBar.UI.Components.Controllers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides upgrade logic for the Persona Bar.</summary>
    public class BusinessController : IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BusinessController));
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="BusinessController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettingsService. Scheduled removal in v12.0.0.")]
        public BusinessController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BusinessController"/> class.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        public BusinessController(IHostSettingsService hostSettingsService)
        {
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
        }

        /// <inheritdoc/>
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.00.00":
                    this.UpdateControlPanel();
                    CreateAdminLinks();
                    break;
                case "01.04.00":
                    UpdateEditPermissions();
                    break;
                case "03.00.00":
                    RemovePersonaBarOldAssemblies();
                    break;
            }

            return "Success";
        }

        private static void RemoveAssembly(string assemblyName)
        {
            Logger.Info(string.Concat(Localization.GetString("LogStart", Localization.GlobalResourceFile), "Removal of assembly:", assemblyName));

            var packageInfo = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                p.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase)
                && p.PackageType.Equals("PersonaBar", StringComparison.OrdinalIgnoreCase));
            if (packageInfo != null)
            {
                var fileName = assemblyName + ".dll";
                if (DataProvider.Instance().UnRegisterAssembly(packageInfo.PackageID, fileName))
                {
                    Logger.Info($"{Util.ASSEMBLY_UnRegistered} - {fileName}");
                }
            }
            else
            {
                Logger.Info($"{Util.ASSEMBLY_InUse} - {assemblyName}");
            }
        }

        private static void CreateAdminLinks()
        {
            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
                CreatePageLinks(portal.PortalId, "Admin");
            }

            CreatePageLinks(Null.NullInteger, "Host");
        }

        private static void CreatePageLinks(int portalId, string parentPath)
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

        private static void UpdateEditPermissions()
        {
            var menuItems = PersonaBarRepository.Instance.GetMenu().AllItems;
            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
                var portalId = portal.PortalId;
                if (MenuPermissionController.PermissionAlreadyInitialized(portalId))
                {
                    menuItems.ForEach(i => SaveEditPermission(portalId, i));
                }
            }
        }

        private static void SaveEditPermission(int portalId, MenuItem menuItem)
        {
            var viewPermission = MenuPermissionController.GetPermissions(menuItem.MenuId).FirstOrDefault(p => p.PermissionKey == "VIEW");
            var editPermission = MenuPermissionController.GetPermissions(menuItem.MenuId).FirstOrDefault(p => p.PermissionKey == "EDIT");

            if (viewPermission == null || editPermission == null)
            {
                return;
            }

            var permissions = MenuPermissionController.GetMenuPermissions(portalId, menuItem.Identifier).ToList();
            permissions.ForEach((IPermissionInfo p) =>
            {
                if (p.PermissionId != viewPermission.PermissionId)
                {
                    return;
                }

                if (permissions.Any((IPermissionInfo c) => c.PermissionId == editPermission.PermissionId && c.RoleId == p.RoleId && c.UserId == p.UserId))
                {
                    return;
                }

                var menuPermissionInfo = new MenuPermissionInfo
                {
                    MenuPermissionId = Null.NullInteger,
                    MenuId = menuItem.MenuId,
                    AllowAccess = p.AllowAccess,
                };

                ((IPermissionInfo)menuPermissionInfo).PermissionId = editPermission.PermissionId;
                ((IPermissionInfo)menuPermissionInfo).RoleId = p.RoleId;
                ((IPermissionInfo)menuPermissionInfo).UserId = p.UserId;

                MenuPermissionController.SaveMenuPermissions(portalId, menuItem, menuPermissionInfo);
            });
        }

        private static void RemovePersonaBarOldAssemblies()
        {
            string[] oldPersonaBarAssemblies =
            [
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
                "Dnn.PersonaBar.Vocabularies"
            ];

            foreach (var assemblyName in oldPersonaBarAssemblies)
            {
                RemoveAssembly(assemblyName);
            }
        }

        private void UpdateControlPanel()
        {
            this.hostSettingsService.Update("ControlPanel", "DesktopModules/admin/Dnn.PersonaBar/UserControls/PersonaBarContainer.ascx");
        }
    }
}
