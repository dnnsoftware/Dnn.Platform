using System;
using System.Linq;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Services.Authentication.OAuth
{
    public class OAuthSettingsBase : AuthenticationSettingsBase
    {
        protected PropertyEditorControl SettingsEditor;

        protected virtual string AuthSystemApplicationName { get { return String.Empty; } }

        public override void UpdateSettings()
        {
            if (SettingsEditor.IsValid && SettingsEditor.IsDirty)
            {
                var config = (OAuthConfigBase)SettingsEditor.DataSource;
                OAuthConfigBase.UpdateConfig(config);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            OAuthConfigBase config = OAuthConfigBase.GetConfig(AuthSystemApplicationName, PortalId);
            SettingsEditor.DataSource = config;
            SettingsEditor.DataBind();
        }
    }
}
