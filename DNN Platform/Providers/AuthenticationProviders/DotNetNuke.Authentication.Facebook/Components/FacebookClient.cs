// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections.Generic;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.Facebook.Components
{
    public class FacebookClient : OAuthClientBase
    {
        #region Constructors

        public FacebookClient(int portalId, AuthMode mode) 
            : base(portalId, mode, "Facebook")
        {
            TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token");
            TokenMethod = HttpMethod.GET;
            AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize");
            MeGraphEndpoint = new Uri("https://graph.facebook.com/me?fields=id,name,email,first_name,last_name,link,birthday,gender,locale,timezone,updated_time");

            Scope = "email";

            AuthTokenName = "FacebookUserToken";

            OAuthVersion = "2.0";

            LoadTokenCookie(String.Empty);
        }

        #endregion

        protected override TimeSpan GetExpiry(string responseText)
        {
            TimeSpan expiry = TimeSpan.MinValue;
            if (!string.IsNullOrEmpty(responseText))
            {
                var dictionary = Json.Deserialize<IDictionary<string, object>>(responseText);
                expiry = new TimeSpan(0, 0, Convert.ToInt32(dictionary["expires_in"]));
            }
            return expiry;
        }

        protected override string GetToken(string responseText)
        {
            string authToken = String.Empty;
            if (!string.IsNullOrEmpty(responseText))
            {
                var dictionary = Json.Deserialize<IDictionary<string, object>>(responseText);
                authToken = Convert.ToString(dictionary["access_token"]);
            }
            return authToken;
        }
    }
}
