// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Facebook
{
    using System;
    using System.Collections.Specialized;

    using DotNetNuke.Authentication.Facebook.Components;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    public partial class Login : OAuthLoginBase
    {
        public override bool SupportsRegistration
        {
            get { return true; }
        }

        protected override string AuthSystemApplicationName
        {
            get { return "Facebook"; }
        }

        protected override UserData GetCurrentUser()
        {
            return this.OAuthClient.GetCurrentUser<FacebookUserData>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.loginButton.Click += this.loginButton_Click;
            this.registerButton.Click += this.loginButton_Click;

            this.OAuthClient = new FacebookClient(this.PortalId, this.Mode);

            this.loginItem.Visible = this.Mode == AuthMode.Login;
            this.registerItem.Visible = this.Mode == AuthMode.Register;
        }

        protected override void AddCustomProperties(NameValueCollection properties)
        {
            base.AddCustomProperties(properties);

            var facebookUserData = this.OAuthClient.GetCurrentUser<FacebookUserData>();
            if (facebookUserData.Link != null)
            {
                properties.Add("Facebook", facebookUserData.Link.ToString());
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            AuthorisationResult result = this.OAuthClient.Authorize();
            if (result == AuthorisationResult.Denied)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }
    }
}
