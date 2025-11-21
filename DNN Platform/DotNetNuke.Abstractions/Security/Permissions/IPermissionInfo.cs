// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Security.Permissions;

/// <summary>Information about an applied instance of a permission.</summary>
public interface IPermissionInfo : IPermissionDefinitionInfo
{
    /// <summary>Gets or sets a value indicating whether the user or role has permission.</summary>
    bool AllowAccess { get; set; }

    /// <summary>Gets or sets the User's DisplayName.</summary>
    string DisplayName { get; set; }

    /// <summary>Gets or sets the Role ID.</summary>
    int RoleId { get; set; }

    /// <summary>Gets or sets the Role Name.</summary>
    string RoleName { get; set; }

    /// <summary>Gets or sets the User ID.</summary>
    int UserId { get; set; }

    /// <summary>Gets or sets the User Name.</summary>
    string Username { get; set; }
}
