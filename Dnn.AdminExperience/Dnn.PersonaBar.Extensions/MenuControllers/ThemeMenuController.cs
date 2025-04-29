// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.MenuControllers;

using System.Collections.Generic;

using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Controls the themes menu.</summary>
public class ThemeMenuController : IMenuItemController
{
    /// <inheritdoc/>
    public void UpdateParameters(MenuItem menuItem)
    {
    }

    /// <inheritdoc/>
    public bool Visible(MenuItem menuItem)
    {
        return true;
    }

    /// <inheritdoc/>
    public IDictionary<string, object> GetSettings(MenuItem menuItem)
    {
        return new Dictionary<string, object>
        {
            { "previewUrl", Globals.DependencyProvider.GetRequiredService<INavigationManager>().NavigateURL() },
        };
    }
}
