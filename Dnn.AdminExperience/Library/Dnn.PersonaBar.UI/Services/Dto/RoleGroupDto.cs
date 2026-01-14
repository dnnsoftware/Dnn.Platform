// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services.DTO
{
    using System;
    using System.Runtime.Serialization;

    using DotNetNuke.Security.Roles;

    /// <summary>Represents information about a role group.</summary>
    [DataContract]
    [Serializable]
    public class RoleGroupDto
    {
        /// <summary>Initializes a new instance of the <see cref="RoleGroupDto"/> class.</summary>
        public RoleGroupDto()
        {
            this.Id = -2;
        }

        /// <summary>Gets or sets the role group ID.</summary>
        [DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>Gets or sets the role group name.</summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the role count.</summary>
        [DataMember(Name = "rolesCount")]
        public int RolesCount { get; set; }

        /// <summary>Gets or sets the role group description.</summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>Converts a <see cref="RoleGroupInfo"/> into a <see cref="RoleGroupDto"/>.</summary>
        /// <param name="roleGroup">The role group.</param>
        /// <returns>A new <see cref="RoleGroupDto"/> instance.</returns>
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

        /// <summary>Converts this instance to a <see cref="RoleGroupInfo"/>.</summary>
        /// <returns>A new <see cref="RoleGroupInfo"/> instance.</returns>
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
}
