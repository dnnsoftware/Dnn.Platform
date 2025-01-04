// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Styles.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Extensions;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// Controls the Styles menu.
    /// </summary>
    public class StylesMenuController : IMenuItemController
    {
        private readonly IPortalController portalController;
        private readonly IUserController userController;

        /// <summary>
        /// Initializes a new instance of the <see cref="StylesMenuController"/> class.
        /// </summary>
        /// <param name="portalController">Provides access to portals information.</param>
        public StylesMenuController(IPortalController portalController)
            : this(portalController, UserController.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StylesMenuController"/> class.
        /// </summary>
        /// <param name="portalController">Provides information about portals.</param>
        /// <param name="userController">Provides information about users.</param>
        public StylesMenuController(IPortalController portalController, IUserController userController)
        {
            this.portalController = portalController;
            this.userController = userController;
        }

        /// <inheritdoc/>
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
           return new Dictionary<string, object>();
        }

        /// <inheritdoc/>
        public void UpdateParameters(MenuItem menuItem)
        {
            menuItem.AllowHost = true;
        }

        /// <inheritdoc/>
        public bool Visible(MenuItem menuItem)
        {
            var allowAdminEdits = this.portalController
                .GetCurrentSettings()
                .GetStyles()
                .AllowAdminEdits;
            var user = this.userController.GetCurrentUserInfo();

            if (user.IsSuperUser)
            {
                return true;
            }

            if (user.IsAdmin && allowAdminEdits)
            {
                return true;
            }

            return false;
        }
    }
}
