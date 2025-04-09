// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Repositories;

    /// <inheritdoc />
    public class ApiTokenController : IApiTokenController
    {
        private const string AuthScheme = "Bearer";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ApiTokenController));
        private static readonly Encoding TextEncoder = Encoding.UTF8;

        private readonly IApiTokenRepository apiTokenRepository;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="ApiTokenController"/> class.</summary>
        /// <param name="apiTokenRepository">The API token repository.</param>
        /// <param name="eventLogger">The event logger.</param>
        public ApiTokenController(
            IApiTokenRepository apiTokenRepository,
            IEventLogger eventLogger)
        {
            this.apiTokenRepository = apiTokenRepository;
            this.eventLogger = eventLogger;
        }

        /// <inheritdoc />
        public string SchemeType => "ApiToken";

        private Abstractions.Portals.IPortalSettings PortalSettings => PortalController.Instance.GetCurrentSettings();

        /// <inheritdoc />
        public (ApiToken Token, UserInfo User) ValidateToken(HttpRequestMessage request)
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
        public void SetApiTokenForRequest(ApiToken token)
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
        public SortedDictionary<string, ApiTokenAttribute> ApiTokenKeyList(ApiTokenScope scope, string locale)
        {
            var res = new SortedDictionary<string, ApiTokenAttribute>();
            var typeLocator = new TypeLocator();
            var attributes = typeLocator.GetAllMatchingTypes(
                t => t != null &&
                     t.IsClass &&
                     !t.IsAbstract &&
                     t.IsVisible)
                .SelectMany(x => x.GetMethods())
                .SelectMany(m => m.GetCustomAttributes(typeof(ApiTokenAuthorizeAttribute), false))
                .Cast<ApiTokenAuthorizeAttribute>()
                .Where(a => a.Scope <= scope);

            foreach (var attr in attributes)
            {
                var key = attr.Key.ToLowerInvariant();
                var k = attr.Scope.ToString() + key;
                if (!res.ContainsKey(k))
                {
                    var name = DotNetNuke.Services.Localization.Localization.GetString(attr.Key + ".Text", attr.ResourceFile, locale);
                    if (string.IsNullOrEmpty(name))
                    {
                        name = attr.Key;
                    }

                    var description = DotNetNuke.Services.Localization.Localization.GetString(attr.Key + ".Help", attr.ResourceFile, locale);
                    res.Add(k, new ApiTokenAttribute((int)attr.Scope, key, name, description));
                }
            }

            return res;
        }

        /// <inheritdoc />
        public IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize)
        {
            return this.apiTokenRepository.GetApiTokens(scope, includeNarrowerScopes, portalId, userId, filter, apiKey, pageIndex, pageSize);
        }

        /// <inheritdoc />
        public string CreateApiToken(int portalId, string tokenName, ApiTokenScope scope, DateTime expiresOn, string apiKeys, int userId)
        {
            if (scope == ApiTokenScope.Host)
            {
                portalId = Null.NullInteger;
            }

            string newToken;
            using (var generator = RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[32];
                generator.GetBytes(tokenBytes);
                newToken = this.EncodeBase64(tokenBytes);
            }

            var tokenAndHostGuid = newToken + Entities.Host.Host.GUID;
            var hashedToken = this.GetHashedStr(tokenAndHostGuid);

            var token = new ApiTokenBase()
            {
                PortalId = portalId,
                TokenName = tokenName,
                Scope = scope,
                ExpiresOn = expiresOn,
                TokenHash = hashedToken,
            };
            var ret = this.apiTokenRepository.AddApiToken(token, apiKeys, userId);
            this.eventLogger.AddLog(ret.ToLogProps(), this.PortalSettings, userId, EventLogType.APITOKEN_CREATED.ToString(), false);
            return newToken;
        }

        /// <inheritdoc />
        public ApiToken GetApiToken(int apiTokenId)
        {
            return this.apiTokenRepository.GetApiToken(apiTokenId);
        }

        /// <inheritdoc />
        public void RevokeOrDeleteApiToken(ApiToken token, bool delete, int userId)
        {
            if (delete)
            {
                this.apiTokenRepository.DeleteApiToken(token.ToBase());
                this.eventLogger.AddLog(token.ToLogProps(), this.PortalSettings, userId, EventLogType.APITOKEN_DELETED.ToString(), false);
            }
            else
            {
                this.apiTokenRepository.RevokeApiToken(token.ToBase(), userId);
                this.eventLogger.AddLog(token.ToLogProps(), this.PortalSettings, userId, EventLogType.APITOKEN_REVOKED.ToString(), false);
            }
        }

        /// <inheritdoc />
        public void DeleteExpiredAndRevokedApiTokens(int portalId, int userId)
        {
            this.apiTokenRepository.DeleteExpiredAndRevokedApiTokens(portalId, userId);
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
                    Logger.Trace("Authorization header scheme in the request is not equal to " + AuthScheme);
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

        private (ApiToken Token, UserInfo User) ValidateAuthorizationValue(string authorization)
        {
            var tokenAndHostGuid = authorization + Entities.Host.Host.GUID;
            var hashedToken = this.GetHashedStr(tokenAndHostGuid);
            var apiToken = this.apiTokenRepository.GetApiToken(this.PortalSettings.PortalId, hashedToken);
            if (apiToken != null)
            {
                if (apiToken.ExpiresOn < DateUtils.GetDatabaseUtcTime() || apiToken.IsRevoked)
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace("Token expired");
                    }

                    this.eventLogger.AddLog("Token Auth", authorization, EventLogType.APITOKEN_AUTHENTICATION_FAILED);
                    return (null, null);
                }

                apiToken.TokenKeys = apiToken.Keys.Split(',').ToList();

                switch (apiToken.Scope)
                {
                    case ApiTokenScope.User:
                        var userInfo = UserController.GetUserById(this.PortalSettings.PortalId, apiToken.CreatedByUserId);
                        if (userInfo == null)
                        {
                            if (Logger.IsTraceEnabled)
                            {
                                Logger.Trace("Invalid user");
                            }

                            return (null, null);
                        }

                        this.apiTokenRepository.SetApiTokenLastUsed(apiToken);
                        return (apiToken, userInfo);

                    case ApiTokenScope.Portal:
                        if (apiToken.PortalId == this.PortalSettings.PortalId)
                        {
                            this.apiTokenRepository.SetApiTokenLastUsed(apiToken);
                            return (apiToken, null);
                        }

                        break;
                    case ApiTokenScope.Host:
                        if (apiToken.PortalId == -1)
                        {
                            this.apiTokenRepository.SetApiTokenLastUsed(apiToken);
                            return (apiToken, null);
                        }

                        break;
                }
            }

            this.eventLogger.AddLog("Token Auth", authorization, EventLogType.APITOKEN_AUTHENTICATION_FAILED);
            return (null, null);
        }

        private string EncodeBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }

        private string GetHashedStr(string data)
        {
            using (var hasher = SHA384.Create())
            {
                return this.EncodeBase64(hasher.ComputeHash(TextEncoder.GetBytes(data)));
            }
        }
    }
}
