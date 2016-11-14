// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using System;
using System.Linq;
using System.Runtime.Serialization;
using Dnn.PersonaBar.Library.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

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

        [IgnoreDataMember]
        public int PortalId { get; set; }


        [DataMember(Name = "profileUrl")]
        public string ProfileUrl => UserId > 0 ? Globals.UserProfileURL(UserId) : null;

        [DataMember(Name = "editProfileUrl")]
        public string EditProfileUrl => UserId > 0 ? GetSettingUrl(PortalId, UserId) : null;

        [DataMember(Name = "userFolder")]
        public string UserFolder { get; set; }

        [DataMember(Name = "userFolderId")]
        public int UserFolderId { get; set; }

        [DataMember(Name = "hasUserFiles")]
        public bool HasUserFiles { get; set; }

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
            PortalId = user.PortalID;
            UserFolder = FolderManager.Instance.GetUserFolder(user).FolderPath.Substring(6);
            var userFolder = FolderManager.Instance.GetUserFolder(user);
            if (userFolder != null)
            {
                UserFolderId = userFolder.FolderID;

                //check whether user had upload files
                var files = FolderManager.Instance.GetFiles(userFolder, true);
                HasUserFiles = files.Any();
            }
        }

        private static string GetSettingUrl(int portalId, int userId)
        {
            var module = ModuleController.Instance.GetModulesByDefinition(portalId, "User Accounts")
                .Cast<ModuleInfo>().FirstOrDefault();
            if (module == null)
            {
                return string.Empty;
            }

            var tabId = TabController.Instance.GetTabsByModuleID(module.ModuleID).Keys.FirstOrDefault();
            if (tabId <= 0)
            {
                return string.Empty;
            }
            //ctl/Edit/mid/345/packageid/52
            return Globals.NavigateURL(tabId, PortalSettings.Current, "Edit",
                                            "mid=" + module.ModuleID,
                                            "popUp=true",
                                            "UserId=" + userId,
                                            "editprofile=true");
        }
    }
}