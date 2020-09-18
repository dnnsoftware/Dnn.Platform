// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications.Data
{
    using System;
    using System.Data;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;

    internal class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
        private const string Prefix = "CoreMessaging_";
        private readonly DataProvider _provider = DataProvider.Instance();

        public int CreateNotificationType(string name, string description, int timeToLive, int desktopModuleId, int createUpdateUserId, bool isTask)
        {
            return this._provider.ExecuteScalar<int>(GetFullyQualifiedName("CreateNotificationType"), name, this._provider.GetNull(description), this._provider.GetNull(timeToLive), this._provider.GetNull(desktopModuleId), createUpdateUserId, isTask);
        }

        public void DeleteNotificationType(int notificationTypeId)
        {
            this._provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationType"), notificationTypeId);
        }

        public IDataReader GetNotificationType(int notificationTypeId)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotificationType"), notificationTypeId);
        }

        public IDataReader GetNotificationTypeByName(string name)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeByName"), name);
        }

        public int AddNotificationTypeAction(int notificationTypeId, string nameResourceKey, string descriptionResourceKey, string confirmResourceKey, string apiCall, int createdByUserId)
        {
            return this._provider.ExecuteScalar<int>(GetFullyQualifiedName("AddNotificationTypeAction"), notificationTypeId, nameResourceKey, this._provider.GetNull(descriptionResourceKey), this._provider.GetNull(confirmResourceKey), apiCall, createdByUserId);
        }

        public void DeleteNotificationTypeAction(int notificationTypeActionId)
        {
            this._provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationTypeAction"), notificationTypeActionId);
        }

        public IDataReader GetNotificationTypeAction(int notificationTypeActionId)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeAction"), notificationTypeActionId);
        }

        public IDataReader GetNotificationTypeActionByName(int notificationTypeId, string name)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActionByName"), notificationTypeId, name);
        }

        public IDataReader GetNotificationTypeActions(int notificationTypeId)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActions"), notificationTypeId);
        }

        public int SendNotification(Notification notification, int portalId)
        {
            var createdByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            return this._provider.ExecuteScalar<int>(
                GetFullyQualifiedName("SendNotification"),
                notification.NotificationTypeID,
                portalId,
                notification.To,
                notification.From,
                notification.Subject,
                notification.Body,
                notification.SenderUserID,
                createdByUserId,
                this._provider.GetNull(notification.ExpirationDate),
                notification.IncludeDismissAction,
                notification.Context);
        }

        public void DeleteNotification(int notificationId)
        {
            this._provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotification"), notificationId);
        }

        public int DeleteUserNotifications(int userId, int portalId)
        {
            return userId <= 0 ? 0 : this._provider.ExecuteScalar<int>(GetFullyQualifiedName("DeleteUserNotifications"), userId, portalId);
        }

        public int CountNotifications(int userId, int portalId)
        {
            return userId <= 0 ? 0 : this._provider.ExecuteScalar<int>(GetFullyQualifiedName("CountNotifications"), userId, portalId);
        }

        public IDataReader GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotifications"), userId, portalId, afterNotificationId, numberOfRecords);
        }

        public IDataReader GetNotification(int notificationId)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotification"), notificationId);
        }

        public IDataReader GetNotificationByContext(int notificationTypeId, string context)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetNotificationByContext"), notificationTypeId, context);
        }

        public bool IsToastPending(int notificationId)
        {
            return this._provider.ExecuteScalar<bool>(
                GetFullyQualifiedName("IsToastPending"),
                notificationId);
        }

        /// <summary>
        /// Mark a Toast ready for sending.
        /// </summary>
        /// <param name="notificationId">The notification Id. </param>
        /// <param name="userId">The Recipient User Id. </param>
        public void MarkReadyForToast(int notificationId, int userId)
        {
            this._provider.ExecuteNonQuery(GetFullyQualifiedName("MarkReadyForToast"), notificationId, userId);
        }

        /// <summary>
        /// Mark Toast being already sent.
        /// </summary>
        /// <param name="notificationId">The notification Id. </param>
        /// <param name="userId">The Recipient User Id. </param>
        public void MarkToastSent(int notificationId, int userId)
        {
            this._provider.ExecuteNonQuery(GetFullyQualifiedName("MarkToastSent"), notificationId, userId);
        }

        public IDataReader GetToasts(int userId, int portalId)
        {
            return this._provider.ExecuteReader(GetFullyQualifiedName("GetToasts"), userId, portalId);
        }

        private static string GetFullyQualifiedName(string procedureName)
        {
            return Prefix + procedureName;
        }
    }
}
