// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Data;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    public class PersonaBarRepository : ServiceLocator<IPersonaBarRepository, PersonaBarRepository>,
        IPersonaBarRepository
    {
        private const string PersonaBarMenuCacheKey = "PersonaBarMenu";
        private static readonly object ThreadLocker = new object();
        private readonly IDataService _dataService = new DataService();

        public PersonaBarMenu GetMenu()
        {
            var menu = DataCache.GetCache<PersonaBarMenu>(PersonaBarMenuCacheKey);
            if (menu == null)
            {
                lock (ThreadLocker)
                {
                    menu = DataCache.GetCache<PersonaBarMenu>(PersonaBarMenuCacheKey);
                    if (menu == null)
                    {
                        menu = new PersonaBarMenu();
                        var menuItems = CBO.FillCollection<MenuItem>(this._dataService.GetPersonaBarMenu())
                            .OrderBy(m => m.Order).ToList();

                        foreach (var menuItem in menuItems.Where(m => m.ParentId == Null.NullInteger))
                        {
                            menu.MenuItems.Add(menuItem);
                            this.InjectMenuItems(menuItem, menuItems);
                        }

                        DataCache.SetCache(PersonaBarMenuCacheKey, menu);
                    }
                }
            }

            return menu;
        }

        public MenuItem GetMenuItem(string identifier)
        {
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase));
        }

        public MenuItem GetMenuItem(int menuId)
        {
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.MenuId == menuId);
        }

        public void SaveMenuItem(MenuItem item)
        {
            var user = UserController.Instance.GetCurrentUserInfo();

            item.MenuId = this._dataService.SavePersonaBarMenu(
                item.Identifier,
                item.ModuleName,
                item.FolderName,
                item.Controller,
                item.ResourceKey,
                item.Path,
                item.Link,
                item.CssClass,
                item.IconFile,
                item.ParentId,
                item.Order,
                item.AllowHost,
                item.Enabled,
                user.UserID);

            this.ClearCache();
        }

        public void DeleteMenuItem(string identifier)
        {
            this._dataService.DeletePersonaBarMenuByIdentifier(identifier);

            this.ClearCache();
        }

        public string GetMenuDefaultPermissions(int menuId)
        {
            return this._dataService.GetPersonaBarMenuDefaultPermissions(menuId);
        }

        public void SaveMenuDefaultPermissions(MenuItem menuItem, string roleNames)
        {
            this._dataService.SavePersonaBarMenuDefaultPermissions(menuItem.MenuId, roleNames);
        }

        public void UpdateMenuController(string identifier, string controller)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            this._dataService.UpdateMenuController(identifier, controller, user.UserID);
            this.ClearCache();
        }

        protected override Func<IPersonaBarRepository> GetFactory()
        {
            return () => new PersonaBarRepository();
        }

        private void InjectMenuItems(MenuItem parent, IList<MenuItem> menuItems)
        {
            foreach (var menuItem in menuItems.Where(m => m.ParentId == parent.MenuId))
            {
                parent.Children.Add(menuItem);
                this.InjectMenuItems(menuItem, menuItems);
            }
        }

        private void ClearCache()
        {
            DataCache.RemoveCache(PersonaBarMenuCacheKey);
        }
    }
}
