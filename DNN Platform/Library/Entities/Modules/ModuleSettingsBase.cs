﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System.Collections;

    using DotNetNuke.UI.Modules;

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

        /// <inheritdoc/>
        public virtual void LoadSettings()
        {
        }

        /// <inheritdoc/>
        public virtual void UpdateSettings()
        {
        }
    }
}
