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