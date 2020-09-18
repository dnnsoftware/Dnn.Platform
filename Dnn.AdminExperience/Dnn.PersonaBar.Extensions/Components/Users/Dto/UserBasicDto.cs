// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Library.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [DataContract]
    public class UserBasicDto
    {
        public UserBasicDto()
        {

        }

        public UserBasicDto(UserInfo user)
        {
            this.UserId = user.UserID;
            this.Username = user.Username;
            this.Displayname = user.DisplayName;
            this.Email = user.Email;
            this.CreatedOnDate = user.CreatedOnDate;
            this.IsDeleted = user.IsDeleted;
            this.Authorized = user.Membership.Approved;
            this.HasAgreedToTerms = user.HasAgreedToTerms;
            this.RequestsRemoval = user.RequestsRemoval;
            this.IsSuperUser = user.IsSuperUser;
            this.IsAdmin = user.Roles.Contains(this.PortalSettings.AdministratorRoleName);
        }

        [DataMember(Name = "avatar")]
        public string AvatarUrl => Utilities.GetProfileAvatar(this.UserId);

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "userName")]
        public string Username { get; set; }

        [DataMember(Name = "displayName")]
        public string Displayname { get; set; }

        [DataMember(Name = "firstName")]
        public string Firstname { get; set; }

        [DataMember(Name = "lastName")]
        public string Lastname { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "createdOnDate")]
        public DateTime CreatedOnDate { get; set; }

        [DataMember(Name = "isDeleted")]
        public bool IsDeleted { get; set; }

        [DataMember(Name = "authorized")]
        public bool Authorized { get; set; }

        [DataMember(Name = "hasAgreedToTerms")]
        public bool HasAgreedToTerms { get; set; }

        [DataMember(Name = "requestsRemoval")]
        public bool RequestsRemoval { get; set; }

        [DataMember(Name = "isSuperUser")]
        public bool IsSuperUser { get; set; }

        [DataMember(Name = "isAdmin")]
        public bool IsAdmin { get; set; }

        private PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        public static UserBasicDto FromUserInfo(UserInfo user)
        {
            if (user == null) return null;
            return new UserBasicDto
            {
                UserId = user.UserID,
                Username = user.Username,
                Displayname = user.DisplayName,
                Email = user.Email,
                CreatedOnDate = user.CreatedOnDate,
                IsDeleted = user.IsDeleted,
                Authorized = user.Membership.Approved,
                HasAgreedToTerms = user.HasAgreedToTerms,
                RequestsRemoval = user.RequestsRemoval,
                IsSuperUser = user.IsSuperUser
            };
        }

        public static UserBasicDto FromUserDetails(UserDetailDto user)
        {
            if (user == null) return null;
            return new UserBasicDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Displayname = user.Displayname,
                Email = user.Email,
                CreatedOnDate = user.CreatedOnDate,
                IsDeleted = user.IsDeleted,
                Authorized = user.Authorized,
                HasAgreedToTerms = user.HasAgreedToTerms,
                RequestsRemoval = user.RequestsRemoval,
                IsSuperUser = user.IsSuperUser
            };
        }
    }
}
