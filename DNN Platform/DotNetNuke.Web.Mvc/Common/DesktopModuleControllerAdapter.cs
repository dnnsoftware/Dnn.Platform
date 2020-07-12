// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Common
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    public class DesktopModuleControllerAdapter : ServiceLocator<IDesktopModuleController, DesktopModuleControllerAdapter>, IDesktopModuleController
    {
        public DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId)
        {
            return Entities.Modules.DesktopModuleController.GetDesktopModule(desktopModuleId, portalId);
        }

        protected override Func<IDesktopModuleController> GetFactory()
        {
            return () => new DesktopModuleControllerAdapter();
        }
    }
}
