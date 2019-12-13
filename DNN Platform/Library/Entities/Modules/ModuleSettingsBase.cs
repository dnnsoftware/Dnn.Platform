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
