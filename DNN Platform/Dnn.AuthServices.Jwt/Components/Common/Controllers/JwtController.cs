// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Components.Common.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;

    using Dnn.AuthServices.Jwt.Auth;
    using Dnn.AuthServices.Jwt.Components.Entity;
    using Dnn.AuthServices.Jwt.Data;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Web.Api;

    using Microsoft.IdentityModel.JsonWebTokens;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>Controls JWT features.</summary>
    internal class JwtController : ServiceLocator<IJwtController, JwtController>, IJwtController
    {
        /// <summary>The name of the authentication scheme header.</summary>
        public const string AuthScheme = "Bearer";

        private const int ClockSkew = 5; // in minutes; default for clock skew
        private const int SessionTokenTtl = 60; // in minutes = 1 hour

        private const int RenewalTokenTtl = 14; // in days = 2 weeks
        private const string SessionClaimType = "sid";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JwtController));
        private static readonly HashAlgorithm Hasher = SHA384.Create();
        private static readonly Encoding TextEncoder = Encoding.UTF8;

        private static object hasherLock = new object();

        /// <inheritdoc/>
        public string SchemeType => "JWT";

        /// <summary>Gets or sets a reference to the DNN data provider.</summary>
        public IDataService DataProvider { get; set; } = DataService.Instance;

        private static string NewSessionId => DateTime.UtcNow.Ticks.ToString("x16") + Guid.NewGuid().ToString("N").Substring(16);

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

            var jwt = new JsonWebToken(rawToken);
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

        /// <inheritdoc/>
        public LoginResultData LoginUser(HttpRequestMessage request, LoginData loginData)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(this.SchemeType + " is not registered/enabled in web.config file");
                return EmptyWithError("disabled");
            }

#pragma warning disable 618 // Obsolete
            var obsoletePortalSettings = PortalController.Instance.GetCurrentPortalSettings();
#pragma warning restore 618 // Obsolete

            IPortalSettings portalSettings = obsoletePortalSettings;
            if (portalSettings == null)
            {
                Logger.Trace("portalSettings = null");
                return EmptyWithError("no-portal");
            }

            IPortalAliasInfo portalAlias = obsoletePortalSettings.PortalAlias;
            var status = UserLoginStatus.LOGIN_FAILURE;
            var ipAddress = request.GetIPAddress() ?? string.Empty;
            var userInfo = UserController.ValidateUser(
                portalSettings.PortalId,
                loginData.Username,
                loginData.Password,
                "DNN",
                string.Empty,
                AuthScheme,
                ipAddress,
                ref status);

            if (userInfo == null)
            {
                Logger.Trace("user = null");
                return EmptyWithError("bad-credentials");
            }

            var valid =
                status == UserLoginStatus.LOGIN_SUCCESS ||
                status == UserLoginStatus.LOGIN_SUPERUSER ||
#pragma warning disable 618 // Obsolete
                status == UserLoginStatus.LOGIN_INSECUREADMINPASSWORD ||
                status == UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD;
