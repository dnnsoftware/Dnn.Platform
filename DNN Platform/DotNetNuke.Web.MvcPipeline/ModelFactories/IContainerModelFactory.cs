// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Creates container models used to render modules within a pane.
    /// </summary>
    public interface IContainerModelFactory
    {
        /// <summary>
        /// Creates a container model for the specified module configuration and portal settings.
        /// </summary>
        /// <param name="configuration">The module configuration.</param>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <param name="containerSrc">The container source path.</param>
        /// <returns>The created <see cref="ContainerModel"/>.</returns>
        ContainerModel CreateContainerModel(ModuleInfo configuration, PortalSettings portalSettings, string containerSrc);
    }
}
