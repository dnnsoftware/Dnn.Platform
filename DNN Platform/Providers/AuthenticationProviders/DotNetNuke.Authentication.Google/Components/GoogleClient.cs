// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Google.Components;

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;

using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.OAuth;

/// <inheritdoc/>
public class GoogleClient : OAuthClientBase
{
    /// <summary>Initializes a new instance of the <see cref="GoogleClient"/> class.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="mode">The auth mode.</param>
    public GoogleClient(int portalId, AuthMode mode)
        : base(portalId, mode, "Google")
    {
        this.TokenEndpoint = new Uri("https://accounts.google.com/o/oauth2/token");
        this.TokenMethod = HttpMethod.POST;
        this.AuthorizationEndpoint = new Uri("https://accounts.google.com/o/oauth2/auth");
        this.MeGraphEndpoint = new Uri("https://www.googleapis.com/oauth2/v1/userinfo");

        this.Scope = HttpUtility.UrlEncode("https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email");

        this.AuthTokenName = "GoogleUserToken";

        this.OAuthVersion = "2.0";

        this.LoadTokenCookie(string.Empty);
    }

    /// <inheritdoc/>
    protected override TimeSpan GetExpiry(string responseText)
    {
        var jsonSerializer = new JavaScriptSerializer();
        var tokenDictionary = jsonSerializer.DeserializeObject(responseText) as Dictionary<string, object>;

        return new TimeSpan(0, 0, Convert.ToInt32(tokenDictionary["expires_in"]));
    }

    /// <inheritdoc/>
    protected override string GetToken(string responseText)
    {
        var jsonSerializer = new JavaScriptSerializer();
        var tokenDictionary = jsonSerializer.DeserializeObject(responseText) as Dictionary<string, object>;
        return Convert.ToString(tokenDictionary["access_token"]);
    }
}
