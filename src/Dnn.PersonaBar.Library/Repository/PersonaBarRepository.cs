// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Data;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Library.Repository
{
    public class PersonaBarRepository : ServiceLocator<IPersonaBarRepository, PersonaBarRepository>,
        IPersonaBarRepository
    {
        private readonly IDataService _dataService = new DataService();
        private const string PersonaBarMenuCacheKey = "PersonaBarMenu";
        private static readonly object ThreadLocker = new object();

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
                        var menuItems = CBO.FillCollection<MenuItem>(_dataService.GetPersonaBarMenu())
                            .OrderBy(m => m.Order).ToList();

                        foreach (var menuItem in menuItems.Where(m => m.ParentId == Null.NullInteger))
                        {
                            menu.MenuItems.Add(menuItem);
                            InjectMenuItems(menuItem, menuItems);
                        }

                        DataCache.SetCache(PersonaBarMenuCacheKey, menu);
                    }
                }
            }

            return menu;
        }

        public MenuItem GetMenuItem(string identifier)
        {
            return GetMenu().AllItems.FirstOrDefault(m => m.Identifier.Equals(identifier, StringComparison.InvariantCultureIgnoreCase));
        }

        public MenuItem GetMenuItem(int menuId)
        {
            return GetMenu().AllItems.FirstOrDefault(m => m.MenuId == menuId);
        }

        public void SaveMenuItem(MenuItem item)
        {
            var user = UserController.Instance.GetCurrentUserInfo();

            item.MenuId = _dataService.SavePersonaBarMenu(
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
                user.UserID
            );

            ClearCache();
        }

        public void DeleteMenuItem(string identifier)
        {
            _dataService.DeletePersonaBarMenuByIdentifier(identifier);

            ClearCache();
        }

        public string GetMenuDefaultPermissions(int menuId)
        {
            return _dataService.GetPersonaBarMenuDefaultPermissions(menuId);
        }

        public void SaveMenuDefaultPermissions(MenuItem menuItem, string roleNames)
        {
            _dataService.SavePersonaBarMenuDefaultPermissions(menuItem.MenuId, roleNames);
        }

        public void UpdateMenuController(string identifier, string controller)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            _dataService.UpdateMenuController(identifier, controller, user.UserID);
        }

        private void InjectMenuItems(MenuItem parent, IList<MenuItem> menuItems)
        {
            foreach (var menuItem in menuItems.Where(m => m.ParentId == parent.MenuId))
            {
                parent.Children.Add(menuItem);
                InjectMenuItems(menuItem, menuItems);
            }
        }

        private void ClearCache()
        {
            DataCache.RemoveCache(PersonaBarMenuCacheKey);
        }

        protected override Func<IPersonaBarRepository> GetFactory()
        {
            return () => new PersonaBarRepository();
        }
    }
}
