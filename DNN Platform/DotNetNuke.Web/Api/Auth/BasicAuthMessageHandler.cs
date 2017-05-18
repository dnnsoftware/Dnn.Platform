#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
            if (request.Headers.Authorization == null)
            {
                return null;
            }

            if (request.Headers.Authorization.Scheme.ToLower() != AuthScheme.ToLower())
            {
                return null;
            }

            string authorization = request.Headers.Authorization.Parameter;
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