#pragma warning restore 618 // Obsolete

            if (!valid)
            {
                Logger.Trace("login status = " + status);
                return EmptyWithError("bad-credentials");
            }

            // save hash values in DB so no one with access can create JWT header from existing data
            var sessionId = NewSessionId;
            var now = DateTime.UtcNow;
            string renewalToken;
            lock (hasherLock)
            {
                renewalToken = EncodeBase64(Hasher.ComputeHash(Guid.NewGuid().ToByteArray()));
            }

            var persistedToken = new PersistedToken
            {
                TokenId = sessionId,
                UserId = userInfo.UserID,
                TokenExpiry = now.AddMinutes(SessionTokenTtl),
                RenewalExpiry = now.AddDays(RenewalTokenTtl),
                RenewalHash = GetHashedStr(renewalToken),
            };

            var secret = ObtainSecret(sessionId, portalSettings.GUID, userInfo.Membership.LastPasswordChangeDate);
            var accessToken = CreateJwtToken(
                secret,
                portalAlias.HttpAlias,
                persistedToken,
                userInfo.Roles);

            persistedToken.TokenHash = GetHashedStr(accessToken);
            this.DataProvider.AddToken(persistedToken);

            return new LoginResultData
            {
                UserId = userInfo.UserID,
                DisplayName = userInfo.DisplayName,
                AccessToken = accessToken,
                RenewalToken = renewalToken,
            };
        }

        /// <inheritdoc/>
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

            var persistedToken = this.DataProvider.GetTokenById(sessionId);
            if (persistedToken == null)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Token not found in DB");
                }

                return EmptyWithError("not-found");
            }

            if (persistedToken.RenewalExpiry <= DateTime.UtcNow)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Token can't bwe renewed anymore");
                }

                return EmptyWithError("not-more-renewal");
            }

            if (persistedToken.RenewalHash != GetHashedStr(renewalToken))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Invalid renewal token");
                }

                return EmptyWithError("bad-token");
            }

            if (persistedToken.TokenHash != GetHashedStr(rawToken))
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

            if (persistedToken.UserId != userInfo.UserID)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Mismatch token and user");
                }

                return EmptyWithError("bad-token");
            }

            return this.UpdateToken(renewalToken, persistedToken, userInfo);
        }

        /// <inheritdoc/>
        protected override Func<IJwtController> GetFactory()
        {
            return () => new JwtController();
        }

        private static LoginResultData EmptyWithError(string error)
        {
            return new LoginResultData { Error = error };
        }

        private static string CreateJwtToken(byte[] symmetricKey, string issuer, PersistedToken persistedToken, IEnumerable<string> roles)
        {
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(symmetricKey),
                "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                "http://www.w3.org/2001/04/xmlenc#sha256");

            var subject = new ClaimsIdentity();
            subject.AddClaim(new Claim(SessionClaimType, persistedToken.TokenId));

            // Add roles using both the standard schema URI (ClaimTypes.Role) for standards compliance
            // and the legacy "role" claim type for backward compatibility with existing consumers
            foreach (var role in roles)
            {
                subject.AddClaim(new Claim(ClaimTypes.Role, role));
                subject.AddClaim(new Claim("role", role));
            }

            // Add deprecation notice for the legacy "role" claim format
            subject.AddClaim(new Claim(
                "dnn:deprecation:role",
                "The role claim is deprecated. Use http://schemas.microsoft.com/ws/2008/06/identity/claims/role instead. The role claim will be removed in DNN v12.0.0."));

            var notBefore = DateTime.UtcNow.AddMinutes(-ClockSkew);
            var expires = persistedToken.TokenExpiry;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                NotBefore = notBefore,
                Expires = expires,
                Subject = subject,
                SigningCredentials = signingCredentials,
            };
            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }

        private static JsonWebToken GetAndValidateJwt(string rawToken, bool checkExpiry)
        {
            JsonWebToken jwt;
            try
            {
                jwt = new JsonWebToken(rawToken);
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
                    Logger.Trace("Invalid session ID claim");
                }

                return null;
            }

            return jwt;
        }

        private static string GetJwtSessionValue(JsonWebToken jwt)
        {
            var sessionClaim = jwt?.Claims?.FirstOrDefault(claim => SessionClaimType.Equals(claim.Type));
            return sessionClaim?.Value;
        }

        private static byte[] ObtainSecret(string sessionId, Guid portalGuid, DateTime userCreationDate)
        {
            // The secret should contain unpredictable components that can't be inferred from the JWT string.
            var secretText = string.Join(".", sessionId, portalGuid.ToString("N"), userCreationDate.ToUniversalTime().ToString("O"));
            return TextEncoder.GetBytes(secretText);
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
            string hash;
            lock (hasherLock)
            {
                hash = EncodeBase64(Hasher.ComputeHash(TextEncoder.GetBytes(data)));
            }

            return hash;
        }

        private LoginResultData UpdateToken(string renewalToken, PersistedToken persistedToken, UserInfo userInfo)
        {
            var expiry = DateTime.UtcNow.AddMinutes(SessionTokenTtl);
            if (expiry > persistedToken.RenewalExpiry)
            {
                // don't extend beyond renewal expiry and make sure it is marked in UTC
                expiry = new DateTime(persistedToken.RenewalExpiry.Ticks, DateTimeKind.Utc);
            }

            persistedToken.TokenExpiry = expiry;

#pragma warning disable 618 // Obsolete
            var obsoletePortalSettings = PortalController.Instance.GetCurrentPortalSettings();
#pragma warning restore 618 // Obsolete
            IPortalSettings portalSettings = obsoletePortalSettings;
            IPortalAliasInfo portalAlias = obsoletePortalSettings.PortalAlias;
            var secret = ObtainSecret(persistedToken.TokenId, portalSettings.GUID, userInfo.Membership.LastPasswordChangeDate);
            var accessToken = CreateJwtToken(secret, portalAlias.HttpAlias, persistedToken, userInfo.Roles);

            // save hash values in DB so no one with access can create JWT header from existing data
            persistedToken.TokenHash = GetHashedStr(accessToken);
            this.DataProvider.UpdateToken(persistedToken);

            return new LoginResultData
            {
                UserId = userInfo.UserID,
                DisplayName = userInfo.DisplayName,
                AccessToken = accessToken,
                RenewalToken = renewalToken,
            };
        }

        /// <summary>Checks for Authorization header and validates it is JWT scheme. If successful, it returns the token string.</summary>
        /// <param name="authHdr">The request authorization header.</param>
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
                    Logger.Trace($"This is not a {this.SchemeType} authentication scheme.");
                }

                return null;
            }

            var jwt = GetAndValidateJwt(authorization, true);
            if (jwt == null)
            {
                return null;
            }

            if (!this.IsValidSchemeType(jwt))
            {
                return null;
            }

            var userInfo = this.TryGetUser(jwt, true);
            return userInfo?.Username;
        }

        private bool IsValidSchemeType(JsonWebToken token)
        {
            if (!this.SchemeType.Equals(token.Typ, StringComparison.OrdinalIgnoreCase))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Unsupported authentication scheme type " + token.Typ);
                }

                return false;
            }

            return true;
        }

        private UserInfo TryGetUser(JsonWebToken jwt, bool checkExpiry)
        {
            // validate against DB saved data
            var sessionId = GetJwtSessionValue(jwt);
            var persistedToken = this.DataProvider.GetTokenById(sessionId);
            if (persistedToken == null)
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
                if (now > persistedToken.TokenExpiry || now > persistedToken.RenewalExpiry)
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace("DB Token is expired");
                    }

                    return null;
                }
            }

            if (persistedToken.TokenHash != GetHashedStr(jwt.EncodedToken))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Mismatch data in received token");
                }

                return null;
            }

            var portalSettings = PortalController.Instance.GetCurrentSettings();
            if (portalSettings == null)
            {
                Logger.Trace("Unable to retrieve portal settings");
                return null;
            }

            var userInfo = UserController.GetUserById(portalSettings.PortalId, persistedToken.UserId);
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

            if (!valid)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Inactive user status: " + status);
                }

                return null;
            }

            if (!userInfo.Membership.Approved)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Non Approved user id: " + userInfo.UserID + " UserName: " + userInfo.Username);
                }

                return null;
            }

            if (userInfo.IsDeleted)
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Deleted user id: " + userInfo.UserID + " UserName: " + userInfo.Username);
                }

                return null;
            }

            return userInfo;
        }
    }
}
