// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

namespace DotNetNuke.Web.Api.Auth
{
    public class BasicAuthMessageHandler : AuthMessageHandlerBase
    {
        public override string AuthScheme => "Basic";

        private readonly Encoding _encoding = Encoding.GetEncoding("iso-8859-1");

        public BasicAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
        }

        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(NeedsAuthentication(request))
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (portalSettings != null)
                {
                    TryToAuthenticate(request, portalSettings.PortalId);
                }
            }

            return base.OnInboundRequest(request, cancellationToken);
        }

        public override HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized && SupportsBasicAuth(response.RequestMessage))
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(AuthScheme, "realm=\"DNNAPI\""));
            }

            return base.OnOutboundResponse(response, cancellationToken);
        }

        private bool SupportsBasicAuth(HttpRequestMessage request)
        {
            return !IsXmlHttpRequest(request);
        }

        private void TryToAuthenticate(HttpRequestMessage request, int portalId)
        {
            UserCredentials credentials = GetCredentials(request);

            if (credentials == null)
            {
                return;
            }

            var status = UserLoginStatus.LOGIN_FAILURE;
            string ipAddress = request.GetIPAddress();

            UserInfo user = UserController.ValidateUser(portalId, credentials.UserName, credentials.Password, "DNN", "",
                                                        "a portal", ipAddress ?? "", ref status);

            if (user != null)
            {
                SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(credentials.UserName, AuthScheme), null), request);
            }
        }

        private UserCredentials GetCredentials(HttpRequestMessage request)
        {
            if (request?.Headers.Authorization == null)
            {
                return null;
            }

            if (request?.Headers.Authorization.Scheme.ToLower() != AuthScheme.ToLower())
            {
                return null;
            }

            string authorization = request?.Headers.Authorization.Parameter;
            if (String.IsNullOrEmpty(authorization))
            {
                return null;
            }

            string decoded = _encoding.GetString(Convert.FromBase64String(authorization));

            string[] parts = decoded.Split(new[] {':'}, 2);
            if (parts.Length < 2)
            {
                return null;
            }

            return new UserCredentials(parts[0], parts[1]);
        }

        #region Nested type: UserCredentials

        internal class UserCredentials
        {
            public UserCredentials(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }

            public string Password { get; set; }
            public string UserName { get; set; }
        }

        #endregion
    }
}
