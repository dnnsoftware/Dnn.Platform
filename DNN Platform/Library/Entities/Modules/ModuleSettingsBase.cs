// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.ComponentModel;
using DotNetNuke.Common;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public class ModuleSettingsBase : PortalModuleBase, ISettingsControl
    {
        public Hashtable ModuleSettings
        {
            get
            {
                return ModuleContext.Configuration.ModuleSettings;
            }
        }

        public Hashtable TabModuleSettings
        {
            get
            {
                return ModuleContext.Configuration.TabModuleSettings;
            }
        }

        #region ISettingsControl Members

        public virtual void LoadSettings()
        {
        }

        public virtual void UpdateSettings()
        {
        }

        #endregion
    }
}
