// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Sites.MenuControllers;

using System.Collections.Generic;

using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

/// <summary>Controls the sites menu.</summary>
public class SitesMenuController : IMenuItemController
{
    /// <inheritdoc/>
    public void UpdateParameters(MenuItem menuItem)
    {
    }

    /// <inheritdoc/>
    public bool Visible(MenuItem menuItem)
    {
        var user = UserController.Instance.GetCurrentUserInfo();
        return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
    }

    /// <inheritdoc/>
    public IDictionary<string, object> GetSettings(MenuItem menuItem)
    {
        return null;
    }
}
