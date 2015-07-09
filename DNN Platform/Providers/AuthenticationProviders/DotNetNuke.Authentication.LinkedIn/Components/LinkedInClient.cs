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
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

#endregion

namespace DotNetNuke.Authentication.LinkedIn.Components
{
    public class LinkedInClient : OAuthClientBase
    {
        #region Constructors

        public LinkedInClient(int portalId, AuthMode mode) : base(portalId, mode, "LinkedIn")
        {
            RequestTokenEndpoint = new Uri("https://www.linkedin.com/uas/oauth2/accessToken");
            RequestTokenMethod = HttpMethod.POST;
            TokenEndpoint = new Uri("https://www.linkedin.com/uas/oauth2/accessToken");
            TokenMethod = HttpMethod.POST;
            AuthorizationEndpoint = new Uri("https://www.linkedin.com/uas/oauth2/authorization");
            MeGraphEndpoint = new Uri("https://api.linkedin.com/v1/people/~:(firstName,lastName,emailAddress,pictureUrl,location:(name))/");

            Scope = "r_basicprofile r_emailaddress";

            AuthTokenName = "LinkedInUserToken";

            OAuthVersion = "2.0";

            AccessToken = "format=json&oauth2_access_token";

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