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

using System;
using System.Web.Caching;
using System.Web.Security;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;

namespace DotNetNuke.Security.Cookies
{
    public class AuthCookieController : ServiceLocator<IAuthCookieController, AuthCookieController>, IAuthCookieController
    {
        private readonly DataProvider _dataProvider = DataProvider.Instance();

        protected override Func<IAuthCookieController> GetFactory()
        {
            return () => new AuthCookieController();
        }

        public void Update(string cookieValue, DateTime utcExpiry, int userId)
        {
            if (string.IsNullOrEmpty(cookieValue)) return;

            DataCache.ClearCache(GetKey(cookieValue));
            _dataProvider.UpdateAuthCookie(cookieValue, utcExpiry, userId);
        }

        public PersistedAuthCookie Find(string cookieValue)
        {
            if (string.IsNullOrEmpty(cookieValue)) return null;

            return CBO.Instance.GetCachedObject<PersistedAuthCookie>(
                new CacheItemArgs(GetKey(cookieValue), (int)FormsAuthentication.Timeout.TotalMinutes, CacheItemPriority.AboveNormal),
                _ => CBO.Instance.FillObject<PersistedAuthCookie>(_dataProvider.FindAuthCookie(cookieValue)), false);
        }

        public void DeleteByValue(string cookieValue)
        {
            if (string.IsNullOrEmpty(cookieValue)) return;

            // keep in cache so hacking tries don't hit the database; it will expire automatically
            //DataCache.ClearCache(GetKey(cookieValue));
            _dataProvider.DeleteAuthCookie(cookieValue);
        }

        public void DeleteExpired(DateTime utcExpiredBefore)
        {
            _dataProvider.DeleteExpiredAuthCookies(utcExpiredBefore);
        }

        private static string GetKey(string cookieValue)
        {
            return $"AuthCookie:{cookieValue}";
        }
    }
}