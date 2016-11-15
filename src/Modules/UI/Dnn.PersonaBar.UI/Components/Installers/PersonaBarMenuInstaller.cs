using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Dnn.PersonaBar.Library.PersonaBar.Model;
using Dnn.PersonaBar.Library.PersonaBar.Permissions;
using Dnn.PersonaBar.Library.PersonaBar.Repository;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Installers;

namespace Dnn.PersonaBar.UI.Components.Installers
{
    /// <summary>
    /// Installer for persona bar menus.
    /// </summary>
    public class PersonaBarMenuInstaller : ComponentInstallerBase
    {
        private IList<MenuItem> _menuItems = new List<MenuItem>();
        private IList<PersonaBarExtension> _extensions = new List<PersonaBarExtension>();
        private IDictionary<string, string> _extensionMenus = new Dictionary<string, string>();
        private IDictionary<string, string> _menuRoles = new Dictionary<string, string>();
        private IDictionary<string, string> _parentMaps = new Dictionary<string, string>();

        public override void Commit()
        {
        }

        public override void Install()
        {
            try
            {
                foreach (var menuItem in _menuItems)
                {
                    if (_parentMaps.ContainsKey(menuItem.Identifier))
                    {
                        var parentItem = PersonaBarRepository.Instance.GetMenuItem(_parentMaps[menuItem.Identifier]);
                        if (parentItem != null)
                        {
                            menuItem.ParentId = parentItem.MenuId;
                        }
                    }

                    PersonaBarRepository.Instance.SaveMenuItem(menuItem);

                    SaveMenuPermissions(menuItem);
                }

                foreach (var extension in _extensions)
                {
                    var menuIdentifier = _extensionMenus[extension.Identifier];
                    var menu = PersonaBarRepository.Instance.GetMenuItem(menuIdentifier);
                    if (menu != null)
                    {
                        extension.MenuId = menu.MenuId;

                        PersonaBarExtensionRepository.Instance.SaveExtension(extension);
                    }
                }

                Completed = true;
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        public override void ReadManifest(XPathNavigator manifestNav)
        {
            foreach (XPathNavigator navigator in manifestNav.Select("menu"))
            {
                ReadMenuItemFromManifest(navigator);
            }

            foreach (XPathNavigator navigator in manifestNav.Select("extension"))
            {
                ReadExtensionFromManifest(navigator);
            }
        }

        public override void Rollback()
        {
            DeleteMenus();
        }

        public override void UnInstall()
        {
            DeleteMenus();
        }

        private void ReadMenuItemFromManifest(XPathNavigator menuNavigator)
        {
            var menuItem = new MenuItem()
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                ModuleName = Util.ReadElement(menuNavigator, "moduleName"),
                Controller = Util.ReadElement(menuNavigator, "controller"),
                ResourceKey = Util.ReadElement(menuNavigator, "resourceKey"),
                Path = Util.ReadElement(menuNavigator, "path"),
                Link = Util.ReadElement(menuNavigator, "link"),
                CssClass = Util.ReadElement(menuNavigator, "css"),
                MobileSupport = Util.ReadElement(menuNavigator, "mobileSupport", "true").ToLowerInvariant() == "true",
                ParentId = Null.NullInteger,
                Order = Convert.ToInt32(Util.ReadElement(menuNavigator, "order", "0")),
                AllowHost = Util.ReadElement(menuNavigator, "allowHost", "true").ToLowerInvariant() == "true",
                Enabled = true
            };

            var parent = Util.ReadElement(menuNavigator, "parent", string.Empty);
            if (!string.IsNullOrEmpty(parent))
            {
                _parentMaps.Add(menuItem.Identifier, parent);
            }

            var defaultRoles = Util.ReadElement(menuNavigator, "defaultRoles", string.Empty);
            if (!string.IsNullOrEmpty(defaultRoles))
            {
                _menuRoles.Add(menuItem.Identifier, defaultRoles);
            }

            _menuItems.Add(menuItem);
        }

        private void ReadExtensionFromManifest(XPathNavigator menuNavigator)
        {
            var extension = new PersonaBarExtension()
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                Controller = Util.ReadElement(menuNavigator, "controller"),
                Container = Util.ReadElement(menuNavigator, "container"),
                Path = Util.ReadElement(menuNavigator, "path"),
                Order = Convert.ToInt32(Util.ReadElement(menuNavigator, "order", "0")),
                Enabled = true
            };

            _extensions.Add(extension);
            _extensionMenus.Add(extension.Identifier, Util.ReadElement(menuNavigator, "menu"));
        }

        private void SaveMenuPermission(MenuItem menuItem, string roleName)
        {
            var portals = PortalController.Instance.GetPortals();
            foreach (PortalInfo portal in portals)
            {
                MenuPermissionController.SaveMenuDefaultPermissions(portal.PortalID, menuItem, roleName);
            }
        }

        private void SaveMenuPermissions(MenuItem menuItem)
        {
            if (_menuRoles.ContainsKey(menuItem.Identifier))
            {
                var defaultRoles = _menuRoles[menuItem.Identifier].Split(',');

                PersonaBarRepository.Instance.SaveMenuDefaultRoles(menuItem, _menuRoles[menuItem.Identifier]);

                //don't save menu permissions during install process
                if (Globals.Status != Globals.UpgradeStatus.Install)
                {
                    foreach (var roleName in defaultRoles)
                    {
                        if (!string.IsNullOrEmpty(roleName.Trim()))
                        {
                            SaveMenuPermission(menuItem, roleName.Trim());
                        }
                    }
                }
            }
        }

        private void DeleteMenus()
        {
            try
            {
                foreach (var menuItem in _menuItems)
                {
                    PersonaBarRepository.Instance.DeleteMenuItem(menuItem.Identifier);
                }
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }
    }
}
