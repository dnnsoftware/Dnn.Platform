// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// A concrete implementation of <see cref="IDesktopModuleController"/>
    /// that relies on the <see cref="DesktopModuleController"/> class.
    /// </summary>
    internal class DesktopModuleControllerShim : IDesktopModuleController
    {
        /// <inheritdoc/>
        public DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName, int portalID)
        {
            return DesktopModuleController.GetDesktopModuleByModuleName(moduleName, portalID);
        }
    }
}
