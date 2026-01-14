// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Components.Constants
{
    public enum LanguageResourceMode
    {
        /// <summary>for any unspecified item; these are usually the second level and lower folders/files.</summary>
        Any = 0,

        /// <summary>The base system/global resources.</summary>
        System = 1,

        /// <summary>The base system/global resources.</summary>
        Global = System,

        /// <summary>Host-level resource customizations.</summary>
        /// <remarks>not supported in PersonaBar; kept here because the base code uses it.</remarks>
        Host = 2,

        /// <summary>Site/portal-level resource customizations.</summary>
        Portal = 3,

        /// <summary>Site/portal-level resource customizations.</summary>
        Site = Portal,
    }

    public enum LanguageResourceType
    {
        /// <summary>Localization files under "~/Portals/_default".</summary>
        SiteTemplates = 0,

        /// <summary>Localization files under "~/App_GlobalResources".</summary>
        GlobalResources = 1,

        /// <summary>All other localization files under "~/xxxx"; e.g., "~/Admin", "~/DesktopModules", etc.</summary>
        LocalResources = 2,
    }
}
