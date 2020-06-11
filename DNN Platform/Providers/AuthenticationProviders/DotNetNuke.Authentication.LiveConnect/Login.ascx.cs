﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Authentication.LiveConnect.Components;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Authentication.LiveConnect
{
    public partial class Login : OAuthLoginBase
    {
        protected override string AuthSystemApplicationName
        {
            get { return "Live"; }
        }

        public override bool SupportsRegistration
        {
            get { return true; }
        }

        protected override UserData GetCurrentUser()
        {
            return this.OAuthClient.GetCurrentUser<LiveUserData>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.loginButton.Click += this.loginButton_Click;
            this.registerButton.Click += this.loginButton_Click;

            this.OAuthClient = new LiveClient(this.PortalId, this.Mode);

            this.loginItem.Visible = (this.Mode == AuthMode.Login);
            this.registerItem.Visible = (this.Mode == AuthMode.Register);
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
