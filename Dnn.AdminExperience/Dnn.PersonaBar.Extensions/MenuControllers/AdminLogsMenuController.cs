// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.AdminLogs.MenuControllers;

using System.Collections.Generic;

using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;

/// <summary>Controls the admin logs menu.</summary>
public class AdminLogsMenuController : IMenuItemController
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
        var settings = new Dictionary<string, object>();
        return settings;
    }
}
