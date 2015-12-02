#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using Newtonsoft.Json;

namespace DotNetNuke.Web.Api.Internal.Auth
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

        private const string AuthScheme = "Bearer";
        private const string DefaultType = "JWT";
        private const string DefaultAlgorithm = "HMACSHA256";
        private static readonly Encoding TextEncoder = Encoding.UTF8;

        private const int ClockSkew = 300; // in seconds; default for clock skew
        private const int TimeToLive = 3600; // in seconds; default token time is 1 hour
        private static readonly DateTime EpochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);

        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (NeedsAuthentication())
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (portalSettings != null)
                {
                    TryToAuthenticate(request, portalSettings.PortalId);
                }
            }

            return base.OnInboundRequest(request, cancellationToken);
        }

        private static bool SupportsJwt(HttpRequestMessage request)
        {
            return !IsXmlHttpRequest(request);
        }

        private static void TryToAuthenticate(HttpRequestMessage request, int portalId)
        {
            try
            {
                var username = ValidateToken(request, portalId);
                if (!string.IsNullOrEmpty(username))
                {
                    if (Logger.IsTraceEnabled) Logger.TraceFormat("Authenticated user {0} in portal {1}", username, portalId);
                    SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(username, DefaultType), null), request);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected problem in authenticating the user. " + ex);
            }
        }

        private static string ValidateToken(HttpRequestMessage request, int portalId)
        {
            var authorization = ValidateAuthHeader(request.Headers.Authorization);
            if (string.IsNullOrEmpty(authorization))
                return null;

            var parts = authorization.Split(':');
            if (parts.Length < 3)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Token must have [header:payload:signature] parts at least");
                return null;
            }

            var header = JsonConvert.DeserializeObject<JwtHeader>(DecodeBase64(parts[0]));
            var algorithm = ValidateTokenHeader(header);
            if (algorithm == JwtSupportedAlgorithms.Unsupported)
                return null;

            var payload = JsonConvert.DeserializeObject<DnnJwtPayload>(DecodeBase64(parts[1]));
            var userInfo = ValidateTokenPayload(payload, portalId);
            if (userInfo == null)
                return null;

            var secret = ObtainSecret(payload, userInfo);
            if (!ValidateSignature(algorithm, secret, parts))
                return null;

            return payload.Username;
        }

        public static string ValidateAuthHeader(AuthenticationHeaderValue authHdr)
        {
            if (authHdr == null)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Authorization header not present in the request");
                return null;
            }

            if (!string.Equals(authHdr.Scheme, AuthScheme, StringComparison.CurrentCultureIgnoreCase))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Authorization header scheme in the request is not equal to " + AuthScheme);
                return null;
            }

            var authorization = authHdr.Parameter;
            if (string.IsNullOrEmpty(authorization))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Missing authorization header value in the request");
                return null;
            }

            if (Logger.IsTraceEnabled) Logger.Trace("Authorization header received: " + authorization);


            var parts = authorization.Split(':');
            if (parts.Length < 3)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Token must have [header:payload:signature] parts at least");
                return null;
            }

            if (DecodeBase64(parts[0]).IndexOf("\"JWT\"", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("This is not a JWT autentication scheme.");
                return null;
            }

            return authorization;
        }

        private static JwtSupportedAlgorithms ValidateTokenHeader(JwtHeader header)
        {
            if (!DefaultType.Equals(header.Type, StringComparison.OrdinalIgnoreCase))
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

        private static UserInfo ValidateTokenPayload(DnnJwtPayload payload, int portalId)
        {
            if (portalId != payload.PortalId)
            {
                if (Logger.IsTraceEnabled)
                    Logger.TraceFormat(
                        "Invalid authentication. Portal ID passed ({0}) <> payload one ({1})", portalId, payload.PortalId);
                return null;
            }

            var validFrom = EpochStart.AddSeconds(payload.NotBefore);
            var validTill = EpochStart.AddSeconds(payload.Expiration);
            var now = DateTime.UtcNow;

            if (now < validFrom || now > validTill)
            {
                if (Logger.IsTraceEnabled)
                    Logger.TraceFormat("Token is outside allowed timeslot (from {0:O} to {1:O})", validFrom, validTill);
                return null;
            }

            var userInfo = UserController.GetUserByName(portalId, payload.Username);
            if (userInfo == null || userInfo.Username != payload.Username)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Invalid user name " + payload.Username);
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
                    algName = DefaultAlgorithm;
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

            var computed = ComputeSignature(algName, secret, tokenParts.Take(tokenParts.Length - 1));
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

        private static string ComputeSignature(string algorithmName, byte[] secret, IEnumerable<string> tokenParts)
        {
            using (var enc = HMAC.Create(algorithmName))
            {
                enc.Key = secret;
                var body = TextEncoder.GetBytes(string.Join(".", tokenParts));
                var sig =  Convert.ToBase64String(enc.ComputeHash(body)).TrimEnd('=');
                //if (Logger.IsTraceEnabled)
                //{
                //    Logger.Trace("Authorization Info:" +
                //        Environment.NewLine + "  SECRET => " + TextEncoder.GetString(secret) +
                //        Environment.NewLine + "  BODY   => " + string.Join(".", tokenParts) +
                //        Environment.NewLine + "  SIG.   => " + sig);
                //}
                return sig;
            }
        }

        private static byte[] ObtainSecret(DnnJwtPayload payload, UserInfo userInfo)
        {
            //TODO: change this to a more secure scheme after the proof of concept is established
            // This is just a proof of concept; the secret should contain components that
            // are not easily predicted and passed with the request of each session. It is better
            // to create and encrypt such keys and make them accessible only to the application.
            // moreover, we canuser the hashed password in the database as part of the secret.
            var stext = string.Join(":", payload.SessionId, payload.PortalId.ToString("x8"),
                userInfo.UserID.ToString("x8"), userInfo.CreatedOnDate.ToUniversalTime().ToString("O"));
            return TextEncoder.GetBytes(stext);
        }

        internal static string DecodeBase64(string b64Str)
        {
            // fix Base64 string padding
            var mod = b64Str.Length % 4;
            if (mod != 0) b64Str += new string('=', 4 - mod);
            return TextEncoder.GetString(Convert.FromBase64String(b64Str));
        }

        internal static string EncodeBase64(string plainStr)
        {
            return EncodeBase64(TextEncoder.GetBytes(plainStr)).TrimEnd('=');
        }

        internal static string EncodeBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }

        internal static string LoginUser(HttpRequestMessage request, LoginData loginData)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                Logger.Trace("portalSettings = null");
                return null;
            }

            var status = UserLoginStatus.LOGIN_FAILURE;
            var ipAddress = request.GetIPAddress() ?? "";
            var user = UserController.ValidateUser(portalSettings.PortalId,
                loginData.Username, loginData.Password, "DNN", "", AuthScheme, ipAddress, ref status);

            if (user == null)
            {
                Logger.Trace("user = null");
                return null;
            }

            var valid =
                status == UserLoginStatus.LOGIN_SUCCESS ||
                status == UserLoginStatus.LOGIN_SUPERUSER ||
                status == UserLoginStatus.LOGIN_INSECUREADMINPASSWORD ||
                status == UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD;

            if (!valid)
            {
                Logger.Trace("login status = " + status);
                return null;
            }

            SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(loginData.Username, DefaultType), null), request);

            var notBefore = (long)(DateTime.UtcNow - EpochStart).TotalSeconds;
            var payload = new DnnJwtPayload
            {
                Username = loginData.Username,
                Issuer = portalSettings.PortalAlias.HTTPAlias,
                NotBefore = notBefore - ClockSkew, // this is necessary in a web farm
                Expiration = notBefore + TimeToLive,
                SessionId = Guid.NewGuid().ToString("N"),
                PortalId = user.PortalID
            };

            var header = new JwtHeader
            {
                Type = DefaultType,
                Algorithm = JwtSupportedAlgorithms.HS256.ToString()
            };

            var parts = new object[] { header, payload }
                .Select(JsonConvert.SerializeObject).Select(EncodeBase64).ToArray();

            var secret = ObtainSecret(payload, user);
            var signature = ComputeSignature(DefaultAlgorithm, secret, parts);
            return string.Join(":", parts.Union(new[] { signature }));
        }
    }
}