// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens
{
    using System.Globalization;

    using DotNetNuke.Entities.Users;

    /// <summary>
    /// Returns an Empty String for all Properties.
    /// </summary>
    /// <remarks></remarks>
    public class EmptyPropertyAccess : IPropertyAccess
    {
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope AccessLevel, ref bool PropertyNotFound)
        {
            return string.Empty;
        }
    }
}
