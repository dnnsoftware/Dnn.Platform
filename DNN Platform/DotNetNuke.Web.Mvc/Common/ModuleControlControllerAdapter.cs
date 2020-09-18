// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    public class ModuleControlControllerAdapter : ServiceLocator<IModuleControlController, ModuleControlControllerAdapter>, IModuleControlController
    {
        public ModuleControlInfo GetModuleControlByControlKey(string controlKey, int moduleDefID)
        {
            return ModuleControlController.GetModuleControlByControlKey(controlKey, moduleDefID);
        }

        protected override Func<IModuleControlController> GetFactory()
        {
            return () => new ModuleControlControllerAdapter();
        }
    }
}
