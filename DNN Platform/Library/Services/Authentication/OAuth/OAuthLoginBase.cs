#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

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
                                                             {Expires = DateTime.Now.AddMinutes(5)});
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