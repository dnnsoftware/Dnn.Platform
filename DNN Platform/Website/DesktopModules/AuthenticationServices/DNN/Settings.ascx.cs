// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Modules.Admin.Authentication
{
    public partial class Settings : AuthenticationSettingsBase
    {
        public override void UpdateSettings()
        {
            if (SettingsEditor.IsValid && SettingsEditor.IsDirty)
            {
                var config = (AuthenticationConfig) SettingsEditor.DataSource;
                AuthenticationConfig.UpdateConfig(config);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                AuthenticationConfig config = AuthenticationConfig.GetConfig(PortalId);
                SettingsEditor.DataSource = config;
                SettingsEditor.DataBind();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
