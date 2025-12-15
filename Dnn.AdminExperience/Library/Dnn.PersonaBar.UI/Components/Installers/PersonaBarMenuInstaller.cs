// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.Installers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.XPath;

    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Library.Repository;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Installers;

    /// <summary>Installer for persona bar menus.</summary>
    public class PersonaBarMenuInstaller : ComponentInstallerBase
    {
        private readonly List<MenuItem> menuItems = [];
        private readonly List<PersonaBarExtension> extensions = [];
        private readonly List<PermissionDefinition> permissionDefinitions = [];
        private readonly Dictionary<string, string> extensionMenus = new Dictionary<string, string>();
        private readonly Dictionary<string, string> menuRoles = new Dictionary<string, string>();
        private readonly Dictionary<string, string> parentMaps = new Dictionary<string, string>();

        /// <inheritdoc/>
        public override void Commit()
        {
        }

        /// <inheritdoc/>
        public override void Install()
        {
            try
            {
                this.SaveMenuItems();

                this.SaveMenuExtensions();

                this.SavePermissionDefinitions();

                if (this.menuItems.Count != 0)
                {
                    foreach (var menuItem in this.menuItems)
                    {
                        this.SaveMenuPermissions(menuItem);
                    }
                }

                this.Completed = true;
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// <inheritdoc/>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            foreach (XPathNavigator navigator in manifestNav.Select("menu"))
            {
                this.ReadMenuItemsFromManifest(navigator);
            }

            foreach (XPathNavigator navigator in manifestNav.Select("extension"))
            {
                this.ReadExtensionsFromManifest(navigator);
            }

            foreach (XPathNavigator navigator in manifestNav.Select("permission"))
            {
                this.ReadPermissionsFromManifest(navigator);
            }
        }

        /// <inheritdoc/>
        public override void Rollback()
        {
            this.DeleteMenus();
        }

        /// <inheritdoc/>
        public override void UnInstall()
        {
            this.DeleteMenus();
        }

        private static void SaveMenuPermission(MenuItem menuItem, string roleName)
        {
            var portals = PortalController.Instance.GetPortals();
            foreach (IPortalInfo portal in portals)
            {
                var portalId = portal.PortalId;

                // when default permission already initialized, then package need to save default permission immediately.
                if (MenuPermissionController.PermissionAlreadyInitialized(portalId))
                {
                    MenuPermissionController.SaveMenuDefaultPermissions(portalId, menuItem, roleName);
                }
            }
        }

        private void SaveMenuItems()
        {
            foreach (var menuItem in this.menuItems.Where(x => !string.IsNullOrEmpty(x.Identifier) && !string.IsNullOrEmpty(x.ModuleName)))
            {
                if (this.parentMaps.TryGetValue(menuItem.Identifier, out var parentId))
                {
                    var parentItem = PersonaBarRepository.Instance.GetMenuItem(parentId);
                    if (parentItem != null)
                    {
                        menuItem.ParentId = parentItem.MenuId;
                    }
                }

                PersonaBarRepository.Instance.SaveMenuItem(menuItem);
            }
        }

        private void SaveMenuExtensions()
        {
            foreach (var extension in this.extensions)
            {
                var menuIdentifier = this.extensionMenus[extension.Identifier];
                var menu = PersonaBarRepository.Instance.GetMenuItem(menuIdentifier);
                if (menu != null)
                {
                    extension.MenuId = menu.MenuId;

                    PersonaBarExtensionRepository.Instance.SaveExtension(extension);
                }
            }
        }

        private void SavePermissionDefinitions()
        {
            if (this.menuItems.Count == 0)
            {
                return;
            }

            foreach (var definition in this.permissionDefinitions)
            {
                var identifier = definition.Identifier;
                var menu = this.menuItems.FirstOrDefault(m => string.IsNullOrEmpty(identifier) || m.Identifier == identifier);
                if (menu?.MenuId <= 0)
                {
                    menu = PersonaBarRepository.Instance.GetMenuItem(identifier);
                }

                if (menu != null)
                {
                    MenuPermissionController.SavePersonaBarPermission(menu.MenuId, definition.Key, definition.Name);
                }
            }
        }

        private void ReadMenuItemsFromManifest(XPathNavigator menuNavigator)
        {
            var menuItem = new MenuItem()
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                ModuleName = Util.ReadElement(menuNavigator, "moduleName"),
                FolderName = Util.ReadElement(menuNavigator, "folderName"),
                Controller = Util.ReadElement(menuNavigator, "controller"),
                ResourceKey = Util.ReadElement(menuNavigator, "resourceKey"),
                Path = Util.ReadElement(menuNavigator, "path"),
                Link = Util.ReadElement(menuNavigator, "link"),
                CssClass = Util.ReadElement(menuNavigator, "css"),
                IconFile = Util.ReadElement(menuNavigator, "icon"),
                ParentId = Null.NullInteger,
                Order = Convert.ToInt32(Util.ReadElement(menuNavigator, "order", "0")),
                AllowHost = Util.ReadElement(menuNavigator, "allowHost", "true").Equals("true", StringComparison.InvariantCultureIgnoreCase),
                Enabled = true,
            };

            var parent = Util.ReadElement(menuNavigator, "parent", string.Empty);
            if (!string.IsNullOrEmpty(parent))
            {
                this.parentMaps.Add(menuItem.Identifier, parent);
            }

            var defaultPermissions = Util.ReadElement(menuNavigator, "defaultPermissions", string.Empty);
            if (!string.IsNullOrEmpty(defaultPermissions))
            {
                this.menuRoles.Add(menuItem.Identifier, defaultPermissions);
            }

            this.menuItems.Add(menuItem);
        }

        private void ReadExtensionsFromManifest(XPathNavigator menuNavigator)
        {
            var extension = new PersonaBarExtension()
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                FolderName = Util.ReadElement(menuNavigator, "folderName"),
                Controller = Util.ReadElement(menuNavigator, "controller"),
                Container = Util.ReadElement(menuNavigator, "container"),
                Path = Util.ReadElement(menuNavigator, "path"),
                Order = Convert.ToInt32(Util.ReadElement(menuNavigator, "order", "0")),
                Enabled = true,
            };

            this.extensions.Add(extension);
            this.extensionMenus.Add(extension.Identifier, Util.ReadElement(menuNavigator, "menu"));
        }

        private void ReadPermissionsFromManifest(XPathNavigator menuNavigator)
        {
            var permission = new PermissionDefinition
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                Key = Util.ReadElement(menuNavigator, "key"),
                Name = Util.ReadElement(menuNavigator, "name"),
            };

            this.permissionDefinitions.Add(permission);
        }

        private void SaveMenuPermissions(MenuItem menuItem)
        {
            if (!this.menuRoles.TryGetValue(menuItem.Identifier, out var role))
            {
                return;
            }

            var defaultPermissions = role.Split(',');
            if (menuItem.MenuId <= 0)
            {
                menuItem = PersonaBarRepository.Instance.GetMenuItem(menuItem.Identifier);
            }

            PersonaBarRepository.Instance.GetMenuDefaultPermissions(menuItem.MenuId);
            PersonaBarRepository.Instance.SaveMenuDefaultPermissions(menuItem, this.menuRoles[menuItem.Identifier]);

            foreach (var roleName in defaultPermissions)
            {
                if (!string.IsNullOrEmpty(roleName.Trim()))
                {
                    SaveMenuPermission(menuItem, roleName.Trim());
                }
            }
        }

        private void DeleteMenus()
        {
            try
            {
                foreach (var menuItem in this.menuItems)
                {
                    PersonaBarRepository.Instance.DeleteMenuItem(menuItem.Identifier);
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
                throw;
            }
        }

        private struct PermissionDefinition
        {
            public string Identifier { get; set; }

            public string Key { get; set; }

            public string Name { get; set; }
        }
    }
}
