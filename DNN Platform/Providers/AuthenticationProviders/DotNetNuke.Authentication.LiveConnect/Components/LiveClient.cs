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
