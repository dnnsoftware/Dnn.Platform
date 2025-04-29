// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens;

using System;
using System.Globalization;

using DotNetNuke.Entities.Users;

public class DateTimePropertyAccess : IPropertyAccess
{
    /// <inheritdoc/>
    public CacheLevel Cacheability
    {
        get
        {
            return CacheLevel.secureforCaching;
        }
    }

    /// <inheritdoc/>
    public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
    {
        TimeZoneInfo userTimeZone = accessingUser.Profile.PreferredTimeZone;
        switch (propertyName.ToLowerInvariant())
        {
            case "current":
                if (format == string.Empty)
                {
                    format = "D";
                }

                return TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString(format, formatProvider);
            case "now":
                if (format == string.Empty)
                {
                    format = "g";
                }

                return TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString(format, formatProvider);
            case "system":
                if (format == string.Empty)
                {
                    format = "g";
                }

                return DateTime.Now.ToString(format, formatProvider);
            case "utc":
                if (format == string.Empty)
                {
                    format = "g";
                }

                return DateTime.Now.ToUniversalTime().ToString(format, formatProvider);
            default:
                propertyNotFound = true;
                return string.Empty;
        }
    }
}
