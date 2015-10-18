using System;

namespace OAuth.AuthorizationServer.Core.Data.Extensions
{
    /// <summary>
    /// utility datatime class, extends DateTime object
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// return a UTC version of the DateTime object
        /// </summary>
        /// <param name="value">a datetime</param>
        /// <returns>UTC formatted datetime</returns>
        public static DateTime AsUtc(this DateTime value)
        {
            return (value.Kind == DateTimeKind.Unspecified) ? 
                new DateTime(value.Ticks, DateTimeKind.Utc) : 
                value.ToUniversalTime();
        }
    }
}
