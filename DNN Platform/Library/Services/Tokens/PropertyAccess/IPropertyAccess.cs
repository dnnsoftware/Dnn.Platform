// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Tokens;

using System.Globalization;

using DotNetNuke.Entities.Users;

/// <summary>Provides access to properties.</summary>
public interface IPropertyAccess
{
    /// <summary>Gets the cache level for property access.</summary>
    CacheLevel Cacheability { get; }

    /// <summary>Gets a property.</summary>
    /// <param name="propertyName">The property name to get.</param>
    /// <param name="format">The format string to format the property with, or <c>null</c> to use the default format.</param>
    /// <param name="formatProvider">The format provider (e.g. <see cref="CultureInfo"/>) to use.</param>
    /// <param name="accessingUser">The user this property belongs to, <see cref="UserInfo"/>.</param>
    /// <param name="accessLevel">The <see cref="Scope"/> of the access level for this property.</param>
    /// <param name="propertyNotFound">An out parameter that indicates if the property was not found.</param>
    /// <returns>A string with the value of the property.</returns>
    string GetProperty(
        string propertyName,
        string format,
        CultureInfo formatProvider,
        UserInfo accessingUser,
        Scope accessLevel,
        ref bool propertyNotFound);
}
