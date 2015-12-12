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

namespace Dnn.AuthServices.Jwt.Components.Common.Controllers
{
    internal class JwtController : ServiceLocator<IJwtController, JwtController>, IJwtController
    {
        #region constants, properties, etc.

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JwtController));
        private static readonly HashAlgorithm Hasher = SHA384.Create();

        private const int ClockSkew = 5; // in minutes; default for clock skew
        private const int SessionTokenTtl = 60; // in minutes = 1 hour
        private const int RenewalTokenTtl = 14; // in days = 2 weeks
        private const string SessionClaimType = "sid";
        private static readonly Encoding TextEncoder = Encoding.UTF8;

        public const string AuthScheme = "Bearer";
        public string SchemeType => "JWT";

        #endregion

        #region constructors / instantiators

        protected override Func<IJwtController> GetFactory()
        {
            return () => new JwtController();
        }

        public readonly IDataService _dataProvider;

        public JwtController() : this(new DataService())
        {
        }

        private JwtController(IDataService dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        #region interface implementation

        /// <summary>
        /// Validates the received JWT against the databas eand returns username when successful.
        /// </summary>
        public string ValidateToken(HttpRequestMessage request)
        {
            var authorization = ValidateAuthHeader(request.Headers.Authorization);
            if (string.IsNullOrEmpty(authorization))
                return null;

            var parts = authorization.Split('.');
            if (parts.Length != 3)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Token must have [header:claims:signature] parts exactly");
                return null;
            }

            var header = JsonConvert.DeserializeObject<JwtHeader>(DecodeBase64(parts[0]));
            if (!IsValidSchemeType(header))
                return null;

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                Logger.Trace("Unable to retrieve portal settings");
                return null;
            }

            var tuple = ValidateJwtAndGetUser(authorization, portalSettings, true);
            return tuple?.Item2?.Username; // item2 holds user info
        }

        public bool LogoutUser(HttpRequestMessage request)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(SchemeType + " is not registered/enabled in web.config file");
                return false;
            }

            var rawToken = ValidateAuthHeader(request.Headers.Authorization);
            if (string.IsNullOrEmpty(rawToken))
            {
                //Logger.Trace("Invalid authorization header."); // already logged something in validation call
                return false;
            }

            var jwt = new JwtSecurityToken(rawToken);
            var sessionId = GetJwtSessionValue(jwt);
            if (string.IsNullOrEmpty(sessionId))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Session ID not found in the claim");
                return false;
            }

            _dataProvider.DeleteToken(sessionId);
            return true;
        }

        /// <summary>
        /// Validates user login credentials and reurns result when successful
        /// </summary>
        public LoginResultData LoginUser(HttpRequestMessage request, LoginData loginData)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(SchemeType + " is not registered/enabled in web.config file");
                return EmptyWithError("disabled");
            }

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                Logger.Trace("portalSettings = null");
                return EmptyWithError("no-portal");
            }

            var status = UserLoginStatus.LOGIN_FAILURE;
            var ipAddress = request.GetIPAddress() ?? "";
            var userInfo = UserController.ValidateUser(portalSettings.PortalId,
                loginData.Username, loginData.Password, "DNN", "", AuthScheme, ipAddress, ref status);

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

            var sessionId = NewGuid;
            var renewalToken = EncodeBase64(Hasher.ComputeHash(Guid.NewGuid().ToByteArray()));
            var secret = ObtainSecret(sessionId, portalSettings.GUID, userInfo.CreatedOnDate);
            var jwt = CreateJwtToken(secret, portalSettings.PortalAlias.HTTPAlias, sessionId, userInfo.Roles);
            var accessToken = jwt.RawData;
            var now = DateTime.UtcNow;

            // save hash values in DB so no one with access can create JWT header from existing data
            var ptoken = new PersistedToken
            {
                TokenId = sessionId,
                UserId = userInfo.UserID,
                TokenExpiry = now.AddMinutes(SessionTokenTtl),
                RenewalExpiry = now.AddDays(RenewalTokenTtl),
                TokenHash = GetHashedStr(accessToken),
                RenewalHash = GetHashedStr(renewalToken),
            };

            _dataProvider.AddToken(ptoken);

            return new LoginResultData
            {
                DisplayName = userInfo.DisplayName,
                AccessToken = accessToken,
                RenewalToken = renewalToken
            };
        }

        public LoginResultData RenewToken(HttpRequestMessage request, string renewalToken)
        {
            if (!JwtAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(SchemeType + " is not registered/enabled in web.config file");
                return EmptyWithError("disabled");
            }

            var rawToken = ValidateAuthHeader(request.Headers.Authorization);
            if (string.IsNullOrEmpty(rawToken))
            {
                //Logger.Trace("Invalid authorization header."); // already logged something in validation call
                return EmptyWithError("bad-credentials");
            }

            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings == null)
            {
                Logger.Trace("Unable to retrieve portal settings");
                return null;
            }

            var tuple = ValidateJwtAndGetUser(rawToken, portalSettings, false);
            var jwt = tuple?.Item1;
            var userInfo = tuple?.Item2;

            var sessionId = GetJwtSessionValue(jwt);
            if (string.IsNullOrEmpty(sessionId))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Session ID not found in the claim");
                return EmptyWithError("bad-claims");
            }

            var ptoken = _dataProvider.GetTokenById(sessionId);
            if (ptoken == null)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Token not found in DB");
                return EmptyWithError("not-found");
            }

            if ((ptoken.TokenHash != GetHashedStr(rawToken)) ||
                (ptoken.UserId != userInfo?.UserID))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Mismatch in received token");
                return EmptyWithError("bad-token");
            }

            var secret = ObtainSecret(sessionId, portalSettings.GUID, userInfo.CreatedOnDate);
            jwt = CreateJwtToken(secret, portalSettings.PortalAlias.HTTPAlias, sessionId, userInfo.Roles);
            var accessToken = jwt.RawData;
            var now = DateTime.UtcNow;
            var expiry = now.AddMinutes(SessionTokenTtl);
            if (expiry > ptoken.RenewalExpiry) expiry = ptoken.RenewalExpiry; // don't extend beyond renewal expiry

            // save hash values in DB so no one with access can create JWT header from existing data
            var newptoken = new PersistedToken
            {
                TokenId = sessionId,
                UserId = userInfo.UserID,
                TokenExpiry = expiry,
                RenewalExpiry = ptoken.RenewalExpiry,
                TokenHash = GetHashedStr(accessToken),
                RenewalHash = ptoken.RenewalHash,
            };

            _dataProvider.UpdateToken(newptoken);

            return new LoginResultData
            {
                DisplayName = userInfo.DisplayName,
                AccessToken = accessToken,
                RenewalToken = renewalToken
            };
        }

        #endregion

        #region private methods

        private static string NewGuid => Guid.NewGuid().ToString("N");

        private static LoginResultData EmptyWithError(string error)
        {
            return new LoginResultData { Error = error };
        }

        private static JwtSecurityToken CreateJwtToken(byte[] symmetricKey, string issuer, string sessionId, IEnumerable<string> roles)
        {
            //var key = Convert.FromBase64String(symmetricKey);
            var credentials = new SigningCredentials(
                new InMemorySymmetricSecurityKey(symmetricKey),
                "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                "http://www.w3.org/2001/04/xmlenc#sha256");

            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(SessionClaimType, sessionId));
            claimsIdentity.AddClaims(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var now = DateTime.UtcNow;
            var notBefore = now.AddMinutes(-ClockSkew);
            var notAfter = now.AddMinutes(SessionTokenTtl);
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(issuer, null, claimsIdentity, notBefore, notAfter, credentials);
            return token;
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
                //if (Logger.IsTraceEnabled) Logger.Trace("Authorization header not present in the request"); // too verbose; shows in all web requests
                return null;
            }

            if (!string.Equals(authHdr.Scheme, AuthScheme, StringComparison.CurrentCultureIgnoreCase))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Authorization header scheme in the request is not equal to " + SchemeType);
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

            if (DecodeBase64(parts[0]).IndexOf("\"" + SchemeType + "\"", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                if (Logger.IsTraceEnabled) Logger.Trace($"This is not a {SchemeType} autentication scheme.");
                return null;
            }

            return authorization;
        }

        private bool IsValidSchemeType(JwtHeader header)
        {
            if (!SchemeType.Equals(header.Typ, StringComparison.OrdinalIgnoreCase))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Unsupported authentication type " + header.Typ);
                return false;
            }

            return true;
        }

        private Tuple<JwtSecurityToken, UserInfo> ValidateJwtAndGetUser(string rawToken, PortalSettings portalSettings, bool checkExpiry)
        {
            var jwt = new JwtSecurityToken(rawToken);

            if (checkExpiry)
            {
                var now = DateTime.UtcNow;
                if (now < jwt.ValidFrom || now > jwt.ValidTo)
                {
                    if (Logger.IsTraceEnabled) Logger.Trace("Token is expired");
                    return null;
                }
            }

            var sessionId = GetJwtSessionValue(jwt);
            if (string.IsNullOrEmpty(sessionId))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Invaid session ID claim");
                return null;
            }

            // validate against DB saved data
            var ptoken = _dataProvider.GetTokenById(sessionId);
            if (ptoken == null)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Token not found in DB");
                return null;
            }

            if (ptoken.TokenHash != GetHashedStr(rawToken))
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Mismatch data in received token");
                return null;
            }

            var userInfo = UserController.GetUserById(portalSettings.PortalId, ptoken.UserId);
            if (userInfo == null)
            {
                if (Logger.IsTraceEnabled) Logger.Trace("Invalid user");
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

            return new Tuple<JwtSecurityToken, UserInfo>(jwt, userInfo);
        }

        private static string GetJwtSessionValue(JwtSecurityToken jwt)
        {
            var sessionClaim = jwt?.Claims?.FirstOrDefault(claim => SessionClaimType.Equals(claim.Type));
            return sessionClaim?.Value;
        }

        private byte[] ObtainSecret(string sessionId, Guid portalGuid, DateTime userCreationDate)
        {
            // The secret should contain unpredicted components htat can't be inferred from the auth-header of requests.
            var stext = string.Join(".", sessionId, portalGuid.ToString("N"), userCreationDate.ToUniversalTime().ToString("O"));
            return TextEncoder.GetBytes(stext);
        }

        private static string DecodeBase64(string b64Str)
        {
            // fix Base64 string padding
            var mod = b64Str.Length % 4;
            if (mod != 0) b64Str += new string('=', 4 - mod);
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

        #endregion
    }
}