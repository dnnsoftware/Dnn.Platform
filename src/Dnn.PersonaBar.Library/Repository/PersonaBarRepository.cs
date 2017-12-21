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
            ClearCache();
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
