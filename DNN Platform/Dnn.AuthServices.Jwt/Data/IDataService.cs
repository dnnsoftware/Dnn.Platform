// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Data
{
    using System.Collections.Generic;

    using Dnn.AuthServices.Jwt.Components.Entity;

    public interface IDataService
    {
        PersistedToken GetTokenById(string tokenId);

        IList<PersistedToken> GetUserTokens(int userId);

        void AddToken(PersistedToken token);

        void UpdateToken(PersistedToken token);

        void DeleteToken(string tokenId);

        void DeleteUserTokens(int userId);

        void DeleteExpiredTokens();
    }
}
