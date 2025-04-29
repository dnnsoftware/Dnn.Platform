// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Application;

/// <summary>Enumeration Of Application upgrade status.</summary>
public enum UpgradeStatus
{
    /// <summary>The application need update to a higher version.</summary>
    Upgrade = 0,

    /// <summary>The application need to install itself.</summary>
    Install = 1,

    /// <summary>The application is normal running.</summary>
    None = 2,

    /// <summary>The application occur error when running.</summary>
    Error = 3,

    /// <summary>The application status is unknown.</summary>
    /// <remarks>This status should never be returned. its is only used as a flag that Status hasn't been determined.</remarks>
    Unknown = 4,
}
