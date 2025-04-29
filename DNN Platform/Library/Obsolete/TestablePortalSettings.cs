// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal;

using System.ComponentModel;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Internal.SourceGenerators;

[EditorBrowsable(EditorBrowsableState.Never)]
[DnnDeprecated(7, 3, 0, "Use PortalController.Instance.GetCurrentPortalSettings to get a mockable PortalSettings", RemovalVersion = 10)]
public partial class TestablePortalSettings : ComponentBase<IPortalSettings, TestablePortalSettings>, IPortalSettings
{
    /// <inheritdoc/>
    public string AdministratorRoleName
    {
        get { return PortalSettings.Current.AdministratorRoleName; }
    }
}
