#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Social.Notifications;

namespace DotNetNuke.Modules.Groups.Components
{
    public class GroupsBusinessController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "06.02.00":
                    AddNotificationTypes();
                    break;
                case "06.02.04":
                    RemoveRejectActionForCreatedNotification();
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
                        action.APICall = action.APICall.Replace(".ashx", "");
                        NotificationsController.Instance.DeleteNotificationTypeAction(action.NotificationTypeActionId);
                    }

                    NotificationsController.Instance.SetNotificationTypeActions(actions, nt.NotificationTypeId);
                }
            }
        }

        private void RemoveRejectActionForCreatedNotification()
        {
            var notificationType = NotificationsController.Instance.GetNotificationType(Constants.GroupCreatedNotification);
            if(notificationType == null)
            {
                return;
            }

            var action = NotificationsController.Instance.GetNotificationTypeAction(notificationType.NotificationTypeId, "RejectGroup");
            if(action == null)
            {
                return;
            }
            NotificationsController.Instance.DeleteNotificationTypeAction(action.NotificationTypeActionId);
        }

        private void AddNotificationTypes()
        {
            var actions = new List<NotificationTypeAction>();
            
            //DesktopModule should not be null
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName("Social Groups").DesktopModuleID;

            //GroupPendingNotification
            var type = new NotificationType { Name = "GroupPendingNotification", Description = "Group Pending Notification", DesktopModuleId = deskModuleId};
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Add(new NotificationTypeAction
                                {
                                    NameResourceKey = "Approve",
                                    DescriptionResourceKey = "ApproveGroup",
                                    APICall = "API/SocialGroups/ModerationService/ApproveGroup"
                                });
                actions.Add(new NotificationTypeAction
                                {
                                    NameResourceKey = "RejectGroup",
                                    DescriptionResourceKey = "RejectGroup",
                                    APICall = "API/SocialGroups/ModerationService/RejectGroup"
                                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            //GroupApprovedNotification
            type = new NotificationType { Name = "GroupApprovedNotification", Description = "Group Approved Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            //GroupCreatedNotification
            type = new NotificationType { Name = "GroupCreatedNotification", Description = "Group Created Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Clear();
                actions.Add(new NotificationTypeAction
                                {
                                    NameResourceKey = "RejectGroup",
                                    DescriptionResourceKey = "RejectGroup",
                                    ConfirmResourceKey = "DeleteItem",
                                    APICall = "API/SocialGroups/ModerationService/RejectGroup"
                                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            //GroupRejectedNotification
            type = new NotificationType { Name = "GroupRejectedNotification", Description = "Group Rejected Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            //GroupMemberPendingNotification
            type = new NotificationType { Name = "GroupMemberPendingNotification", Description = "Group Member Pending Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                actions.Clear();
                actions.Add(new NotificationTypeAction
                                {
                                    NameResourceKey = "Approve",
                                    DescriptionResourceKey = "ApproveGroupMember",
                                    ConfirmResourceKey = "",
                                    APICall = "API/SocialGroups/ModerationService/ApproveMember"
                                });
                actions.Add(new NotificationTypeAction
                                {
                                    NameResourceKey = "RejectMember",
                                    DescriptionResourceKey = "RejectGroupMember",
                                    APICall = "API/SocialGroups/ModerationService/RejectMember"
                                });
                NotificationsController.Instance.CreateNotificationType(type);
                NotificationsController.Instance.SetNotificationTypeActions(actions, type.NotificationTypeId);
            }

            //GroupMemberApprovedNotification
            type = new NotificationType { Name = "GroupMemberApprovedNotification", Description = "Group Member Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }

            //GroupMemberRejectedNotification
            type = new NotificationType { Name = "GroupMemberRejectedNotification", Description = "Group Rejected Notification", DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }
    }
}