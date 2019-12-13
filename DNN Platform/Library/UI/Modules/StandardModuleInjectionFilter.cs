// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                            || Globals.IsEditMode()
                    );
        }

        #endregion
    }
}
