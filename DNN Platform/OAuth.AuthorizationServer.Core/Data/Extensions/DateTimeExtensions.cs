using System;

namespace OAuth.AuthorizationServer.Core.Data.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime AsUtc(this DateTime value)
        {
            return (value.Kind == DateTimeKind.Unspecified) ? 
                new DateTime(value.Ticks, DateTimeKind.Utc) : 
                value.ToUniversalTime();
        }
    }
}
