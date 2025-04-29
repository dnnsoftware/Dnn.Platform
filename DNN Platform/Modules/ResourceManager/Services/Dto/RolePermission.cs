// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>Represents a role based permission.</summary>
[DataContract]
public class RolePermission
{
    /// <summary>Initializes a new instance of the <see cref="RolePermission"/> class.</summary>
    public RolePermission()
    {
        this.Permissions = new List<Permission>();
    }

    /// <summary>Gets or sets the id of the role.</summary>
    [DataMember(Name = "roleId")]
    public int RoleId { get; set; }

    /// <summary>Gets or sets the name of the role.</summary>
    [DataMember(Name = "roleName")]
    public string RoleName { get; set; }

    /// <summary>Gets or sets the list of permissions.</summary>
    [DataMember(Name = "permissions")]
    public IList<Permission> Permissions { get; set; }

    /// <summary>Gets or sets a value indicating whether this permission is locked.</summary>
    [DataMember(Name = "locked")]
    public bool Locked { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a default permission.</summary>
    [DataMember(Name = "default")]
    public bool IsDefault { get; set; }
}
