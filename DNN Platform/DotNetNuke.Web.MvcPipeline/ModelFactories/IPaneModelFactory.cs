// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Creates and processes pane models used to host modules within a skin.
    /// </summary>
    public interface IPaneModelFactory
    {
        /// <summary>
        /// Creates a new pane with the specified name.
        /// </summary>
        /// <param name="name">The name of the pane.</param>
        /// <returns>The created <see cref="PaneModel"/>.</returns>
        PaneModel CreatePane(string name);

        /// <summary>
        /// Injects a module into the specified pane.
        /// </summary>
        /// <param name="pane">The target pane.</param>
        /// <param name="module">The module to inject.</param>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <returns>The updated <see cref="PaneModel"/>.</returns>
        PaneModel InjectModule(PaneModel pane, ModuleInfo module, PortalSettings portalSettings);

        /// <summary>
        /// Applies layout and behavior rules to the specified pane.
        /// </summary>
        /// <param name="pane">The pane to process.</param>
        /// <returns>The processed <see cref="PaneModel"/>.</returns>
        PaneModel ProcessPane(PaneModel pane);
    }
}
