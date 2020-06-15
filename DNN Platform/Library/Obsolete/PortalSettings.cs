// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.Skins;

    public partial class PortalSettings
    {
        [Obsolete("Deprecated in DNN 7.4. Replaced by PortalSettingsController.Instance().GetPortalAliasMappingMode. Scheduled removal in v10.0.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static PortalAliasMapping GetPortalAliasMappingMode(int portalId)
        {
            return PortalSettingsController.Instance().GetPortalAliasMappingMode(portalId);
        }
    }
}
