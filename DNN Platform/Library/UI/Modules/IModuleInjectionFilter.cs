// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
