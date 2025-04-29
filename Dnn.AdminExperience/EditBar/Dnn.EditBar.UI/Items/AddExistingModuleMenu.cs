// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dnn.EditBar.Library;
using Dnn.EditBar.Library.Items;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;

[Serializable]
public class AddExistingModuleMenu : BaseMenuItem
{
    /// <inheritdoc/>
    public override string Name { get; } = "AddExistingModule";

    /// <inheritdoc/>
    public override string Text
    {
        get
        {
            return "Add Existing Module";
        }
    }

    /// <inheritdoc/>
    public override string CssClass
    {
        get
        {
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public override string Template { get; } = string.Empty;

    /// <inheritdoc/>
    public override string Parent { get; } = Constants.LeftMenu;

    /// <inheritdoc/>
    public override string Loader { get; } = "AddExistingModule";

    /// <inheritdoc/>
    public override int Order { get; } = 10;

    /// <inheritdoc/>
    public override bool Visible()
    {
        return Personalization.GetUserMode() == PortalSettings.Mode.Edit;
    }
}
