// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Cookies
{
    using System;

    public interface IAuthCookieController
    {
        void Update(string cookieValue, DateTime utcExpiry, int userId);

        PersistedAuthCookie Find(string cookieValue);

        void DeleteByValue(string cookieValue);

        void DeleteExpired(DateTime utcExpiredBefore);
    }
}
