// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Repositories
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <inheritdoc />
    internal class ApiTokenRepository : IApiTokenRepository
    {
        private readonly IDataContext dataContext;

        public ApiTokenRepository(IDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        /// <inheritdoc />
        public ApiToken GetApiToken(int portalId, string tokenHash)
        {
            var rep = this.dataContext.GetRepository<ApiToken>();
            return rep.Find("WHERE (PortalId=@0 OR PortalId=-1) AND TokenHash=@1 AND IsDeleted=0", portalId, tokenHash).FirstOrDefault();
        }

        /// <inheritdoc />
        public ApiToken GetApiToken(int apiTokenId)
        {
            var rep = this.dataContext.GetRepository<ApiToken>();
            return rep.GetById(apiTokenId);
        }

        /// <inheritdoc />
        public List<string> GetApiTokenKeys(int apiTokenId)
        {
            var rep = this.dataContext.GetRepository<ApiTokenKey>();
            return rep.Find("WHERE ApiTokenId=@0", apiTokenId).Select(k => k.TokenKey).ToList();
        }

        /// <inheritdoc />
        public ApiToken AddApiToken(ApiTokenBase apiToken, string apiKeys, int userId)
        {
            Requires.NotNull(apiToken);
            apiToken.CreatedByUserId = userId;
            apiToken.CreatedOnDate = DateUtils.GetDatabaseUtcTime();
            var rep = this.dataContext.GetRepository<ApiTokenBase>();
            rep.Insert(apiToken);

            var repKeys = this.dataContext.GetRepository<ApiTokenKey>();
            foreach (var key in apiKeys.Split(','))
            {
                repKeys.Insert(new ApiTokenKey { ApiTokenId = apiToken.ApiTokenId, TokenKey = key });
            }

            return this.GetApiToken(apiToken.ApiTokenId);
        }

        /// <inheritdoc />
        public void RevokeApiToken(ApiTokenBase apiToken, int userId)
        {
            Requires.NotNull(apiToken);
            apiToken.IsRevoked = true;
            apiToken.RevokedOnDate = DateUtils.GetDatabaseUtcTime();
            apiToken.RevokedByUserId = userId;
            var rep = this.dataContext.GetRepository<ApiTokenBase>();
            rep.Update(apiToken);
        }

        /// <inheritdoc />
        public void DeleteApiToken(ApiTokenBase apiToken)
        {
            Requires.NotNull(apiToken);
            var rep = this.dataContext.GetRepository<ApiTokenBase>();
            apiToken.IsDeleted = true;
            rep.Update(apiToken);
        }

        /// <inheritdoc />
        public void DeleteExpiredAndRevokedApiTokens(int portalId, int userId)
        {
            var now = DateUtils.GetDatabaseUtcTime();
            var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
SET IsDeleted=1
WHERE (IsRevoked=1 OR ExpiresOn<@0)
AND (PortalId=@1 OR @1=-1)
AND (CreatedByUserId=@2 OR @2=-1)
";
            this.dataContext.Execute(System.Data.CommandType.Text, sql, now, portalId, userId);
        }

        /// <inheritdoc />
        public IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize)
        {
            var wheres = new List<string>
                    {
                      "IsDeleted=0",
                    };
            if (includeNarrowerScopes)
            {
                wheres.Add("Scope<=@2");
            }
            else
            {
                wheres.Add("Scope=@2");
            }

            if (portalId > Null.NullInteger)
            {
                wheres.Add("PortalId=@0");
            }

            if (userId > Null.NullInteger)
            {
                wheres.Add("CreatedByUserId=@1");
            }

            switch (filter)
            {
                case ApiTokenFilter.All:
                    break;
                case ApiTokenFilter.Active:
                    wheres.Add("IsRevoked=0 AND ExpiresOn > GETUTCDATE()");
                    break;
                case ApiTokenFilter.Revoked:
                    wheres.Add("IsRevoked=1");
                    break;
                case ApiTokenFilter.Expired:
                    wheres.Add("ExpiresOn <= GETUTCDATE()");
                    break;
                case ApiTokenFilter.RevokedOrExpired:
                    wheres.Add("(IsRevoked=1 OR ExpiresOn <= GETUTCDATE())");
                    break;
            }

            var sql = string.Empty;
            if (!string.IsNullOrEmpty(apiKey))
            {
                sql = "INNER JOIN {databaseOwner}[{objectQualifier}ApiTokenKeys] atk ON atk.ApiTokenId = {objectQualifier}vw_ApiTokens.ApiTokenId ";
                wheres.Add("atk.TokenKey=@3");
            }

            sql += $"WHERE {string.Join(" AND ", wheres)} ORDER BY CreatedOnDate DESC";

            var rep = this.dataContext.GetRepository<ApiToken>();
            return rep.Find(pageIndex, pageSize, sql, portalId, userId, (int)scope, apiKey);
        }

        /// <inheritdoc/>
        public void SetApiTokenLastUsed(ApiTokenBase apiToken)
        {
            var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
SET LastUsedOnDate=GETUTCDATE()
WHERE ApiTokenId=@0";
            this.dataContext.Execute(System.Data.CommandType.Text, sql, apiToken.ApiTokenId);
        }
    }
}
