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
using Dnn.EditBar.UI.Helpers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Personalization;

[Serializable]
public class PageSettingsMenu : BaseMenuItem
{
    /// <inheritdoc/>
    public override string Name { get; } = "PageSettings";

    /// <inheritdoc/>
    public override string Text { get; } = "PageSettings";

    /// <inheritdoc/>
    public override string Parent { get; } = Constants.LeftMenu;

    /// <inheritdoc/>
    public override string Loader { get; } = "PageSettings";

    /// <inheritdoc/>
    public override int Order { get; } = 15;

    /// <inheritdoc/>
    public override bool Visible()
    {
        return Personalization.GetUserMode() == PortalSettings.Mode.Edit
               && Host.ControlPanel.EndsWith("PersonaBarContainer.ascx", StringComparison.InvariantCultureIgnoreCase);
    }
}
