// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Globalization;

using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Services.Tokens
{
    public interface IPropertyAccess
    {
        CacheLevel Cacheability { get; }

        string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound);
    }
}
