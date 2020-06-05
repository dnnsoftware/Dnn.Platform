// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
                return this.ModuleContext.Configuration.ModuleSettings;
            }
        }

        public Hashtable TabModuleSettings
        {
            get
            {
                return this.ModuleContext.Configuration.TabModuleSettings;
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
