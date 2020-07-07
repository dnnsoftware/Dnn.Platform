// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    using DotNetNuke.Security.Roles;

    [DataContract]
    public class RoleGroupDto
    {
        public RoleGroupDto()
        {
            this.Id = -2;
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "rolesCount")]
        public int RolesCount { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

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
