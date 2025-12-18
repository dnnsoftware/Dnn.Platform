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
        private readonly IDataService dataService = new DataService();

        /// <inheritdoc/>
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
                        var menuItems = CBO.FillCollection<MenuItem>(this.dataService.GetPersonaBarMenu())
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

        /// <inheritdoc/>
        public MenuItem GetMenuItem(string identifier)
        {
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <inheritdoc/>
        public MenuItem GetMenuItem(int menuId)
        {
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.MenuId == menuId);
        }

        /// <inheritdoc/>
        public void SaveMenuItem(MenuItem item)
        {
            var user = UserController.Instance.GetCurrentUserInfo();

            item.MenuId = this.dataService.SavePersonaBarMenu(
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

        /// <inheritdoc/>
        public void DeleteMenuItem(string identifier)
        {
            this.dataService.DeletePersonaBarMenuByIdentifier(identifier);

            this.ClearCache();
        }

        /// <inheritdoc/>
        public string GetMenuDefaultPermissions(int menuId)
        {
            return this.dataService.GetPersonaBarMenuDefaultPermissions(menuId);
        }

        /// <inheritdoc/>
        public void SaveMenuDefaultPermissions(MenuItem menuItem, string roleNames)
        {
            this.dataService.SavePersonaBarMenuDefaultPermissions(menuItem.MenuId, roleNames);
        }

        /// <inheritdoc/>
        public void UpdateMenuController(string identifier, string controller)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            this.dataService.UpdateMenuController(identifier, controller, user.UserID);
            this.ClearCache();
        }

        /// <inheritdoc/>
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
