// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using DotNetNuke.Entities.Modules;

    /// <summary>An abstraction of the <see cref="DesktopModuleController"/> class to enable DI and unit testing.</summary>
    internal interface IDesktopModuleController
    {
        /// <inheritdoc cref="DesktopModuleController.GetDesktopModuleByModuleName(string, int)" />
        DesktopModuleInfo GetDesktopModuleByModuleName(string moduleName, int portalID);
    }
}
