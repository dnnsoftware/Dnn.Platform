// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvc.Common
{
    public interface IDesktopModuleController
    {
        DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId);
    }
}
