// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;

    using DotNetNuke.UI.WebControls;

    public class OAuthSettingsBase : AuthenticationSettingsBase
    {
        protected PropertyEditorControl settingsEditor;

        protected virtual string AuthSystemApplicationName
        {
            get { return string.Empty; }
        }

        /// <inheritdoc/>
        public override void UpdateSettings()
        {
            if (this.settingsEditor.IsValid && this.settingsEditor.IsDirty)
            {
                var config = (OAuthConfigBase)this.settingsEditor.DataSource;
                OAuthConfigBase.UpdateConfig(config);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            OAuthConfigBase config = OAuthConfigBase.GetConfig(this.AuthSystemApplicationName, this.PortalId);
            this.settingsEditor.DataSource = config;
            this.settingsEditor.DataBind();
        }
    }
}
