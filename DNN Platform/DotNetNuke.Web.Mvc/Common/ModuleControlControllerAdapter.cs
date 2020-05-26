// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Web.Mvc.Common
{
    public class ModuleControlControllerAdapter: ServiceLocator<IModuleControlController, ModuleControlControllerAdapter>, IModuleControlController
    {
        protected override Func<IModuleControlController> GetFactory()
        {
            return () => new ModuleControlControllerAdapter();
        }

        public ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID)
        {
            return ModuleControlController.GetModuleControlByControlKey(controlKey, moduleDefID);
        }
    }
}
