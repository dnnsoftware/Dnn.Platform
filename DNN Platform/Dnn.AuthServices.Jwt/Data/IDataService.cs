using System.Collections.Generic;
using Dnn.AuthServices.Jwt.Components.Entity;

namespace Dnn.AuthServices.Jwt.Data
{
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
