// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Authentication
{
    using System;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Exceptions;

    public partial class Settings : AuthenticationSettingsBase
    {
        public override void UpdateSettings()
        {
            if (this.SettingsEditor.IsValid && this.SettingsEditor.IsDirty)
            {
                var config = (AuthenticationConfig)this.SettingsEditor.DataSource;
                AuthenticationConfig.UpdateConfig(config);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                AuthenticationConfig config = AuthenticationConfig.GetConfig(this.PortalId);
                this.SettingsEditor.DataSource = config;
                this.SettingsEditor.DataBind();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
