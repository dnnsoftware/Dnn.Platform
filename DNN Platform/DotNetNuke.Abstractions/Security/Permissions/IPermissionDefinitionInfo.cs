// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Security.Permissions;

/// <summary>PermissionInfo provides the Entity Layer for Permissions.</summary>
public interface IPermissionDefinitionInfo
{
    /// <summary>Gets or sets the Mdoule Definition ID.</summary>
    public int ModuleDefId { get; set; }

    /// <summary>Gets or sets the Permission Code.</summary>
    public string PermissionCode { get; set; }

    /// <summary>Gets or sets the Permission ID.</summary>
    public int PermissionId { get; set; }

    /// <summary>Gets or sets the Permission Key.</summary>
    public string PermissionKey { get; set; }

    /// <summary>Gets or sets the Permission Name.</summary>
    public string PermissionName { get; set; }
}
