// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Cookies
{
    using System;
    using System.Web.Caching;
    using System.Web.Security;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class AuthCookieController : ServiceLocator<IAuthCookieController, AuthCookieController>, IAuthCookieController
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        public void Update(string cookieValue, DateTime utcExpiry, int userId)
        {
            if (string.IsNullOrEmpty(cookieValue))
            {
                return;
            }

            DataCache.ClearCache(GetKey(cookieValue));
            this._dataProvider.UpdateAuthCookie(cookieValue, utcExpiry, userId);
        }

        public PersistedAuthCookie Find(string cookieValue)
        {
            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

            return CBO.Instance.GetCachedObject<PersistedAuthCookie>(
                new CacheItemArgs(GetKey(cookieValue), (int)FormsAuthentication.Timeout.TotalMinutes, CacheItemPriority.AboveNormal),
                _ => CBO.Instance.FillObject<PersistedAuthCookie>(this._dataProvider.FindAuthCookie(cookieValue)), false);
        }

        public void DeleteByValue(string cookieValue)
        {
            if (string.IsNullOrEmpty(cookieValue))
            {
                return;
            }

            // keep in cache so hacking tries don't hit the database; it will expire automatically
            // DataCache.ClearCache(GetKey(cookieValue));
            this._dataProvider.DeleteAuthCookie(cookieValue);
        }

        public void DeleteExpired(DateTime utcExpiredBefore)
        {
            this._dataProvider.DeleteExpiredAuthCookies(utcExpiredBefore);
        }

        protected override Func<IAuthCookieController> GetFactory()
        {
            return () => new AuthCookieController();
        }

        private static string GetKey(string cookieValue)
        {
            return $"AuthCookie:{cookieValue}";
        }
    }
}
