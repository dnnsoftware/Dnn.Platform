// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections.Specialized;
using DotNetNuke.Authentication.Facebook.Components;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Authentication.Facebook
{
    public partial class Login : OAuthLoginBase
    {
        protected override string AuthSystemApplicationName
        {
            get { return "Facebook"; }
        }

        public override bool SupportsRegistration
        {
            get { return true; }
        }

        protected override UserData GetCurrentUser()
        {
            return OAuthClient.GetCurrentUser<FacebookUserData>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            loginButton.Click += loginButton_Click;
            registerButton.Click += loginButton_Click;

            OAuthClient = new FacebookClient(PortalId, Mode);

            loginItem.Visible = (Mode == AuthMode.Login);
            registerItem.Visible = (Mode == AuthMode.Register);
        }

        protected override void AddCustomProperties(NameValueCollection properties)
        {
            base.AddCustomProperties(properties);

            var facebookUserData = OAuthClient.GetCurrentUser<FacebookUserData>();
            if (facebookUserData.Link != null) {
                properties.Add("Facebook", facebookUserData.Link.ToString());
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            AuthorisationResult result = OAuthClient.Authorize();
            if (result == AuthorisationResult.Denied)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                
            }
        }
    }
}
