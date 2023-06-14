// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Repositories;

    /// <inheritdoc />
    internal class ApiTokenController : ServiceLocator<IApiTokenController, ApiTokenController>, IApiTokenController
    {
        private const string AuthScheme = "Bearer";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ApiTokenController));
        private static readonly HashAlgorithm Hasher = SHA384.Create();
        private static readonly Encoding TextEncoder = Encoding.UTF8;

        private readonly Abstractions.Portals.IPortalSettings portalSettings = PortalController.Instance.GetCurrentSettings();

        /// <inheritdoc />
        public string SchemeType => "ApiToken";

        /// <inheritdoc />
        public (ApiToken, UserInfo) ValidateToken(HttpRequestMessage request)
        {
            if (!ApiTokenAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(this.SchemeType + " is not registered/enabled in web.config file");
                return (null, null);
            }

            var authorization = this.ValidateAuthHeader(request?.Headers.Authorization);
            return string.IsNullOrEmpty(authorization) ? (null, null) : this.ValidateAuthorizationValue(authorization);
        }

        /// <inheritdoc />
        public void SetCurrentThreadApiToken(ApiToken token)
        {
            HttpContext.Current.Items["ApiToken"] = token;
        }

        /// <inheritdoc />
        public ApiToken GetCurrentThreadApiToken()
        {
            if (HttpContext.Current != null && HttpContext.Current.Items["ApiToken"] is ApiToken token)
            {
                return token;
            }

            return null;
        }

        /// <inheritdoc />
        protected override Func<IApiTokenController> GetFactory()
        {
            return () => new ApiTokenController();
        }

        private string ValidateAuthHeader(AuthenticationHeaderValue authHdr)
        {
            if (authHdr == null)
            {
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

        private (ApiToken, UserInfo) ValidateAuthorizationValue(string authorization)
        {
            var tokenAndHostGuid = authorization + Entities.Host.Host.GUID;
            var hashedToken = this.GetHashedStr(tokenAndHostGuid);
            var apiToken = ApiTokenRepository.Instance.GetApiToken(this.portalSettings.PortalId, hashedToken);
            if (apiToken != null)
            {
                if (apiToken.ExpiresOn < DateTime.UtcNow || apiToken.IsRevoked)
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace("Token expired");
                    }

                    return (null, null);
                }

                apiToken.TokenKeys = ApiTokenRepository.Instance.GetApiTokenKeys(apiToken.ApiTokenId);

                switch (apiToken.Scope)
                {
                    case ApiTokenScope.User:
                        var userInfo = UserController.GetUserById(this.portalSettings.PortalId, apiToken.CreatedByUserID);
                        if (userInfo == null)
                        {
                            if (Logger.IsTraceEnabled)
                            {
                                Logger.Trace("Invalid user");
                            }

                            return (null, null);
                        }

                        return (apiToken, userInfo);

                    case ApiTokenScope.Portal:
                        if (apiToken.PortalId == this.portalSettings.PortalId)
                        {
                            return (apiToken, null);
                        }

                        break;
                    case ApiTokenScope.Host:
                        if (apiToken.PortalId == -1)
                        {
                            return (apiToken, null);
                        }

                        break;
                }
            }

            return (null, null);
        }

        private string EncodeBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }

        private string GetHashedStr(string data)
        {
            return this.EncodeBase64(Hasher.ComputeHash(TextEncoder.GetBytes(data)));
        }
    }
}
