// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Membership;
using DotNetNuke.Web.Api.Internal.Auth;
using DotNetNuke.Web.ConfigSection;

namespace DotNetNuke.Web.Api.Auth
{
    public class DigestAuthMessageHandler : AuthMessageHandlerBase
    {
        public override string AuthScheme => DigestAuthentication.AuthenticationScheme;

        public DigestAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
        }

        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (NeedsAuthentication(request))
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (portalSettings != null)
                {
                    var isStale = TryToAuthenticate(request, portalSettings.PortalId);

                    if (isStale)
                    {
                        var staleResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
                        AddStaleWwwAuthenticateHeader(staleResponse);

                        return staleResponse;
                    }
                }
            }

            return base.OnInboundRequest(request, cancellationToken);
        }

        public override HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized && SupportsDigestAuth(response.RequestMessage))
            {
                AddWwwAuthenticateHeader(response);
            }

            return base.OnOutboundResponse(response, cancellationToken);
        }

        private void AddStaleWwwAuthenticateHeader(HttpResponseMessage response)
        {
            AddWwwAuthenticateHeader(response, true);
        }

        private void AddWwwAuthenticateHeader(HttpResponseMessage response, bool isStale = false)
        {
            var value = string.Format("realm=\"DNNAPI\", nonce=\"{0}\",  opaque=\"0000000000000000\", stale={1}, algorithm=MD5, qop=\"auth\"", CreateNewNonce(), isStale);
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(AuthScheme, value));
        }

        private static string CreateNewNonce()
        {
            DateTime nonceTime = DateTime.Now + TimeSpan.FromMinutes(1);
            string expireStr = nonceTime.ToString("G");

            byte[] expireBytes = Encoding.Default.GetBytes(expireStr);
            string nonce = Convert.ToBase64String(expireBytes);

            nonce = nonce.TrimEnd(new[] { '=' });
            return nonce;
        }

        private static bool TryToAuthenticate(HttpRequestMessage request, int portalId)
        {
            if (request?.Headers.Authorization == null)
            {
                return false;
            }

            string authHeader = request?.Headers.Authorization.ToString();

            var digestAuthentication = new DigestAuthentication(new DigestAuthenticationRequest(authHeader, request.Method.Method), portalId, request.GetIPAddress());

            if (digestAuthentication.IsValid)
            {
                SetCurrentPrincipal(digestAuthentication.User, request);
            }
            else if(digestAuthentication.IsNonceStale)
            {
                return true;
            }

            return false;
        }

        private bool SupportsDigestAuth(HttpRequestMessage request)
        {
            return !IsXmlHttpRequest(request) && MembershipProviderConfig.PasswordRetrievalEnabled;
        }
    }
}
