// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Web.Mvc.Common
{
    public class DesktopModuleControllerAdapter : ServiceLocator<IDesktopModuleController, DesktopModuleControllerAdapter>, IDesktopModuleController
    {
        protected override Func<IDesktopModuleController> GetFactory()
        {
            return () => new DesktopModuleControllerAdapter();
        }

        public DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId)
        {
            return Entities.Modules.DesktopModuleController.GetDesktopModule(desktopModuleId, portalId);
        }
    }
}
