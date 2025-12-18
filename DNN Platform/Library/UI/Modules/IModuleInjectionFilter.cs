// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;

    /// <summary>A contract specifying the ability to determine whether a module should be included on a page.</summary>
    public interface IModuleInjectionFilter
    {
        /// <summary>Determines whether the given <paramref name="module"/> should be included on the page.</summary>
        /// <param name="module">The module to be injected.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns><see langword="true"/> if the module can be injected, otherwise <see langword="false"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        bool CanInjectModule(ModuleInfo module, PortalSettings portalSettings);
    }
}
