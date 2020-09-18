// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// Interface responsible to manage the PersonaBar structure by User's Roles and Sku.
    /// </summary>
    public interface IPersonaBarController
    {
        /// <summary>
        /// Gets the menu structure of the persona bar.
        /// </summary>
        /// <param name="portalSettings"></param>
        /// <param name="userInfo">the user that will be used to filter the menu.</param>
        /// <returns>Persona bar menu structure for the user.</returns>
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
