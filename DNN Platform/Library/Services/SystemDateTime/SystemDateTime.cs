// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.SystemDateTime;

using System;

using DotNetNuke.Data;
using DotNetNuke.Internal.SourceGenerators;

/// <summary>The SystemDateTime provides static method to obtain System's Time.</summary>
/// <remarks>
/// DateTime information is collected from Database. The methods are created to find one unified timestamp from database
/// as opposed to depending on web server's timestamp. This method becomes more relevant in a web farm configuration.
/// </remarks>
public partial class SystemDateTime
{
    private static readonly DataProvider Provider = DataProvider.Instance();

    [DnnDeprecated(7, 1, 2, "Replaced by DateUtils.GetDatabaseUtcTime, which includes caching", RemovalVersion = 10)]
    public static partial DateTime GetCurrentTimeUtc()
    {
        return Provider.GetDatabaseTimeUtc();
    }

    /// <summary>GetCurrentTime get current time from database.</summary>
    /// <returns>DateTime.</returns>
    [DnnDeprecated(9, 1, 0, "Replaced by DateUtils.GetDatabaseLocalTime, which includes caching", RemovalVersion = 10)]
    public static partial DateTime GetCurrentTime()
    {
        return Provider.GetDatabaseTime();
    }
}
