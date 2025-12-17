// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules.Definitions;

    /// <summary>
    /// A concrete implementation of the <see cref="IModuleDefinitionController"/> interface
    /// that relies on the <see cref="ModuleDefinitionController"/> class.
    /// </summary>
    internal sealed class ModuleDefinitionControllerShim : IModuleDefinitionController
    {
        /// <inheritdoc/>
        public Dictionary<string, ModuleDefinitionInfo> GetModuleDefinitionsByDesktopModuleID(int desktopModuleID)
        {
            return ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleID);
        }
    }
}
