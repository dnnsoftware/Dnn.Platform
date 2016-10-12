// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using System;
using System.Runtime.Serialization;
using Dnn.PersonaBar.Library.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    [DataContract]
    public class UserDetailDto : UserBasicDto
    {
        [DataMember(Name = "lastLogin")]
        public DateTime LastLogin { get; set; }

        [DataMember(Name = "lastActivity")]
        public DateTime LastActivity { get; set; }

        [DataMember(Name = "lastPasswordChange")]
        public DateTime LastPasswordChange { get; set; }

        [DataMember(Name = "lastLockout")]
        public DateTime LastLockout { get; set; }

        [DataMember(Name = "isOnline")]
        public bool IsOnline { get; set; }

        [DataMember(Name = "isLocked")]
        public bool IsLocked { get; set; }

        [DataMember(Name = "needUpdatePassword")]
        public bool NeedUpdatePassword { get; set; }

        [DataMember(Name = "userFolder")]
        public string UserFolder { get; set; }

        public UserDetailDto()
        {
            
        }

        public UserDetailDto(UserInfo user) : base(user)
        {
            LastLogin = user.Membership.LastLoginDate;
            LastActivity = user.Membership.LastActivityDate;
            LastPasswordChange = user.Membership.LastPasswordChangeDate;
            LastLockout = user.Membership.LastLockoutDate;
            IsOnline = user.Membership.IsOnLine;
            IsLocked = user.Membership.LockedOut;
            NeedUpdatePassword = user.Membership.UpdatePassword;
            UserFolder = FolderManager.Instance.GetUserFolder(user).FolderPath.Substring(6);
        }
    }
}