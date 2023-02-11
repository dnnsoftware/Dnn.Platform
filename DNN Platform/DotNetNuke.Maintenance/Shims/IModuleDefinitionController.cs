// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules.Definitions;

    /// <summary>An abstraction of the <see cref="ModuleDefinitionController"/> class to enable DI and unit testing.</summary>
    internal interface IModuleDefinitionController
    {
        /// <inheritdoc cref="ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(int)" />
        Dictionary<string, ModuleDefinitionInfo> GetModuleDefinitionsByDesktopModuleID(int desktopModuleID);
    }
}
