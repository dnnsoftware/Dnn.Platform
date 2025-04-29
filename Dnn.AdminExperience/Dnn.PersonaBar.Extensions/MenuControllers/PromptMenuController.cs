// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.MenuControllers;

using System.Collections.Generic;

using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Users;

/// <summary>Controls the prompt menu.</summary>
public class PromptMenuController : IMenuItemController
{
    /// <inheritdoc/>
    public void UpdateParameters(MenuItem menuItem)
    {
    }

    /// <inheritdoc/>
    public bool Visible(MenuItem menuItem)
    {
        var user = UserController.Instance.GetCurrentUserInfo();
        return user.IsSuperUser;
    }

    /// <inheritdoc/>
    public IDictionary<string, object> GetSettings(MenuItem menuItem)
    {
        return null;
    }
}
