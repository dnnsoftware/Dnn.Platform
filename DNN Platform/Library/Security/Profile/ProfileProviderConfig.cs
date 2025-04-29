// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Profile;

using System.ComponentModel;

/// <summary>The ProfileProviderConfig class provides a wrapper to the Profile providers configuration.</summary>
public class ProfileProviderConfig
{
    private static readonly ProfileProvider ProfileProvider = ProfileProvider.Instance();

    /// <summary>Gets a value indicating whether the Provider Properties can be edited.</summary>
    [Browsable(false)]
    public static bool CanEditProviderProperties
    {
        get
        {
            return ProfileProvider.CanEditProviderProperties;
        }
    }
}
