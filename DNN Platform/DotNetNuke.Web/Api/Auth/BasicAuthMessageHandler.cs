// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;

/// <summary>Basic authentication message handler.</summary>
public class BasicAuthMessageHandler : AuthMessageHandlerBase
{
    private readonly Encoding encoding = Encoding.GetEncoding("iso-8859-1");

    /// <summary>Initializes a new instance of the <see cref="BasicAuthMessageHandler"/> class.</summary>
    /// <param name="includeByDefault">Should this handler be included by default on all routes.</param>
    /// <param name="forceSsl">Should this handler enforce SSL usage.</param>
    public BasicAuthMessageHandler(bool includeByDefault, bool forceSsl)
        : base(includeByDefault, forceSsl)
    {
    }

    /// <summary>Gets the name of the authentication scheme.</summary>
    public override string AuthScheme => "Basic";

    /// <inheritdoc/>
    public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (this.NeedsAuthentication(request))
        {
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            if (portalSettings != null)
            {
                this.TryToAuthenticate(request, portalSettings.PortalId);
            }
        }

        return base.OnInboundRequest(request, cancellationToken);
    }

    /// <inheritdoc/>
    public override HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized && this.SupportsBasicAuth(response.RequestMessage))
        {
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(this.AuthScheme, "realm=\"DNNAPI\""));
        }

        return base.OnOutboundResponse(response, cancellationToken);
    }

    private bool SupportsBasicAuth(HttpRequestMessage request)
    {
        return !IsXmlHttpRequest(request);
    }

    private void TryToAuthenticate(HttpRequestMessage request, int portalId)
    {
        UserCredentials credentials = this.GetCredentials(request);

        if (credentials == null)
        {
            return;
        }

        var status = UserLoginStatus.LOGIN_FAILURE;
        string ipAddress = request.GetIPAddress();

        UserInfo user = UserController.ValidateUser(
            portalId,
            credentials.UserName,
            credentials.Password,
            "DNN",
            string.Empty,
            "a portal",
            ipAddress ?? string.Empty,
            ref status);

        if (user != null)
        {
            SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(credentials.UserName, this.AuthScheme), null), request);
        }
    }

    private UserCredentials GetCredentials(HttpRequestMessage request)
    {
        if (request?.Headers.Authorization == null)
        {
            return null;
        }

        if (request?.Headers.Authorization.Scheme.ToLower() != this.AuthScheme.ToLower())
        {
            return null;
        }

        string authorization = request?.Headers.Authorization.Parameter;
        if (string.IsNullOrEmpty(authorization))
        {
            return null;
        }

        string decoded = this.encoding.GetString(Convert.FromBase64String(authorization));

        string[] parts = decoded.Split(new[] { ':' }, 2);
        if (parts.Length < 2)
        {
            return null;
        }

        return new UserCredentials(parts[0], parts[1]);
    }

    /// <summary>Represents the user credentials.</summary>
    internal class UserCredentials
    {
        /// <summary>Initializes a new instance of the <see cref="UserCredentials"/> class.</summary>
        /// <param name="userName">The user username.</param>
        /// <param name="password">The user password.</param>
        public UserCredentials(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }

        /// <summary>Gets or sets the password for the user.</summary>
        public string Password { get; set; }

        /// <summary>Gets or sets the username for the user.</summary>
        public string UserName { get; set; }
    }
}
