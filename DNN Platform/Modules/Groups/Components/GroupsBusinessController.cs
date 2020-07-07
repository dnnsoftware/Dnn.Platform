// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Social.Notifications;

    public class GroupsBusinessController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "06.02.00":
                    this.AddNotificationTypes();
                    break;
                case "06.02.04":
                    this.RemoveRejectActionForCreatedNotification();
                    break;
                case "07.00.00":
                    ConvertNotificationTypeActionsFor700();
                    break;
            }

            return string.Empty;
        }

        private static void ConvertNotificationTypeActionsFor700()
        {
            var notificationTypeNames = new[] { "GroupPendingNotification", "GroupApprovedNotification", "GroupCreatedNotification", "GroupRejectedNotification", "GroupMemberPendingNotification", "GroupMemberApprovedNotification", "GroupMemberRejectedNotification" };

            foreach (var name in notificationTypeNames)
            {
                var nt = NotificationsController.Instance.GetNotificationType(name);

                var actions = NotificationsController.Instance.GetNotificationTypeActions(nt.NotificationTypeId).ToList();

                if (actions.Any())
                {
                    foreach (var action in actions)
                    {
                        action.APICall = action.APICall.Replace(".ashx", string.Empty);
                        NotificationsController.Instance.DeleteNotificationTypeAction(action.NotificationTypeActionId);
                    }

                    NotificationsController.Instance.SetNotificationTypeActions(actions, nt.NotificationTypeId);
                }
            }
        }

        private void RemoveRejectActionForCreatedNotification()
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(Constants.GroupCreatedNotification);
            if (notificationType == null)
            {
                return;
            }

            var action = NotificationsController.Instance.GetNotificationTypeAction(notificationType.NotificationTypeId, "RejectGroup");
            if (action == null)
            {
                return;
            }

            NotificationsController.Instance.DeleteNotificationTypeAction(action.NotificationTypeActionId);
        }

        private void AddNotificationTypes()
        {
            var actions = new List<NotificationTypeAction>();

            // DesktopModule should not be null
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName("Social Groups").DesktopModuleID;

            // GroupPendingNotification
            var type = new NotificationType { Name = "GroupPendingNotification", Description = "Group Pending Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "Approve",
                    DescriptionResourceKey = "ApproveGroup",
                    APICall = "API/SocialGroups/ModerationService/ApproveGroup",
                });
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "RejectGroup",
                    DescriptionResourceKey = "RejectGroup",
                    APICall = "API/SocialGroups/ModerationService/RejectGroup",
                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            // GroupApprovedNotification
            type = new NotificationType { Name = "GroupApprovedNotification", Description = "Group Approved Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            // GroupCreatedNotification
            type = new NotificationType { Name = "GroupCreatedNotification", Description = "Group Created Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Clear();
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "RejectGroup",
                    DescriptionResourceKey = "RejectGroup",
                    ConfirmResourceKey = "DeleteItem",
                    APICall = "API/SocialGroups/ModerationService/RejectGroup",
                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            // GroupRejectedNotification
            type = new NotificationType { Name = "GroupRejectedNotification", Description = "Group Rejected Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            // GroupMemberPendingNotification
            type = new NotificationType { Name = "GroupMemberPendingNotification", Description = "Group Member Pending Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Clear();
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "Approve",
                    DescriptionResourceKey = "ApproveGroupMember",
                    ConfirmResourceKey = string.Empty,
                    APICall = "API/SocialGroups/ModerationService/ApproveMember",
                });
                actions.Add(new NotificationTypeAction
                {
                    NameResourceKey = "RejectMember",
                    DescriptionResourceKey = "RejectGroupMember",
                    APICall = "API/SocialGroups/ModerationService/RejectMember",
                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            // GroupMemberApprovedNotification
            type = new NotificationType { Name = "GroupMemberApprovedNotification", Description = "Group Member Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            // GroupMemberRejectedNotification
            type = new NotificationType { Name = "GroupMemberRejectedNotification", Description = "Group Rejected Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }
    }
}
