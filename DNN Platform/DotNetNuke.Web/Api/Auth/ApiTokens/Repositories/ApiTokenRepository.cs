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
        /// <inheritdoc />
        public ApiToken GetApiToken(int portalId, string tokenHash)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiToken>();
                return rep.Find("WHERE (PortalId=@0 OR PortalId=-1) AND TokenHash=@1 AND IsDeleted=0", portalId, tokenHash).FirstOrDefault();
            }
        }

        /// <inheritdoc />
        public ApiToken GetApiToken(int apiTokenId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiToken>();
                return rep.GetById(apiTokenId);
            }
        }

        /// <inheritdoc />
        public List<string> GetApiTokenKeys(int apiTokenId)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiTokenKey>();
                return rep.Find("WHERE ApiTokenId=@0", apiTokenId).Select(k => k.TokenKey).ToList();
            }
        }

        /// <inheritdoc />
        public ApiToken AddApiToken(ApiTokenBase apiToken, string apiKeys, int userId)
        {
            Requires.NotNull(apiToken);
            apiToken.CreatedByUserId = userId;
            apiToken.CreatedOnDate = DateUtils.GetDatabaseUtcTime();
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiTokenBase>();
                rep.Insert(apiToken);

                var repKeys = context.GetRepository<ApiTokenKey>();
                foreach (var key in apiKeys.Split(','))
                {
                    repKeys.Insert(new ApiTokenKey { ApiTokenId = apiToken.ApiTokenId, TokenKey = key });
                }
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
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiTokenBase>();
                rep.Update(apiToken);
            }
        }

        /// <inheritdoc />
        public void DeleteApiToken(ApiTokenBase apiToken)
        {
            Requires.NotNull(apiToken);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiTokenBase>();
                apiToken.IsDeleted = true;
                rep.Update(apiToken);
            }
        }

        /// <inheritdoc />
        public void DeleteExpiredAndRevokedApiTokens(int portalId, int userId)
        {
            using (var context = DataContext.Instance())
            {
                var now = DateUtils.GetDatabaseUtcTime();
                var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
SET IsDeleted=1
WHERE (IsRevoked=1 OR ExpiresOn<@0)
AND (PortalId=@1 OR @1=-1)
AND (CreatedByUserId=@2 OR @2=-1)
";
                context.Execute(System.Data.CommandType.Text, sql, now, portalId, userId);
            }
        }

        /// <inheritdoc />
        public IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize)
        {
            using (var context = DataContext.Instance())
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

                var rep = context.GetRepository<ApiToken>();
                return rep.Find(pageIndex, pageSize, sql, portalId, userId, (int)scope, apiKey);
            }
        }

        /// <inheritdoc/>
        public void SetApiTokenLastUsed(ApiTokenBase apiToken)
        {
            using (var context = DataContext.Instance())
            {
                var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
SET LastUsedOnDate=GETUTCDATE()
WHERE ApiTokenId=@0";
                context.Execute(System.Data.CommandType.Text, sql, apiToken.ApiTokenId);
            }
        }
    }
}
