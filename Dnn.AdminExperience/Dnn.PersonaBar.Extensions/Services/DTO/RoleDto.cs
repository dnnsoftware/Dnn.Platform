// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Services.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    using DotNetNuke.Security.Roles;

    [DataContract]
    public class RoleDto
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "groupId")]
        public int GroupId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "serviceFee")]
        public float ServiceFee { get; set; }

        [DataMember(Name = "billingPeriod")]
        public int BillingPeriod { get; set; }

        [DataMember(Name = "billingFrequency")]
        public string BillingFrequency { get; set; }

        [DataMember(Name = "trialFee")]
        public float TrialFee { get; set; }

        [DataMember(Name = "trialPeriod")]
        public int TrialPeriod { get; set; }

        [DataMember(Name = "trialFrequency")]
        public string TrialFrequency { get; set; }

        [DataMember(Name = "isPublic")]
        public bool IsPublic { get; set; }

        [DataMember(Name = "autoAssign")]
        public bool AutoAssign { get; set; }

        [DataMember(Name = "rsvpCode")]
        public string RsvpCode { get; set; }

        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "status")]
        public RoleStatus Status { get; set; }

        [DataMember(Name = "securityMode")]
        public SecurityMode SecurityMode { get; set; }

        [DataMember(Name = "isSystem")]
        public bool IsSystem { get; set; }

        [DataMember(Name = "usersCount")]
        public long UsersCount { get; set; }

        [DataMember(Name = "allowOwner")]
        public bool AllowOwner { get; set; }

        public static RoleDto FromRoleInfo(RoleInfo role)
        {
            if (role == null) return null;
            return new RoleDto()
            {
                Id = role.RoleID,
                GroupId = role.RoleGroupID,
                Name = role.RoleName,
                Description = role.Description,
                ServiceFee = role.ServiceFee,
                BillingPeriod = role.BillingPeriod,
                BillingFrequency = role.BillingFrequency,
                TrialFee = role.TrialFee,
                TrialPeriod = role.TrialPeriod,
                TrialFrequency = role.TrialFrequency,
                IsPublic = role.IsPublic,
                AutoAssign = role.AutoAssignment,
                RsvpCode = role.RSVPCode,
                Icon = role.IconFile,
                Status = role.Status,
                SecurityMode = role.SecurityMode,
                IsSystem = role.IsSystemRole,
                UsersCount = role.UserCount,
                AllowOwner = (role.SecurityMode == SecurityMode.SocialGroup) || (role.SecurityMode == SecurityMode.Both)
            };
        }

        public RoleInfo ToRoleInfo()
        {
            return new RoleInfo()
            {
                RoleID = this.Id,
                RoleGroupID = this.GroupId,
                RoleName = this.Name,
                Description = this.Description,
                ServiceFee = this.ServiceFee,
                BillingPeriod = this.BillingPeriod,
                BillingFrequency = this.BillingFrequency,
                TrialFee = this.TrialFee,
                TrialPeriod = this.TrialPeriod,
                TrialFrequency = this.TrialFrequency,
                IsPublic = this.IsPublic,
                AutoAssignment = this.AutoAssign,
                RSVPCode = this.RsvpCode,
                IconFile = this.Icon,
                Status = this.Status,
                SecurityMode = this.SecurityMode,
                IsSystemRole = this.IsSystem
            };
        }
    }
}
