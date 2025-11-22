// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.MvcPipeline.Models;

    public interface IContainerModelFactory
    {
        ContainerModel CreateContainerModel(ModuleInfo configuration, PortalSettings portalSettings, string containerSrc);
    }
}
