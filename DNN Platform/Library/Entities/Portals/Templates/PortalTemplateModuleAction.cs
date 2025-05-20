// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    /// <inheritdoc cref="DotNetNuke.Abstractions.Portals.Templates.PortalTemplateModuleAction"/>
    public enum PortalTemplateModuleAction
    {
        /// <inheritdoc cref="Abstractions.Portals.Templates.PortalTemplateModuleAction.Ignore"/>
        Ignore = 0,

        /// <inheritdoc cref="Abstractions.Portals.Templates.PortalTemplateModuleAction.Merge"/>
        Merge = 1,

        /// <inheritdoc cref="Abstractions.Portals.Templates.PortalTemplateModuleAction.Replace"/>
        Replace = 2,
    }
}
