// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;

#endregion

namespace DotNetNuke.UI.Modules
{
    public class StandardModuleInjectionFilter : IModuleInjectionFilter
    {
        #region Implementation of IModuleInjectionFilter

        public bool CanInjectModule(ModuleInfo module, PortalSettings portalSettings)
        {
            return ModulePermissionController.CanViewModule(module)
                    && module.IsDeleted == false
                    && ((module.StartDate < DateTime.Now && module.EndDate > DateTime.Now)
                            || Globals.IsLayoutMode()
                            || Globals.IsEditMode());
        }

        #endregion
    }
}
