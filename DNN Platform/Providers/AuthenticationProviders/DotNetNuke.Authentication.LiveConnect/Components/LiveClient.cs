// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.LiveConnect.Components
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Script.Serialization;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;

    public class LiveClient : OAuthClientBase
    {
        public LiveClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Live")
        {
            // DNN-6464 Correct TokenEndpoint and AuthorizationEndpoint Urls
            // Add TokenMethod of Post to conform to other OAuth extensions
            this.TokenMethod = HttpMethod.POST;
            this.TokenEndpoint = new Uri("https://login.live.com/oauth20_token.srf");
            this.AuthorizationEndpoint = new Uri("https://login.live.com/oauth20_authorize.srf");
            this.MeGraphEndpoint = new Uri("https://apis.live.net/v5.0/me");

            this.Scope = HttpContext.Current.Server.UrlEncode("wl.signin wl.basic wl.emails");

            this.AuthTokenName = "LiveUserToken";

            this.OAuthVersion = "2.0";

            this.LoadTokenCookie(string.Empty);
        }

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
