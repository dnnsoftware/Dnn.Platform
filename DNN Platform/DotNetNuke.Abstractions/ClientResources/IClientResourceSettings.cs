// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources;

/// <summary>
/// Represents settings for client resource management, including override options and CRM versioning.
/// </summary>
public interface IClientResourceSettings
{
    /// <summary>
    /// Gets a value indicating whether to override the global (host) client resource settings.
    /// </summary>
    bool OverrideDefaultSettings { get; }

    /// <summary>
    /// Gets the host CRM version.
    /// </summary>
    int HostCrmVersion { get; }

    /// <summary>
    /// Gets the portal CRM version.
    /// </summary>
    int PortalCrmVersion { get; }
}
