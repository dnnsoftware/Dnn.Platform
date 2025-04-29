// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Dnn.Modules.ResourceManager.Components;

using DotNetNuke.Abstractions.Security.Permissions;
using DotNetNuke.Security.Permissions;

/// <summary>Represents a permissions set.</summary>
[DataContract]
public class Permissions
{
    /// <summary>Initializes a new instance of the <see cref="Permissions"/> class.</summary>
    public Permissions()
    {
        this.RolePermissions = new List<RolePermission>();
        this.UserPermissions = new List<UserPermission>();
        this.PermissionDefinitions = new List<Permission>();
    }

    /// <summary>Gets or sets the list of permissions definitions.</summary>
    [DataMember(Name = "permissionDefinitions")]
    public IList<Permission> PermissionDefinitions { get; set; }

    /// <summary>Gets or sets a list of role based permissions.</summary>
    [DataMember(Name = "rolePermissions")]
    public IList<RolePermission> RolePermissions { get; set; }

    /// <summary>Gets or sets a list of user based permissions.</summary>
    [DataMember(Name = "userPermissions")]
    public IList<UserPermission> UserPermissions { get; set; }
}
