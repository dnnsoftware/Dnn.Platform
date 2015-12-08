#region Copyright
//
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Web.Api.Auth.Jwt;
using DotNetNuke.Web.Api.Internal.Auth;
using DotNetNuke.Web.ConfigSection;
using Newtonsoft.Json;

namespace DotNetNuke.Web.Api.Auth
{
    /// <summary>
    /// This class implements Json Web Token (JWT) authentication scheme.
    /// For detailed description of JWT refer to:
    /// <para>- JTW standard https://tools.ietf.org/html/rfc7519 </para>
    /// <para>- Introduction to JSON Web Tokens http://jwt.io/introduction/ </para>
    /// </summary>
    public class JwtAuthMessageHandler : AuthMessageHandlerBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JwtAuthMessageHandler));

        public override string AuthScheme => JwtUtil.SchemeType;

        public JwtAuthMessageHandler(bool includeByDefault, SslModes sslMode)
            : base(includeByDefault, sslMode)
        {
        }

        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (NeedsAuthentication(request))
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (portalSettings != null)
                {
                    TryToAuthenticate(request, portalSettings);
                }
            }

            return base.OnInboundRequest(request, cancellationToken);
        }

        public override HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized && SupportsJwt(response.RequestMessage))
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(AuthScheme));
            }

            return base.OnOutboundResponse(response, cancellationToken);
        }

        private static bool SupportsJwt(HttpRequestMessage request)
        {
            return !IsXmlHttpRequest(request);
        }

        private void TryToAuthenticate(HttpRequestMessage request, PortalSettings portalSettings)
        {
            try
            {
                var username = ValidateToken(request, portalSettings);
                if (!string.IsNullOrEmpty(username))
                {
                    if (Logger.IsTraceEnabled) Logger.Trace($"Authenticated user {username} in portal {portalSettings.PortalId}");
                    SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(username, AuthScheme), null), request);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in authenticating the user. " + ex);
            }
        }

        private string ValidateToken(HttpRequestMessage request, PortalSettings portalSettings)
        {
            var authorization = JwtUtil.ValidateAuthHeader(request.Headers.Authorization);
            if (string.IsNullOrEmpty(authorization))
                return null;

            var parts = authorization.Split('.');
            if (parts.Length != 3)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Token must have [header:claims:signature] parts exactly");
                return null;
            }

            var header = JsonConvert.DeserializeObject<JwtHeader>(JwtUtil.DecodeBase64(parts[0]));
            var algorithm = ValidateTokenHeader(header);
            if (algorithm == JwtSupportedAlgorithms.Unsupported)
                return null;

            var claims = JsonConvert.DeserializeObject<DnnJwtClaim>(JwtUtil.DecodeBase64(parts[1]));
            var userInfo = ValidateTokenClaim(claims, portalSettings.PortalId);
            if (userInfo == null)
                return null;

            var secret = JwtUtil.ObtainSecret(claims, portalSettings.GUID, userInfo.CreatedOnDate);
            if (!ValidateSignature(algorithm, secret, parts))
                return null;

            return claims.Username;
        }

        private JwtSupportedAlgorithms ValidateTokenHeader(JwtHeader header)
        {
            if (!AuthScheme.Equals(header.Type, StringComparison.OrdinalIgnoreCase))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Unsupported authentication type " + header.Type);
                return JwtSupportedAlgorithms.Unsupported;
            }

            JwtSupportedAlgorithms algorithm;
            if (!Enum.TryParse(header.Algorithm, true, out algorithm))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Unsupported authentication algorithm " + algorithm);
                return JwtSupportedAlgorithms.Unsupported;
            }

            return algorithm;
        }

        private static UserInfo ValidateTokenClaim(DnnJwtClaim claim, int portalId)
        {
            if (portalId != claim.PortalId)
            {
                if (Logger.IsTraceEnabled)
                    Logger.TraceFormat(
                        "Invalid authentication. Portal ID passed ({0}) <> claim one ({1})", portalId, claim.PortalId);
                return null;
            }

            var validFrom = JwtUtil.EpochStart.AddSeconds(claim.NotBefore);
            var validTill = JwtUtil.EpochStart.AddSeconds(claim.Expiration);
            var now = DateTime.UtcNow;

            if (now < validFrom || now > validTill)
            {
                if (Logger.IsTraceEnabled)
                    Logger.TraceFormat("Token is outside allowed timeslot (from {0:O} to {1:O})", validFrom, validTill);
                return null;
            }

            var userInfo = UserController.GetUserByName(portalId, claim.Username);
            if (userInfo == null || userInfo.Username != claim.Username)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Invalid user name " + claim.Username);
                return null;
            }

            var status = UserController.ValidateUser(userInfo, portalId, false);
            var valid =
                status == UserValidStatus.VALID ||
                status == UserValidStatus.UPDATEPROFILE ||
                status == UserValidStatus.UPDATEPASSWORD;

            if (!valid && Logger.IsTraceEnabled)
            {
                Logger.Trace("Inactive user status: " + status);
                return null;
            }

            return userInfo;
        }

        private static bool ValidateSignature(JwtSupportedAlgorithms algorithm, byte[] secret, string[] tokenParts)
        {
            string algName;
            switch (algorithm)
            {
                case JwtSupportedAlgorithms.HS256:
                    algName = JwtUtil.DefaultJwtAlgorithm;
                    break;
                case JwtSupportedAlgorithms.HS384:
                    algName = "HMACSHA384";
                    break;
                case JwtSupportedAlgorithms.HS512:
                    algName = "HMACSHA512";
                    break;
                default:
                    if (Logger.IsTraceEnabled) Logger.Trace("Invalid algorithm value " + algorithm);
                    return false;
            }

            var computed = JwtUtil.ComputeSignature(algName, secret, tokenParts.Take(tokenParts.Length - 1));
            var tokenSig = tokenParts.Last();
            if (tokenSig.Equals(computed)) return true;
            if (Logger.IsTraceEnabled)
            {
                Logger.Trace("Received and computed token signatuers are not equal!" +
                    Environment.NewLine + "  Received signature = " + tokenSig +
                    Environment.NewLine + "  Computed signature = " + computed);
            }
            return false;
        }
    }
}