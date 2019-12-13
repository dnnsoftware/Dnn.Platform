// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
// ReSharper disable CheckNamespace

using System;
using System.Globalization;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Tokens
{
    public class AntiForgeryTokenPropertyAccess : IPropertyAccess
    {
        public CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            return String.Empty;
        }
    }
}
