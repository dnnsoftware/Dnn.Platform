// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Specialized;
using System.Web;
using DotNetNuke.Common;

#endregion

namespace DotNetNuke.Services.Authentication.OAuth
{
    public abstract class OAuthLoginBase : AuthenticationLoginBase
    {
        protected virtual string AuthSystemApplicationName
        {
            get { return String.Empty; }
        }

        protected OAuthClientBase OAuthClient { get; set; }

        protected abstract UserData GetCurrentUser();

        protected virtual void AddCustomProperties(NameValueCollection properties)
        {
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                //Save the return Url in the cookie
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", RedirectURL)
                {
                    Expires = DateTime.Now.AddMinutes(5),
                    Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                });
            }

            bool shouldAuthorize = OAuthClient.IsCurrentService() && OAuthClient.HaveVerificationCode();
            if(Mode == AuthMode.Login)
            {
                shouldAuthorize = shouldAuthorize || OAuthClient.IsCurrentUserAuthorized();
            }

            if (shouldAuthorize)
            {
                if (OAuthClient.Authorize() == AuthorisationResult.Authorized)
                {
                    OAuthClient.AuthenticateUser(GetCurrentUser(), PortalSettings, IPAddress, AddCustomProperties, OnUserAuthenticated);
                }
            }
        }

        #region Overrides of AuthenticationLoginBase

        public override bool Enabled
        {
            get { return OAuthConfigBase.GetConfig(AuthSystemApplicationName, PortalId).Enabled; }
        }

        #endregion
    }
}
