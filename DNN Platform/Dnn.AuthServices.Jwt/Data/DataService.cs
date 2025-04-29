// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Data;

using System;
using System.Collections.Generic;
using System.Web.Caching;

using Dnn.AuthServices.Jwt.Components.Entity;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;

/// <summary>This class provides the Data Access Layer for the JWT Authentication library.</summary>
public class DataService : ComponentBase<IDataService, DataService>, IDataService
{
    private readonly DataProvider dataProvider = DataProvider.Instance();

    /// <inheritdoc/>
    public virtual PersistedToken GetTokenById(string tokenId)
    {
        try
        {
            return CBO.GetCachedObject<PersistedToken>(
                new CacheItemArgs(GetCacheKey(tokenId), 60, CacheItemPriority.Default),
                _ => CBO.FillObject<PersistedToken>(this.dataProvider.ExecuteReader("JsonWebTokens_GetById", tokenId)));
        }
        catch (InvalidCastException)
        {
            // occurs when no record found in th DB
            return null;
        }
    }

    /// <inheritdoc/>
    public virtual IList<PersistedToken> GetUserTokens(int userId)
    {
        return CBO.FillCollection<PersistedToken>(this.dataProvider.ExecuteReader("JsonWebTokens_GetByUserId", userId));
    }

    /// <inheritdoc/>
    public virtual void AddToken(PersistedToken token)
    {
        this.dataProvider.ExecuteNonQuery(
            "JsonWebTokens_Add",
            token.TokenId,
            token.UserId,
            token.TokenExpiry,
            token.RenewalExpiry,
            token.TokenHash,
            token.RenewalHash);
        DataCache.SetCache(GetCacheKey(token.TokenId), token, token.TokenExpiry.ToLocalTime());
    }

    /// <inheritdoc/>
    public virtual void UpdateToken(PersistedToken token)
    {
        this.dataProvider.ExecuteNonQuery("JsonWebTokens_Update", token.TokenId, token.TokenExpiry, token.TokenHash);
        token.RenewCount += 1;
        DataCache.SetCache(GetCacheKey(token.TokenId), token, token.TokenExpiry.ToLocalTime());
    }

    /// <inheritdoc/>
    public virtual void DeleteToken(string tokenId)
    {
        this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteById", tokenId);
        DataCache.RemoveCache(GetCacheKey(tokenId));
    }

    /// <inheritdoc/>
    public virtual void DeleteUserTokens(int userId)
    {
        this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteByUser", userId);
        foreach (var token in this.GetUserTokens(userId))
        {
            DataCache.RemoveCache(GetCacheKey(token.TokenId));
        }
    }

    /// <inheritdoc/>
    public virtual void DeleteExpiredTokens()
    {
        // don't worry about caching; these will already be invalidated by cache manager
        this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteExpired");
    }

    private static string GetCacheKey(string tokenId)
    {
        return string.Join(":", "JsonWebTokens", tokenId);
    }
}
