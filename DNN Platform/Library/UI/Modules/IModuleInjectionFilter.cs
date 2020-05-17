// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.UI.Modules
{
    public interface IModuleInjectionFilter
    {
        bool CanInjectModule(ModuleInfo module, PortalSettings portalSettings);
    }
}
