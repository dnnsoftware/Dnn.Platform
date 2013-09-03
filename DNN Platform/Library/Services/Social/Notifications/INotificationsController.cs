#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;

using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Services.Social.Notifications
{
    /// <summary>
    /// Defines the methods to work with Notifications, NotificationTypes, NotificationTypeActions and NotificationActions.
    /// </summary>
    public interface INotificationsController
    {
        #region NotificationTypes Methods

        /// <summary>
        /// Creates a new notification type.
        /// </summary>
        /// <param name="notificationType"> </param>
        void CreateNotificationType(NotificationType notificationType);
        
        /// <summary>
        /// Deletes an existing notification type.
        /// </summary>
        /// <param name="notificationTypeId">The notification type identifier.</param>
        void DeleteNotificationType(int notificationTypeId);
        
        /// <summary>
        /// Gets a notification type by identifier.
        /// </summary>
        /// <param name="notificationTypeId">The notification type identifier.</param>
        /// <returns>The notification type with the provided identifier.</returns>
        NotificationType GetNotificationType(int notificationTypeId);
        
        /// <summary>
        /// Gets a notification type by name.
        /// </summary>
        /// <param name="name">The notification type name.</param>
        /// <returns>The notification type with the provided name.</returns>
        NotificationType GetNotificationType(string name);

        #endregion

        #region NotificationTypeActions Methods
        
        /// <summary>
        /// Deletes an existing notification type action.
        /// </summary>
        /// <param name="notificationTypeActionId">The notification type action identifier.</param>
        void DeleteNotificationTypeAction(int notificationTypeActionId);
        
        /// <summary>
        /// Gets a notification type action by identifier.
        /// </summary>
        /// <param name="notificationTypeActionId">The notification type action identifier.</param>
        /// <returns>The notification type action with the provided identifier.</returns>
        NotificationTypeAction GetNotificationTypeAction(int notificationTypeActionId);
        
        /// <summary>
        /// Gets a notification type action by notification type and name.
        /// </summary>
        /// <param name="notificationTypeId">The notification type identifier.</param>
        /// <param name="name">The notification type action name.</param>
        /// <returns>The notification type action with the provided notification type and name.</returns>
        NotificationTypeAction GetNotificationTypeAction(int notificationTypeId, string name);
        
        /// <summary>
        /// Gets the list of notification type actions for the provided notification type.
        /// </summary>
        /// <param name="notificationTypeId">The notification type identifier.</param>
        /// <returns>An ordered list of notification type actions for the provided notification type.</returns>
        IList<NotificationTypeAction> GetNotificationTypeActions(int notificationTypeId);

        /// <summary>
        /// Set the actions for a NotificationType
        /// </summary>
        /// <param name="actions">The actions</param>
        /// <param name="notificationTypeId">Id of the notification type</param>
        void SetNotificationTypeActions(IList<NotificationTypeAction> actions, int notificationTypeId);

        #endregion

        #region Notifications Methods

        /// <summary>
        /// Creates a new notification and sets is sender as the portal administrator.
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="portalId">The portalId</param>
        /// <param name="roles">The list of roles to send the notification to. Leave it as null to send only to individual users.</param>
        /// <param name="users">The list of users to send the notification to. Leave it as null to send only to roles.</param>
        /// <returns>The new notification.</returns>
        void SendNotification(Notification notification, int portalId, IList<RoleInfo> roles, IList<UserInfo> users);
        
        /// <summary>
        /// Counts the notifications sent to the provided user in the specified portal.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The number of notifications sent to the provided user in the specified portal.</returns>
        int CountNotifications(int userId, int portalId);
        
        /// <summary>
        /// Gets a list of notifications sent to the provided user in the specified portal.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="afterNotificationId">The notification identifier of the last notification displayed. Use -1 to start from the beggining of the list.</param>
        /// <param name="numberOfRecords">The number of results to retrieve.</param>
        /// <returns>The filtered list of notifications sent to the provided user in the specified portal.</returns>
        /// <example>For example, if we have the following ordered notification identifiers: 4, 6, 2, 12, 45, and we pass 2 as the afterNotificationId and 2 as the numberOfRecords, the method will return the notifications 12 and 45.</example>
        IList<Notification> GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords);

        /// <summary>
        /// Deletes an existing notification.
        /// </summary>
        /// <remarks>It does not delete NotificationRecipient.</remarks>
        /// <param name="notificationId">The notification identifier.</param>
        void DeleteNotification(int notificationId);
        
        /// <summary>
        /// Deletes an individual notification recipient.
        /// </summary>
        /// <remarks>It also deletes the notification if there are no more recipients.</remarks>
        /// <param name="notificationId">The notification identifier.</param>
        /// <param name="userId">The user identifier.</param>
        void DeleteNotificationRecipient(int notificationId, int userId);

        /// <summary>
        /// Deletes all NotificationRecipient for the NotificationId.
        /// </summary>
        /// <remarks>It also deletes the notification.</remarks>
        /// <param name="notificationId">The notification identifier.</param>        
        void DeleteAllNotificationRecipients(int notificationId);

        /// <summary>
        /// Deletes an individual notification recipient based on NotificationTypeId, Context and UserId.
        /// </summary>
        /// <remarks>It also deletes the notification if there are no more recipients.</remarks>
        /// <param name="notificationTypeId">Id of the notification type</param>
        /// <param name="context">Context set by creator of the notification.</param>
        /// <param name="userId">The user identifier.</param>
        void DeleteNotificationRecipient(int notificationTypeId, string context, int userId);

        /// <summary>
        /// Get a Notification
        /// </summary>
        /// <param name="notificationId">The notificationId</param>
        /// <returns>A notification</returns>
        Notification GetNotification(int notificationId);

        /// <summary>
        /// Get a Notification by NotificationTypeId and Context
        /// </summary>
        /// <param name="notificationTypeId">Id of the notification type</param>
        /// <param name="context">Context set by creator of the notification.</param>
        /// <returns>The filtered list of notifications sent to the provided user in the specified portal.</returns>
        IList<Notification> GetNotificationByContext(int notificationTypeId, string context);

        #endregion

		#region Toast APIS

		/// <summary>
		/// Is there a Toast that needs to be sent for a Notification
		/// </summary>
		/// <param name="notificationId">The Notification Id </param>
		/// <returns>True if Toast needs to be sent. False otherwise.</returns>
		bool IsToastPending(int notificationId);

		/// <summary>
		/// Mark a Toast ready for sending
		/// </summary>
		/// <param name="notification">The notification Object </param>
		/// <param name="userInfo">The Recipient User Info Object</param>
		void MarkReadyForToast(Notification notification, UserInfo userInfo);

		/// <summary>
		/// Mark Toast being already sent
		/// </summary>
		/// <param name="notificationId">The notification Id </param>
		/// <param name="userId">The Recipient User Id </param>
		void MarkToastSent(int notificationId, int userId);

		/// <summary>
		/// Get a list of Toasts that have not been delivered yet.
		/// </summary>
		/// <param name="userInfo">UserInfo object</param>
		/// <returns>List of Undelivered Toasts for the user specific to the portal</returns>
		IList<Notification> GetToasts(UserInfo userInfo);

		#endregion
    }
}
