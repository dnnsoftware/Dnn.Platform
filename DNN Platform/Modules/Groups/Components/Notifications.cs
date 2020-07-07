// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Notifications;

    public class Notifications
    {
        internal virtual Notification AddGroupNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser, IList<RoleInfo> moderators)
        {
            return this.AddGroupNotification(notificationTypeName, tabId, moduleId, group, initiatingUser, moderators, null as UserInfo);
        }

        internal virtual Notification AddGroupNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser, IList<RoleInfo> moderators, UserInfo recipient)
        {
            return this.AddGroupNotification(notificationTypeName, tabId, moduleId, group, initiatingUser, moderators, recipient == null ? null : new List<UserInfo> { recipient });
        }

        internal virtual Notification AddGroupNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser, IList<RoleInfo> moderators, IList<UserInfo> recipients)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(notificationTypeName);
            var tokenReplace = new GroupItemTokenReplace(group);

            var subject = Localization.GetString(notificationTypeName + ".Subject", Constants.SharedResourcesPath);
            subject = tokenReplace.ReplaceGroupItemTokens(subject);

            var body = Localization.GetString(notificationTypeName + ".Body", Constants.SharedResourcesPath);

            body = tokenReplace.ReplaceGroupItemTokens(body);
            body = body.Replace("Public.Text", Localization.GetString("Public.Text", Constants.SharedResourcesPath));
            body = body.Replace("Private.Text", Localization.GetString("Private.Text", Constants.SharedResourcesPath));

            bool dismiss = notificationTypeName != Constants.GroupPendingNotification;
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = dismiss,
                SenderUserID = initiatingUser.UserID,
                Context = string.Format("{0}:{1}:{2}", tabId, moduleId, group.RoleID),
            };
            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, moderators, recipients);

            return notification;
        }

        internal virtual Notification AddGroupOwnerNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo initiatingUser)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(notificationTypeName);

            var tokenReplace = new GroupItemTokenReplace(group);

            var subject = Localization.GetString(notificationTypeName + ".Subject", Constants.SharedResourcesPath);
            var body = Localization.GetString(notificationTypeName + ".Body", Constants.SharedResourcesPath);
            subject = subject.Replace("[DisplayName]", initiatingUser.DisplayName);
            subject = subject.Replace("[ProfileUrl]", Globals.UserProfileURL(initiatingUser.UserID));
            subject = tokenReplace.ReplaceGroupItemTokens(subject);
            body = body.Replace("[DisplayName]", initiatingUser.DisplayName);
            body = body.Replace("[ProfileUrl]", Globals.UserProfileURL(initiatingUser.UserID));
            body = tokenReplace.ReplaceGroupItemTokens(body);
            var roleCreator = UserController.GetUserById(group.PortalID, group.CreatedByUserID);

            var roleOwners = new List<UserInfo>();

            foreach (UserInfo userInfo in RoleController.Instance.GetUsersByRole(group.PortalID, group.RoleName))
            {
                var userRoleInfo = RoleController.Instance.GetUserRole(group.PortalID, userInfo.UserID, group.RoleID);
                if (userRoleInfo.IsOwner && userRoleInfo.UserID != group.CreatedByUserID)
                {
                    roleOwners.Add(UserController.GetUserById(group.PortalID, userRoleInfo.UserID));
                }
            }

            roleOwners.Add(roleCreator);

            // Need to add from sender details
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                SenderUserID = initiatingUser.UserID,
                Context = string.Format("{0}:{1}:{2}:{3}", tabId, moduleId, group.RoleID, initiatingUser.UserID),
            };
            NotificationsController.Instance.SendNotification(notification, initiatingUser.PortalID, null, roleOwners);

            return notification;
        }

        internal virtual Notification AddMemberNotification(string notificationTypeName, int tabId, int moduleId, RoleInfo group, UserInfo sender, UserInfo recipient)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(notificationTypeName);

            var subject = Localization.GetString(notificationTypeName + ".Subject", Constants.SharedResourcesPath);

            var body = Localization.GetString(notificationTypeName + ".Body", Constants.SharedResourcesPath);
            var tokenReplace = new GroupItemTokenReplace(group);
            subject = subject.Replace("[DisplayName]", recipient.DisplayName);
            subject = subject.Replace("[ProfileUrl]", Globals.UserProfileURL(recipient.UserID));
            subject = tokenReplace.ReplaceGroupItemTokens(subject);
            body = body.Replace("[DisplayName]", recipient.DisplayName);
            body = body.Replace("[ProfileUrl]", Globals.UserProfileURL(recipient.UserID));
            body = tokenReplace.ReplaceGroupItemTokens(body);

            // Need to add from sender details
            var notification = new Notification
            {
                NotificationTypeID = notificationType.NotificationTypeId,
                Subject = subject,
                Body = body,
                IncludeDismissAction = true,
                SenderUserID = sender.UserID,
                Context = string.Format("{0}:{1}:{2}", tabId, moduleId, group.RoleID),
            };
            NotificationsController.Instance.SendNotification(notification, recipient.PortalID, null, new List<UserInfo> { recipient });

            return notification;
        }
    }
}
