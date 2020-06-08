﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Globalization;

using DotNetNuke.Entities.Users;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.Tokens
// ReSharper restore CheckNamespace
{
    public class TicksPropertyAccess : IPropertyAccess
    {
        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "now":
                    return DateTime.Now.Ticks.ToString(formatProvider);
                case "today":
                    return DateTime.Today.Ticks.ToString(formatProvider);
                case "ticksperday":
                    return TimeSpan.TicksPerDay.ToString(formatProvider);
            }
            PropertyNotFound = true;
            return string.Empty;
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.secureforCaching;
            }
        }

        #endregion
    }
}
