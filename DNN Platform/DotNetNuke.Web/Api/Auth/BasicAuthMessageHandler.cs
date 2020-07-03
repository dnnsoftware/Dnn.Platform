// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth
{
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
    using DotNetNuke.Web.ConfigSection;

    public class BasicAuthMessageHandler : AuthMessageHandlerBase
    {
        private readonly Encoding _encoding = Encoding.GetEncoding("iso-8859-1");

        public BasicAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
        }

        public override string AuthScheme => "Basic";

        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.NeedsAuthentication(request))
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (portalSettings != null)
                {
                    this.TryToAuthenticate(request, portalSettings.PortalId);
                }
            }

            return base.OnInboundRequest(request, cancellationToken);
        }

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

            UserInfo user = UserController.ValidateUser(portalId, credentials.UserName, credentials.Password, "DNN", string.Empty,
                                                        "a portal", ipAddress ?? string.Empty, ref status);

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

            string decoded = this._encoding.GetString(Convert.FromBase64String(authorization));

            string[] parts = decoded.Split(new[] { ':' }, 2);
            if (parts.Length < 2)
            {
                return null;
            }

            return new UserCredentials(parts[0], parts[1]);
        }

        internal class UserCredentials
        {
            public UserCredentials(string userName, string password)
            {
                this.UserName = userName;
                this.Password = password;
            }

            public string Password { get; set; }

            public string UserName { get; set; }
        }
    }
}
