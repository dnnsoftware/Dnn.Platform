// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Notifications.Data
{
    using System;
    using System.Data;

    public interface IDataService
    {
        /// <summary>
        /// Creates a new Notification Type.
        /// </summary>
        /// <param name="name">Name of the Notification Type.</param>
        /// <param name="description">Description of the Notification Type.</param>
        /// <param name="timeToLive">Time to live of the Notification Type.</param>
        /// <param name="desktopModuleId">Id of the desktop module to which the Notification Type is associated.</param>
        /// <param name="createUpdateUserId">Id of the user that creates and updates the Notification Type.</param>
        /// <param name="isTask">Whether this Notification Type is task or not.</param>
        /// <returns>Id of the created Notification Type. </returns>
        int CreateNotificationType(string name, string description, int timeToLive, int desktopModuleId, int createUpdateUserId, bool isTask);

        /// <summary>
        /// Deletes an existing Notification Type.
        /// </summary>
        /// <param name="notificationTypeId">Id of the Notification Type to delete.</param>
        void DeleteNotificationType(int notificationTypeId);

        /// <summary>
        /// Gets a Notification Type.
        /// </summary>
        /// <param name="notificationTypeId">If of the Notification Type to get.</param>
        /// <returns>IDataReader with the retrieved data.</returns>
        IDataReader GetNotificationType(int notificationTypeId);

        /// <summary>
        /// Gets a Notification Type.
        /// </summary>
        /// <param name="name">Name of the Notification Type to get.</param>
        /// <returns>IDataReader with the retrieved data.</returns>
        IDataReader GetNotificationTypeByName(string name);

        /// <summary>
        /// Creates a new Notification Type Action.
        /// </summary>
        /// <param name="notificationTypeId">Id of the Notification Type to which the Notification Type Action is associated.</param>
        /// <param name="nameResourceKey">Resource key used to get the localized name of the Notification Type Action.</param>
        /// <param name="descriptionResourceKey">Resource key used to get the localized description of the Notification Type Action.</param>
        /// <param name="confirmResourceKey">Resource key used to get the localized confirm text of the Notification Type Action.</param>
        /// <param name="apiCall">Relative url to the api that has to be call when selection the Notification Type Action.</param>
        /// <param name="createdByUserId">Id of the user that created the Notification Type Action.</param>
        /// <returns>The created Notification Type Action.</returns>
        int AddNotificationTypeAction(int notificationTypeId, string nameResourceKey, string descriptionResourceKey, string confirmResourceKey, string apiCall, int createdByUserId);

        /// <summary>
        /// Deletes a Notification Type Action.
        /// </summary>
        /// <param name="notificationTypeActionId">Id of the Notification Type Action to deleted.</param>
        void DeleteNotificationTypeAction(int notificationTypeActionId);

        /// <summary>
        /// Gets a Notification Type Action.
        /// </summary>
        /// <param name="notificationTypeActionId">Id of the Notification Type Action to get.</param>
        /// <returns>DataReader with the retrieved data.</returns>
        IDataReader GetNotificationTypeAction(int notificationTypeActionId);

        /// <summary>
        /// Gets a Notification Type Action.
        /// </summary>
        /// <param name="notificationTypeId">Id of the Notification Type to which the Notification Type Action is associated.</param>
        /// <param name="name">Name of the Notification Type Action.</param>
        /// <returns>DataReader with the retrieved data.</returns>
        IDataReader GetNotificationTypeActionByName(int notificationTypeId, string name);

        /// <summary>
        /// Gets all the Notification Type Action of a Notification Type.
        /// </summary>
        /// <param name="notificationTypeId">Id of the Notification Type from which we want to get the associated Notification Type Actions.</param>
        /// <returns>DataReader with the retrieved data.</returns>
        IDataReader GetNotificationTypeActions(int notificationTypeId);

        /// <summary>
        /// Send a notification to its receivers.
        /// </summary>
        /// <param name="notification">Notification to send.</param>
        /// <param name="portalId">Portal id of the Notification.</param>
        /// <returns>Id of the new message.</returns>
        int SendNotification(Notification notification, int portalId);

        /// <summary>
        /// Delete a Notification.
        /// </summary>
        /// <param name="notificationId">Id of the Notification to delete.</param>
        void DeleteNotification(int notificationId);

        /// <summary>
        /// Deletes all the Notifications of a user.
        /// </summary>
        /// <param name="portalID">Portal Id of the user.</param>
        /// <param name="userID">User from who delete all the Notifications.</param>
        /// <returns>Amount of deleted Notifications.</returns>
        int DeleteUserNotifications(int portalID, int userID);

        /// <summary>
        /// Gets the amount of Notifications of a user.
        /// </summary>
        /// <param name="userId">Id of the user from who count the Notifications.</param>
        /// <param name="portalId">Portal Id of the user.</param>
        /// <returns>The amount of Notifications of a user.</returns>
        int CountNotifications(int userId, int portalId);

        /// <summary>
        /// Gets a paginated list of Notifications.
        /// </summary>
        /// <param name="userId">Id of the user from get the Notifications.</param>
        /// <param name="portalId">Portal Id of the user.</param>
        /// <param name="afterNotificationId">Id of the Notification after which get the list of Notificationss.</param>
        /// <param name="numberOfRecords">Maximum amount of retrieved Notifications.</param>
        /// <returns>DataReader with the retrieved data.</returns>
        IDataReader GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords);

        /// <summary>
        /// Gets a Notification.
        /// </summary>
        /// <param name="notificationId">Id of the Notification to get.</param>
        /// <returns>DataReader with the retrieved data.</returns>
        IDataReader GetNotification(int notificationId);

        /// <summary>
        /// Gets a Notification.
        /// </summary>
        /// <param name="notificationTypeId">Id of the Noticationb type of the Notification.</param>
        /// <param name="context">Context of the Notification to get.</param>
        /// <returns>DataReader with the retrieved data.</returns>
        IDataReader GetNotificationByContext(int notificationTypeId, string context);

        /// <summary>
        /// Whether a Toast Notification is pending or not.
        /// </summary>
        /// <param name="notificationId">Id of the Notification to check.</param>
        /// <returns>True if the Toast Notification is pending, False if it is not.</returns>
        bool IsToastPending(int notificationId);

        /// <summary>
        /// Mark a Toast ready for sending.
        /// </summary>
        /// <param name="notificationId">The notification Id. </param>
        /// <param name="userId">The Recipient User Id. </param>
        void MarkReadyForToast(int notificationId, int userId);

        /// <summary>
        /// Mark Toast being already sent.
        /// </summary>
        /// <param name="notificationId">The notification Id. </param>
        /// <param name="userId">The Recipient User Id. </param>
        void MarkToastSent(int notificationId, int userId);

        /// <summary>
        /// Get a list of Toast Notifications that have not been delivered yet.
        /// </summary>
        /// <param name="userId">Id of the user from who we want to know which Toast Notifications have not been delivered yet.</param>
        /// <param name="portalId">Portal Id of the user.</param>
        /// <returns>DataReader with the retrieved data.</returns>
        IDataReader GetToasts(int userId, int portalId);
    }
}
