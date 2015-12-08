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
using System.Text;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Web.Api.Internal.Auth;
using Newtonsoft.Json;

namespace DotNetNuke.Web.Api.Auth.Jwt
{
    internal static class JwtUtil
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JwtUtil));

        private const int ClockSkew = 300; // in seconds; default for clock skew
        private const int TimeToLive = 3600; // in seconds; default token time is 1 hour

        internal static readonly DateTime EpochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
        internal const string AuthScheme = "Bearer";
        internal const string SchemeType = "JWT";
        internal const string JwtCombiner = ".";
        internal const string DefaultJwtAlgorithm = "HMACSHA256";
        internal static readonly Encoding TextEncoder = Encoding.UTF8;

        /// <summary>
        /// Checks for Authorization header and validates it is JWT scheme. If successful, it returns the token string.
        /// </summary>
        /// <param name="authHdr">The request auhorization header.</param>
        /// <returns>The JWT passed in the request; otherwise, it returns null.</returns>
        public static string ValidateAuthHeader(AuthenticationHeaderValue authHdr)
        {
            if (authHdr == null)
            {
                //if (Logger.IsTraceEnabled) Logger.Trace("Authorization header not present in the request"); // too verbose; shows in all web requests
                return null;
            }

            if (!string.Equals(authHdr.Scheme, JwtUtil.SchemeType, StringComparison.CurrentCultureIgnoreCase))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Authorization header scheme in the request is not equal to " + JwtUtil.SchemeType);
                return null;
            }

            var authorization = authHdr.Parameter;
            if (string.IsNullOrEmpty(authorization))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Missing authorization header value in the request");
                return null;
            }

            if (Logger.IsTraceEnabled) Logger.Trace("Authorization header received: " + authorization);


            var parts = authorization.Split('.');
            if (parts.Length < 3)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Token must have [header:claims:signature] parts at least");
                return null;
            }

            if (JwtUtil.DecodeBase64(parts[0]).IndexOf("\"JWT\"", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("This is not a JWT autentication scheme.");
                return null;
            }

            return authorization;
        }

        internal static Tuple<string, string> LoginUser(HttpRequestMessage request, LoginData loginData)
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

            //SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(loginData.Username, DefaultType), null), request);

            var notBefore = (long)(DateTime.UtcNow - EpochStart).TotalSeconds;
            var claim = new DnnJwtClaim
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
                Type = SchemeType,
                Algorithm = JwtSupportedAlgorithms.HS256.ToString()
            };

            var parts = new object[] { header, claim }
                .Select(JsonConvert.SerializeObject).Select(EncodeBase64).ToArray();

            var secret = ObtainSecret(claim, portalSettings.GUID, user.CreatedOnDate);
            var signature = ComputeSignature(DefaultJwtAlgorithm, secret, parts);
            var token = string.Join(JwtCombiner, parts.Union(new[] { signature }));
            return new Tuple<string, string>(user.DisplayName, token);
        }

        internal static string ComputeSignature(string algorithmName, byte[] secret, IEnumerable<string> tokenParts)
        {
            using (var enc = HMAC.Create(algorithmName))
            {
                enc.Key = secret;
                var body = TextEncoder.GetBytes(string.Join(JwtCombiner, tokenParts));
                var sig = Convert.ToBase64String(enc.ComputeHash(body)).TrimEnd('=');
                //if (Logger.IsTraceEnabled)
                //{
                //    Logger.Trace("Authorization Info:" +
                //        Environment.NewLine + "  SECRET => " + TextEncoder.GetString(secret) +
                //        Environment.NewLine + "  BODY   => " + string.Join(JwtCombiner, tokenParts) +
                //        Environment.NewLine + "  SIG.   => " + sig);
                //}
                return sig;
            }
        }

        internal static byte[] ObtainSecret(DnnJwtClaim claim, Guid portalGuid, DateTime userCreationDate)
        {
            //TODO: change this to a more secure scheme after the proof of concept is established
            // This is just a proof of concept; the secret should contain components that
            // are not easily predicted and passed with the request of each session. It is better
            // to create and encrypt such keys and make them accessible only to the application.
            // moreover, we canuser the hashed password in the database as part of the secret.
            var stext = string.Join(JwtCombiner, claim.SessionId, portalGuid.ToString("N"), userCreationDate.ToUniversalTime().ToString("O"));
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
    }
}
