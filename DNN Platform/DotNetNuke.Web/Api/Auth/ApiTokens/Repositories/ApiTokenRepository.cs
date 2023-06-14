// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <inheritdoc />
    internal class ApiTokenRepository : ServiceLocator<IApiTokenRepository, ApiTokenRepository>, IApiTokenRepository
    {
        /// <inheritdoc />
        public ApiToken GetApiToken(int portalId, string tokenHash)
        {
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiToken>();
                return rep.Find("WHERE (PortalId=@0 OR PortalId=-1) AND TokenHash=@1", portalId, tokenHash).FirstOrDefault();
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
        public ApiToken AddApiToken(ApiToken apiToken)
        {
            Requires.NotNull(apiToken);
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiToken>();
                rep.Insert(apiToken);
            }
            return apiToken;
        }

        /// <inheritdoc />
        public void RevokeApiToken(ApiToken apiToken)
        {
            Requires.NotNull(apiToken);
            apiToken.IsRevoked = true;
            apiToken.RevokedOnDate = DateTime.UtcNow;
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<ApiToken>();
                rep.Update(apiToken);
            }
        }

        /// <inheritdoc />
        protected override Func<IApiTokenRepository> GetFactory()
        {
            return () => new ApiTokenRepository();
        }
    }
}
