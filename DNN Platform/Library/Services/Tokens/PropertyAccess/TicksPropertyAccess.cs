// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.Tokens

// ReSharper restore CheckNamespace
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;

    public class TicksPropertyAccess : IPropertyAccess
    {
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.secureforCaching;
            }
        }

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
    }
}
