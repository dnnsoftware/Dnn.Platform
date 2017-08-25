#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
                var rootItems = personaBarMenu.MenuItems.Where(m => PersonaBarContainer.Instance.RootItems.Contains(m.Identifier)).ToList();
                GetPersonaBarMenuWithPermissionCheck(portalSettings, user, filteredMenu.MenuItems, rootItems);

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
                try
                {
                    var menuController = GetMenuItemController(menuItem);
                    visible = menuController == null || menuController.Visible(menuItem);
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex);
                    visible = false;
                }
                
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
            try
            {
                menuController?.UpdateParameters(menuItem);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }
        }

        private string GetMenuSettings(MenuItem menuItem)
        {
            IDictionary<string, object> settings;
            try
            {
                var menuController = GetMenuItemController(menuItem);
                settings = menuController?.GetSettings(menuItem) ?? new Dictionary<string, object>();
                AddPermissions(menuItem, settings);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
                settings = new Dictionary<string, object>();
            }

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
