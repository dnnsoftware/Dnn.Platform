﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.Controllers
{
    /// <summary>
    /// Interface responsible to manage the PersonaBar structure by User's Roles and Sku
    /// </summary>
    public interface IPersonaBarController
    {
        /// <summary>
        /// Gets the menu structure of the persona bar
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="userInfo">the user that will be used to filter the menu</param>
        /// <returns>Persona bar menu structure for the user</returns>
        PersonaBarMenu GetMenu(PortalSettings portalSettings, UserInfo userInfo);

        /// <summary>
        /// Whether the menu item is visible.
        /// </summary>
        /// <param name="portalSettings">Portal Settings.</param>
        /// <param name="user">User Info.</param>
        /// <param name="menuItem">The menu item.</param>
        /// <returns></returns>
        bool IsVisible(PortalSettings portalSettings, UserInfo user, MenuItem menuItem);
    }
}
