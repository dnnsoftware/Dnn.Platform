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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using OAuth.AuthorizationServer.Core.Utilities;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.OAUTHLogin
// ReSharper restore CheckNamespace
{

    public partial class OAUTHLogin : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdLogin.Click += cmdLogin_Click;
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            //string returnUrl = "http://localhost/dnn_platform/DesktopModules/internalservices/API/OAUth/Authorize";
            string returnUrl = Request.QueryString["ReturnUrl"].ToString();
            // No checking of password for this sample.  Just care about the username
            // as that's what we're including in the token to send back to the authorization server
            
            // Corresponds to shared secret the authorization server knows about for this resource
            const string encryptionKey = "WebAPIsAreAwesome";

            // Build token with info the authorization server needs to know
           // var tokenContent = txtUsername.Text+ ";" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + ";" + model.RememberMe;
            var tokenContent = txtUsername.Text + ";" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + ";" + "False";
            var encryptedToken = EncodingUtility.Encode(tokenContent, encryptionKey);

            // Redirect back to the authorization server, including the authentication token 
            // Name of authentication token corresponds to that known by the authorization server
           // returnUrl += (returnUrl.Contains("?") ? "&" : "?");

            returnUrl =
                "http://localhost/dnn_platform/DesktopModules/internalservices/API/OAUth/Authorize?scope=Resource1-Read&redirect_uri=http://localhost:51090/TokenRequest/CacheTokenFromImplicitFlow&response_type=token&client_id=client1&resource-authentication-token=";

          //  returnUrl += "resource-authentication-token=" + encryptedToken;

            returnUrl +=  encryptedToken;
            //returnUrl += "&scope=Resource1-Read";
            // returnUrl +="&response_type=" + Request.QueryString["response_type"].ToString();
            //if (Request.QueryString["state"] != null)
            //{
            //    returnUrl += "&state=" + Request.QueryString["state"].ToString();
            //}
            
            //returnUrl += "&redirect_uri=" + Request.QueryString["redirect_uri"].ToString();


            //copy of original code
            // Redirect back to the authorization server, including the authentication token 
            // Name of authentication token corresponds to that known by the authorization server
            //returnUrl += (returnUrl.Contains("?") ? "&" : "?");
            //returnUrl += "resource-authentication-token=" + encryptedToken;
            //returnUrl += "&scope=Resource1-Read";
            //returnUrl += "&response_type=" + Request.QueryString["response_type"].ToString();
            //if (Request.QueryString["state"] != null)
            //{
            //    returnUrl += "&state=" + Request.QueryString["state"].ToString();
            //}

            //returnUrl += "&redirect_uri=" + Request.QueryString["redirect_uri"].ToString();

            
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
                redirectUrl = url.GetLeftPart(UriPartial.Path) + "?" + String.Join("&", qsParts.AllKeys.Select(x => x + "=" + HttpUtility.UrlEncode(qsParts[x])));
            }

            //return Redirect(redirectUrl);
            Response.Redirect(redirectUrl);
        }
    }
}