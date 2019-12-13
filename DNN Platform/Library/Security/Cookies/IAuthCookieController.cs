using System;

namespace DotNetNuke.Security.Cookies
{
    public interface IAuthCookieController
    {
        void Update(string cookieValue, DateTime utcExpiry, int userId);
        PersistedAuthCookie Find(string cookieValue);
        void DeleteByValue(string cookieValue);
        void DeleteExpired(DateTime utcExpiredBefore);
    }
}
