// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Containers;
using Dnn.PersonaBar.Library.Permissions;
using Dnn.PersonaBar.Library.Repository;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using Newtonsoft.Json;
using MenuItem = Dnn.PersonaBar.Library.Model.MenuItem;
using PersonaBarMenu = Dnn.PersonaBar.Library.Model.PersonaBarMenu;

namespace Dnn.PersonaBar.Library.Controllers
{
    public class PersonaBarController : ServiceLocator<IPersonaBarController, PersonaBarController>, IPersonaBarController
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(PersonaBarController));

        private readonly IPersonaBarRepository _personaBarRepository;

        public PersonaBarController()
        {
            _personaBarRepository = PersonaBarRepository.Instance;
        }

        public PersonaBarMenu GetMenu(PortalSettings portalSettings, UserInfo user)
        {
            try
            {
                var personaBarMenu = _personaBarRepository.GetMenu();
                var filteredMenu = new PersonaBarMenu();
                GetPersonaBarMenuWithPermissionCheck(portalSettings, user, filteredMenu.MenuItems, personaBarMenu.MenuItems);

                PersonaBarContainer.Instance.FilterMenu(filteredMenu);
                return filteredMenu;
            }
            catch (Exception e)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                return new PersonaBarMenu();
            }
        }

        public bool IsVisible(PortalSettings portalSettings, UserInfo user, MenuItem menuItem)
        {
            var visible = menuItem.Enabled
                   && !(user.IsSuperUser && !menuItem.AllowHost)
                   && MenuPermissionController.CanView(portalSettings.PortalId, menuItem);

            if (visible)
            {
                var menuController = GetMenuItemController(menuItem);
                visible = menuController == null || menuController.Visible(menuItem);
            }

            return visible;
        }

        private bool GetPersonaBarMenuWithPermissionCheck(PortalSettings portalSettings, UserInfo user, IList<MenuItem> filterItems, IList<MenuItem> menuItems)
        {
            var menuFiltered = false;
            foreach (var menuItem in menuItems)
            {
                try
                {
                    if (!IsVisible(portalSettings, user, menuItem))
                    {
                        menuFiltered = true;
                        continue;
                    }

                    var cloneItem = new MenuItem()
                    {
                        MenuId = menuItem.MenuId,
                        Identifier = menuItem.Identifier,
                        ModuleName = menuItem.ModuleName,
                        FolderName = menuItem.FolderName,
                        Controller = menuItem.Controller,
                        ResourceKey = menuItem.ResourceKey,
                        Path = menuItem.Path,
                        Link = menuItem.Link,
                        CssClass = menuItem.CssClass,
                        IconFile = menuItem.IconFile,
                        AllowHost = menuItem.AllowHost,
                        Order = menuItem.Order,
                        ParentId = menuItem.ParentId
                    };

                    UpdateParamters(cloneItem);
                    cloneItem.Settings = GetMenuSettings(menuItem);

                    var filtered = GetPersonaBarMenuWithPermissionCheck(portalSettings, user, cloneItem.Children,
                        menuItem.Children);
                    if (!filtered || cloneItem.Children.Count > 0)
                    {
                        filterItems.Add(cloneItem);
                    }
                }
                catch (Exception e) //Ignore the failure and still load personaBar
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                }
            }

            return menuFiltered;
        }

        private void UpdateParamters(MenuItem menuItem)
        {
            var menuController = GetMenuItemController(menuItem);
            menuController?.UpdateParameters(menuItem);
        }

        private string GetMenuSettings(MenuItem menuItem)
        {
            var menuController = GetMenuItemController(menuItem);
            var settings = menuController?.GetSettings(menuItem) ?? new Dictionary<string, object>();
            AddPermissions(menuItem, settings);
            return JsonConvert.SerializeObject(settings);
        }

        private void AddPermissions(MenuItem menuItem, IDictionary<string, object> settings)
        {
            var portalSettings = PortalSettings.Current;
            if (!settings.ContainsKey("permissions") && portalSettings != null)
            {
                var menuPermissions = MenuPermissionController.GetPermissions(menuItem.MenuId)
                    .Where(p => p.PermissionKey != "VIEW");
                var portalId = portalSettings.PortalId;
                var permissions = new Dictionary<string, bool>();
                foreach (var permission in menuPermissions)
                {
                    var key = permission.PermissionKey;
                    var hasPermission = MenuPermissionController.HasMenuPermission(portalId, menuItem, key);
                    permissions.Add(key, hasPermission);
                }

                settings.Add("permissions", permissions);
            }
        }

        private IMenuItemController GetMenuItemController(MenuItem menuItem)
        {
            var identifier = menuItem.Identifier;
            var controller = menuItem.Controller;

            if (string.IsNullOrEmpty(controller))
            {
                return null;
            }

            try
            {
                var cacheKey = $"PersonaBarMenuController_{identifier}";
                return Reflection.CreateObject(controller, cacheKey) as IMenuItemController;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }

        }

        protected override Func<IPersonaBarController> GetFactory()
        {
            return () => new PersonaBarController();
        }

    }
}
