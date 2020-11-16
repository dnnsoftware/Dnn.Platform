﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components.Constants
{
    public enum LanguageResourceMode
    {
        // for any unspecified item; these are usually the second level and lower folders/files
        Any,

        System, // aka. Global
        Global = System,

        Host,   // not supported in PersonaBar; kept here because the base code uses it

        Portal, // aka. Site
        Site = Portal,
    }

    public enum LanguageResourceType
    {
        // Localization files under "~/Portals/_default"
        SiteTemplates,

        // Localization files under "~/App_GlobalResources"
        GlobalResources,

        // All other ocalization files under "~/xxxx"; e.g., "~/Admin", "~/DesktopModules", etc.
        LocalResources,
    }
}
