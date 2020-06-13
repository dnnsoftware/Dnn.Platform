// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Twitter.Components
{
    using System;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;

    public class TwitterClient : OAuthClientBase
    {
        public TwitterClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Twitter")
        {
            this.AuthorizationEndpoint = new Uri("https://api.twitter.com/oauth/authorize");
            this.RequestTokenEndpoint = new Uri("https://api.twitter.com/oauth/request_token");
            this.RequestTokenMethod = HttpMethod.POST;
            this.TokenEndpoint = new Uri("https://api.twitter.com/oauth/access_token");
            this.MeGraphEndpoint = new Uri("https://api.twitter.com/1.1/account/verify_credentials.json");

            this.AuthTokenName = "TwitterUserToken";

            this.OAuthVersion = "1.0";

            this.LoadTokenCookie(string.Empty);
        }
    }
}
