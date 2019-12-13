// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvc.Common
{
    public interface IDesktopModuleController
    {
        DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId);
    }
}
