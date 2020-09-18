// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;
    using System.Collections.Specialized;
    using System.Web;

    using DotNetNuke.Common;

    public abstract class OAuthLoginBase : AuthenticationLoginBase
    {
        public override bool Enabled
        {
            get { return OAuthConfigBase.GetConfig(this.AuthSystemApplicationName, this.PortalId).Enabled; }
        }

        protected virtual string AuthSystemApplicationName
        {
            get { return string.Empty; }
        }

        protected OAuthClientBase OAuthClient { get; set; }

        protected abstract UserData GetCurrentUser();

        protected virtual void AddCustomProperties(NameValueCollection properties)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.IsPostBack)
            {
                // Save the return Url in the cookie
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", this.RedirectURL)
                {
                    Expires = DateTime.Now.AddMinutes(5),
                    Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                });
            }

            bool shouldAuthorize = this.OAuthClient.IsCurrentService() && this.OAuthClient.HaveVerificationCode();
            if (this.Mode == AuthMode.Login)
            {
                shouldAuthorize = shouldAuthorize || this.OAuthClient.IsCurrentUserAuthorized();
            }

            if (shouldAuthorize)
            {
                if (this.OAuthClient.Authorize() == AuthorisationResult.Authorized)
                {
                    this.OAuthClient.AuthenticateUser(this.GetCurrentUser(), this.PortalSettings, this.IPAddress, this.AddCustomProperties, this.OnUserAuthenticated);
                }
            }
        }
    }
}
