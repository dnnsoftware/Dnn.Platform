using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.Roles.Services.DTO
{
    [DataContract]
    public class RoleGroupDto
    {
        public RoleGroupDto()
        {
            Id = -2;
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
                Name = roleGroup.RoleGroupName?.Trim(),
                Description = roleGroup.Description,
                RolesCount = roleGroup.Roles?.Count ?? 0
            };
        }

        public RoleGroupInfo ToRoleGroupInfo()
        {
            return new RoleGroupInfo()
            {
                RoleGroupID = Id,
                RoleGroupName = Name?.Trim(),
                Description = Description ?? ""
            };
        }
    }
}