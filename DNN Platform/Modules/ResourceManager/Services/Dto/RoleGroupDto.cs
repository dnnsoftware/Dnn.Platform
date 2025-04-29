// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Services.Dto;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using DotNetNuke.Security.Roles;

/// <summary>A data-transfer-object for role groups.</summary>
[DataContract]
public class RoleGroupDto
{
    /// <summary>Initializes a new instance of the <see cref="RoleGroupDto"/> class.</summary>
    public RoleGroupDto()
    {
        this.Id = -2;
    }

    /// <summary>Gets or sets the ID for this role group.</summary>
    [DataMember(Name = "id")]
    public int Id { get; set; }

    /// <summary>Gets or sets the name of the role group.</summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>Gets or sets a value indicating how many roles are associated with this group.</summary>
    [DataMember(Name = "rolesCount")]
    public int RolesCount { get; set; }

    /// <summary>Gets or sets the description for this role group.</summary>
    [DataMember(Name = "description")]
    public string Description { get; set; }

    /// <summary>Converts a <see cref="RoleGroupInfo"/> into a <see cref="RoleGroupDto"/>.</summary>
    /// <param name="roleGroup">The role group to convert from.</param>
    /// <returns><see cref="RoleGroupDto"/>.</returns>
    public static RoleGroupDto FromRoleGroupInfo(RoleGroupInfo roleGroup)
    {
        return new RoleGroupDto()
        {
            Id = roleGroup.RoleGroupID,
            Name = roleGroup.RoleGroupName,
            Description = roleGroup.Description,
            RolesCount = roleGroup.Roles?.Count ?? 0,
        };
    }

    /// <summary>Converts this <see cref="RoleGroupDto"/> into a <see cref="RoleGroupInfo"/>.</summary>
    /// <returns><see cref="RoleGroupInfo"/>.</returns>
    public RoleGroupInfo ToRoleGroupInfo()
    {
        return new RoleGroupInfo()
        {
            RoleGroupID = this.Id,
            RoleGroupName = this.Name,
            Description = this.Description ?? string.Empty,
        };
    }
}
