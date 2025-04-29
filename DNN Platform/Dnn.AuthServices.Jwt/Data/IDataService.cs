// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Data;

using System.Collections.Generic;

using Dnn.AuthServices.Jwt.Components.Entity;

/// <summary>Provides data access services.</summary>
public interface IDataService
{
    /// <summary>Gets a token by a given token id.</summary>
    /// <param name="tokenId">The token id.</param>
    /// <returns><see cref="PersistedToken"/>.</returns>
    PersistedToken GetTokenById(string tokenId);

    /// <summary>Gets a user token by the user id.</summary>
    /// <param name="userId">The id of the user.</param>
    /// <returns>A list of tokens.</returns>
    IList<PersistedToken> GetUserTokens(int userId);

    /// <summary>Adds (persists) a token.</summary>
    /// <param name="token">The token to persist.</param>
    void AddToken(PersistedToken token);

    /// <summary>Updates an existing token.</summary>
    /// <param name="token">The token to persist.</param>
    void UpdateToken(PersistedToken token);

    /// <summary>Deletes an existing token.</summary>
    /// <param name="tokenId">The id of the token to delete.</param>
    void DeleteToken(string tokenId);

    /// <summary>Deletes all tokens for a user.</summary>
    /// <param name="userId">The id of user for which to delete the tokens.</param>
    void DeleteUserTokens(int userId);

    /// <summary>Deletes the expired tokens.</summary>
    void DeleteExpiredTokens();
}
