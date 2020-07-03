// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Common.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;

    using Dnn.AuthServices.Jwt.Auth;
    using Dnn.AuthServices.Jwt.Components.Entity;
    using Dnn.AuthServices.Jwt.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Web.Api;
    using Newtonsoft.Json;

    internal class JwtController : ServiceLocator<IJwtController, JwtController>, IJwtController
    {
        public const string AuthScheme = "Bearer";
        public readonly IDataService DataProvider = DataService.Instance;

        private const int ClockSkew = 5; // in minutes; default for clock skew
        private const int SessionTokenTtl = 60; // in minutes = 1 hour

        private const int RenewalTokenTtl = 14; // in days = 2 weeks
        private const string SessionClaimType = "sid";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JwtController));
        private static readonly HashAlgorithm Hasher = SHA384.Create();
        private static readonly Encoding TextEncoder = Encoding.UTF8;

        public string SchemeType => "JWT";

        private static string NewSessionId => DateTime.UtcNow.Ticks.ToString("x16") + Guid.NewGuid().ToString("N").Substring(16);

        /// <summary>
        /// Validates the received JWT against the databas eand returns username when successful.
        /// </summary>
        /// <returns></returns>
        public string ValidateToken(HttpRequestMessage request)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(this.SchemeType + " is not registered/enabled in web.config file");
                return null;
            }

            var authorization = this.ValidateAuthHeader(request?.Headers.Authorization);
            return string.IsNullOrEmpty(authorization) ? null : this.ValidateAuthorizationValue(authorization);
        }

        public bool LogoutUser(HttpRequestMessage request)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(this.SchemeType + " is not registered/enabled in web.config file");
                return false;
            }

            var rawToken = this.ValidateAuthHeader(request?.Headers.Authorization);
            if (string.IsNullOrEmpty(rawToken))
            {
                return false;
            }

            var jwt = new JwtSecurityToken(rawToken);
            var sessionId = GetJwtSessionValue(jwt);
            if (string.IsNullOrEmpty(sessionId))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Session ID not found in the claim");
                }

                return false;
            }

            this.DataProvider.DeleteToken(sessionId);
            return true;
        }

        /// <summary>
        /// Validates user login credentials and returns result when successful.
        /// </summary>
        /// <returns></returns>
        public LoginResultData LoginUser(HttpRequestMessage request, LoginData loginData)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(this.SchemeType + " is not registered/enabled in web.config file");
                return EmptyWithError("disabled");
            }

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                Logger.Trace("portalSettings = null");
                return EmptyWithError("no-portal");
            }

            var status = UserLoginStatus.LOGIN_FAILURE;
            var ipAddress = request.GetIPAddress() ?? string.Empty;
            var userInfo = UserController.ValidateUser(
                portalSettings.PortalId,
                loginData.Username, loginData.Password, "DNN", string.Empty, AuthScheme, ipAddress, ref status);

            if (userInfo == null)
            {
                Logger.Trace("user = null");
                return EmptyWithError("bad-credentials");
            }

            var valid =
                status == UserLoginStatus.LOGIN_SUCCESS ||
                status == UserLoginStatus.LOGIN_SUPERUSER ||
                status == UserLoginStatus.LOGIN_INSECUREADMINPASSWORD ||
                status == UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD;

            if (!valid)
            {
                Logger.Trace("login status = " + status);
                return EmptyWithError("bad-credentials");
            }

            // save hash values in DB so no one with access can create JWT header from existing data
            var sessionId = NewSessionId;
            var now = DateTime.UtcNow;
            var renewalToken = EncodeBase64(Hasher.ComputeHash(Guid.NewGuid().ToByteArray()));
            var ptoken = new PersistedToken
            {
                TokenId = sessionId,
                UserId = userInfo.UserID,
                TokenExpiry = now.AddMinutes(SessionTokenTtl),
                RenewalExpiry = now.AddDays(RenewalTokenTtl),
                RenewalHash = GetHashedStr(renewalToken),
            };

            var secret = ObtainSecret(sessionId, portalSettings.GUID, userInfo.Membership.LastPasswordChangeDate);
            var jwt = CreateJwtToken(secret, portalSettings.PortalAlias.HTTPAlias, ptoken, userInfo.Roles);
            var accessToken = jwt.RawData;

            ptoken.TokenHash = GetHashedStr(accessToken);
            this.DataProvider.AddToken(ptoken);

            return new LoginResultData
            {
                UserId = userInfo.UserID,
                DisplayName = userInfo.DisplayName,
                AccessToken = accessToken,
                RenewalToken = renewalToken,
            };
        }

        public LoginResultData RenewToken(HttpRequestMessage request, string renewalToken)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(this.SchemeType + " is not registered/enabled in web.config file");
                return EmptyWithError("disabled");
            }

            var rawToken = this.ValidateAuthHeader(request?.Headers.Authorization);
            if (string.IsNullOrEmpty(rawToken))
            {
                return EmptyWithError("bad-credentials");
            }

            var jwt = GetAndValidateJwt(rawToken, false);
            if (jwt == null)
            {
                return EmptyWithError("bad-jwt");
            }

            var sessionId = GetJwtSessionValue(jwt);
            if (string.IsNullOrEmpty(sessionId))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Session ID not found in the claim");
                }

                return EmptyWithError("bad-claims");
            }

            var ptoken = this.DataProvider.GetTokenById(sessionId);
            if (ptoken == null)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Token not found in DB");
                }

                return EmptyWithError("not-found");
            }

            if (ptoken.RenewalExpiry <= DateTime.UtcNow)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Token can't bwe renewed anymore");
                }

                return EmptyWithError("not-more-renewal");
            }

            if (ptoken.RenewalHash != GetHashedStr(renewalToken))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Invalid renewal token");
                }

                return EmptyWithError("bad-token");
            }

            if (ptoken.TokenHash != GetHashedStr(rawToken))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Invalid access token");
                }

                return EmptyWithError("bad-token");
            }

            var userInfo = this.TryGetUser(jwt, false);
            if (userInfo == null)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("User not found in DB");
                }

                return EmptyWithError("not-found");
            }

            if (ptoken.UserId != userInfo.UserID)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Mismatch token and user");
                }

                return EmptyWithError("bad-token");
            }

            return this.UpdateToken(renewalToken, ptoken, userInfo);
        }

        protected override Func<IJwtController> GetFactory()
        {
            return () => new JwtController();
        }

        private static LoginResultData EmptyWithError(string error)
        {
            return new LoginResultData { Error = error };
        }

        private static JwtSecurityToken CreateJwtToken(byte[] symmetricKey, string issuer, PersistedToken ptoken, IEnumerable<string> roles)
        {
            // var key = Convert.FromBase64String(symmetricKey);
            var credentials = new SigningCredentials(
                new InMemorySymmetricSecurityKey(symmetricKey),
                "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                "http://www.w3.org/2001/04/xmlenc#sha256");

            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(SessionClaimType, ptoken.TokenId));
            claimsIdentity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var notBefore = DateTime.UtcNow.AddMinutes(-ClockSkew);
            var notAfter = ptoken.TokenExpiry;
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(issuer, null, claimsIdentity, notBefore, notAfter, credentials);
            return token;
        }

        private static JwtSecurityToken GetAndValidateJwt(string rawToken, bool checkExpiry)
        {
            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityToken(rawToken);
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to construct JWT object from authorization value. " + ex.Message);
                return null;
            }

            if (checkExpiry)
            {
                var now = DateTime.UtcNow;
                if (now < jwt.ValidFrom || now > jwt.ValidTo)
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace("Token is expired");
                    }

                    return null;
                }
            }

            var sessionId = GetJwtSessionValue(jwt);
            if (string.IsNullOrEmpty(sessionId))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Invaid session ID claim");
                }

                return null;
            }

            return jwt;
        }

        private static string GetJwtSessionValue(JwtSecurityToken jwt)
        {
            var sessionClaim = jwt?.Claims?.FirstOrDefault(claim => SessionClaimType.Equals(claim.Type));
            return sessionClaim?.Value;
        }

        private static byte[] ObtainSecret(string sessionId, Guid portalGuid, DateTime userCreationDate)
        {
            // The secret should contain unpredictable components that can't be inferred from the JWT string.
            var stext = string.Join(".", sessionId, portalGuid.ToString("N"), userCreationDate.ToUniversalTime().ToString("O"));
            return TextEncoder.GetBytes(stext);
        }

        private static string DecodeBase64(string b64Str)
        {
            // fix Base64 string padding
            var mod = b64Str.Length % 4;
            if (mod != 0)
            {
                b64Str += new string('=', 4 - mod);
            }

            return TextEncoder.GetString(Convert.FromBase64String(b64Str));
        }

        private static string EncodeBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }

        private static string GetHashedStr(string data)
        {
            return EncodeBase64(Hasher.ComputeHash(TextEncoder.GetBytes(data)));
        }

        private LoginResultData UpdateToken(string renewalToken, PersistedToken ptoken, UserInfo userInfo)
        {
            var expiry = DateTime.UtcNow.AddMinutes(SessionTokenTtl);
            if (expiry > ptoken.RenewalExpiry)
            {
                // don't extend beyond renewal expiry and make sure it is marked in UTC
                expiry = new DateTime(ptoken.RenewalExpiry.Ticks, DateTimeKind.Utc);
            }

            ptoken.TokenExpiry = expiry;

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var secret = ObtainSecret(ptoken.TokenId, portalSettings.GUID, userInfo.Membership.LastPasswordChangeDate);
            var jwt = CreateJwtToken(secret, portalSettings.PortalAlias.HTTPAlias, ptoken, userInfo.Roles);
            var accessToken = jwt.RawData;

            // save hash values in DB so no one with access can create JWT header from existing data
            ptoken.TokenHash = GetHashedStr(accessToken);
            this.DataProvider.UpdateToken(ptoken);

            return new LoginResultData
            {
                UserId = userInfo.UserID,
                DisplayName = userInfo.DisplayName,
                AccessToken = accessToken,
                RenewalToken = renewalToken,
            };
        }

        /// <summary>
        /// Checks for Authorization header and validates it is JWT scheme. If successful, it returns the token string.
        /// </summary>
        /// <param name="authHdr">The request auhorization header.</param>
        /// <returns>The JWT passed in the request; otherwise, it returns null.</returns>
        private string ValidateAuthHeader(AuthenticationHeaderValue authHdr)
        {
            if (authHdr == null)
            {
                // if (Logger.IsTraceEnabled) Logger.Trace("Authorization header not present in the request"); // too verbose; shows in all web requests
                return null;
            }

            if (!string.Equals(authHdr.Scheme, AuthScheme, StringComparison.CurrentCultureIgnoreCase))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Authorization header scheme in the request is not equal to " + this.SchemeType);
                }

                return null;
            }

            var authorization = authHdr.Parameter;
            if (string.IsNullOrEmpty(authorization))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Missing authorization header value in the request");
                }

                return null;
            }

            return authorization;
        }

        private string ValidateAuthorizationValue(string authorization)
        {
            var parts = authorization.Split('.');
            if (parts.Length < 3)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Token must have [header:claims:signature] parts at least");
                }

                return null;
            }

            var decoded = DecodeBase64(parts[0]);
            if (decoded.IndexOf("\"" + this.SchemeType + "\"", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace($"This is not a {this.SchemeType} autentication scheme.");
                }

                return null;
            }

            var header = JsonConvert.DeserializeObject<JwtHeader>(decoded);
            if (!this.IsValidSchemeType(header))
            {
                return null;
            }

            var jwt = GetAndValidateJwt(authorization, true);
            if (jwt == null)
            {
                return null;
            }

            var userInfo = this.TryGetUser(jwt, true);
            return userInfo?.Username;
        }

        private bool IsValidSchemeType(JwtHeader header)
        {
            if (!this.SchemeType.Equals(header["typ"] as string, StringComparison.OrdinalIgnoreCase))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Unsupported authentication scheme type " + header.Typ);
                }

                return false;
            }

            return true;
        }

        private UserInfo TryGetUser(JwtSecurityToken jwt, bool checkExpiry)
        {
            // validate against DB saved data
            var sessionId = GetJwtSessionValue(jwt);
            var ptoken = this.DataProvider.GetTokenById(sessionId);
            if (ptoken == null)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Token not found in DB");
                }

                return null;
            }

            if (checkExpiry)
            {
                var now = DateTime.UtcNow;
                if (now > ptoken.TokenExpiry || now > ptoken.RenewalExpiry)
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace("DB Token is expired");
                    }

                    return null;
                }
            }

            if (ptoken.TokenHash != GetHashedStr(jwt.RawData))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Mismatch data in received token");
                }

                return null;
            }

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                Logger.Trace("Unable to retrieve portal settings");
                return null;
            }

            var userInfo = UserController.GetUserById(portalSettings.PortalId, ptoken.UserId);
            if (userInfo == null)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Invalid user");
                }

                return null;
            }

            var status = UserController.ValidateUser(userInfo, portalSettings.PortalId, false);
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
    }
}
