// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;

using DotNetNuke.Authentication.Twitter.Components;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Authentication.Twitter
{
    public partial class Login : OAuthLoginBase
    {
        protected override string AuthSystemApplicationName
        {
            get { return "Twitter"; }
        }

        public override bool SupportsRegistration
        {
            get { return true; }
        }

        protected override UserData GetCurrentUser()
        {
            return OAuthClient.GetCurrentUser<TwitterUserData>();
        }

        protected override void AddCustomProperties(System.Collections.Specialized.NameValueCollection properties)
        {
            base.AddCustomProperties(properties);

            properties.Add("Twitter", string.Format("http://twitter.com/{0}", OAuthClient.GetCurrentUser<TwitterUserData>().ScreenName));
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            loginButton.Click += loginButton_Click;
            registerButton.Click += loginButton_Click;

            OAuthClient = new TwitterClient(PortalId, Mode);

            loginItem.Visible = (Mode == AuthMode.Login);
            registerItem.Visible = (Mode == AuthMode.Register);
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            OAuthClient.CallbackUri = new Uri(OAuthClient.CallbackUri + "?state=Twitter");
            AuthorisationResult result = OAuthClient.Authorize();
            if (result == AuthorisationResult.Denied)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);

            }
        }
    }
}
