﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using Microsoft.Extensions.DependencyInjection;

    [DataContract]
    public class UserDetailDto : UserBasicDto
    {
        public UserDetailDto()
        {
        }

        public UserDetailDto(UserInfo user)
            : base(user)
        {
            this.LastLogin = user.Membership.LastLoginDate;
            this.LastActivity = user.Membership.LastActivityDate;
            this.LastPasswordChange = user.Membership.LastPasswordChangeDate;
            this.LastLockout = user.Membership.LastLockoutDate;
            this.IsOnline = user.Membership.IsOnLine;
            this.IsLocked = user.Membership.LockedOut;
            this.NeedUpdatePassword = user.Membership.UpdatePassword;
            this.PortalId = user.PortalID;
            this.UserFolder = FolderManager.Instance.GetUserFolder(user).FolderPath.Substring(6);
            var userFolder = FolderManager.Instance.GetUserFolder(user);
            if (userFolder != null)
            {
                this.UserFolderId = userFolder.FolderID;

                // check whether user had upload files
                var files = FolderManager.Instance.GetFiles(userFolder, true);
                this.HasUserFiles = files.Any();
            }

            this.HasAgreedToTermsOn = user.HasAgreedToTermsOn;
            this.ProfileUrl = this.UserId > 0 ? Globals.UserProfileURL(this.UserId) : null;
            this.EditProfileUrl = this.UserId > 0 ? GetSettingUrl(this.PortalId, this.UserId) : null;
        }

        [DataMember(Name = "profileUrl")]
        public string ProfileUrl { get; set; }

        [DataMember(Name = "editProfileUrl")]
        public string EditProfileUrl { get; set; }

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

        [DataMember(Name = "userFolder")]
        public string UserFolder { get; set; }

        [DataMember(Name = "userFolderId")]
        public int UserFolderId { get; set; }

        [DataMember(Name = "hasUserFiles")]
        public bool HasUserFiles { get; set; }

        [DataMember(Name = "hasAgreedToTermsOn")]
        public DateTime HasAgreedToTermsOn { get; set; }

        private static INavigationManager NavigationManager => Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        private static string GetSettingUrl(int portalId, int userId)
        {
            var module = ModuleController.Instance.GetModulesByDefinition(UserController.Instance.GetUserById(portalId, userId).IsSuperUser ? -1 : portalId, "User Accounts")
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

            var extraParams = new Dictionary<string, string>();
            var skinSrc = Globals.HostPath + "Skins/_default/popUpSkin";
            var containerSrc = Globals.HostPath + "Containers/_default/popUpContainer";
            extraParams.Add("SkinSrc", skinSrc);
            extraParams.Add("ContainerSrc", containerSrc);
            extraParams.Add("mid", module.ModuleID.ToString());
            extraParams.Add("popUp", "true");
            extraParams.Add("UserId", userId.ToString());
            extraParams.Add("editprofile", "true");

            // ctl/Edit/mid/345/packageid/52
            return NavigationManager.NavigateURL(
                tabId,
                PortalSettings.Current,
                "Edit",
                extraParams.Select(p => $"{p.Key}={p.Value}").ToArray());
        }
    }
}
