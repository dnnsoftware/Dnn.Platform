#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using OAuth.AuthorizationServer.Core.Utilities;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.OAUTHLogin
// ReSharper restore CheckNamespace
{

    public partial class OAUTHLogin : CDefault
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdLogin.Click += cmdLogin_Click;
        }
       
        private void cmdLogin_Click(object sender, EventArgs e)
        {
            PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings != null)
            {


                UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
                UserInfo objUser = UserController.ValidateUser(portalSettings.PortalId,
                    txtUsername.Text,
                    txtPassword.Text,
                    "DNN",
                    "",
                    PortalSettings.PortalName,
                    AuthenticationLoginBase.GetIPAddress(),
                    ref loginStatus);
                if (loginStatus == UserLoginStatus.LOGIN_SUCCESS || loginStatus == UserLoginStatus.LOGIN_SUPERUSER)
                {
                    
                    string returnUrl = Request.QueryString["ReturnUrl"].ToString();
                    // No checking of password for this sample.  Just care about the username
                    // as that's what we're including in the token to send back to the authorization server

                    // Corresponds to shared secret the authorization server knows about for this resource
                    const string encryptionKey = "WebAPIsAreAwesome";

                    // Build token with info the authorization server needs to know
                    // var tokenContent = txtUsername.Text+ ";" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + ";" + model.RememberMe;
                    var tokenContent = txtUsername.Text + ";" + DateTime.Now.ToString(CultureInfo.InvariantCulture) +
                                       ";" + "False";
                    var encryptedToken = EncodingUtility.Encode(tokenContent, encryptionKey);


                    returnUrl = Request.QueryString["ReturnUrl"].ToString() + "&scope=DNN-ALL&redirect_uri=" + Request.QueryString["redirect_uri"].ToString() + "&response_type=token";
                        if (returnUrl.Contains("client_id=client1"))
                        {
                            returnUrl=returnUrl + "&resource-authentication-token=";
                        }
                        else
	{
                            returnUrl=returnUrl + "&client_id=client1&resource-authentication-token=";
	}
                        
                    
                    
                    //  returnUrl += "resource-authentication-token=" + encryptedToken;

                    returnUrl += encryptedToken;
                  


                    var url = new Uri(returnUrl);


                    var redirectUrl = url.ToString();

                    // URL Encode the values of the querystring parameters
                    if (url.Query.Length > 1)
                    {
                        // var helper = new UrlHelper(HttpContext.Request.RequestContext);
                        //var helper = HttpUtility.UrlEncode(HttpContext.Request.RequestContext);
                        var helper = HttpUtility.UrlEncode(HttpContext.Current.Request.ToString());
                        var qsParts = HttpUtility.ParseQueryString(url.Query);
                        //redirectUrl = url.GetLeftPart(UriPartial.Path) + "?" + String.Join("&", qsParts.AllKeys.Select(x => x + "=" + helper.Encode(qsParts[x])));
                        redirectUrl = url.GetLeftPart(UriPartial.Path) + "?" +
                                      String.Join("&",
                                          qsParts.AllKeys.Select(x => x + "=" + HttpUtility.UrlEncode(qsParts[x])));
                    }

                    //return Redirect(redirectUrl);
                    Response.Redirect(redirectUrl);
                }
            }


        }
    }
}