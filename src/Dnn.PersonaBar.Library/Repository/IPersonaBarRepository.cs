// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using Dnn.PersonaBar.Library.Model;

namespace Dnn.PersonaBar.Library.Repository
{
    /// <summary>
    /// Interface responsible to get the Persona Bar menu structure from the data layer
    /// </summary>
    public interface IPersonaBarRepository
    {        
        /// <summary>
        /// Gets the menu structure of the persona bar
        /// </summary>
        /// <returns>Persona bar menu structure</returns>
        PersonaBarMenu GetMenu();

        /// <summary>
        /// Save menu item info.
        /// </summary>
        /// <param name="item"></param>
        void SaveMenuItem(MenuItem item);

        /// <summary>
        /// remove a menu item.
        /// </summary>
        /// <param name="identifier"></param>
        void DeleteMenuItem(string identifier);

        /// <summary>
        /// Get the menu item by identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        MenuItem GetMenuItem(string identifier);

        /// <summary>
        /// Get the menu item by menu id.
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        MenuItem GetMenuItem(int menuId);

        /// <summary>
        /// Get a menu item's default allowed permissions.
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        string GetMenuDefaultPermissions(int menuId);

        /// <summary>
        /// Save a menu item's default allowed permissions.
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="roleNames"></param>
        void SaveMenuDefaultPermissions(MenuItem menuItem, string roleNames);

        void UpdateMenuController(string identifier, string controller);
    }
}
