// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Security;

/// <summary>The SiteSslSetup is used in the portal settings to indicate what the global SSL setup is for the site.</summary>
public enum SiteSslSetup
{
    /// <summary>No SSL.</summary>
    Off = 0,

    /// <summary>Complete SSL.</summary>
    On = 1,

    /// <summary>Ability to tweak SSL settings.</summary>
    Advanced = 2,
}
