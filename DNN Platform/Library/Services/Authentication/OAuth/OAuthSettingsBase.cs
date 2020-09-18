// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;
    using System.Linq;

    using DotNetNuke.UI.WebControls;

    public class OAuthSettingsBase : AuthenticationSettingsBase
    {
        protected PropertyEditorControl SettingsEditor;

        protected virtual string AuthSystemApplicationName
        {
            get { return string.Empty; }
        }

        public override void UpdateSettings()
        {
            if (this.SettingsEditor.IsValid && this.SettingsEditor.IsDirty)
            {
                var config = (OAuthConfigBase)this.SettingsEditor.DataSource;
                OAuthConfigBase.UpdateConfig(config);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            OAuthConfigBase config = OAuthConfigBase.GetConfig(this.AuthSystemApplicationName, this.PortalId);
            this.SettingsEditor.DataSource = config;
            this.SettingsEditor.DataBind();
        }
    }
}
