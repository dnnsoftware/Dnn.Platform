// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    public class AntiForgeryTokenPropertyAccess : IPropertyAccess
    {
        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            return string.Empty;
        }
    }
}
