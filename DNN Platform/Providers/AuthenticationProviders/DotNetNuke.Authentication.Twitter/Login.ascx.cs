// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Twitter;

using System;

using DotNetNuke.Authentication.Twitter.Components;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

/// <inheritdoc/>
public partial class Login : OAuthLoginBase
{
    /// <inheritdoc/>
    public override bool SupportsRegistration
    {
        get { return true; }
    }

    /// <inheritdoc/>
    protected override string AuthSystemApplicationName
    {
        get { return "Twitter"; }
    }

    /// <inheritdoc/>
    protected override UserData GetCurrentUser()
    {
        return this.OAuthClient.GetCurrentUser<TwitterUserData>();
    }

    /// <inheritdoc/>
    protected override void AddCustomProperties(System.Collections.Specialized.NameValueCollection properties)
    {
        base.AddCustomProperties(properties);

        properties.Add("Twitter", string.Format("http://twitter.com/{0}", this.OAuthClient.GetCurrentUser<TwitterUserData>().ScreenName));
    }

    /// <inheritdoc/>
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        this.loginButton.Click += this.LoginButton_Click;
        this.registerButton.Click += this.LoginButton_Click;

        this.OAuthClient = new TwitterClient(this.PortalId, this.Mode);

        this.loginItem.Visible = this.Mode == AuthMode.Login;
        this.registerItem.Visible = this.Mode == AuthMode.Register;
    }

    private void LoginButton_Click(object sender, EventArgs e)
    {
        this.OAuthClient.CallbackUri = new Uri(this.OAuthClient.CallbackUri + "?state=Twitter");
        AuthorisationResult result = this.OAuthClient.Authorize();
        if (result == AuthorisationResult.Denied)
        {
            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
        }
    }
}
