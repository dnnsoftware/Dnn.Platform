// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
