#region Usings

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.Google.Components
{
    public class GoogleClient : OAuthClientBase
    {
        #region Constructors

        public GoogleClient(int portalId, AuthMode mode) 
            : base(portalId, mode, "Google")
        {
            TokenEndpoint = new Uri("https://accounts.google.com/o/oauth2/token");
            TokenMethod = HttpMethod.POST;
            AuthorizationEndpoint = new Uri("https://accounts.google.com/o/oauth2/auth");
            MeGraphEndpoint = new Uri("https://www.googleapis.com/oauth2/v1/userinfo");

            Scope = HttpUtility.UrlEncode("https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email");

            AuthTokenName = "GoogleUserToken";

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
