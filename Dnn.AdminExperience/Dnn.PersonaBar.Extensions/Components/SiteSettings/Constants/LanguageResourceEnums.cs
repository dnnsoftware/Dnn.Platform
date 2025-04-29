// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components.Constants;

public enum LanguageResourceMode
{
    // for any unspecified item; these are usually the second level and lower folders/files
    Any = 0,

    System = 1, // aka. Global
    Global = System,

    Host = 2,   // not supported in PersonaBar; kept here because the base code uses it

    Portal = 3, // aka. Site
    Site = Portal,
}

public enum LanguageResourceType
{
    // Localization files under "~/Portals/_default"
    SiteTemplates = 0,

    // Localization files under "~/App_GlobalResources"
    GlobalResources = 1,

    // All other localization files under "~/xxxx"; e.g., "~/Admin", "~/DesktopModules", etc.
    LocalResources = 2,
}
