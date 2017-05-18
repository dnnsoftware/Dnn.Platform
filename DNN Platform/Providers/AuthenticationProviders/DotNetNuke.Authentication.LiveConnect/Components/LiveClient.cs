#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.LiveConnect.Components
{
    public class LiveClient : OAuthClientBase
    {
        #region Constructors

        public LiveClient(int portalId, AuthMode mode) : base(portalId, mode, "Live")
        {
            //DNN-6464 Correct TokenEndpoint and AuthorizationEndpoint Urls
            // Add TokenMethod of Post to conform to other OAuth extensions
            TokenMethod = HttpMethod.POST;
            TokenEndpoint = new Uri("https://login.live.com/oauth20_token.srf");
            AuthorizationEndpoint = new Uri("https://login.live.com/oauth20_authorize.srf");
            MeGraphEndpoint = new Uri("https://apis.live.net/v5.0/me");

            Scope = HttpContext.Current.Server.UrlEncode("wl.signin wl.basic wl.emails");

            AuthTokenName = "LiveUserToken";

            OAuthVersion = "2.0";

            LoadTokenCookie(String.Empty);
        }

        #endregion

        protected override TimeSpan GetExpiry(string responseText)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var tokenDictionary = jsonSerializer.DeserializeObject(responseText) as Dictionary<string, object>;

            return new TimeSpan(0, 0, Convert.ToInt32(tokenDictionary["expires_in"]));
        }

        protected override string GetToken(string responseText)
        {
            var jsonSerializer = new JavaScriptSerializer();
            var tokenDictionary = jsonSerializer.DeserializeObject(responseText) as Dictionary<string, object>;
            return Convert.ToString(tokenDictionary["access_token"]);
        }
    }
}
