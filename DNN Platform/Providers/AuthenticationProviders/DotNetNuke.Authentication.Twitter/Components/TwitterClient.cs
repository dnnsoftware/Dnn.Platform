#region Usings

using System;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.Twitter.Components
{
    public class TwitterClient : OAuthClientBase
    {
        public TwitterClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Twitter")
        {
            AuthorizationEndpoint = new Uri("https://api.twitter.com/oauth/authorize");
            RequestTokenEndpoint = new Uri("https://api.twitter.com/oauth/request_token");
            RequestTokenMethod = HttpMethod.POST;
            TokenEndpoint = new Uri("https://api.twitter.com/oauth/access_token");
            MeGraphEndpoint = new Uri("https://api.twitter.com/1.1/account/verify_credentials.json");

            AuthTokenName = "TwitterUserToken";

            OAuthVersion = "1.0";

            LoadTokenCookie(String.Empty);
        }
    }
}
