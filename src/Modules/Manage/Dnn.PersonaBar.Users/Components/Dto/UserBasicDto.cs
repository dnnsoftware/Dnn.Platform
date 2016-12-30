// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using System;
using System.Linq;
using System.Runtime.Serialization;
using Dnn.PersonaBar.Library.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    [DataContract]
    public class UserBasicDto
    {
        private PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "userName")]
        public string Username { get; set; }

        [DataMember(Name = "displayName")]
        public string Displayname { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "createdOnDate")]
        public DateTime CreatedOnDate { get; set; }

        [DataMember(Name = "isDeleted")]
        public bool IsDeleted { get; set; }

        [DataMember(Name = "authorized")]
        public bool Authorized { get; set; }

        [DataMember(Name = "isSuperUser")]
        public bool IsSuperUser { get; set; }

        [DataMember(Name = "isAdmin")]
        public bool IsAdmin { get; set; }


        [DataMember(Name = "avatar")]
        public string AvatarUrl => Utilities.GetProfileAvatar(UserId);

        public UserBasicDto()
        {

        }

        public UserBasicDto(UserInfo user)
        {
            UserId = user.UserID;
            Username = user.Username;
            Displayname = user.DisplayName;
            Email = user.Email;
            CreatedOnDate = user.CreatedOnDate;
            IsDeleted = user.IsDeleted;
            Authorized = user.Membership.Approved;
            IsSuperUser = user.IsSuperUser;
            IsAdmin = user.Roles.Contains(PortalSettings.AdministratorRoleName);
        }

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
                IsSuperUser = user.IsSuperUser
        };
        }
    }
}