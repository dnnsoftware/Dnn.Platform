// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications.Data;

using System.Data;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;

internal class DataService : ComponentBase<IDataService, DataService>, IDataService
{
    private const string Prefix = "CoreMessaging_";
    private readonly DataProvider provider = DataProvider.Instance();

    /// <inheritdoc/>
    public int CreateNotificationType(string name, string description, int timeToLive, int desktopModuleId, int createUpdateUserId, bool isTask)
    {
        return this.provider.ExecuteScalar<int>(GetFullyQualifiedName("CreateNotificationType"), name, this.provider.GetNull(description), this.provider.GetNull(timeToLive), this.provider.GetNull(desktopModuleId), createUpdateUserId, isTask);
    }

    /// <inheritdoc/>
    public void DeleteNotificationType(int notificationTypeId)
    {
        this.provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationType"), notificationTypeId);
    }

    /// <inheritdoc/>
    public IDataReader GetNotificationType(int notificationTypeId)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationType"), notificationTypeId);
    }

    /// <inheritdoc/>
    public IDataReader GetNotificationTypeByName(string name)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeByName"), name);
    }

    /// <inheritdoc/>
    public int AddNotificationTypeAction(int notificationTypeId, string nameResourceKey, string descriptionResourceKey, string confirmResourceKey, string apiCall, int createdByUserId)
    {
        return this.provider.ExecuteScalar<int>(GetFullyQualifiedName("AddNotificationTypeAction"), notificationTypeId, nameResourceKey, this.provider.GetNull(descriptionResourceKey), this.provider.GetNull(confirmResourceKey), apiCall, createdByUserId);
    }

    /// <inheritdoc/>
    public void DeleteNotificationTypeAction(int notificationTypeActionId)
    {
        this.provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotificationTypeAction"), notificationTypeActionId);
    }

    /// <inheritdoc/>
    public IDataReader GetNotificationTypeAction(int notificationTypeActionId)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeAction"), notificationTypeActionId);
    }

    /// <inheritdoc/>
    public IDataReader GetNotificationTypeActionByName(int notificationTypeId, string name)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActionByName"), notificationTypeId, name);
    }

    /// <inheritdoc/>
    public IDataReader GetNotificationTypeActions(int notificationTypeId)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationTypeActions"), notificationTypeId);
    }

    /// <inheritdoc/>
    public int SendNotification(Notification notification, int portalId)
    {
        var createdByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
        return this.provider.ExecuteScalar<int>(
            GetFullyQualifiedName("SendNotification"),
            notification.NotificationTypeID,
            portalId,
            notification.To,
            notification.From,
            notification.Subject,
            notification.Body,
            notification.SenderUserID,
            createdByUserId,
            this.provider.GetNull(notification.ExpirationDate),
            notification.IncludeDismissAction,
            notification.Context);
    }

    /// <inheritdoc/>
    public void DeleteNotification(int notificationId)
    {
        this.provider.ExecuteNonQuery(GetFullyQualifiedName("DeleteNotification"), notificationId);
    }

    /// <inheritdoc/>
    public int DeleteUserNotifications(int userId, int portalId)
    {
        return userId <= 0 ? 0 : this.provider.ExecuteScalar<int>(GetFullyQualifiedName("DeleteUserNotifications"), userId, portalId);
    }

    /// <inheritdoc/>
    public int CountNotifications(int userId, int portalId)
    {
        return userId <= 0 ? 0 : this.provider.ExecuteScalar<int>(GetFullyQualifiedName("CountNotifications"), userId, portalId);
    }

    /// <inheritdoc/>
    public IDataReader GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotifications"), userId, portalId, afterNotificationId, numberOfRecords);
    }

    /// <inheritdoc/>
    public IDataReader GetNotification(int notificationId)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotification"), notificationId);
    }

    /// <inheritdoc/>
    public IDataReader GetNotificationByContext(int notificationTypeId, string context)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetNotificationByContext"), notificationTypeId, context);
    }

    /// <inheritdoc/>
    public bool IsToastPending(int notificationId)
    {
        return this.provider.ExecuteScalar<bool>(
            GetFullyQualifiedName("IsToastPending"),
            notificationId);
    }

    /// <summary>Mark a Toast ready for sending.</summary>
    /// <param name="notificationId">The notification Id. </param>
    /// <param name="userId">The Recipient User Id. </param>
    public void MarkReadyForToast(int notificationId, int userId)
    {
        this.provider.ExecuteNonQuery(GetFullyQualifiedName("MarkReadyForToast"), notificationId, userId);
    }

    /// <summary>Mark Toast being already sent.</summary>
    /// <param name="notificationId">The notification Id. </param>
    /// <param name="userId">The Recipient User Id. </param>
    public void MarkToastSent(int notificationId, int userId)
    {
        this.provider.ExecuteNonQuery(GetFullyQualifiedName("MarkToastSent"), notificationId, userId);
    }

    /// <inheritdoc/>
    public IDataReader GetToasts(int userId, int portalId)
    {
        return this.provider.ExecuteReader(GetFullyQualifiedName("GetToasts"), userId, portalId);
    }

    private static string GetFullyQualifiedName(string procedureName)
    {
        return Prefix + procedureName;
    }
}
