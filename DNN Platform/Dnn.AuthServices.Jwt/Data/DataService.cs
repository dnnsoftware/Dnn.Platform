#region Copyright
//
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Web.Caching;
using Dnn.AuthServices.Jwt.Components.Entity;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;

namespace Dnn.AuthServices.Jwt.Data
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///  This class provides the Data Access Layer for the JWT Authentication library
    /// </summary>
    public class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        #region implementation

        public virtual PersistedToken GetTokenById(string tokenId)
        {
            try
            {
                return CBO.GetCachedObject<PersistedToken>(
                    new CacheItemArgs(GetCacheKey(tokenId), 60, CacheItemPriority.Default),
                    _ => CBO.FillObject<PersistedToken>(_dataProvider.ExecuteReader("JsonWebTokens_GetById", tokenId)));
            }
            catch (InvalidCastException)
            {
                // occurs when no record found in th DB
                return null;
            }
        }

        public virtual IList<PersistedToken> GetUserTokens(int userId)
        {
            return CBO.FillCollection<PersistedToken>(_dataProvider.ExecuteReader("JsonWebTokens_GetByUserId", userId));
        }

        public virtual void AddToken(PersistedToken token)
        {
            _dataProvider.ExecuteNonQuery("JsonWebTokens_Add", token.TokenId, token.UserId,
                token.TokenExpiry, token.RenewalExpiry, token.TokenHash, token.RenewalHash);
            DataCache.SetCache(GetCacheKey(token.TokenId), token, token.TokenExpiry.ToLocalTime());
        }

        public virtual void UpdateToken(PersistedToken token)
        {
            _dataProvider.ExecuteNonQuery("JsonWebTokens_Update", token.TokenId, token.TokenExpiry, token.TokenHash);
            token.RenewCount += 1;
            DataCache.SetCache(GetCacheKey(token.TokenId), token, token.TokenExpiry.ToLocalTime());
        }

        public virtual void DeleteToken(string tokenId)
        {
            _dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteById", tokenId);
            DataCache.RemoveCache(GetCacheKey(tokenId));
        }

        public virtual void DeleteUserTokens(int userId)
        {
            _dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteByUser", userId);
            foreach (var token in GetUserTokens(userId))
            {
                DataCache.RemoveCache(GetCacheKey(token.TokenId));
            }
        }

        public virtual void DeleteExpiredTokens()
        {
            // don't worry aabout caching; these will already be invalidated by cache manager
            _dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteExpired");
        }

        #endregion

        #region helper methods

        private static string GetCacheKey(string tokenId)
        {
            return string.Join(":", "JsonWebTokens", tokenId);
        }

        #endregion
    }
}