// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Globalization;

using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Services.Tokens
{
    public class DateTimePropertyAccess : IPropertyAccess
    {
        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            TimeZoneInfo userTimeZone = AccessingUser.Profile.PreferredTimeZone;
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
                    if (format == String.Empty)
                    {
                        format = "g";
                    }
                    return DateTime.Now.ToString(format, formatProvider);
                case "utc":
                    if (format == String.Empty)
                    {
                        format = "g";
                    }
                    return DateTime.Now.ToUniversalTime().ToString(format, formatProvider);
                default:
                    PropertyNotFound = true;
                    return string.Empty;
            }
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
