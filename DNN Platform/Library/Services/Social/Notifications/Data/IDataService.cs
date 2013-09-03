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
using System.Data;

namespace DotNetNuke.Services.Social.Notifications.Data
{
    public interface IDataService
    {
        #region NotificationTypes CRUD

        int CreateNotificationType(string name, string description, int timeToLive, int desktopModuleId, int createUpdateUserId);
        void DeleteNotificationType(int notificationTypeId);
        IDataReader GetNotificationType(int notificationTypeId);
        IDataReader GetNotificationTypeByName(string name);

        #endregion

        #region NotificationTypeActions CRUD

        int AddNotificationTypeAction(int notificationTypeId, string nameResourceKey, string descriptionResourceKey, string confirmResourceKey, string apiCall, int createdByUserId);
        void DeleteNotificationTypeAction(int notificationTypeActionId);
        IDataReader GetNotificationTypeAction(int notificationTypeActionId);
        IDataReader GetNotificationTypeActionByName(int notificationTypeId, string name);
        IDataReader GetNotificationTypeActions(int notificationTypeId);

        #endregion

        #region Notifications

        int SendNotification(Notification notification, int portalId);
        void DeleteNotification(int notificationId);
        int CountNotifications(int userId, int portalId);
        IDataReader GetNotifications(int userId, int portalId, int afterNotificationId, int numberOfRecords);
        IDataReader GetNotification(int notificationId);
        IDataReader GetNotificationByContext(int notificationTypeId, string context);

        #endregion

		#region Toast

		bool IsToastPending(int notificationId);

		/// <summary>
		/// Mark a Toast ready for sending
		/// </summary>
		/// <param name="notificationId">The notification Id </param>
		/// <param name="userId">The Recipient User Id </param>
		void MarkReadyForToast(int notificationId, int userId);

		/// <summary>
		/// Mark Toast being already sent
		/// </summary>
		/// <param name="notificationId">The notification Id </param>
		/// <param name="userId">The Recipient User Id </param>
		void MarkToastSent(int notificationId, int userId);

		/// <summary>
		/// Get a list of Toasts that have not been delivered yet.
		/// </summary>
		IDataReader GetToasts(int userId, int portalId);

		#endregion
    }
}
